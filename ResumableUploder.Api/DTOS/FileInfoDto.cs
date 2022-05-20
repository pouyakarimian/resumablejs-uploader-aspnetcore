namespace ResumableUploder.Api.DTOS
{
    public class FileInfoDto
    {
        public string? ResumableUploadToken { get; set; }
        public string? ResumableChunkNumber { get; set; }
        public string? ResumableFilename { get; set; }
        public string? ResumableFileExtension { get; set; }
        public string? ResumableIdentifier { get; set; }
        public string? ResumableChunkSize { get; set; }
        public string? ResumableTotalSize { get; set; }
        public string? ResumableFolderPath { get; set; }
        public string? ResumableUid { get; set; }
        public string? ResumableTotalChunks { get; set; }
    }
}
