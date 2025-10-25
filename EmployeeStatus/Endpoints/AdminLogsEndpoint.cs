using EmployeeStatus.DataAccess;
using EmployeeStatus.Models;
using FastEndpoints;

namespace EmployeeStatus.Endpoints;

public class AdminLogsEndpoint : EndpointWithoutRequest<List<LogEntry>>
{
    private readonly IDataAccess _dataAccess;
    private readonly ILogger<AdminLogsEndpoint> _logger;

    public AdminLogsEndpoint(IDataAccess dataAccess, ILogger<AdminLogsEndpoint> logger)
    {
        _dataAccess = dataAccess;
        _logger = logger;
    }

    public override void Configure()
    {
        Get("/api/admin/logs");
        Roles();
        Description(d => d
            .Produces<List<LogEntry>>(200)
            .Produces(500));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        try
        {
            var limitParam = Query<int?>("limit", isRequired: false);
            var limit = limitParam ?? 100;
            var logs = await _dataAccess.GetLogsAsync(limit);
            await SendOkAsync(logs, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving logs");
            ThrowError("Failed to retrieve logs", 500);
        }
    }
}
