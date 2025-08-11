using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.PixelFormats;

namespace ToiletGhost.Services
{
    public interface IImageProcessingService
    {
        Task<(byte[] originalBytes, byte[] thumbnailBytes)> ProcessImageAsync(Stream imageStream, string contentType);
        string GetThumbnailBlobName(string originalBlobName);
    }

    public class ImageProcessingService : IImageProcessingService
    {
        private const int ThumbnailSize = 200;

        public async Task<(byte[] originalBytes, byte[] thumbnailBytes)> ProcessImageAsync(Stream imageStream, string contentType)
        {
            try
            {
                Console.WriteLine($"ImageProcessingService: Starting image processing for content type: {contentType}");
                
                // Read the entire stream into memory first - DON'T set position on BrowserFileStream
                byte[] originalBytes;
                using (var memoryStream = new MemoryStream())
                {
                    await imageStream.CopyToAsync(memoryStream);
                    originalBytes = memoryStream.ToArray();
                    Console.WriteLine($"ImageProcessingService: Original image size: {originalBytes.Length} bytes");
                }

                // Create thumbnail from the original bytes
                byte[] thumbnailBytes;
                using (var originalMemoryStream = new MemoryStream(originalBytes))
                {
                    Console.WriteLine("ImageProcessingService: Loading image...");
                    var image = await Image.LoadAsync(originalMemoryStream);
                    Console.WriteLine($"ImageProcessingService: Image loaded, dimensions: {image.Width}x{image.Height}");
                    
                    // Calculate resize dimensions to maintain aspect ratio
                    var (width, height) = CalculateThumbnailDimensions(image.Width, image.Height, ThumbnailSize);
                    Console.WriteLine($"ImageProcessingService: Thumbnail dimensions: {width}x{height}");
                    
                    // Resize the image
                    Console.WriteLine("ImageProcessingService: Resizing image...");
                    image.Mutate(x => x.Resize(width, height));

                    // Convert to bytes
                    Console.WriteLine("ImageProcessingService: Saving thumbnail...");
                    using var thumbnailMemoryStream = new MemoryStream();
                    await image.SaveAsJpegAsync(thumbnailMemoryStream, new JpegEncoder { Quality = 85 });
                    thumbnailBytes = thumbnailMemoryStream.ToArray();
                    Console.WriteLine($"ImageProcessingService: Thumbnail saved, size: {thumbnailBytes.Length} bytes");
                    
                    image.Dispose();
                }

                Console.WriteLine("ImageProcessingService: Processing completed successfully");
                return (originalBytes, thumbnailBytes);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ImageProcessingService ERROR: {ex.Message}");
                Console.WriteLine($"ImageProcessingService STACK TRACE: {ex.StackTrace}");
                throw;
            }
        }

        public string GetThumbnailBlobName(string originalBlobName)
        {
            var extension = Path.GetExtension(originalBlobName);
            var nameWithoutExtension = Path.GetFileNameWithoutExtension(originalBlobName);
            return $"{nameWithoutExtension}_thumb.jpg"; // Always use .jpg for thumbnails
        }

        private static (int width, int height) CalculateThumbnailDimensions(int originalWidth, int originalHeight, int maxSize)
        {
            // Calculate dimensions to fit within maxSize x maxSize while maintaining aspect ratio
            if (originalWidth <= maxSize && originalHeight <= maxSize)
            {
                return (originalWidth, originalHeight);
            }

            double aspectRatio = (double)originalWidth / originalHeight;
            
            if (originalWidth > originalHeight)
            {
                // Landscape
                return (maxSize, (int)(maxSize / aspectRatio));
            }
            else
            {
                // Portrait or square
                return ((int)(maxSize * aspectRatio), maxSize);
            }
        }
    }
}
