namespace TrafficJamAnalyzer.Shared.Models
{
    public sealed class TrafficResult
    {
        public int Id { get; set; }
        public int TrafficId { get; set; }
        public string TrafficTitle { get; set; }
        public int TrafficAmount { get; set; }
        public string CreatedAt { get; set; }
    }
}
