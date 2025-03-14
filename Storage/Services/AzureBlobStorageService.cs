using Greenhouse.Data.Interfaces;
using Greenhouse.Storage.Interfaces;
using Greenhouse.Storage.Providers;

namespace Greenhouse.Storage.Services;

public class AzureBlobStorageService(
    IAzureBlobStorageProvider azureBlobStorageProvider,
    IGreenhouseMetricService greenhouseMetricService)
    : IAzureBlobStorageService
{
    public async Task UploadFile(string metricId, Stream content, string fileName)
    {
        var attachmentMetadata = await azureBlobStorageProvider.UploadBlob(content, fileName);
        await greenhouseMetricService.AddAttachment(metricId, attachmentMetadata);
    }

    public async Task<Stream> DownloadFile(string blobName)
    {
        return await azureBlobStorageProvider.DownloadBlob(blobName);
    }
    
    public async Task DeleteFile(string metricId, string blobName)
    {
        await azureBlobStorageProvider.DeleteBlob(blobName);
        await greenhouseMetricService.DeleteAttachment(metricId, blobName);
    }
}