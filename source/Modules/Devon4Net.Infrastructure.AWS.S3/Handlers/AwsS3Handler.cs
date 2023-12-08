using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Devon4Net.Infrastructure.AWS.S3.Interfaces;
using Devon4Net.Infrastructure.Common;
using System.Net;

namespace Devon4Net.Infrastructure.AWS.S3.Handlers
{
    public class AwsS3Handler : IAwsS3Handler
    {
        private IAmazonS3 S3Client { get; init; }
        private bool _disposed = false;

        public AwsS3Handler(string awsRegion, string awsSecretAccessKeyId, string awsSecretAccessKey)
        {
            S3Client = GetS3Client(awsRegion, awsSecretAccessKeyId, awsSecretAccessKey);
        }

        public AwsS3Handler(AWSCredentials awsCredentials, RegionEndpoint awsRegion)
        {
            S3Client = new AmazonS3Client(awsCredentials, awsRegion);
        }

        public AwsS3Handler(IAmazonS3 amazonS3Client)
        {
            S3Client = amazonS3Client;
        }

        #region Async methods
        public async Task<Stream> GetObject(string bucketName, string objectKey, CancellationToken cancellationToken = default)
        {
            try
            {
                var request = new GetObjectRequest { BucketName = bucketName, Key = objectKey };
                var response = await S3Client.GetObjectAsync(request, cancellationToken).ConfigureAwait(false);
                return response.ResponseStream;
            }
            catch (AmazonS3Exception ex)
            {
                if (ex.StatusCode == HttpStatusCode.NotFound) return null;

                Devon4NetLogger.Error($"File {objectKey} could not be retrieved from bucket {bucketName}.");
                LogS3Exception(ex);

                throw;
            }
        }

        public async Task<GetObjectResponse> GetObjectWithMetadata(string bucketName, string objectKey, CancellationToken cancellationToken = default)
        {
            try
            {
                var request = new GetObjectRequest { BucketName = bucketName, Key = objectKey };
                return await S3Client.GetObjectAsync(request, cancellationToken).ConfigureAwait(false);
            }
            catch (AmazonS3Exception ex)
            {
                if (ex.StatusCode == HttpStatusCode.NotFound) return null;

                Devon4NetLogger.Error($"File with metadata {objectKey} could not be retrieved from bucket {bucketName}.");
                LogS3Exception(ex);

                throw;
            }
        }

        public async Task<bool> UploadObject(Stream streamFile, string keyName, string bucketName, string contentType, bool autoCloseStream = false, List<Tag> tagList = null, CancellationToken cancellationToken = default)
        {
            try
            {
                CheckUploadObjectParams(streamFile, keyName, bucketName);

                var fileTransferUtility = new TransferUtility(S3Client);

                var transferUtilityUploadRequest = new TransferUtilityUploadRequest
                {
                    InputStream = streamFile,
                    Key = keyName,
                    BucketName = bucketName,
                    CannedACL = S3CannedACL.BucketOwnerFullControl,
                    ContentType = contentType,
                    AutoCloseStream = autoCloseStream,
                    TagSet = tagList ?? new List<Tag>()
                };

                await fileTransferUtility.UploadAsync(transferUtilityUploadRequest, cancellationToken).ConfigureAwait(false);
                if (!autoCloseStream) streamFile.Position = 0;
            }
            catch (AmazonS3Exception ex)
            {
                Devon4NetLogger.Error($"File {keyName} could not be updated to bucket {bucketName}.");
                LogS3Exception(ex);

                throw;
            }

            return true;
        }

        public async Task<bool> UploadObjectWithMetadata(Stream streamFile, string keyName, string bucketName, string contentType, IDictionary<string, string> metadata, bool autoCloseStream = false, CancellationToken cancellationToken = default)
        {
            try
            {
                CheckUploadObjectParams(streamFile, keyName, bucketName);

                var request = new PutObjectRequest
                {
                    BucketName = bucketName,
                    Key = keyName,
                    InputStream = streamFile,
                    ContentType = contentType,
                    CannedACL = S3CannedACL.BucketOwnerFullControl,
                    AutoCloseStream = autoCloseStream
                };

                foreach (var key in metadata)
                {
                    request.Metadata.Add(key.Key, key.Value);
                }

                var upload = await S3Client.PutObjectAsync(request, cancellationToken).ConfigureAwait(false);
                if (!autoCloseStream) streamFile.Position = 0;

                return upload.HttpStatusCode == HttpStatusCode.OK;
            }
            catch (AmazonS3Exception ex)
            {
                Devon4NetLogger.Error($"File {keyName} with metadata could not be updated to bucket {bucketName}.");
                LogS3Exception(ex);
                throw;
            }
        }

