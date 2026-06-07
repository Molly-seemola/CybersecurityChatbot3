using System;
using System.Collections.Generic;

namespace CybersecurityChatbot
{
    public class ActivityLog
    {
        private readonly List<string> _logs = new List<string>();

        public void Add(string entry)
        {
            string timestamped = $"[{DateTime.Now:HH:mm:ss}] {entry}";
            _logs.Add(timestamped);
        }

        public List<string> GetRecent(int count = 10)
        {
            int start = Math.Max(0, _logs.Count - count);
            return _logs.GetRange(start, _logs.Count - start);
        }

        public List<string> GetAll() => new List<string>(_logs);

        public void Clear() => _logs.Clear();

        public string GetSummary()
        {
            var recent = GetRecent(10);
            if (recent.Count == 0) return "No actions recorded yet.";
            return string.Join("\n", recent);
        }
    }
}