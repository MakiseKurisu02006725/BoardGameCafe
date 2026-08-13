namespace BoardGameCafe.Common.Helpers;

/// <summary>
/// JWT配置类
/// </summary>
public class JwtSettings
{
    /// <summary>
    /// 配置节名称
    /// </summary>
    public const string SectionName = "JwtSettings";

    /// <summary>
    /// 密钥（用于签名Token）
    /// </summary>
    public string SecretKey { get; set; } = string.Empty;

    /// <summary>
    /// 发布者
    /// </summary>
    public string Issuer { get; set; } = string.Empty;

    /// <summary>
    /// 接收者
    /// </summary>
    public string Audience { get; set; } = string.Empty;

    /// <summary>
    /// AccessToken过期时间（分钟，默认为30分钟）
    /// </summary>
    public int AccessTokenExpirationMinutes { get; set; } = 30;

    /// <summary>
    /// RefreshToken过期时间（天，默认为7天）
    /// </summary>
    public int RefreshTokenExpirationDays { get; set; } = 7;

    /// <summary>
    /// 是否验证发布者
    /// </summary>
    public bool ValidateIssuer { get; set; } = true;

    /// <summary>
    /// 是否验证接收者
    /// </summary>
    public bool ValidateAudience { get; set; } = true;

    /// <summary>
    /// 是否验证密钥
    /// </summary>
    public bool ValidateIssuerSigningKey { get; set; } = true;

    /// <summary>
    /// 是否验证过期时间
    /// </summary>
    public bool ValidateLifetime { get; set; } = true;

    /// <summary>
    /// 验证配置是否有效
    /// </summary>
    /// <returns>如果配置有效返回true，否则返回false</returns>
    public bool IsValid()
    {
        return !string.IsNullOrWhiteSpace(SecretKey) &&
               SecretKey.Length >= 16 &&
               !string.IsNullOrWhiteSpace(Issuer) &&
               !string.IsNullOrWhiteSpace(Audience) &&
               AccessTokenExpirationMinutes > 0 &&
               RefreshTokenExpirationDays > 0;
    }
}
