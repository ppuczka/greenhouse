namespace Greenhouse.Data.Models;

public class AttachmentMetadata(string fileName, string blobUri)
{
    public string FileName { get; set; } = fileName;
    public string BlobUri { get; set; } = blobUri;
    public DateTime UploadedOn { get; set; } = DateTime.UtcNow;
}