using System;

namespace BenchmarkAspNet.Models
{
    public class ImageResponse
    {
        public required string FileName { get; set; }
        public required string FilePath { get; set; }
        public DateTime UploadDate { get; set; }
    }
}