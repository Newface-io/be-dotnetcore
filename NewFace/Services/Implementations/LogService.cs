using NewFace.Data;
using NewFace.Models;

namespace NewFace.Services;

public class LogService : ILogService
{
    private readonly DataContext _context;

    public LogService(DataContext context)
    {
        _context = context;
    }

    public void LogError(string message, string exception, string additionalData)
    {
        var log = new SystemLog
        {
            LogLevel = "Error",
            Message = message,
            Exception = exception,
            AdditionalData = additionalData
        };
        _context.SystemLogs.Add(log);
        _context.SaveChanges();
    }
}