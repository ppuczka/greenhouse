using System.Text;
using Azure;
using Azure.Identity;
using Azure.Storage.Blobs;
using Greenhouse.Storage.Interfaces;

namespace Greenhouse.Storage.Providers;

public class AzureBlobStorageProvider: IAzureBlobStorageProvider
{
    private readonly BlobContainerClient _blobContainerClient;

    public AzureBlobStorageProvider(Microsoft.Extensions.Options.IOptions<Config.Config> appConfig, DefaultAzureCredential azureCredentials)
    {
        var containerEndpoint =
            $"https://{appConfig.Value.AzureBlobContainerName}.blob.core.windows.net/{appConfig.Value.AzureBlobContainerName}";
        _blobContainerClient = new BlobContainerClient(new Uri(containerEndpoint), azureCredentials);
    }

    public async Task UploadBlob(Stream stream, string blobName)
    {
        try
        {
            await _blobContainerClient.CreateIfNotExistsAsync();
            await _blobContainerClient.UploadBlobAsync(blobName, stream);
        }
        catch (RequestFailedException e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}