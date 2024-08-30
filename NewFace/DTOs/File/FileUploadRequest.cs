namespace NewFace.DTOs.File;

// upload file dto
public class FileUploadRequest
{
    public int UserId { get; set; }
    public List<FileUploadInfo> Files { get; set; } = new List<FileUploadInfo>();
}

public class FileUploadInfo
{
    public IFormFile File { get; set; }
    public string Category { get; set; } = string.Empty;
}

// get user file by type
public class UserFileInfo
{
    public int Id { get; set; }
    public string FileName { get; set; }
    public string FilePath { get; set; }
    public string FileType { get; set; }
    public DateTime UploadDate { get; set; }
}