namespace EmployeeStatus.Services;

public interface IJwtService
{
    string GenerateToken(string username);
}
