using BenchmarkAspNet.Services;
using Microsoft.AspNetCore.Mvc;

namespace BenchmarkAspNet.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ImageController : ControllerBase
{
    private readonly IImageService _imageService;
    private readonly ILogger<ImageController> _logger;

    public ImageController(IImageService imageService, ILogger<ImageController> logger)
    {
        _imageService = imageService;
        _logger = logger;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadImage(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");

        var result = await _imageService.SaveImageAsync(file);
        return Ok(result);
    }

    [HttpGet("{fileName}")]
    public async Task<IActionResult> GetImage(string fileName)
    {
        var image = await _imageService.GetImageAsync(fileName);
        return File(image, "image/png");
    }

    [HttpGet("grayscale/{fileName}")]
    public async Task<IActionResult> GetGrayscaleImage(string fileName)
    {
        var image = await _imageService.ApplyGrayscaleAsync(fileName);
        return File(image, "image/png");
    }

    [HttpGet("resize/{fileName}")]
    public async Task<IActionResult> ResizeImage(string fileName, int width, int height)
    {
        var image = await _imageService.ResizeImageAsync(fileName, width, height);
        return File(image, "image/png");
    }

    [HttpGet("crop/{fileName}")]
    public async Task<IActionResult> CropImage(string fileName, int x, int y, int width, int height)
    {
        var image = await _imageService.CropImageAsync(fileName, x, y, width, height);
        return File(image, "image/png");
    }

    [HttpPost("MultiModificationsImage")]
    public async Task<IActionResult> MultiModificationsImage(
        IFormFile file, 
        int width = 512, 
        int height = 512, 
        int x = 100, 
        int y = 100, 
        int cropWidth = 300, 
        int cropHeight = 300)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded.");

        try
        {
            var result = await _imageService.MultiModificationsImageAsync(file, width, height, x, y, cropWidth, cropHeight);
            return File(result, "image/png");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
    
    [HttpGet("test")]
    public async Task<IActionResult> Test()
    {
        var result = await _imageService.TestOpenCVAsync();
        return Ok(result);
    }
    [HttpDelete("{fileName}")]
    public async Task<IActionResult> DeleteImage(string fileName)
    {
        await _imageService.DeleteImageAsync(fileName);
        return NoContent();
    }
}