namespace BoardGameCafe.Common.Helpers;

/// <summary>
/// BCrypt密码哈希帮助类
/// </summary>
public static class PasswordHelper
{
    /// <summary>
    /// 默认工作因子（影响哈希计算强度，数值越大越慢但越安全）
    /// </summary>
    private const int DefaultWorkFactor = 12;

    /// <summary>
    /// 使用BCrypt算法对密码进行哈希处理
    /// </summary>
    /// <param name="password">原始密码</param>
    /// <param name="workFactor">工作因子（默认为12）</param>
    /// <returns>哈希后的密码字符串</returns>
    /// <exception cref="ArgumentNullException">当password为null或空时抛出</exception>
    public static string HashPassword(string password, int workFactor = DefaultWorkFactor)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentNullException(nameof(password), "密码不能为空");
        }

        if (workFactor < 4 || workFactor > 31)
        {
            workFactor = DefaultWorkFactor;
        }

        return BCrypt.Net.BCrypt.HashPassword(password, workFactor);
    }

    /// <summary>
    /// 验证密码是否与哈希值匹配
    /// </summary>
    /// <param name="password">原始密码</param>
    /// <param name="hashedPassword">哈希后的密码</param>
    /// <returns>如果匹配返回true，否则返回false</returns>
    /// <exception cref="ArgumentNullException">当password或hashedPassword为null或空时抛出</exception>
    public static bool VerifyPassword(string password, string hashedPassword)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentNullException(nameof(password), "密码不能为空");
        }

        if (string.IsNullOrWhiteSpace(hashedPassword))
        {
            throw new ArgumentNullException(nameof(hashedPassword), "哈希密码不能为空");
        }

        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// 验证密码强度是否符合要求
    /// </summary>
    /// <param name="password">待验证的密码</param>
    /// <param name="minLength">最小长度（默认为6）</param>
    /// <returns>如果密码强度符合要求返回true，否则返回false</returns>
    public static bool ValidatePasswordStrength(string password, int minLength = 6)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            return false;
        }

        if (password.Length < minLength)
        {
            return false;
        }

        return true;
    }
}
