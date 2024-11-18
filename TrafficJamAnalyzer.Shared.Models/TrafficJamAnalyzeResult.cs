namespace TrafficJamAnalyzer.Shared.Models
{
    public class TrafficJamAnalyzeResult
    {
        public string SourceUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public TrafficJamAnalyze Result { get; set; }
    }
}