        public async Task<GetObjectMetadataResponse> GetObjectMetadata(string key, string bucketName, CancellationToken cancellationToken = default)
        {
            try
            {
                return await S3Client.GetObjectMetadataAsync(new GetObjectMetadataRequest { Key = key, BucketName = bucketName }, cancellationToken).ConfigureAwait(false);
            }
            catch (AmazonS3Exception ex)
            {
                if (ex.StatusCode == HttpStatusCode.NotFound) return null;

                Devon4NetLogger.Error($"File {key} metadata in bucket {bucketName} could not be retrieved.");
                LogS3Exception(ex);

                throw;
            }
        }

        public async Task<bool> CheckObjectExists(string key, string bucketName, CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await S3Client.GetObjectMetadataAsync(new GetObjectMetadataRequest { Key = key, BucketName = bucketName }, cancellationToken).ConfigureAwait(false);
                if (response.HttpStatusCode == HttpStatusCode.NotFound) return false;
                return response.HttpStatusCode == HttpStatusCode.OK && response.LastModified != DateTime.MinValue && response.LastModified != DateTime.MaxValue && response.LastModified != default;
            }
            catch (AmazonS3Exception ex)
            {
                if (ex.StatusCode == HttpStatusCode.NotFound) return false;

                Devon4NetLogger.Error($"File {key} in bucket {bucketName} could not be found.");
                LogS3Exception(ex);

                throw;
            }
        }
        public async Task<bool> DeleteFiles(string key, string bucketName, CancellationToken cancellationToken = default)
        {
            try
            {
                await S3Client.DeleteObjectAsync(bucketName, key, cancellationToken).ConfigureAwait(false);
                return true;
            }
            catch (AmazonS3Exception ex)
            {
                Devon4NetLogger.Error($"File {key} could not be deleted.");
                LogS3Exception(ex);
                return false;
            }
        }

