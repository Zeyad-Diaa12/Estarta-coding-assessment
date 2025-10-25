using EmployeeStatus.DTOs;
using EmployeeStatus.Services;
using FastEndpoints;
using AppErrorResponse = EmployeeStatus.DTOs.ErrorResponse;

namespace EmployeeStatus.Endpoints;

public class GetEmpStatusEndpoint : Endpoint<GetEmpStatusRequest, GetEmpStatusResponse>
{
    private readonly IProcessStatusService _processStatusService;
    private readonly ICacheService _cacheService;
    private readonly ILogger<GetEmpStatusEndpoint> _logger;
    private readonly IConfiguration _configuration;

    public GetEmpStatusEndpoint(
        IProcessStatusService processStatusService,
        ICacheService cacheService,
        ILogger<GetEmpStatusEndpoint> logger,
        IConfiguration configuration)
    {
        _processStatusService = processStatusService;
        _cacheService = cacheService;
        _logger = logger;
        _configuration = configuration;
    }

    public override void Configure()
    {
        Post("/api/GetEmpStatus");
        Roles();
        Description(d => d
            .Accepts<GetEmpStatusRequest>("application/json")
            .Produces<GetEmpStatusResponse>(200, "application/json")
            .Produces<AppErrorResponse>(404, "application/json")
            .Produces<AppErrorResponse>(406, "application/json")
            .Produces<AppErrorResponse>(422, "application/json")
            .Produces<AppErrorResponse>(400, "application/json")
            .Produces<AppErrorResponse>(500, "application/json"));
    }

    public override async Task HandleAsync(GetEmpStatusRequest req, CancellationToken ct)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(req.NationalNumber))
        {
            _logger.LogWarning("Received request with empty National Number");
            ThrowError("National Number is required", 400);
        }

        if (req.NationalNumber.Length < 5)
        {
            _logger.LogWarning("Received request with invalid National Number format: {NationalNumber}", req.NationalNumber);
            ThrowError("Invalid National Number format", 400);
        }

        _logger.LogInformation("Processing GetEmpStatus request for National Number: {NationalNumber}", req.NationalNumber);

        // Check cache
        var cacheKey = $"empstatus:{req.NationalNumber}";
        var cachedResponse = await _cacheService.GetAsync<GetEmpStatusResponse>(cacheKey);
        
        if (cachedResponse != null)
        {
            _logger.LogInformation("Returning cached response for National Number: {NationalNumber}", req.NationalNumber);
            await SendOkAsync(cachedResponse, ct);
            return;
        }

        // Process request
        var (response, statusCode, error) = await _processStatusService.ProcessEmployeeStatusAsync(req.NationalNumber);

        if (response == null)
        {
            _logger.LogWarning("Failed to process status for {NationalNumber}: {Error}", req.NationalNumber, error);
            ThrowError(error ?? "Unknown error", statusCode);
        }

        // Cache the response
        var cacheTtl = TimeSpan.FromMinutes(_configuration.GetValue<int>("Cache:TtlMinutes", 10));
        await _cacheService.SetAsync(cacheKey, response, cacheTtl);

        _logger.LogInformation("Successfully processed and cached status for {NationalNumber}", req.NationalNumber);
        await SendOkAsync(response, ct);
    }
}
