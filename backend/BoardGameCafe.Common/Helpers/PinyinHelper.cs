using System.Text;

namespace BoardGameCafe.Common.Helpers;

/// <summary>
/// 拼音首字母助手：用于生成桌游副本编号前缀（如 璀璨宝石 → CG）
/// </summary>
public static class PinyinHelper
{
    /// <summary>
    /// GB2312 一级字库（区16-55）未收录、但常见于桌游名称等场景的汉字回退映射
    /// （一级字库按拼音边界表可查，二级字库区号56-87的字无法直接映射，此处手工补齐常用字）
    /// </summary>
    private static readonly Dictionary<char, char> FallbackLetters = new()
    {
        { '璀', 'C' }, { '璨', 'C' }, // 璀璨宝石
        { '氤', 'Y' }, { '氲', 'Y' }, // 氤氲
        { '旖', 'Y' }, { '旎', 'N' }, // 旖旎
        { '焱', 'Y' }, { '燚', 'Y' },
        { '赟', 'Y' }, { '囍', 'X' },
        { '琦', 'Q' }, { '玮', 'W' }, { '钰', 'Y' }, { '姝', 'S' },
        { '枭', 'X' }, { '濑', 'L' }, { '珩', 'H' },
        { '璇', 'X' }, { '珺', 'J' }, { '珞', 'L' }, { '珉', 'M' }, { '琨', 'K' }, { '璟', 'J' },
        { '湫', 'J' }, { '茜', 'Q' }, { '芃', 'P' }, { '茗', 'M' }, { '荃', 'Q' }, { '芸', 'Y' },
        { '鼋', 'Y' }, { '鼍', 'T' }, { '骠', 'B' }, { '犇', 'B' }, { '骉', 'B' }, { '猋', 'B' },
        { '菡', 'H' }, { '萏', 'D' }, { '蒹', 'J' }, { '葭', 'J' }, { '蓁', 'Z' }, { '蓥', 'Y' },
        { '蘅', 'H' }, { '芷', 'Z' }, { '芮', 'R' }, { '萱', 'X' }, { '蕙', 'H' }, { '纨', 'W' },
        { '缱', 'Q' }, { '绻', 'Q' }, { '缵', 'Z' }, { '颢', 'H' }, { '颀', 'Q' }, { '颋', 'T' }
    };

    static PinyinHelper()
    {
        // .NET Core 需要注册代码页编码提供程序才能使用 GB2312
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    /// <summary>
    /// 获取字符串的拼音首字母缩写：中文取拼音首字母，英文/数字原样保留（转大写）
    /// </summary>
    /// <param name="text">源文本（如桌游名称）</param>
    /// <returns>首字母缩写（如 璀璨宝石 → CG、UNO → UNO）</returns>
    public static string GetInitials(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return "GM"; // 通用前缀
        }

        var sb = new StringBuilder();
        foreach (var ch in text.Trim())
        {
            if ((ch >= 'a' && ch <= 'z') || (ch >= 'A' && ch <= 'Z') || (ch >= '0' && ch <= '9'))
            {
                sb.Append(char.ToUpperInvariant(ch));
            }
            else if (ch >= 0x4E00 && ch <= 0x9FA5)
            {
                sb.Append(GetFirstLetter(ch));
            }
        }

        var result = sb.ToString();
        return result.Length > 0 ? result : "GM";
    }

    /// <summary>
    /// 获取单个汉字的拼音首字母（大写）
    /// </summary>
    /// <param name="ch">汉字</param>
    /// <returns>拼音首字母，无法识别时返回 'Z'</returns>
    private static char GetFirstLetter(char ch)
    {
        // 优先使用手工回退映射（覆盖一级字库未收录的常用汉字）
        if (FallbackLetters.TryGetValue(ch, out var fallback))
        {
            return fallback;
        }

        var bytes = Encoding.GetEncoding("GB2312").GetBytes(ch.ToString());
        if (bytes.Length != 2)
        {
            return 'Z';
        }

        // 区位码 = (区号) * 100 + 位号
        var area = bytes[0] - 160;
        var position = bytes[1] - 160;
        var code = area * 100 + position;

        // 拼音首字母对应的区位码上界（GB2312）
        var boundaries = new[]
        {
            1601, 1637, 1833, 2078, 2274, 2302, 2433, 2594, 2787,
            3106, 3212, 3472, 3635, 3722, 3730, 3858, 4027, 4086,
            4390, 4558, 4684, 4925, 5249, 5590
        };
        var letters = new[]
        {
            'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'J',
            'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S',
            'T', 'W', 'X', 'Y', 'Z'
        };

        var index = 0;
        while (index < boundaries.Length && code >= boundaries[index])
        {
            index++;
        }

        if (index == 0 || index > letters.Length)
        {
            return 'Z';
        }

        return letters[index - 1];
    }
}
