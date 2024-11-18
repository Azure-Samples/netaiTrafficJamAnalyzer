using Microsoft.EntityFrameworkCore;
using TrafficJamAnalyzer.Shared.Models;

namespace TrafficJamAnalyzer.Web.Models
{
    public class Context(DbContextOptions options) : DbContext(options)
    {
        public DbSet<TrafficEntry> Traffics => Set<TrafficEntry>();
        public DbSet<TrafficResult> TrafficResults => Set<TrafficResult>();
    }
}
