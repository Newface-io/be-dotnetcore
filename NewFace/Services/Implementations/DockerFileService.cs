using NewFace.Data;
using NewFace.DTOs.File;
using NewFace.Models;
using System.IO.Compression;

namespace NewFace.Services;

public class DockerFileService : IDockerFileService
{
    private readonly string _basePath;
    private readonly DataContext _context;
    private readonly ILogService _logService;

    public DockerFileService(IConfiguration configuration, DataContext context, ILogService logService)
    {
        _context = context;
        _logService = logService;
        _basePath = configuration["FileStorage:BasePath"] ?? "/app/uploads";
    }

    private bool IsAllowedFileType(string fileExtension)
    {
        string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".mp4", ".avi", ".mov", ".wmv", ".flv", ".mkv" };
        return allowedExtensions.Contains(fileExtension);
    }

    private bool IsVideoFile(string fileExtension)
    {
        string[] videoExtensions = { ".mp4", ".avi", ".mov", ".wmv", ".flv", ".mkv" };
        return videoExtensions.Contains(fileExtension);
    }

    public bool IsAllowedImageFileType(string fileExtension)
    {
        string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif"};
        return allowedExtensions.Contains(fileExtension);
    }

    public async Task<bool> DeleteFileAndEmptyFolder(string relativePath)
    {
        try
        {
            string fullPath = Path.Combine(_basePath, relativePath.TrimStart('/'));
            string directory = Path.GetDirectoryName(fullPath);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }

            if (Directory.Exists(directory))
            {
                if (!Directory.EnumerateFileSystemEntries(directory).Any())
                {
                    Directory.Delete(directory, false);
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            _logService.LogError("EXCEPTION: DeleteFileAndEmptyFolder", ex.Message, "relativePath: " + relativePath);
            return false;
        }
    }

    public async Task<List<UserFileInfo>> GetUserFilesByType(int userId, string type)
    {
        try
        {
            var userFiles = await _context.UserFile
                .Where(uf => uf.UserId == userId && uf.Type == type && !uf.IsDeleted)
                .OrderBy(uf => uf.Sort)
                .Select(uf => new UserFileInfo
                {
                    Id = uf.Id,
                    FileName = uf.Name,
                    FilePath = uf.Path,
                    FileType = uf.Type,
                    UploadDate = uf.CreatedDate
                })
                .ToListAsync();

            return userFiles;
        }
        catch (Exception ex)
        {
            _logService.LogError("EXCEPTION", ex.Message, $"Error getting files for userId: {userId}, fileType: {type}");
            return new List<UserFileInfo>();
        }
    }

    public async Task<string> UploadImageAndGetUrl(IFormFile file, string relativePath)
    {
        var result = string.Empty;

        try
        {
            if (file == null || file.Length == 0)
            {
                _logService.LogError("ERROR", "파일이 존재하지 않습니다.", $"Error uploading file to path: {relativePath}");
                return result;
            }

            string fileExtension = Path.GetExtension(file.FileName).ToLower();

            if (!IsAllowedFileType(fileExtension))
            {
                _logService.LogError("ERROR", file.FileName + ": 허용되지 않는 파일 타입입니다.", $"file type: {fileExtension}");
                return result;
            }

            string fileName = GenerateFileName(file.FileName);
            string fullRelativePath = Path.Combine(relativePath, fileName);
            string fullPath = Path.Combine(_basePath, fullRelativePath);

            await SaveFileAsync(file, fullPath);

            result = _basePath + fullRelativePath.Replace("\\", "/");

            return result;
        }
        catch (Exception ex)
        {
            _logService.LogError("EXCEPTION", ex.Message, $"Error uploading file to path: {relativePath}");
            return result;
        }
    }


    public async Task<bool> UploadFiles(FileUploadRequest request)
    {
        if (request.Files == null || request.Files.Count == 0)
            return false;

        try
        {
            var user = await _context.Users.FindAsync(request.UserId);
            if (user == null)
                return false;

            // 모든 파일의 형식을 먼저 검사
            foreach (var fileInfo in request.Files)
            {
                string fileExtension = Path.GetExtension(fileInfo.File.FileName).ToLower();
                if (!IsAllowedFileType(fileExtension))
                {
                    _logService.LogError("Invalid file type", $"Invalid file type: {fileInfo.File.FileName}", $"UserId: {request.UserId}");
                    return false;
                }
            }

            var userFiles = new List<UserFile>();

            foreach (var fileInfo in request.Files)
            {
                string originalFileName = fileInfo.File.FileName;
                string fileExtension = Path.GetExtension(originalFileName).ToLower();
                bool isVideo = IsVideoFile(fileExtension);

                string fileName;
                string relativePath;
                string fullPath;

                if (isVideo)
                {
                    // 동영상 파일 처리
                    var (manifestPath, videoSegmentsPaths) = await ProcessVideoFileAsync(fileInfo.File, request.UserId, fileInfo.Category);

                    var userFile = new UserFile
                    {
                        UserId = request.UserId,
                        Category = fileInfo.Category,
                        FileType = fileExtension.TrimStart('.'),
                        Type = "video",
                        Path = manifestPath,
                        Name = Path.GetFileName(manifestPath),
                        Sort = await GetNextSortOrderAsync(request.UserId, fileInfo.Category),
                        CreatedDate = DateTime.Now,
                        LastUpdated = DateTime.Now
                    };

                    userFiles.Add(userFile);
                }
                else
                {
                    // 이미지 파일 처리
                    fileName = GenerateFileName(originalFileName);
                    relativePath = Path.Combine(request.UserId.ToString(), fileInfo.Category, fileName);
                    fullPath = Path.Combine(_basePath, relativePath);
                    await SaveFileAsync(fileInfo.File, fullPath);

                    var userFile = new UserFile
                    {
                        UserId = request.UserId,
                        Category = fileInfo.Category,
                        FileType = Path.GetExtension(fileInfo.File.FileName).TrimStart('.').ToLower(), // 파일 확장자
                        Type = isVideo ? "video" : "image",
                        Path = relativePath,
                        Name = fileName,
                        Sort = await GetNextSortOrderAsync(request.UserId, fileInfo.Category),
                        CreatedDate = DateTime.Now,
                        LastUpdated = DateTime.Now
                    };

                    userFiles.Add(userFile);
                }

            }

            _context.UserFile.AddRange(userFiles);
            await _context.SaveChangesAsync();

            return true;
        }
        catch (Exception ex)
        {
            _logService.LogError("EXCEPTION", ex.Message, $"Error uploading files for userId: {request.UserId}");
            return false;
        }
    }

    private async Task<(string manifestPath, List<string> segmentPaths)> ProcessVideoFileAsync(IFormFile file, int userId, string category)
    {
        var baseFileName = Path.GetFileNameWithoutExtension(file.FileName);
        var directoryPath = Path.Combine(_basePath, userId.ToString(), category, baseFileName);
        Directory.CreateDirectory(directoryPath);

        var qualities = new[] { "360p", "720p", "1080p" };
        var segmentPaths = new List<string>();

        foreach (var quality in qualities)
        {
            var qualityPath = Path.Combine(directoryPath, quality);
            Directory.CreateDirectory(qualityPath);

            // 여기서 실제로 FFmpeg 등을 사용하여 비디오를 인코딩하고 세그먼트화해야 합니다.
            // 이 예제에서는 단순화를 위해 더미 파일을 생성합니다.
            for (int i = 0; i < 10; i++)
            {
                var segmentPath = Path.Combine(qualityPath, $"segment_{i}.ts");
                await File.WriteAllTextAsync(segmentPath, $"Dummy segment {i} for {quality}");
                segmentPaths.Add(segmentPath);
            }
        }

        var manifestPath = Path.Combine(directoryPath, "manifest.m3u8");
        await GenerateManifestFileAsync(manifestPath, qualities, baseFileName);

        return (manifestPath, segmentPaths);
    }

    private async Task GenerateManifestFileAsync(string manifestPath, string[] qualities, string baseFileName)
    {
        var manifestContent = "#EXTM3U\n";
        foreach (var quality in qualities)
        {
            manifestContent += $"#EXT-X-STREAM-INF:BANDWIDTH={GetBandwidthForQuality(quality)},RESOLUTION={GetResolutionForQuality(quality)}\n";
            manifestContent += $"{quality}/{baseFileName}_{quality}.m3u8\n";
        }

        await File.WriteAllTextAsync(manifestPath, manifestContent);
    }

    private int GetBandwidthForQuality(string quality)
    {
        // 실제 구현에서는 적절한 대역폭 값을 반환해야 합니다.
        return quality switch
        {
            "360p" => 800000,
            "720p" => 2400000,
            "1080p" => 4800000,
            _ => 800000
        };
    }

    private string GetResolutionForQuality(string quality)
    {
        return quality switch
        {
            "360p" => "640x360",
            "720p" => "1280x720",
            "1080p" => "1920x1080",
            _ => "640x360"
        };
    }

    private async Task<int> GetNextSortOrderAsync(int userId, string fileType)
    {
        var maxSort = await _context.UserFile
            .Where(uf => uf.UserId == userId && uf.Type == fileType)
            .MaxAsync(uf => (int?)uf.Sort) ?? 0;
        return maxSort + 1;
    }

    private string GenerateFileName(string originalFileName)
    {
        string dateString = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(originalFileName);
        string fileExtension = Path.GetExtension(originalFileName);
        return $"{fileNameWithoutExtension}_{dateString}_{Guid.NewGuid():N}{fileExtension}";
    }

    private async Task SaveFileAsync(IFormFile file, string fullPath)
    {
        string directory = Path.GetDirectoryName(fullPath);
        Directory.CreateDirectory(directory);

        using var stream = new FileStream(fullPath, FileMode.Create);
        await file.CopyToAsync(stream);
    }

    private async Task CompressAndSaveFileAsync(IFormFile file, string fullPath)
    {
        string directory = Path.GetDirectoryName(fullPath);
        Directory.CreateDirectory(directory);

        using (var compressedFileStream = new FileStream(fullPath, FileMode.Create))
        using (var gzipStream = new GZipStream(compressedFileStream, CompressionLevel.Optimal))
        using (var originalFileStream = file.OpenReadStream())
        {
            await originalFileStream.CopyToAsync(gzipStream);
        }
    }

    private string GenerateCompressedFileName(string originalFileName)
    {
        string dateString = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(originalFileName);
        return $"{fileNameWithoutExtension}_{dateString}_{Guid.NewGuid():N}.gz";
    }

}