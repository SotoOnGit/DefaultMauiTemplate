using System.ComponentModel.DataAnnotations;

namespace Common.Models.Account;

public class RefreshTokenRequest
{
    [Required]
    [MaxLength(64)]
    public string RefreshToken { get; set; }
}