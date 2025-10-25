using EmployeeStatus.Models;

namespace EmployeeStatus.DataAccess;

public interface IDataAccess
{
    Task<User?> GetUserByNationalNumberAsync(string nationalNumber);
    Task<List<Salary>> GetSalariesByUserIdAsync(int userId);
    Task<List<LogEntry>> GetLogsAsync(int limit = 100);
}
