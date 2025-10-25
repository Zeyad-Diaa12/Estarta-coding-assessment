using EmployeeStatus.DTOs;

namespace EmployeeStatus.Services;

public interface IProcessStatusService
{
    Task<(GetEmpStatusResponse? response, int statusCode, string? error)> ProcessEmployeeStatusAsync(string nationalNumber);
}
