using Microsoft.AspNetCore.StaticFiles;
using Microsoft.VisualBasic.FileIO;
using NewFace.Data;
using NewFace.DTOs.File;
using NewFace.Models;

namespace NewFace.Services;

public class FileService : IFileService
{
    private readonly string _basePath;
    private readonly DataContext _context;
    private readonly ILogService _logService;

    public FileService(IConfiguration configuration, DataContext context, ILogService logService)
    {
        _context = context;
        _logService = logService;
        _basePath = configuration["FileStorage:BasePath"] ?? "/app/uploads";
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


    public async Task<bool> UploadFiles(FileUploadRequest request)
    {
        if (request.Files == null || request.Files.Count == 0)
            return false;

        try
        {
            var user = await _context.Users.FindAsync(request.UserId);
            if (user == null)
                return false;

            var userFiles = new List<UserFile>();

            foreach (var fileInfo in request.Files)
            {
                string fileName = GenerateFileName(fileInfo.File.FileName);
                string relativePath = Path.Combine(request.UserId.ToString(), fileInfo.FileType, fileName);
                string fullPath = Path.Combine(_basePath, relativePath);

                await SaveFileAsync(fileInfo.File, fullPath);

                var userFile = new UserFile
                {
                    UserId = request.UserId,
                    Type = fileInfo.FileType,
                    Path = relativePath,
                    Name = fileName,
                    Sort = await GetNextSortOrderAsync(request.UserId, fileInfo.FileType),
                    CreatedDate = DateTime.Now,
                    LastUpdated = DateTime.Now
                };

                userFiles.Add(userFile);
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

}