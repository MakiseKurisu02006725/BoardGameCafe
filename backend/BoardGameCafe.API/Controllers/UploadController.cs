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
}
