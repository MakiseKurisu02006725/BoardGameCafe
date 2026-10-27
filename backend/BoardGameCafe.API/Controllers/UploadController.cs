using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BoardGameCafe.Common;

namespace BoardGameCafe.API.Controllers;

/// <summary>
/// 文件上传控制器
/// </summary>
[ApiController]
[Route("api/upload")]
[Produces("application/json")]
public class UploadController : ControllerBase
{
    private const long MaxFileSize = 1 * 1024 * 1024;
    private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };
    private readonly IWebHostEnvironment _environment;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="environment">Web宿主环境</param>
    public UploadController(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    /// <summary>
    /// 上传桌游封面图
    /// </summary>
    /// <param name="file">图片文件</param>
    /// <returns>图片URL路径</returns>
    [HttpPost("image")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<string>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<string>>> UploadImage(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest(ApiResponse.Fail("请选择要上传的文件", 400));
        }

        if (file.Length > MaxFileSize)
        {
            return BadRequest(ApiResponse.Fail("文件大小不能超过1MB", 400));
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(extension) || !AllowedExtensions.Contains(extension))
        {
            return BadRequest(ApiResponse.Fail("仅支持jpg/jpeg/png/gif格式的图片", 400));
        }

        // 安全校验：检查文件真实内容（magic bytes），防止伪装图片上传
        using (var headerStream = new MemoryStream())
        {
            file.OpenReadStream().CopyTo(headerStream);
            var header = headerStream.ToArray();
            if (!IsValidImageHeader(header, extension))
            {
                return BadRequest(ApiResponse.Fail("文件内容与图片格式不符", 400));
            }
        }

        var uploadsDir = Path.Combine(_environment.WebRootPath ?? Directory.GetCurrentDirectory(), "uploads");
        if (!Directory.Exists(uploadsDir))
        {
            Directory.CreateDirectory(uploadsDir);
        }

        var fileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(uploadsDir, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var fileUrl = $"/uploads/{fileName}";
        return Ok(ApiResponse<string>.Success(fileUrl, "上传成功"));
    }

    /// <summary>
    /// 校验文件头字节与声明扩展名是否一致
    /// </summary>
    private static bool IsValidImageHeader(byte[] header, string extension)
    {
        if (header.Length < 8)
        {
            return false;
        }

        // JPEG: FF D8 FF；PNG: 89 50 4E 47 0D 0A 1A 0A；GIF: 47 49 46 38 ('GIF8')
        return extension switch
        {
            ".jpg" or ".jpeg" => header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF,
            ".png" => header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47,
            ".gif" => header[0] == 0x47 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x38,
            _ => false
        };
    }
}
