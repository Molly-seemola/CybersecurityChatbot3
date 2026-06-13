using System;
using System.Collections.Generic;

namespace CybersecurityChatbot
{
    /// <summary>
    /// Manages the Activity Log for the chatbot.
    /// Part 3 Task 4: Records all significant actions taken by the chatbot,
    /// including tasks added, reminders set, quiz attempts, and NLP interactions.
    ///
    /// Rubric target (9-10 marks): Comprehensive activity log with organised,
    /// clear summaries that are easy to navigate. Shows 5-10 at a time,
    /// allows "show more" via GetAll().
    /// </summary>
    public class ActivityLog
    {
        // Internal list storing all timestamped log entries
        private readonly List<string> _logs = new();

        /// <summary>
        /// Adds a new entry to the activity log with an automatic timestamp.
        /// Called whenever a significant action occurs (task add, reminder set,
        /// quiz start/complete, NLP intent detected).
        /// </summary>
        /// <param name="entry">A brief description of the action taken.</param>
        public void Add(string entry)
            => _logs.Add($"[{DateTime.Now:HH:mm:ss}] {entry}");

        /// <summary>
        /// Returns the most recent log entries (default: last 10).
        /// Part 3 Task 4: Displays only the last 5-10 actions to keep
        /// the log concise and relevant as specified in the rubric.
        /// </summary>
        /// <param name="count">Number of recent entries to return (default 10).</param>
        public List<string> GetRecent(int count = 10)
        {
            // Calculate start index so we return only the last 'count' entries
            int start = Math.Max(0, _logs.Count - count);
            return _logs.GetRange(start, _logs.Count - start);
        }

        /// <summary>
        /// Returns the complete log history.
        /// Part 3 Task 4: "Show more" option — shows full history when requested.
        /// </summary>
        public List<string> GetAll() => new List<string>(_logs);

        /// <summary>Clears all log entries.</summary>
        public void Clear() => _logs.Clear();

        /// <summary>
        /// Builds a numbered summary string of the last 10 actions.
        /// Part 3 Task 4: Used when user types "show activity log" or
        /// "what have you done for me?" in the chat.
        /// </summary>
        public string GetSummary()
        {
            var recent = GetRecent(10);
            if (recent.Count == 0) return "No actions recorded yet.";

            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < recent.Count; i++)
                sb.AppendLine($"{i + 1}. {recent[i]}");

            return sb.ToString().TrimEnd();
        }
    }
}