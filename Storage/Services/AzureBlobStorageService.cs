using Greenhouse.Storage.Interfaces;
using Greenhouse.Storage.Providers;

namespace Greenhouse.Storage.Services;

public class AzureBlobStorageService : IAzureBlobStorageService
{
    private readonly IAzureBlobStorageProvider _azureBlobStorageProvider;

    public AzureBlobStorageService(IAzureBlobStorageProvider azureBlobStorageProvider)
    {
        _azureBlobStorageProvider = azureBlobStorageProvider;
    }

    public async Task UploadFile(Stream content, string fileName)
    {
        await _azureBlobStorageProvider.UploadBlob(content, fileName);
    }
}