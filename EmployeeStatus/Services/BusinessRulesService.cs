using EmployeeStatus.Configuration;
using EmployeeStatus.Models;

namespace EmployeeStatus.Services;

public class BusinessRulesService : IBusinessRulesService
{
    private readonly ILogger<BusinessRulesService> _logger;

    public BusinessRulesService(ILogger<BusinessRulesService> logger)
    {
        _logger = logger;
    }

    public List<decimal> ApplySalaryAdjustments(List<Salary> salaries)
    {
        var adjusted = new List<decimal>();

        foreach (var salary in salaries)
        {
            var amount = salary.SalaryAmount;
            var originalAmount = amount;

            // Business Rule 1: December bonus (+10%)
            if (salary.Month == BusinessRulesConfiguration.SalaryAdjustments.DecemberMonth)
            {
                amount *= BusinessRulesConfiguration.SalaryAdjustments.DecemberBonusMultiplier;
                _logger.LogDebug("December bonus applied to salary {SalaryId}: {Original} -> {Adjusted} (+10%)",
                    salary.Id, originalAmount, amount);
            }

            // Business Rule 2: Summer deduction (-5%)
            if (salary.Month >= BusinessRulesConfiguration.SalaryAdjustments.SummerStartMonth && 
                salary.Month <= BusinessRulesConfiguration.SalaryAdjustments.SummerEndMonth)
            {
                amount *= BusinessRulesConfiguration.SalaryAdjustments.SummerDeductionMultiplier;
                _logger.LogDebug("Summer deduction applied to salary {SalaryId}: {Original} -> {Adjusted} (-5%)",
                    salary.Id, originalAmount, amount);
            }

            adjusted.Add(amount);
        }

        _logger.LogInformation("Salary adjustments completed: {Count} salaries processed", salaries.Count);
        return adjusted;
    }

    public List<decimal> ApplyTaxAndDeductions(List<decimal> salaries)
    {
        var taxThreshold = BusinessRulesConfiguration.Tax.Threshold;
        var taxRate = BusinessRulesConfiguration.Tax.Rate;

        var total = salaries.Sum();

        // Business Rule 3: Tax deduction when total exceeds threshold
        if (total > taxThreshold)
        {
            var taxAmount = total * taxRate;
            var afterTaxTotal = total * (1 - taxRate);

            _logger.LogInformation(
                "Tax applied: Total={Total}, Threshold={Threshold}, Rate={Rate:P}, TaxAmount={TaxAmount}, AfterTax={AfterTax}",
                total, taxThreshold, taxRate, taxAmount, afterTaxTotal);

            return salaries.Select(s => s * (1 - taxRate)).ToList();
        }

        _logger.LogDebug("No tax applied: Total={Total} is below threshold={Threshold}", total, taxThreshold);
        return salaries;
    }

    public string DetermineStatus(decimal averageSalary)
    {
        var greenThreshold = BusinessRulesConfiguration.Status.GreenThreshold;
        var orangeThreshold = BusinessRulesConfiguration.Status.OrangeThreshold;

        string status;

        // Business Rule 4: Status determination
        if (averageSalary > greenThreshold)
        {
            status = "GREEN";
        }
        else if (averageSalary == orangeThreshold)
        {
            status = "ORANGE";
        }
        else
        {
            status = "RED";
        }

        _logger.LogDebug(
            "Status determined: Average={Average}, GreenThreshold={GreenThreshold}, OrangeThreshold={OrangeThreshold}, Status={Status}",
            averageSalary, greenThreshold, orangeThreshold, status);

        return status;
    }
}
