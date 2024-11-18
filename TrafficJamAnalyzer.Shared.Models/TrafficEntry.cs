namespace TrafficJamAnalyzer.Shared.Models
{
    public sealed class TrafficEntry
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public bool Enabled { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public List<TrafficResult> Results { get; set; }
    }
}