        public async Task<List<S3Object>> GetAllFiles(string bucketName, int maxKeys = 1000, string prefix = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var allObjects = new List<S3Object>();
                var listObjectsrequest = new ListObjectsV2Request()
                {
                    BucketName = bucketName,
                    Prefix = prefix,
                    MaxKeys = maxKeys,
                };

                var listofObjects = await S3Client.ListObjectsV2Async(listObjectsrequest, cancellationToken).ConfigureAwait(false);
                allObjects.AddRange(listofObjects.S3Objects);
                while (listofObjects.IsTruncated)
                {
                    listObjectsrequest.ContinuationToken = listofObjects.NextContinuationToken;
                    listofObjects = await S3Client.ListObjectsV2Async(listObjectsrequest, cancellationToken).ConfigureAwait(false);
                    allObjects.AddRange(listofObjects.S3Objects);
                }
                return allObjects;
            }
            catch (AmazonS3Exception ex)
            {
                Devon4NetLogger.Error($"Could not get files for {bucketName} bucket");
                LogS3Exception(ex);
                throw;
            }
        }

        public async Task<Dictionary<string, string>> GetObjectTags(string bucketName, string key, CancellationToken cancellationToken = default)
        {
            var response = await S3Client.GetObjectTaggingAsync(new GetObjectTaggingRequest
            {
                BucketName = bucketName,
                Key = key
            }, cancellationToken).ConfigureAwait(false);

            if (response?.HttpStatusCode != HttpStatusCode.OK)
            {
                return null;
            }

            return response.Tagging.ToDictionary(keySelector: x => x.Key, elementSelector: x => x.Value);
        }

        public async Task<bool> SetObjectTags(string key, string bucketName, Dictionary<string, string> tags, CancellationToken cancellationToken = default)
        {
            var tagList = tags.Select(x => new Tag
            {
                Key = x.Key,
                Value = x.Value
            }).ToList();

            var response = await S3Client.PutObjectTaggingAsync(new PutObjectTaggingRequest
            {
                BucketName = bucketName,
                Key = key,
                Tagging = new Tagging
                {
                    TagSet = tagList
                }
            }, cancellationToken).ConfigureAwait(false);

            return response?.HttpStatusCode == HttpStatusCode.OK;
        }

        public async Task CopyObject(string sourceBucket, string sourceKey, string destinationBucket, string destinationKey, Dictionary<string, string> newMetadata, CancellationToken cancellationToken = default)
        {
            try
            {
                var copyObjectRequest = new CopyObjectRequest
                {
                    BucketKeyEnabled = true,
                    SourceBucket = sourceBucket,
                    DestinationBucket = destinationBucket,
                    SourceKey = sourceKey,
                    DestinationKey = destinationKey,
                    CannedACL = S3CannedACL.BucketOwnerFullControl,
                    MetadataDirective = S3MetadataDirective.REPLACE
                };
                foreach (var key in newMetadata)
                {
                    copyObjectRequest.Metadata.Add(key.Key, key.Value);
                }

                await S3Client.CopyObjectAsync(copyObjectRequest, cancellationToken).ConfigureAwait(false);
            }
            catch (AmazonS3Exception ex)
            {
                Devon4NetLogger.Error($"Could not copy or update the desired object: {sourceKey}");
                LogS3Exception(ex);
                throw;
            }
        }

        public Task<List<string>> GetDirectoriesNameFromBucket(string bucketName, List<string> foldersInBucket, CancellationToken cancellationToken = default)
        {
            var listObjectsrequest = new ListObjectsV2Request()
            {
                BucketName = bucketName,
                Delimiter = "/" //This is only to get the folder
            };

            return CreateListOfDirectories(bucketName, foldersInBucket, S3Client, listObjectsrequest, cancellationToken);
        }

        public async Task<string> GetDirectoryNameFromBucket(string bucketName, string prefix, CancellationToken cancellationToken = default)
        {
            try
            {
                var listObjectsrequest = new ListObjectsV2Request()
                {
                    BucketName = bucketName,
                    Prefix = prefix,
                    Delimiter = "/" //This is only to get the folder
                };

                var listofObjects = await S3Client.ListObjectsV2Async(listObjectsrequest, cancellationToken).ConfigureAwait(false);
                return listofObjects.CommonPrefixes.FirstOrDefault();
            }
            catch (AmazonS3Exception ex)
            {
                Devon4NetLogger.Error($"Could not get the directory name from bucket {bucketName} using prefix {prefix}");
                LogS3Exception(ex);
                throw;
            }
        }

        #endregion
        #region Sync methods
        // https://docs.microsoft.com/en-us/archive/msdn-magazine/2015/july/async-programming-brownfield-async-development#the-blocking-hack
        public Stream GetObjectSync(string bucketName, string objectKey)
        {
            return GetObject(bucketName, objectKey).GetAwaiter().GetResult();
        }

        public GetObjectResponse GetObjectWithMetadataSync(string bucketName, string objectKey, CancellationToken cancellationToken = default)
        {
            return GetObjectWithMetadata(bucketName, objectKey, cancellationToken).GetAwaiter().GetResult();
        }

        public bool UploadObjectSync(Stream streamFile, string keyName, string bucketName, string contentType, bool autoCloseStream = false, List<Tag> tagList = null)
        {
            return UploadObject(streamFile, keyName, bucketName, contentType, autoCloseStream, tagList).GetAwaiter().GetResult();
        }

        public bool UploadObjectWithMetadataSync(Stream streamFile, string keyName, string bucketName, string contentType, IDictionary<string, string> metadata, bool autoCloseStream = false)
        {
            return UploadObjectWithMetadata(streamFile, keyName, bucketName, contentType, metadata, autoCloseStream).GetAwaiter().GetResult();
        }

        public GetObjectMetadataResponse GetObjectMetadataSync(string key, string bucketName)
        {
            return GetObjectMetadata(key, bucketName).GetAwaiter().GetResult();
        }

        public bool CheckObjectExistsSync(string key, string bucketName)
        {
            return CheckObjectExists(key, bucketName).GetAwaiter().GetResult();
        }

        public List<S3Object> GetAllFilesSync(string bucketName, int maxKeys = 1000, string prefix = null)
        {
            return GetAllFiles(bucketName, maxKeys, prefix).GetAwaiter().GetResult();
        }

        public bool DeleteFilesSync(string key, string bucketName)
        {
            return DeleteFiles(key, bucketName).GetAwaiter().GetResult();
        }

        public void CopyObjectSync(string sourceBucket, string sourceKey, string destinationBucket, string destinationKey, Dictionary<string, string> newMetadata)
        {
            CopyObject(sourceBucket, sourceKey, destinationBucket, destinationKey, newMetadata).GetAwaiter().GetResult();
        }

        public List<string> GetDirectoriesNameFromBucketSync(string bucketName, List<string> foldersInBucket)
        {
            return GetDirectoriesNameFromBucket(bucketName, foldersInBucket).GetAwaiter().GetResult();
        }

        public string GetDirectoryNameFromBucketSync(string bucketName, string prefix)
        {
            return GetDirectoryNameFromBucket(bucketName, prefix).GetAwaiter().GetResult();
        }

        #endregion
        #region Private methods
        private static void LogS3Exception(Exception exception)
        {
            var message = exception?.Message;
            var innerException = exception?.InnerException;
            Devon4NetLogger.Error($"Error performing the S3 action:{message} {innerException}");
        }

        private static void CheckUploadObjectParams(Stream streamFile, string keyName, string bucketName)
        {
            if (streamFile == null || streamFile.Length == 0 || !streamFile.CanRead)
            {
                Devon4NetLogger.Fatal("No base64Image to create the S3 upload");
                throw new ArgumentException("No base64Image to create the S3 upload");
            }

            if (string.IsNullOrEmpty(keyName))
            {
                Devon4NetLogger.Fatal("No keyName provided to create the S3 upload");
                throw new ArgumentException("No keyName provided to create the S3 upload");
            }

            if (string.IsNullOrEmpty(bucketName))
            {
                Devon4NetLogger.Fatal("No bucketName provided to create the S3 upload");
                throw new ArgumentException("No bucketName provided to create the S3 upload");
            }
        }

        private static IAmazonS3 GetS3Client(string awsRegion, string awsSecretAccessKeyId, string awsSecretAccessKey)
        {
            if (string.IsNullOrEmpty(awsRegion))
            {
                Devon4NetLogger.Fatal("No region provided to create the S3 client");
                throw new ArgumentException("No region provided to create the S3 client");
            }

            if (string.IsNullOrEmpty(awsSecretAccessKeyId))
            {
                Devon4NetLogger.Fatal("No awsSecretAccessKeyId provided to create the S3 client");
                throw new ArgumentException("No awsSecretAccessKeyId provided to create the S3 client");
            }

            if (string.IsNullOrEmpty(awsSecretAccessKey))
            {
                Devon4NetLogger.Fatal("No awsSecretAccessKey provided to create the S3 client");
                throw new ArgumentException("No awsSecretAccessKey provided to create the S3 client");
            }

            var region = RegionEndpoint.GetBySystemName(awsRegion);

            return new AmazonS3Client(awsSecretAccessKeyId, awsSecretAccessKey, region);
        }

        private async Task<List<string>> CreateListOfDirectories(string bucketName, List<string> foldersInBucket, IAmazonS3 S3Client, ListObjectsV2Request listObjectsrequest, CancellationToken cancellationToken = default)
        {
            var listofObjects = await S3Client.ListObjectsV2Async(listObjectsrequest, cancellationToken).ConfigureAwait(false);
            foldersInBucket.AddRange(listofObjects.CommonPrefixes);
            if (listofObjects.CommonPrefixes.Count != 0)
            {
                foreach (var prefix in listofObjects.CommonPrefixes)
                {
                    listObjectsrequest.Prefix = prefix;
                    await CreateListOfDirectories(bucketName, foldersInBucket, S3Client, listObjectsrequest, cancellationToken);
                }
            }

            return foldersInBucket;
        }
        #endregion
        #region Dispose
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
                S3Client?.Dispose();
            }

            _disposed = true;
        }
        #endregion
    }
}
