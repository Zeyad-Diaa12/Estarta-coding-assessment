using EmployeeStatus.DataAccess;
using EmployeeStatus.DTOs;
using EmployeeStatus.Models;

namespace EmployeeStatus.Services;

public class ProcessStatusService : IProcessStatusService
{
    private readonly IDataAccess _dataAccess;
    private readonly IBusinessRulesService _businessRules;
    private readonly ILogger<ProcessStatusService> _logger;

    public ProcessStatusService(
        IDataAccess dataAccess,
        IBusinessRulesService businessRules,
        ILogger<ProcessStatusService> logger)
    {
        _dataAccess = dataAccess;
        _businessRules = businessRules;
        _logger = logger;
    }

    public async Task<(GetEmpStatusResponse? response, int statusCode, string? error)> ProcessEmployeeStatusAsync(string nationalNumber)
    {
        try
        {
            // Fetch user
            var user = await _dataAccess.GetUserByNationalNumberAsync(nationalNumber);
            if (user == null)
            {
                _logger.LogWarning("User not found with National Number: {NationalNumber}", nationalNumber);
                return (null, 404, "Invalid National Number");
            }

            // Check if user is active
            if (!user.IsActive)
            {
                _logger.LogWarning("User with National Number {NationalNumber} is not active", nationalNumber);
                return (null, 406, "User is not Active");
            }

            // Fetch salaries
            var salaries = await _dataAccess.GetSalariesByUserIdAsync(user.Id);
            if (salaries.Count < 3)
            {
                _logger.LogWarning("Insufficient salary data for user {NationalNumber}. Found {Count} records, need at least 3", 
                    nationalNumber, salaries.Count);
                return (null, 422, "INSUFFICIENT_DATA");
            }

            // Apply salary adjustments
            var adjustedSalaries = _businessRules.ApplySalaryAdjustments(salaries);

            // Apply tax and deductions
            var finalSalaries = _businessRules.ApplyTaxAndDeductions(adjustedSalaries);

            // Compute statistics
            var sum = finalSalaries.Sum();
            var average = finalSalaries.Average();
            var highest = finalSalaries.Max();

            // Determine status using business rules
            var status = _businessRules.DetermineStatus(average);

            var response = new GetEmpStatusResponse
            {
                EmployeeName = user.Username,
                NationalNumber = user.NationalNumber,
                AverageSalary = Math.Round(average, 2),
                HighestSalary = Math.Round(highest, 2),
                SumSalary = Math.Round(sum, 2),
                Status = status,
                IsActive = user.IsActive,
                LastUpdated = DateTime.UtcNow
            };

            _logger.LogInformation(
                "Successfully processed status for {NationalNumber}: Average={Average}, Status={Status}",
                nationalNumber, average, status);

            return (response, 200, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing employee status for {NationalNumber}", nationalNumber);
            return (null, 500, "Internal Server Error");
        }
    }
}
