namespace Greenhouse.Storage.Interfaces;

public interface IAzureBlobStorageService
{
    Task UploadFile(string metricId, Stream content, string blobName);
    
    Task<Stream> DownloadFile(string blobName);
    Task DeleteFile(string metricId, string blobName);
}