using BenchmarkAspNet.Models;
using ImageMagick;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;

namespace BenchmarkAspNet.Services
{
    public class MagicNetService : IImageService
    {
        private readonly string _imageDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Images_after");

        public MagicNetService()
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

        public Task<byte[]> ApplyGrayscaleAsync(string fileName)
        {
            var filePath = Path.Combine(_imageDirectory, fileName);
            if (!File.Exists(filePath)) throw new FileNotFoundException();

            using var image = new MagickImage(filePath);
            image.ColorType = ColorType.Grayscale;

            return Task.FromResult(image.ToByteArray());
        }

        public Task<byte[]> ResizeImageAsync(string fileName, int width, int height)
        {
            var filePath = Path.Combine(_imageDirectory, fileName);
            if (!File.Exists(filePath)) throw new FileNotFoundException();

            using var image = new MagickImage(filePath);

            uint uWidth = (uint)Math.Max(width, 1);
            uint uHeight = (uint)Math.Max(height, 1);

            image.Resize(uWidth, uHeight);

            return Task.FromResult(image.ToByteArray());
        }

        public Task<byte[]> CropImageAsync(string fileName, int x, int y, int width, int height)
        {
            var filePath = Path.Combine(_imageDirectory, fileName);
            if (!File.Exists(filePath)) throw new FileNotFoundException();

            using var image = new MagickImage(filePath);

            uint uWidth = (uint)Math.Max(width, 1);
            uint uHeight = (uint)Math.Max(height, 1);

            image.Crop(new MagickGeometry(x, y, uWidth, uHeight));

            return Task.FromResult(image.ToByteArray());
        }

        public async Task<byte[]> MultiModificationsImageAsync(IFormFile file, int width, int height, int x, int y, int cropWidth, int cropHeight)
        {
            var modifiedDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Images_modified");
            if (!Directory.Exists(modifiedDirectory))
                Directory.CreateDirectory(modifiedDirectory);

            using var input = file.OpenReadStream();
            using var memoryStream = new MemoryStream();
            await input.CopyToAsync(memoryStream);

            using var image = new MagickImage(memoryStream.ToArray());

            image.ColorType = ColorType.Grayscale;

            uint uWidth = (uint)Math.Max(width, 1);
            uint uHeight = (uint)Math.Max(height, 1);
            image.Resize(uWidth, uHeight);

            uint uCropWidth = (uint)Math.Max(cropWidth, 1);
            uint uCropHeight = (uint)Math.Max(cropHeight, 1);

            var cropGeometry = new MagickGeometry(x, y, uCropWidth, uCropHeight);
            image.Crop(cropGeometry);

            return image.ToByteArray();
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
