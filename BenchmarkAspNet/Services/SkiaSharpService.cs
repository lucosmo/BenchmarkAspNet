using BenchmarkAspNet.Models;
using Microsoft.AspNetCore.Http;
using SkiaSharp;
using System;
using System.IO;
using System.Threading.Tasks;

namespace BenchmarkAspNet.Services
{
    public class SkiaSharpService : IImageService
    {
        private readonly string _imageDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Images_after");

        public SkiaSharpService()
        {
            if (!Directory.Exists(_imageDirectory))
                Directory.CreateDirectory(_imageDirectory);
        }

        public async Task<ImageResponse> SaveImageAsync(IFormFile file)
        {
            var filePath = Path.Combine(_imageDirectory, file.FileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return new ImageResponse
            {
                FileName = file.FileName,
                FilePath = filePath,
                UploadDate = DateTime.Now
            };
        }

        public async Task<byte[]> GetImageAsync(string fileName)
        {
            var filePath = Path.Combine(_imageDirectory, fileName);
            if (!File.Exists(filePath)) throw new FileNotFoundException();

            return await File.ReadAllBytesAsync(filePath);
        }

        public async Task<byte[]> ApplyGrayscaleAsync(string fileName)
        {
             var filePath = Path.Combine(_imageDirectory, fileName);
            if (!File.Exists(filePath)) throw new FileNotFoundException();
            
            using var input = File.OpenRead(filePath);
            using var bitmap = SKBitmap.Decode(input);
            if (bitmap == null)
                throw new Exception("Can't load the image");
            
            using var grayscaleBitmap = new SKBitmap(bitmap.Width, bitmap.Height);
            
            using var canvas = new SKCanvas(grayscaleBitmap);
            
            using var paint = new SKPaint
            {
                ColorFilter = SKColorFilter.CreateColorMatrix(new float[]
                {
                    0.21f, 0.72f, 0.07f, 0, 0,
                    0.21f, 0.72f, 0.07f, 0, 0,
                    0.21f, 0.72f, 0.07f, 0, 0,
                    0,     0,     0,     1, 0
                })
            };
            
            canvas.DrawBitmap(bitmap, 0, 0, paint);
            
            using var ms = new MemoryStream();
            using var image = SKImage.FromBitmap(grayscaleBitmap);
            var data = image.Encode(SKEncodedImageFormat.Png, 100);
            data.SaveTo(ms);
            ms.Position = 0;
            
            return ms.ToArray();
        }

        public async Task<byte[]> ResizeImageAsync(string fileName, int width, int height)
        {
            var filePath = Path.Combine(_imageDirectory, fileName);
            if (!File.Exists(filePath)) throw new FileNotFoundException();

            using var input = File.OpenRead(filePath);
            using var bitmap = SKBitmap.Decode(input);

            var resizedBitmap = bitmap.Resize(new SKImageInfo(width, height), SKFilterQuality.High);

            using var ms = new MemoryStream();
            using var image = SKImage.FromBitmap(resizedBitmap);
            image.Encode(SKEncodedImageFormat.Png, 100).SaveTo(ms);

            return ms.ToArray();
        }

        public async Task<byte[]> CropImageAsync(string fileName, int x, int y, int width, int height)
        {
            var filePath = Path.Combine(_imageDirectory, fileName);
            if (!File.Exists(filePath)) throw new FileNotFoundException();

            using var input = File.OpenRead(filePath);
            using var bitmap = SKBitmap.Decode(input);

            var croppedBitmap = new SKBitmap(width, height);
            using var canvas = new SKCanvas(croppedBitmap);
            var sourceRect = new SKRectI(x, y, x + width, y + height);
            var destRect = new SKRectI(0, 0, width, height);
            canvas.DrawBitmap(bitmap, sourceRect, destRect);

            using var ms = new MemoryStream();
            using var image = SKImage.FromBitmap(croppedBitmap);
            image.Encode(SKEncodedImageFormat.Png, 100).SaveTo(ms);

            return ms.ToArray();
        }

        public async Task<byte[]> MultiModificationsImageAsync(IFormFile file, int width, int height, int x, int y, int cropWidth, int cropHeight)
        {
            var modifiedDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Images_modified");
            if (!Directory.Exists(modifiedDirectory))
                Directory.CreateDirectory(modifiedDirectory);

            using var input = file.OpenReadStream();
            using var bitmap = SKBitmap.Decode(input);

            if (bitmap == null)
                throw new Exception("Can't load an image.");

            var resizedBitmap = bitmap.Resize(new SKImageInfo(width, height), SKFilterQuality.High);

            if (resizedBitmap == null)
                throw new Exception("Can't change size of the image.");

            var croppedBitmap = new SKBitmap(cropWidth, cropHeight);
            using var canvas = new SKCanvas(croppedBitmap);

            var paint = new SKPaint
            {
                ColorFilter = SKColorFilter.CreateColorMatrix(new float[]
                {
                    0.21f, 0.72f, 0.07f, 0, 0,
                    0.21f, 0.72f, 0.07f, 0, 0,
                    0.21f, 0.72f, 0.07f, 0, 0,
                    0,     0,     0,     1, 0
                })
            };

            var sourceRect = new SKRectI(x, y, x + cropWidth, y + cropHeight);
            var destRect = new SKRectI(0, 0, cropWidth, cropHeight);
            canvas.DrawBitmap(resizedBitmap, sourceRect, destRect, paint);

            using var ms = new MemoryStream();
            using var image = SKImage.FromBitmap(croppedBitmap);
            image.Encode(SKEncodedImageFormat.Png, 100).SaveTo(ms);

            return ms.ToArray();
        }


        public Task DeleteImageAsync(string fileName)
        {
            var filePath = Path.Combine(_imageDirectory, fileName);
            if (File.Exists(filePath))
                File.Delete(filePath);

            return Task.CompletedTask;
        }

        public Task<string> TestOpenCVAsync()
        {
            throw new NotImplementedException();
        }
    }
}
