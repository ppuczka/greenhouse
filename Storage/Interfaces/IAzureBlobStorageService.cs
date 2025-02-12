namespace Greenhouse.Storage.Interfaces;

public interface IAzureBlobStorageService
{
    Task UploadFile(string fileName, string content);
}