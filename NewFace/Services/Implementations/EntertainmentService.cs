using Microsoft.EntityFrameworkCore;
using NewFace.Common.Constants;
using NewFace.Data;
using NewFace.DTOs.Actor;
using NewFace.Models;
using NewFace.Models.Entertainment;
using NewFace.Responses;
using NewFace.Services.Interfaces;

namespace NewFace.Services.Implementations;

public class EntertainmentService : IEntertainmentService
{

    private readonly DataContext _context;
    private readonly ILogService _logService;

    public EntertainmentService(DataContext context, ILogService logService)
    {
        _context = context;
        _logService = logService;
    }

    public string CompanyType { get; set; } = string.Empty;

    public string CompanyName { get; set; } = string.Empty;
    public string CeoName { get; set; } = string.Empty;
    public string CompanyAddress { get; set; } = string.Empty;

    public string ContactName { get; set; } = string.Empty;

    public string ContactPhone { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;

    public string ContactDepartment { get; set; } = string.Empty;
    public string ContactPosition { get; set; } = string.Empty;

    public async Task<ServiceResponse<int>> AddEntertainmentProfile(AddEntertainmentProfileRequestDto model)
    {
        var response = new ServiceResponse<int>();

        try
        {
            if (!await _context.Users.AnyAsync(u => u.Id == model.UserId))
            {
                response.Success = false;
                response.Data = 0;
                response.Code = MessageCode.Custom.NOT_FOUND_USER.ToString();
                response.Message = MessageCode.CustomMessages[MessageCode.Custom.NOT_FOUND_USER];

                return response;
            }

            var entertainment = new Entertainment
            {
                UserId = model.UserId,
                CompanyType = model.CompanyType,
                CompanyName = model.CompanyName,
                CeoName = model.CeoName,
                CompanyAddress = model.CompanyAddress,
                ContactName = model.ContactName,
                ContactPhone = model.ContactPhone,
                ContactEmail = model.ContactEmail,
                ContactDepartment = model.ContactDepartment,
                ContactPosition = model.ContactPosition,
            };

            await _context.Entertainments.AddAsync(entertainment);
            await _context.SaveChangesAsync();

            response.Success = true;
            response.Data = entertainment.Id;
            return response;
        }
        catch (Exception ex)
        {
            response.Success = false;
            response.Code = MessageCode.Custom.UNKNOWN_ERROR.ToString();
            response.Message = MessageCode.CustomMessages[MessageCode.Custom.UNKNOWN_ERROR];

            _logService.LogError("EXCEPTION", ex.Message, "user id: " + model.UserId);

            return response;
        }
    }
}
