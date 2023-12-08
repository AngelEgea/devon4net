using Amazon.Rekognition.Model;

namespace Devon4Net.Infrastructure.AWS.Rekognition.Handler
{
    public interface IAwsRekognitionHandler : IDisposable
    {
        Task<IList<ModerationLabel>> GetModerationLabels(string encodedImage, float? minConfidence = default);
        Task<IList<ModerationLabel>> GetModerationLabels(string bucketImageName, string bucketName, float? minConfidence = default);
        Task<IList<string>> GetImageText(string bucketImageName, string bucketName = null, float? minConfidence = default);
    }
}
