using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechXpress.Services.DTOs
{
    public class ActivityItem
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
        public string GradientClass { get; set; }
        public DateTime Timestamp { get; set; }
        public string TimeAgo => GetTimeAgo(Timestamp);

        private string GetTimeAgo(DateTime timestamp)
        {
            var timeSpan = DateTime.Now - timestamp;
            if (timeSpan.TotalMinutes < 1) return "Just now";
            if (timeSpan.TotalMinutes < 60) return $"{(int)timeSpan.TotalMinutes} minutes ago";
            if (timeSpan.TotalHours < 24) return $"{(int)timeSpan.TotalHours} hours ago";
            return $"{(int)timeSpan.TotalDays} days ago";
        }
    }
}
