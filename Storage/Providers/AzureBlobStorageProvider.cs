using System.Text;
using Azure;
using Azure.Identity;
using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Greenhouse.Data.Models;
using Greenhouse.Storage.Interfaces;

namespace Greenhouse.Storage.Providers;

public class AzureBlobStorageProvider : IAzureBlobStorageProvider
{
    private readonly BlobContainerClient _blobContainerClient;

    public AzureBlobStorageProvider(Microsoft.Extensions.Options.IOptions<Config.Config> appConfig,
        DefaultAzureCredential azureCredentials)
    {
        var containerEndpoint = new Uri($"https://{appConfig.Value.AzureStorageAccountName}.blob.core.windows.net");

        var blobServiceClient = new BlobServiceClient(containerEndpoint, azureCredentials);
        _blobContainerClient = blobServiceClient.GetBlobContainerClient(appConfig.Value.AzureBlobContainerName);
    }

    public AzureBlobStorageProvider(Microsoft.Extensions.Options.IOptions<Config.Config> appConfig)
    {
        // At this moment connecting using System / User MI not working hence using Shared Keys
        var storageCredentials = new StorageSharedKeyCredential(appConfig.Value.AzureStorageAccountName,
            appConfig.Value.AzureStorageAccountKey);
        var containerEndpoint = new Uri($"https://{appConfig.Value.AzureStorageAccountName}.blob.core.windows.net");

        var blobServiceClient = new BlobServiceClient(containerEndpoint, storageCredentials);
        _blobContainerClient = blobServiceClient.GetBlobContainerClient(appConfig.Value.AzureBlobContainerName);
    }

    public async Task<AttachmentMetadata> UploadBlob(Stream stream, string blobName)
    {
        stream.Position = 0;
        try
        {
            var blobClient = _blobContainerClient.GetBlobClient(blobName);
            await blobClient.UploadAsync(stream, true);
            stream.Close();
            
            return new AttachmentMetadata(blobName, blobClient.Uri.ToString());
        }
        catch (RequestFailedException e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}