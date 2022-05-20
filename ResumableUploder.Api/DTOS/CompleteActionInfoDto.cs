namespace ResumableUploder.Api.DTOS
{
    public class CompleteActionInfoDto
    {
        public string? UniqueId { get; set; }
        public string? FileName { get; set; }
        public string? FilePath { get; set; }
    }
}
