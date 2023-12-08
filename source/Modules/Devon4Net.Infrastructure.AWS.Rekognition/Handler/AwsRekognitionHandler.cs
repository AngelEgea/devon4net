using Amazon;
using Amazon.Rekognition;
using Amazon.Rekognition.Model;
using Amazon.Runtime;
using Devon4Net.Infrastructure.Common;
using Devon4Net.Infrastructure.Common.Extensions;

namespace Devon4Net.Infrastructure.AWS.Rekognition.Handler
{
    public class AwsRekognitionHandler : IAwsRekognitionHandler
    {
        private AmazonRekognitionClient _amazonRekognitionClient { get; }
        private AWSCredentials _awsCredentials { get; }
        private RegionEndpoint _regionEndpoint { get; }

        private bool _disposed = false;

        public AwsRekognitionHandler(AWSCredentials awsCredentials = null, RegionEndpoint regionEndpoint = null)
        {
            _awsCredentials = awsCredentials;
            _regionEndpoint = regionEndpoint;
            _amazonRekognitionClient = CreateRekognitionClient();
        }

        public AwsRekognitionHandler(AmazonRekognitionClient amazonRekognitionClient, AWSCredentials awsCredentials = null, RegionEndpoint regionEndpoint = null)
        {
            _awsCredentials = awsCredentials;
            _regionEndpoint = regionEndpoint;
            _amazonRekognitionClient = amazonRekognitionClient;
        }

        public async Task<IList<ModerationLabel>> GetModerationLabels(string encodedImage, float? minConfidence = default)
        {
            try
            {
                var decodingSuccessful = encodedImage.ToByteArrayFromBase64(out var imageBytes);

                if (!decodingSuccessful)
                {
                    throw new ArgumentException("You must provide a base64-encoded image");
                }

                using var memoryStream = new MemoryStream(imageBytes);

                var analyzeImage = new Image
                {
                    Bytes = memoryStream
                };

                var requestModerationLabels = new DetectModerationLabelsRequest
                {
                    Image = analyzeImage
                };

                if (minConfidence != null)
                {
                    requestModerationLabels.MinConfidence = minConfidence.Value;
                }

                var responseModerationLabels = await _amazonRekognitionClient.DetectModerationLabelsAsync(requestModerationLabels);

                return responseModerationLabels.ModerationLabels;
            }
            catch (Exception ex)
            {
                Devon4NetLogger.Error("An error ocurred while trying to get moderation labels from an image");
                Devon4NetLogger.Error(ex);
                throw;
            }
        }

        public async Task<IList<ModerationLabel>> GetModerationLabels(string bucketImageName, string bucketName, float? minConfidence = default)
        {
            try
            {
                var analyzeImage = new Image
                {
                    S3Object = new S3Object
                    {
                        Bucket = bucketName,
                        Name = bucketImageName
                    }
                };

                var requestModerationLabels = new DetectModerationLabelsRequest
                {
                    Image = analyzeImage
                };

                if (minConfidence != null)
                {
                    requestModerationLabels.MinConfidence = minConfidence.Value;
                }

                var responseModerationLabels = await _amazonRekognitionClient.DetectModerationLabelsAsync(requestModerationLabels);

                return responseModerationLabels.ModerationLabels;
            }
            catch (Exception ex)
            {
                Devon4NetLogger.Error("An error ocurred while trying to get moderation labels from an image");
                Devon4NetLogger.Error(ex);
                throw;
            }
        }

        public async Task<IList<string>> GetImageText(string bucketImageName, string bucketName = null, float? minConfidence = default)
        {
            var analyzeImage = new Image
            {
                S3Object = new S3Object
                {
                    Bucket = bucketName,
                    Name = bucketImageName
                },
            };

            var filter = new DetectTextFilters
            {
                WordFilter = new DetectionFilter
                {
                    MinConfidence = minConfidence ?? default
                }
            };

            var requestDetectText = new DetectTextRequest
            {
                Image = analyzeImage,
                Filters = filter
            };

            var responseDetectText = await _amazonRekognitionClient.DetectTextAsync(requestDetectText);

            var wordsDetected = responseDetectText.TextDetections.Where(x => x.Type == TextTypes.WORD);

            return wordsDetected.Select(x => x.DetectedText).ToList();
        }

        private AmazonRekognitionClient CreateRekognitionClient()
        {
            return _awsCredentials == null ? new AmazonRekognitionClient(_regionEndpoint) : new AmazonRekognitionClient(_awsCredentials, _regionEndpoint);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                _amazonRekognitionClient.Dispose();
            }

            _disposed = true;
        }
    }
}
