using EmployeeStatus.DataAccess;
using FastEndpoints;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace EmployeeStatus.Endpoints;

public class HealthEndpoint : EndpointWithoutRequest
{
    private readonly HealthCheckService _healthCheckService;

    public HealthEndpoint(HealthCheckService healthCheckService)
    {
        _healthCheckService = healthCheckService;
    }

    public override void Configure()
    {
        Get("/health");
        AllowAnonymous();
        Description(d => d
            .Produces(200)
            .Produces(503));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var healthReport = await _healthCheckService.CheckHealthAsync(ct);

        var result = new
        {
            status = healthReport.Status.ToString(),
            checks = healthReport.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                duration = e.Value.Duration.TotalMilliseconds
            }),
            totalDuration = healthReport.TotalDuration.TotalMilliseconds
        };

        if (healthReport.Status == HealthStatus.Healthy)
        {
            await SendOkAsync(result, ct);
        }
        else
        {
            await SendAsync(result, 503, ct);
        }
    }
}
