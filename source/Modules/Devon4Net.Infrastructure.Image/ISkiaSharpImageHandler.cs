using ADC.PostNL.BuildingBlocks.Image.Common.Enum;
using SkiaSharp;

namespace ADC.PostNL.BuildingBlocks.Image
{
    public interface ISkiaSharpImageHandler
    {
        Stream ResizeImage(byte[] fileContents, ImageFileFormat inputImageFormat, int targetImageWidth, SKEncodedImageFormat destinationFormat, int qualityPercentage = 95);
        Stream ResizeImage(Stream fileContents, ImageFileFormat inputImageFormat, int targetImageWidth, SKEncodedImageFormat destinationFormat, int qualityPercentage = 95);
        void ResizeImageWithThumbnail(Stream fileContents, ImageFileFormat inputImageFormat, int targetImageWidth, int targetThumbnailWidth, SKEncodedImageFormat destinationFormat, int qualityPercentage, out Stream resized, out Stream thumbnail);
        void ResizeImageWithThumbnail(byte[] fileContents, ImageFileFormat inputImageFormat, int targetImageWidth, int targetThumbnailWidth, SKEncodedImageFormat destinationFormat, int qualityPercentage, out Stream resized, out Stream thumbnail);
        public void ResizeImageToThumbnail(Stream fileContents, ImageFileFormat inputImageFormat, int targetThumbnailWidth, SKEncodedImageFormat destinationFormat, int qualityPercentage, out Stream thumbnail);
        public void ResizeImageToThumbnail(byte[] fileContents, ImageFileFormat inputImageFormat, int targetThumbnailWidth, SKEncodedImageFormat destinationFormat, int qualityPercentage, out Stream originalImage, out Stream thumbnail);

    }
}