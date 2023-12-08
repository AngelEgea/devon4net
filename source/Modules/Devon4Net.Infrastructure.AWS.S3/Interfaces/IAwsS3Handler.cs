using Amazon.S3.Model;

namespace Devon4Net.Infrastructure.AWS.S3.Interfaces
{
    public interface IAwsS3Handler : IDisposable
    {
        Task<Stream> GetObject(string bucketName, string objectKey, CancellationToken cancellationToken = default);
        Task<GetObjectResponse> GetObjectWithMetadata(string bucketName, string objectKey, CancellationToken cancellationToken = default);
        Task<bool> UploadObject(Stream streamFile, string keyName, string bucketName, string contentType, bool autoCloseStream = false, List<Tag> tagList = null, CancellationToken cancellationToken = default);
        Task<bool> UploadObjectWithMetadata(Stream streamFile, string keyName, string bucketName, string contentType, IDictionary<string, string> metadata, bool autoCloseStream = false, CancellationToken cancellationToken = default);
        Task<GetObjectMetadataResponse> GetObjectMetadata(string key, string bucketName, CancellationToken cancellationToken = default);
        Task<bool> CheckObjectExists(string key, string bucketName, CancellationToken cancellationToken = default);
        Task<bool> DeleteFiles(string key, string bucketName, CancellationToken cancellationToken = default);
        Task<List<S3Object>> GetAllFiles(string bucketName, int maxKeys = 1000, string prefix = null, CancellationToken cancellationToken = default);
        Task<Dictionary<string, string>> GetObjectTags(string bucketName, string key, CancellationToken cancellationToken = default);
        Task<bool> SetObjectTags(string key, string bucketName, Dictionary<string, string> tags, CancellationToken cancellationToken = default);
        Task CopyObject(string sourceBucket, string sourceKey, string destinationBucket, string destinationKey, Dictionary<string, string> newMetadata, CancellationToken cancellationToken = default);
        Task<List<string>> GetDirectoriesNameFromBucket(string bucketName, List<string> foldersInBucket, CancellationToken cancellationToken = default);
        Task<string> GetDirectoryNameFromBucket(string bucketName, string prefix, CancellationToken cancellationToken = default);
        Stream GetObjectSync(string bucketName, string objectKey);
        GetObjectResponse GetObjectWithMetadataSync(string bucketName, string objectKey, CancellationToken cancellationToken = default);
        bool UploadObjectSync(Stream streamFile, string keyName, string bucketName, string contentType, bool autoCloseStream = false, List<Tag> tagList = null);
        bool UploadObjectWithMetadataSync(Stream streamFile, string keyName, string bucketName, string contentType, IDictionary<string, string> metadata, bool autoCloseStream = false);
        GetObjectMetadataResponse GetObjectMetadataSync(string key, string bucketName);
        bool CheckObjectExistsSync(string key, string bucketName);
        List<S3Object> GetAllFilesSync(string bucketName, int maxKeys = 1000, string prefix = null);
        bool DeleteFilesSync(string key, string bucketName);
        void CopyObjectSync(string sourceBucket, string sourceKey, string destinationBucket, string destinationKey, Dictionary<string, string> newMetadata);
        List<string> GetDirectoriesNameFromBucketSync(string bucketName, List<string> foldersInBucket);
        string GetDirectoryNameFromBucketSync(string bucketName, string prefix);
    }
}
