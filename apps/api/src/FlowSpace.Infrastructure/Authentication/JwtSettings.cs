using System.ComponentModel.DataAnnotations;

namespace FlowSpace.Infrastructure.Authentication;

public class JwtSettings
{
    public const string SectionName = "JwtSettings";
    
    [Required(ErrorMessage = "JWT Secret is required.")]
    [MinLength(32, ErrorMessage = "JWT Secret must be at least 32 characters long.")]
    public string Secret { get; init; } = null!;
    
    [Range(1, int.MaxValue)]
    public int ExpiryMinutes { get; init; }
    
    [Required]
    public string Issuer { get; init; } = null!;
    
    [Required]
    public string Audience { get; init; } = null!;
}
