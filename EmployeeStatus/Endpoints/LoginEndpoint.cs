using EmployeeStatus.DTOs;
using EmployeeStatus.Services;
using FastEndpoints;

namespace EmployeeStatus.Endpoints;

public class LoginEndpoint : Endpoint<LoginRequest, LoginResponse>
{
    private readonly IJwtService _jwtService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<LoginEndpoint> _logger;

    public LoginEndpoint(
        IJwtService jwtService, 
        IConfiguration configuration,
        ILogger<LoginEndpoint> logger)
    {
        _jwtService = jwtService;
        _configuration = configuration;
        _logger = logger;
    }

    public override void Configure()
    {
        Post("/api/login");
        AllowAnonymous();
        Summary(s =>
        {
            s.ExampleRequest = new LoginRequest { Username = "admin", Password = "Admin@123" };
        });
    }

    public override async Task HandleAsync(LoginRequest req, CancellationToken ct)
    {
        var validUsername = _configuration["Credentials:Username"]!;
        var validPassword = _configuration["Credentials:Password"]!;

        if (string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
        {
            _logger.LogWarning("Login attempt with empty credentials from {IP}", HttpContext.Connection.RemoteIpAddress);
            ThrowError("Username and password are required");
        }

        if (req.Username != validUsername || req.Password != validPassword)
        {
            _logger.LogWarning("Failed login attempt for username: {Username} from {IP}", 
                req.Username, HttpContext.Connection.RemoteIpAddress);
            
            await Task.Delay(1000, ct);
            ThrowError("Invalid username or password", 401);
        }

        var token = _jwtService.GenerateToken(req.Username);
        var expirationMinutes = int.Parse(_configuration["Jwt:ExpirationMinutes"]!);

        _logger.LogInformation("Successful login for user: {Username} from {IP}", 
            req.Username, HttpContext.Connection.RemoteIpAddress);

        await SendAsync(new LoginResponse
        {
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes),
            TokenType = "Bearer"
        }, cancellation: ct);
    }
}
