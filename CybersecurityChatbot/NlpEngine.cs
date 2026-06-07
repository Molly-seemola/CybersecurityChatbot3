using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CybersecurityChatbot
{
    /// <summary>
    /// Simulates NLP using keyword detection and string manipulation (string.Contains, Regex).
    /// Recognises user intent even when phrased differently.
    /// </summary>
    public class NlpEngine
    {
        public enum Intent
        {
            AddTask, SetReminder, ViewTasks, CompleteTask, DeleteTask,
            StartQuiz, ShowActivityLog,
            CyberTopic, Greeting, Farewell, Thanks, ShowHelp,
            Unknown
        }

        public class ParsedIntent
        {
            public Intent Intent { get; set; }
            public string ExtractedContent { get; set; } // e.g. task title, topic keyword
            public string ReminderTime { get; set; }
        }

        // ── Pattern groups ──────────────────────────────────────────────────
        private static readonly string[] AddTaskPatterns =
            { "add task", "add a task", "create task", "new task", "add reminder", "remind me to",
              "can you remind", "set a task", "i need to", "add to my list" };

        private static readonly string[] ReminderPatterns =
            { "remind me", "set reminder", "set a reminder", "reminder for", "can you remind",
              "remind me in", "remind me tomorrow", "notify me" };

        private static readonly string[] ViewTaskPatterns =
            { "show tasks", "view tasks", "list tasks", "my tasks", "show my tasks",
              "what are my tasks", "display tasks" };

        private static readonly string[] CompleteTaskPatterns =
            { "complete task", "mark as done", "mark done", "finish task", "task done",
              "i completed", "i finished" };

        private static readonly string[] DeleteTaskPatterns =
            { "delete task", "remove task", "cancel task", "get rid of task" };

        private static readonly string[] QuizPatterns =
            { "start quiz", "begin quiz", "take quiz", "play quiz", "quiz me",
              "test my knowledge", "start the quiz", "open quiz" };

        private static readonly string[] LogPatterns =
            { "show activity log", "activity log", "what have you done", "what have you done for me",
              "show log", "recent actions", "show history", "view log" };

        private static readonly string[] HelpPatterns =
            { "help", "what can you do", "commands", "topics", "options" };

        private static readonly string[] GreetingPatterns =
            { "hello", "hi", "hey", "howdy", "good morning", "good afternoon", "good evening" };

        private static readonly string[] FarewellPatterns =
            { "bye", "goodbye", "exit", "quit", "see you", "farewell" };

        private static readonly string[] ThanksPatterns =
            { "thank you", "thanks", "thx", "appreciate it", "ty" };

        // Cyber topic keywords → mapped to topic name
        private static readonly Dictionary<string[], string> CyberTopics = new()
        {
            { new[]{"password","passwords"}, "password" },
            { new[]{"phish","phishing"}, "phishing" },
            { new[]{"malware","virus","trojan","ransomware","spyware"}, "malware" },
            { new[]{"vpn","virtual private network"}, "vpn" },
            { new[]{"2fa","two factor","two-factor","mfa","multi factor"}, "2fa" },
            { new[]{"firewall"}, "firewall" },
            { new[]{"social engineering","social engineer"}, "social engineering" },
            { new[]{"safe browsing","browse safely","browsing","https"}, "safe browsing" },
            { new[]{"update","software update","patch"}, "software updates" },
            { new[]{"backup","back up","back-up"}, "backup" },
            { new[]{"public wifi","public wi-fi","open wifi","open network"}, "public wifi" },
        };

        // ── Main Parse Method ───────────────────────────────────────────────
        public ParsedIntent Parse(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return new ParsedIntent { Intent = Intent.Unknown };

            string lower = input.Trim().ToLower();

            if (MatchesAny(lower, AddTaskPatterns))
            {
                string content = ExtractAfterKeyword(lower, AddTaskPatterns);
                string reminder = ExtractReminderTime(lower);
                return new ParsedIntent { Intent = Intent.AddTask, ExtractedContent = content, ReminderTime = reminder };
            }

            if (MatchesAny(lower, ReminderPatterns))
            {
                string content = ExtractAfterKeyword(lower, ReminderPatterns);
                string reminder = ExtractReminderTime(lower);
                return new ParsedIntent { Intent = Intent.SetReminder, ExtractedContent = content, ReminderTime = reminder };
            }

            if (MatchesAny(lower, ViewTaskPatterns))
                return new ParsedIntent { Intent = Intent.ViewTasks };

            if (MatchesAny(lower, CompleteTaskPatterns))
            {
                string content = ExtractAfterKeyword(lower, CompleteTaskPatterns);
                return new ParsedIntent { Intent = Intent.CompleteTask, ExtractedContent = content };
            }

            if (MatchesAny(lower, DeleteTaskPatterns))
            {
                string content = ExtractAfterKeyword(lower, DeleteTaskPatterns);
                return new ParsedIntent { Intent = Intent.DeleteTask, ExtractedContent = content };
            }

            if (MatchesAny(lower, QuizPatterns))
                return new ParsedIntent { Intent = Intent.StartQuiz };

            if (MatchesAny(lower, LogPatterns))
                return new ParsedIntent { Intent = Intent.ShowActivityLog };

            if (MatchesAny(lower, GreetingPatterns))
                return new ParsedIntent { Intent = Intent.Greeting };

            if (MatchesAny(lower, FarewellPatterns))
                return new ParsedIntent { Intent = Intent.Farewell };

            if (MatchesAny(lower, ThanksPatterns))
                return new ParsedIntent { Intent = Intent.Thanks };

            if (MatchesAny(lower, HelpPatterns))
                return new ParsedIntent { Intent = Intent.ShowHelp };

            // Cyber topic detection
            foreach (var kvp in CyberTopics)
                foreach (var kw in kvp.Key)
                    if (lower.Contains(kw))
                        return new ParsedIntent { Intent = Intent.CyberTopic, ExtractedContent = kvp.Value };

            return new ParsedIntent { Intent = Intent.Unknown };
        }

        private bool MatchesAny(string input, string[] patterns)
        {
            foreach (var p in patterns)
                if (input.Contains(p)) return true;
            return false;
        }

        private string ExtractAfterKeyword(string input, string[] patterns)
        {
            foreach (var p in patterns)
            {
                int idx = input.IndexOf(p, StringComparison.Ordinal);
                if (idx >= 0)
                {
                    string after = input.Substring(idx + p.Length).Trim();
                    after = Regex.Replace(after, @"^[-:]+\s*", "").Trim(); // remove leading dash/colon
                    if (!string.IsNullOrWhiteSpace(after)) return after;
                }
            }
            return "";
        }

        private string ExtractReminderTime(string input)
        {
            // "in X days/hours/weeks"
            var match = Regex.Match(input, @"in (\d+)\s*(day|days|hour|hours|week|weeks)");
            if (match.Success) return $"in {match.Groups[1].Value} {match.Groups[2].Value}";

            if (input.Contains("tomorrow")) return "tomorrow";
            if (input.Contains("next week")) return "next week";
            if (input.Contains("today")) return "today";

            // "on [date]" or specific number
            var dateMatch = Regex.Match(input, @"on (\d{1,2}[/-]\d{1,2}[/-]?\d{0,4})");
            if (dateMatch.Success) return $"on {dateMatch.Groups[1].Value}";

            return null;
        }
    }
}
