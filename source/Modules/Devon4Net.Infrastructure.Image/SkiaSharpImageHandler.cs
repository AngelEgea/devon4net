using ADC.PostNL.BuildingBlocks.Common.Helpers;
using ADC.PostNL.BuildingBlocks.Image.Common.Enum;
using BitMiracle.LibTiff.Classic;
using SkiaSharp;
using System.Runtime.InteropServices;

namespace ADC.PostNL.BuildingBlocks.Image
{
    public class SkiaSharpImageHandler : ISkiaSharpImageHandler
    {
        private RecyclableMemoryHelper RecyclableMemoryHelper { get; }

        public SkiaSharpImageHandler(RecyclableMemoryHelper recyclableMemoryHelper)
        {
            RecyclableMemoryHelper = recyclableMemoryHelper;
        }

        public SkiaSharpImageHandler()
        {
            RecyclableMemoryHelper = new RecyclableMemoryHelper();
        }

        public Stream ResizeImage(byte[] fileContents, ImageFileFormat inputImageFormat, int targetImageWidth, SKEncodedImageFormat destinationFormat, int qualityPercentage = 95)
        {
            using var ms = RecyclableMemoryHelper.GetMemoryStream(fileContents);
            return ResizeImage(ms, inputImageFormat, targetImageWidth, destinationFormat, qualityPercentage);
        }

        public Stream ResizeImage(Stream fileContents, ImageFileFormat inputImageFormat, int targetImageWidth, SKEncodedImageFormat destinationFormat, int qualityPercentage = 95)
        {
            using SKBitmap sourceBitmap = GetBitmapFromStream(ref fileContents, inputImageFormat);
            return ResizeImage(sourceBitmap, targetImageWidth, destinationFormat, qualityPercentage);
        }

        public void ResizeImageWithThumbnail(Stream fileContents, ImageFileFormat inputImageFormat, int targetImageWidth, int targetThumbnailWidth, SKEncodedImageFormat destinationFormat, int qualityPercentage, out Stream resized, out Stream thumbnail)
        {
            using SKBitmap sourceBitmap = GetBitmapFromStream(ref fileContents, inputImageFormat);
            resized = ResizeImage(sourceBitmap, targetImageWidth, destinationFormat, qualityPercentage);
            thumbnail = ResizeImage(sourceBitmap, targetThumbnailWidth, destinationFormat, qualityPercentage);
        }

        public void ResizeImageWithThumbnail(byte[] fileContents, ImageFileFormat inputImageFormat, int targetImageWidth, int targetThumbnailWidth, SKEncodedImageFormat destinationFormat, int qualityPercentage, out Stream resized, out Stream thumbnail)
        {
            using var ms = RecyclableMemoryHelper.GetMemoryStream(fileContents);
            ResizeImageWithThumbnail(ms, inputImageFormat, targetImageWidth, targetThumbnailWidth, destinationFormat, qualityPercentage, out resized, out thumbnail);
        }

        public void ResizeImageToThumbnail(Stream fileContents, ImageFileFormat inputImageFormat, int targetThumbnailWidth, SKEncodedImageFormat destinationFormat, int qualityPercentage, out Stream thumbnail)
        {
            using SKBitmap sourceBitmap = GetBitmapFromStream(ref fileContents, inputImageFormat);
            thumbnail = ResizeImage(sourceBitmap, targetThumbnailWidth, destinationFormat, qualityPercentage);
        }

        public void ResizeImageToThumbnail(byte[] fileContents, ImageFileFormat inputImageFormat, int targetThumbnailWidth, SKEncodedImageFormat destinationFormat, int qualityPercentage, out Stream originalImage, out Stream thumbnail)
        {
            Stream imageToConvert = RecyclableMemoryHelper.GetMemoryStream(fileContents);
            originalImage = RecyclableMemoryHelper.GetMemoryStream();

            try
            {
                using SKBitmap sourceBitmap = GetBitmapFromStream(ref imageToConvert, inputImageFormat);
                thumbnail = ResizeImage(sourceBitmap, targetThumbnailWidth, destinationFormat, qualityPercentage);

                // This is done to remove the EXIF data that might come in the source image
                using var fileBitmap = SKImage.FromBitmap(sourceBitmap);
                using var encodedFile = fileBitmap.Encode(destinationFormat, qualityPercentage);
                encodedFile.SaveTo(originalImage);
            }
            finally
            {
                imageToConvert?.Dispose();
            }
        }

        private static SKBitmap GetBitmapFromStream(ref Stream input, ImageFileFormat imageFileFormat)
        {
            if (imageFileFormat.Equals(ImageFileFormat.Tiff))
            {
                return ConvertToTiff(input);
            }

            using var codec = SKCodec.Create(input);
            return SKBitmap.Decode(codec);
        }

        private Stream ResizeImage(SKBitmap sourceImage, int targetImageWidth, SKEncodedImageFormat destinationFormat, int qualityPercentage = 95)
        {
            var result = RecyclableMemoryHelper.GetMemoryStream();
            var imageResizedHeight = GetAspectRatioImageHeight(sourceImage.Width, sourceImage.Height, targetImageWidth);

            using SKBitmap scaledBitmap = sourceImage.Resize(new SKImageInfo(targetImageWidth, imageResizedHeight), SKFilterQuality.High);
            using SKImage scaledImage = SKImage.FromBitmap(scaledBitmap);
            using SKData data = scaledImage.Encode(destinationFormat, qualityPercentage);
            data.SaveTo(result);
            return result;
        }

        private static int GetAspectRatioImageHeight(int originalWidth, int originalHeight, int targetWidth)
        {
            return originalHeight * targetWidth / originalWidth;
        }

        private static SKBitmap ConvertToTiff(Stream tiffStream)
        {
            using var tifImg = Tiff.ClientOpen("in-memory", "r", tiffStream, new TiffStream());
            var width = tifImg.GetField(TiffTag.IMAGEWIDTH)[0].ToInt();
            var height = tifImg.GetField(TiffTag.IMAGELENGTH)[0].ToInt();

            var bitmap = new SKBitmap();
            var raster = new int[width * height];
            var ptr = GCHandle.Alloc(raster, GCHandleType.Pinned);
            var info = new SKImageInfo(width, height);
            bitmap.InstallPixels(new SKImageInfo(width, height), ptr.AddrOfPinnedObject(), info.RowBytes, (addr, ctx) => ptr.Free());

            if (!tifImg.ReadRGBAImageOriented(width, height, raster, Orientation.TOPLEFT))
            {
                // not a valid TIF image.
                return null;
            }

            // swap the red and blue because SkiaSharp may differ from the tiff
            if (SKImageInfo.PlatformColorType == SKColorType.Bgra8888)
            {
                SKSwizzle.SwapRedBlue(ptr.AddrOfPinnedObject(), raster.Length);
            }

            return bitmap;
        }
    }
}
