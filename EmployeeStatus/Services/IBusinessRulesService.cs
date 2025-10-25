namespace EmployeeStatus.Services;

/// <summary>
/// Interface for business rules related to salary calculations
/// </summary>
public interface IBusinessRulesService
{
    /// <summary>
    /// Applies monthly adjustments to salaries (December bonus, summer deductions)
    /// </summary>
    List<decimal> ApplySalaryAdjustments(List<Models.Salary> salaries);

    /// <summary>
    /// Applies tax deductions based on total salary and configured thresholds
    /// </summary>
    List<decimal> ApplyTaxAndDeductions(List<decimal> salaries);

    /// <summary>
    /// Determines employee status (GREEN, ORANGE, RED) based on average salary
    /// </summary>
    string DetermineStatus(decimal averageSalary);
}
