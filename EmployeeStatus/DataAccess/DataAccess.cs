using EmployeeStatus.Models;
using EmployeeStatus.Policies;
using Microsoft.EntityFrameworkCore;
using Polly;

namespace EmployeeStatus.DataAccess;

public class DataAccess : IDataAccess
{
    private readonly AppDbContext _context;
    private readonly ILogger<DataAccess> _logger;

    public DataAccess(AppDbContext context, ILogger<DataAccess> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<User?> GetUserByNationalNumberAsync(string nationalNumber)
    {
        var context = new Context
        {
            ["Logger"] = _logger
        };

        return await ResiliencePolicies.DatabaseRetryPolicy.ExecuteAsync(async (ctx) =>
        {
            return await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.NationalNumber == nationalNumber);
        }, context);
    }

    public async Task<List<Salary>> GetSalariesByUserIdAsync(int userId)
    {
        var context = new Context
        {
            ["Logger"] = _logger
        };

        return await ResiliencePolicies.DatabaseRetryPolicy.ExecuteAsync(async (ctx) =>
        {
            return await _context.Salaries
                .AsNoTracking()
                .Where(s => s.UserId == userId)
                .OrderByDescending(s => s.Year)
                .ThenByDescending(s => s.Month)
                .ToListAsync();
        }, context);
    }

    public async Task<List<LogEntry>> GetLogsAsync(int limit = 100)
    {
        var context = new Context
        {
            ["Logger"] = _logger
        };

        return await ResiliencePolicies.DatabaseRetryPolicy.ExecuteAsync(async (ctx) =>
        {
            var sql = @"
                SELECT 
                    ""timestamp"" as Timestamp,
                    ""level"" as Level,
                    ""message"" as Message,
                    ""exception"" as Exception,
                    ""log_event""::text as Properties
                FROM ""logs""
                ORDER BY ""timestamp"" DESC
                LIMIT {0}";

            var logs = await _context.Database
                .SqlQueryRaw<LogEntry>(sql, limit)
                .ToListAsync();

            return logs;
        }, context);
    }
}
