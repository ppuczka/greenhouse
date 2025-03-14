using Greenhouse.Data.Models;

namespace Greenhouse.Storage.Interfaces;

public interface IAzureBlobStorageProvider
{
    Task<AttachmentMetadata> UploadBlob(Stream stream, string blobName);
    
    Task<Stream> DownloadBlob(string blobName);
    
    Task DeleteBlob(string blobName);
}