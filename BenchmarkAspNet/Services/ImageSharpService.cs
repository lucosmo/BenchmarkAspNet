using BenchmarkAspNet.Models;
using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Png;
using System;
using System.IO;
using System.Threading.Tasks;

namespace BenchmarkAspNet.Services
{
    public class ImageSharpService : IImageService
    {
        private readonly string _imageDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Images_after");

        public ImageSharpService()
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

            using (var image = await Image.LoadAsync(filePath))
            {
                image.Mutate(x => x.Grayscale());

                var newFilePath = Path.Combine(_imageDirectory, $"grayscale_{fileName}");
                await image.SaveAsync(newFilePath, new PngEncoder());

                using var ms = new MemoryStream();
                await image.SaveAsync(ms, new PngEncoder()); 
                return ms.ToArray();
            }
        }

        public async Task<byte[]> ResizeImageAsync(string fileName, int width, int height)
        {
            var filePath = Path.Combine(_imageDirectory, fileName);
            if (!File.Exists(filePath)) throw new FileNotFoundException();

            using (var image = await Image.LoadAsync(filePath))
            {
                image.Mutate(x => x.Resize(width, height));

                var newFilePath = Path.Combine(_imageDirectory, $"resized_{fileName}");
                await image.SaveAsync(newFilePath, new PngEncoder());

                using var ms = new MemoryStream();
                await image.SaveAsync(ms, new PngEncoder()); 
                return ms.ToArray();
            }
        }

        public async Task<byte[]> CropImageAsync(string fileName, int x, int y, int width, int height)
        {
            var filePath = Path.Combine(_imageDirectory, fileName);
            if (!File.Exists(filePath)) throw new FileNotFoundException();

            using (var image = await Image.LoadAsync(filePath))
            {
                var cropRectangle = new Rectangle(x, y, width, height);
                image.Mutate(x => x.Crop(cropRectangle));

                var newFilePath = Path.Combine(_imageDirectory, $"cropped_{fileName}");
                await image.SaveAsync(newFilePath, new PngEncoder());

                using var ms = new MemoryStream();
                await image.SaveAsync(ms, new PngEncoder());
                return ms.ToArray();
            }
        }

        public async Task<byte[]> MultiModificationsImageAsync(IFormFile file, int width, int height, int x, int y, int cropWidth, int cropHeight)
        {
            var modifiedDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Images_modified");
            if (!Directory.Exists(modifiedDirectory))
                Directory.CreateDirectory(modifiedDirectory);

            using var image = await Image.LoadAsync(file.OpenReadStream());

            image.Mutate(ctx => ctx.Grayscale());

            image.Mutate(ctx => ctx.Resize(width, height));

            var cropRectangle = new Rectangle(x, y, cropWidth, cropHeight);
            image.Mutate(ctx => ctx.Crop(cropRectangle));

            var modifiedFileName = $"modified_{file.FileName}";
            var modifiedFilePath = Path.Combine(modifiedDirectory, modifiedFileName);

            await image.SaveAsync(modifiedFilePath, new PngEncoder());

            using var ms = new MemoryStream();
            await image.SaveAsync(ms, new PngEncoder());
            return ms.ToArray();
        }

        public Task DeleteImageAsync(string fileName)
        {
            var filePath = Path.Combine(_imageDirectory, fileName);
            if (File.Exists(filePath))
                File.Delete(filePath);

            return Task.CompletedTask;
        }

        Task<string> IImageService.TestOpenCVAsync()
        {
            throw new NotImplementedException();
        }
    }
}