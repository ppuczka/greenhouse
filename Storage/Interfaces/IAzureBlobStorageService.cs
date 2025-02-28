namespace Greenhouse.Storage.Interfaces;

public interface IAzureBlobStorageService
{
    Task UploadFile(Stream content, string blobName);
}