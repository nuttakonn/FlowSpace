namespace FlowSpace.Contracts.Authentication;

public record RegisterRequest(string Email, string Password, string DisplayName, string? InviteCode = null);

public record LoginRequest(string Email, string Password);

public record RefreshRequest(string RefreshToken);

public record AuthenticationResponse(
    Guid Id,
    string Email,
    string DisplayName,
    string AccessToken,
    string RefreshToken);
