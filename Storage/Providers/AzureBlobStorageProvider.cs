using System.Text;
using Azure;
using Azure.Identity;
using Azure.Storage.Blobs;
using Greenhouse.Storage.Interfaces;

namespace Greenhouse.Storage.Providers;

public class AzureBlobStorageProvider: IAzureBlobStorageProvider
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly BlobContainerClient _blobContainerClient;
    public AzureBlobStorageProvider(Microsoft.Extensions.Options.IOptions<Config.Config> appConfig, DefaultAzureCredential azureCredentials)
    {
        var containerEndpoint = $"https://{appConfig.Value.AzureStorageAccountName}.blob.core.windows.net";
        _blobServiceClient = new BlobServiceClient(new Uri(containerEndpoint), azureCredentials);
        
        _blobContainerClient = _blobServiceClient.GetBlobContainerClient(appConfig.Value.AzureBlobContainerName);
    }

    public async Task UploadBlob(Stream stream, string blobName)
    {
        stream.Position = 0;
        try
        {
            // await _blobServiceClient.CreateBlobContainerAsync(blobName);
           var response = await _blobContainerClient.UploadBlobAsync(blobName, stream);
        }
        catch (RequestFailedException e)
        {
            Console.WriteLine(e);
            throw;
        }
        stream.Close();
    }
}