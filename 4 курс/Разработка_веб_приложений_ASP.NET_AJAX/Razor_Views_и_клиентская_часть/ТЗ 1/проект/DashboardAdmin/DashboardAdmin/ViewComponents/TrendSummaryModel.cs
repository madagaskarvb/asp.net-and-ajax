using DashboardAdmin.Models;

namespace DashboardAdmin.ViewComponents
{
    public class TrendSummaryModel
    {
        public Trend Trend { get; set; }
        public string Label { get; set; } = string.Empty;
        public int Count { get; set; }
        public decimal Total { get; set; }
        public string CssClass { get; set; } = string.Empty;
    }
}