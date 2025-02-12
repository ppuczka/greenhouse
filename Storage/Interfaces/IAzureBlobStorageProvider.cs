namespace Greenhouse.Storage.Interfaces;

public interface IAzureBlobStorageProvider
{
    Task UploadBlob(string toBeUploaded, string blobName);
}