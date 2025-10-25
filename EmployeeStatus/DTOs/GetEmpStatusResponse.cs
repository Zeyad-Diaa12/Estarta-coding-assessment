namespace EmployeeStatus.DTOs;

public class GetEmpStatusResponse
{
    public string EmployeeName { get; set; } = string.Empty;
    public string NationalNumber { get; set; } = string.Empty;
    public decimal AverageSalary { get; set; }
    public decimal HighestSalary { get; set; }
    public decimal SumSalary { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime LastUpdated { get; set; }
}
