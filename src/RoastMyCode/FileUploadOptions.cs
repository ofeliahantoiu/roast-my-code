namespace RoastMyCode
{
    public class FileUploadOptions
    {
        public int MaxFileSizeMB { get; set; } = 10;
        public int MaxTotalSizeMB { get; set; } = 50;
        public string[] AllowedExtensions { get; set; } = new[] { ".*" }; // Allow all file types
        
        public long MaxFileSizeBytes => MaxFileSizeMB * 1024 * 1024;
        public long MaxTotalSizeBytes => MaxTotalSizeMB * 1024 * 1024;
    }
}
