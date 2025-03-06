namespace Greenhouse.Data.Models;

public class AttachmentMetadata(string fileName, string blobUri, long fileSize)
{
    public string FileName { get; set; } = fileName;
    public string BlobUri { get; set; } = blobUri;
    public long FileSize { get; set; } = fileSize;
    public DateTime UploadedOn { get; set; } = DateTime.UtcNow;
}