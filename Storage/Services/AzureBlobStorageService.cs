using Greenhouse.Data.Interfaces;
using Greenhouse.Storage.Interfaces;
using Greenhouse.Storage.Providers;

namespace Greenhouse.Storage.Services;

public class AzureBlobStorageService : IAzureBlobStorageService
{
    private readonly IAzureBlobStorageProvider _azureBlobStorageProvider;
    private readonly IGreenhouseMetricService _greenhouseMetricService;

    public AzureBlobStorageService(IAzureBlobStorageProvider azureBlobStorageProvider)
    {
        _azureBlobStorageProvider = azureBlobStorageProvider;
    }

    public async Task UploadFile(string metricId, Stream content, string fileName)
    {
        var attachmentMetadata = await _azureBlobStorageProvider.UploadBlob(content, fileName);
        await _greenhouseMetricService.AddAttachment(metricId, attachmentMetadata);
    }
}