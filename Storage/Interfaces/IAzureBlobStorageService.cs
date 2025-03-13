namespace Greenhouse.Storage.Interfaces;

public interface IAzureBlobStorageService
{
    Task UploadFile(string metricId, Stream content, string blobName);
    
    Task DownloadFile(string blobName);
    Task DeleteFile(string metricId, string blobName);
}