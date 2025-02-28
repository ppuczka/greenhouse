namespace Greenhouse.Storage.Interfaces;

public interface IAzureBlobStorageProvider
{
    Task UploadBlob(Stream stream, string blobName);
}