namespace Greenhouse.Storage.Interfaces;

public interface IAzureBlobStorageService
{
    Task UploadFile(string metricId, Stream content, string blobName);
}