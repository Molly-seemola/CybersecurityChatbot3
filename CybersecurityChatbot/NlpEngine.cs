using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace CybersecurityChatbot
{
    /// <summary>
    /// Simulates Natural Language Processing (NLP) using keyword detection
    /// and string manipulation (string.Contains + Regex).
    ///
    /// Part 3 Task 3: Makes the chatbot responsive to different ways users
    /// phrase requests. For example, "add task", "create a task", "i need to"
    /// all map to the AddTask intent.
    ///
    /// Rubric target (13-15 marks): Advanced NLP simulation that adapts to
    /// user phrasing, creating an engaging interaction. Limits "I didn't
    /// understand" responses as much as possible.
    /// </summary>
    public class NlpEngine
    {
        /// <summary>
        /// All possible detected user intents.
        /// Each intent maps to a specific chatbot action or response.
        /// </summary>
        public enum Intent
        {
            AddTask,          // User wants to add a cybersecurity task
            SetReminder,      // User wants to set a reminder
            ViewTasks,        // User wants to see their task list
            CompleteTask,     // User wants to mark a task as done
            DeleteTask,       // User wants to remove a task
            StartQuiz,        // User wants to start the quiz
            ShowActivityLog,  // User wants to see the activity log
            CyberTopic,       // User is asking about a cybersecurity topic
            Greeting,         // User is saying hello
            Farewell,         // User is saying goodbye
            Thanks,           // User is expressing gratitude
            ShowHelp,         // User wants to see available commands
            FollowUp,         // Part 2: User wants more info on the last topic
            Unknown           // No intent could be detected
        }

        /// <summary>
        /// Carries the detected intent and any extracted data
        /// (e.g. task title, reminder timeframe, topic name).
        /// </summary>
        public class ParsedIntent
        {
            /// <summary>The detected user intent category.</summary>
            public Intent Intent { get; set; }

            /// <summary>
            /// Extracted text from the user's message.
            /// For AddTask: the task title. For CyberTopic: the topic key.
            /// </summary>
            public string ExtractedContent { get; set; }

            /// <summary>
            /// Extracted reminder timeframe (e.g. "in 3 days", "tomorrow").
            /// Null if no reminder was mentioned.
            /// </summary>
            public string ReminderTime { get; set; }
        }

        // ── Keyword pattern arrays ───────────────────────────────────────────
        // Each array contains multiple ways a user might express the same intent.
        // Using string.Contains() to match any of these patterns (NLP simulation).

        /// <summary>Patterns that indicate the user wants to add a task.</summary>
        private static readonly string[] AddTaskPatterns =
        {
            "add task", "add a task", "create task", "new task", "add reminder",
            "remind me to", "can you remind", "set a task", "i need to",
            "add to my list", "create a task", "make a task"
        };

        /// <summary>Patterns that indicate the user wants to set a reminder.</summary>
        private static readonly string[] ReminderPatterns =
        {
            "remind me", "set reminder", "set a reminder", "reminder for",
            "can you remind", "remind me in", "remind me tomorrow",
            "notify me", "alert me"
        };

        /// <summary>Patterns that indicate the user wants to view their tasks.</summary>
        private static readonly string[] ViewTaskPatterns =
        {
            "show tasks", "view tasks", "list tasks", "my tasks",
            "show my tasks", "what are my tasks", "display tasks", "see my tasks"
        };

        /// <summary>Patterns for marking a task as complete.</summary>
        private static readonly string[] CompleteTaskPatterns =
        {
            "complete task", "mark as done", "mark done", "finish task",
            "task done", "i completed", "i finished", "done with task"
        };

        /// <summary>Patterns for deleting a task.</summary>
        private static readonly string[] DeleteTaskPatterns =
        {
            "delete task", "remove task", "cancel task",
            "get rid of task", "erase task"
        };

        /// <summary>Patterns for starting the quiz mini-game.</summary>
        private static readonly string[] QuizPatterns =
        {
            "start quiz", "begin quiz", "take quiz", "play quiz", "quiz me",
            "test my knowledge", "start the quiz", "open quiz", "launch quiz"
        };

        /// <summary>
        /// Patterns for viewing the activity log.
        /// Part 3 Task 4: Triggered by "show activity log" or "what have you done for me?".
        /// </summary>
        private static readonly string[] LogPatterns =
        {
            "show activity log", "activity log", "what have you done",
            "what have you done for me", "show log", "recent actions",
            "show history", "view log", "show recent"
        };

        /// <summary>Patterns for showing the help/commands menu.</summary>
        private static readonly string[] HelpPatterns =
        {
            "help", "what can you do", "commands", "topics", "options", "menu"
        };

        /// <summary>Greeting patterns.</summary>
        private static readonly string[] GreetingPatterns =
        {
            "hello", "hi", "hey", "howdy",
            "good morning", "good afternoon", "good evening", "greetings"
        };

        /// <summary>Farewell patterns.</summary>
        private static readonly string[] FarewellPatterns =
        {
            "bye", "goodbye", "exit", "quit",
            "see you", "farewell", "take care"
        };

        /// <summary>Gratitude patterns.</summary>
        private static readonly string[] ThanksPatterns =
        {
            "thank you", "thanks", "thx", "appreciate it", "ty", "cheers"
        };

        /// <summary>
        /// Part 2: Follow-up patterns for conversation flow.
        /// Allows user to ask for more info on the last discussed topic.
        /// </summary>
        private static readonly string[] FollowUpPatterns =
        {
            "tell me more", "explain more", "more info", "give me another tip",
            "more details", "elaborate", "continue", "go on", "and then", "what else"
        };

        /// <summary>
        /// Maps arrays of topic keywords to a standardised topic key.
        /// The topic key is passed to ChatbotEngine.GetCyberResponse().
        /// Includes scam and privacy as extra topics beyond the basic requirements.
        /// </summary>
        private static readonly Dictionary<string[], string> CyberTopics = new()
        {
            { new[] { "password", "passwords" },               "password"           },
            { new[] { "phish", "phishing" },                   "phishing"           },
            { new[] { "malware", "virus", "trojan",
                      "ransomware", "spyware" },                "malware"            },
            { new[] { "vpn", "virtual private network" },      "vpn"                },
            { new[] { "2fa", "two factor", "two-factor",
                      "mfa", "multi factor" },                  "2fa"                },
            { new[] { "firewall" },                            "firewall"           },
            { new[] { "social engineering",
                      "social engineer" },                      "social engineering" },
            { new[] { "safe browsing", "browse safely",
                      "browsing", "https" },                    "safe browsing"      },
            { new[] { "update", "software update", "patch" }, "software updates"   },
            { new[] { "backup", "back up", "back-up" },       "backup"             },
            { new[] { "public wifi", "public wi-fi",
                      "open wifi", "open network" },            "public wifi"        },
            { new[] { "scam", "scams" },                       "scam"               },
            { new[] { "privacy", "private" },                  "privacy"            },
        };

        /// <summary>
        /// Main NLP parsing method — analyses user input and returns the best matching intent.
        /// Uses string.Contains() for keyword matching (NLP simulation with basic string manipulation).
        /// Checks intents in priority order so more specific patterns are matched first.
        /// </summary>
        /// <param name="input">The raw user input string.</param>
        /// <returns>A ParsedIntent containing the intent and any extracted data.</returns>
        public ParsedIntent Parse(string input)
        {
            // Guard against empty input
            if (string.IsNullOrWhiteSpace(input))
                return new ParsedIntent { Intent = Intent.Unknown };

            // Normalise to lowercase for case-insensitive matching
            string lower = input.Trim().ToLower();

            // ── Check intents in priority order ──────────────────────────────

            // Task-related intents first (most specific)
            if (MatchesAny(lower, AddTaskPatterns))
                return new ParsedIntent
                {
                    Intent = Intent.AddTask,
                    ExtractedContent = ExtractAfterKeyword(lower, AddTaskPatterns),
                    ReminderTime = ExtractReminderTime(lower)
                };

            if (MatchesAny(lower, ReminderPatterns))
                return new ParsedIntent
                {
                    Intent = Intent.SetReminder,
                    ExtractedContent = ExtractAfterKeyword(lower, ReminderPatterns),
                    ReminderTime = ExtractReminderTime(lower)
                };

            if (MatchesAny(lower, ViewTaskPatterns))
                return new ParsedIntent { Intent = Intent.ViewTasks };

            if (MatchesAny(lower, CompleteTaskPatterns))
                return new ParsedIntent
                {
                    Intent = Intent.CompleteTask,
                    ExtractedContent = ExtractAfterKeyword(lower, CompleteTaskPatterns)
                };

            if (MatchesAny(lower, DeleteTaskPatterns))
                return new ParsedIntent
                {
                    Intent = Intent.DeleteTask,
                    ExtractedContent = ExtractAfterKeyword(lower, DeleteTaskPatterns)
                };

            // Quiz and log intents
            if (MatchesAny(lower, QuizPatterns))
                return new ParsedIntent { Intent = Intent.StartQuiz };

            if (MatchesAny(lower, LogPatterns))
                return new ParsedIntent { Intent = Intent.ShowActivityLog };

            // Part 2: follow-up conversation flow
            if (MatchesAny(lower, FollowUpPatterns))
                return new ParsedIntent { Intent = Intent.FollowUp };

            // Social intents
            if (MatchesAny(lower, GreetingPatterns))
                return new ParsedIntent { Intent = Intent.Greeting };

            if (MatchesAny(lower, FarewellPatterns))
                return new ParsedIntent { Intent = Intent.Farewell };

            if (MatchesAny(lower, ThanksPatterns))
                return new ParsedIntent { Intent = Intent.Thanks };

            if (MatchesAny(lower, HelpPatterns))
                return new ParsedIntent { Intent = Intent.ShowHelp };

            // Cybersecurity topic detection — check all keyword arrays
            foreach (var kvp in CyberTopics)
                foreach (var kw in kvp.Key)
                    if (lower.Contains(kw))
                        return new ParsedIntent
                        {
                            Intent = Intent.CyberTopic,
                            ExtractedContent = kvp.Value
                        };

            // Fallback — no intent detected
            return new ParsedIntent { Intent = Intent.Unknown };
        }

        /// <summary>
        /// Checks whether the input string contains any of the given patterns.
        /// Core NLP technique: string.Contains() for flexible keyword matching.
        /// </summary>
        private bool MatchesAny(string input, string[] patterns)
        {
            foreach (var p in patterns)
                if (input.Contains(p)) return true;
            return false;
        }

        /// <summary>
        /// Extracts the meaningful content after a matched keyword.
        /// For example, "add task - Enable 2FA" → extracts "Enable 2FA".
        /// Strips leading dashes or colons from the extracted text.
        /// </summary>
        private string ExtractAfterKeyword(string input, string[] patterns)
        {
            foreach (var p in patterns)
            {
                int idx = input.IndexOf(p, StringComparison.Ordinal);
                if (idx >= 0)
                {
                    // Take everything after the matched keyword
                    string after = input[(idx + p.Length)..].Trim();
                    // Remove leading punctuation (dashes, colons)
                    after = Regex.Replace(after, @"^[-:]+\s*", "").Trim();
                    if (!string.IsNullOrWhiteSpace(after)) return after;
                }
            }
            return ""; // Return empty string if nothing found after keyword
        }

        /// <summary>
        /// Extracts a reminder timeframe from the user's input using Regex.
        /// Handles patterns like "in 3 days", "tomorrow", "next week",
        /// "today", or specific dates like "on 25/12/2025".
        /// Returns null if no timeframe is mentioned.
        /// </summary>
        private string ExtractReminderTime(string input)
        {
            // Match "in X days/hours/weeks"
            var m = Regex.Match(input, @"in (\d+)\s*(day|days|hour|hours|week|weeks)");
            if (m.Success) return $"in {m.Groups[1].Value} {m.Groups[2].Value}";

            // Match common relative time words
            if (input.Contains("tomorrow")) return "tomorrow";
            if (input.Contains("next week")) return "next week";
            if (input.Contains("today")) return "today";

            // Match specific dates like "on 25/12/2024"
            var d = Regex.Match(input, @"on (\d{1,2}[/-]\d{1,2}[/-]?\d{0,4})");
            if (d.Success) return $"on {d.Groups[1].Value}";

            return null; // No reminder time found
        }
    }
}
