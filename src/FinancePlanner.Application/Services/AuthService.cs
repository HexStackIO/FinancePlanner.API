using FinancePlanner.Application.DTOs;
using FinancePlanner.Application.Interfaces;
using FinancePlanner.Core.Entities;
using FinancePlanner.Core.Interfaces;

namespace FinancePlanner.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;

    public AuthService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    /// <summary>
    /// No longer used — login is handled by Entra External ID via MSAL.
    /// Kept to satisfy interface until legacy endpoints are fully removed.
    /// </summary>
    public Task<AuthResponse?> LoginAsync(LoginRequest request)
        => Task.FromResult<AuthResponse?>(null);

    /// <summary>
    /// No longer used — registration is handled by Entra External ID via MSAL.
    /// Kept to satisfy interface until legacy endpoints are fully removed.
    /// </summary>
    public Task<AuthResponse?> RegisterAsync(RegisterRequest request)
        => Task.FromResult<AuthResponse?>(null);

    public async Task<UserDto?> GetCurrentUserAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        return user == null ? null : MapToUserDto(user);
    }

    public async Task<UserDto?> SyncEntraUserAsync(
        string objectId, string? email, string? firstName, string? lastName)
    {
        // Check if user already exists by Entra object ID
        var user = await _userRepository.GetByEntraIdAsync(objectId);

        if (user == null && !string.IsNullOrEmpty(email))
        {
            // Check if they registered with email/password before — link the accounts
            user = await _userRepository.GetByEmailAsync(email);
        }

        if (user == null)
        {
            // First time this Entra user has logged in — create their record
            user = new User
            {
                UserId = Guid.Parse(objectId),
                EntraObjectId = objectId,
                Email = email ?? string.Empty,
                FirstName = firstName ?? string.Empty,
                LastName = lastName ?? string.Empty,
                PasswordHash = string.Empty,
                CreatedAt = DateTimeOffset.UtcNow,
                LastLoginAt = DateTimeOffset.UtcNow
            };
            await _userRepository.CreateAsync(user);
        }
        else
        {
            // Existing user — update their Entra ID if not already set and refresh last login
            if (string.IsNullOrEmpty(user.EntraObjectId))
                user.EntraObjectId = objectId;

            user.LastLoginAt = DateTimeOffset.UtcNow;
            await _userRepository.UpdateAsync(user);
        }

        return MapToUserDto(user);
    }

    private static UserDto MapToUserDto(User user) => new()
    {
        UserId = user.UserId,
        Email = user.Email,
        FirstName = user.FirstName,
        LastName = user.LastName,
        CreatedAt = user.CreatedAt
    };
}
