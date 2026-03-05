using FinancePlanner.Application.DTOs;

namespace FinancePlanner.Application.Interfaces;

public interface IAuthService
{
    Task<AuthResponse?> RegisterAsync(RegisterRequest request);
    Task<AuthResponse?> LoginAsync(LoginRequest request);
    Task<UserDto?> GetCurrentUserAsync(Guid userId);
    Task<UserDto?> SyncEntraUserAsync(string objectId, string? email, string? firstName, string? lastName);
}
