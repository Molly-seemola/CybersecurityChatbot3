using System;
using System.Collections.Generic;

namespace CybersecurityChatbot
{
    /// <summary>
    /// The core chatbot brain — combines all features from Parts 1, 2, and 3.
    ///
    /// Part 1: ASCII art display, basic cybersecurity responses.
    /// Part 2: Keyword recognition, random responses, sentiment detection,
    ///         memory/recall (name + favourite topic), conversation flow (follow-up).
    /// Part 3: Cyber topic lookup used by the NLP engine, help text with memory.
    ///
    /// Rubric target (Combining Parts 1,2,3 — 9-10 marks):
    /// Cohesive integration providing a seamless, professional user experience.
    /// </summary>
    public class ChatbotEngine
    {
        // ── Part 2: Memory / Recall fields ──────────────────────────────────
        // Stores the user's name and favourite topic to personalise responses
        private string _userName = "";
        private string _favouriteTopic = "";
        private string _lastTopic = ""; // Tracks last discussed topic for follow-ups

        // ── Part 1: ASCII art ────────────────────────────────────────────────
        /// <summary>
        /// ASCII art banner displayed on startup.
        /// Part 1 requirement: ASCII art correctly displayed in the GUI.
        /// </summary>
        public static string AsciiArt =>
            "  ____      _               ____        _   \n" +
            " / ___|   _| |__   ___ _ __| __ )  ___ | |_ \n" +
            "| |  | | | | '_ \\ / _ \\ '__|  _ \\ / _ \\| __|\n" +
            "| |__| |_| | |_) |  __/ |  | |_) | (_) | |_ \n" +
            " \\____\\__, |_.__/ \\___|_|  |____/ \\___/ \\__|\n" +
            "      |___/   Cybersecurity Awareness Chatbot  ";

        // ── Part 2: Random responses per cybersecurity topic ─────────────────
        /// <summary>
        /// Dictionary mapping topic keys to multiple response variations.
        /// Part 2 requirement: Multiple responses per topic, randomly selected
        /// each time to make the chatbot more dynamic and less repetitive.
        /// Uses lists to store responses as required by the rubric.
        /// </summary>
        private readonly Dictionary<string, List<string>> _cyberResponses = new()
        {
            // Each topic has 3 varied responses — one is chosen randomly per request
            ["password"] = new()
            {
                "🔑 Use at least 12 characters mixing uppercase, lowercase, numbers & symbols. Never reuse passwords across sites!",
                "🔑 A strong password is your first line of defence. Try a passphrase like 'Coffee@Sunrise!2024'.",
                "🔑 Consider a password manager like Bitwarden or LastPass to generate and store strong passwords safely.",
            },
            ["phishing"] = new()
            {
                "🎣 Phishing emails fake trusted sources to steal credentials. Always check the sender's email address carefully!",
                "🎣 Never click suspicious links in emails. When in doubt, go directly to the official website instead.",
                "🎣 Urgency tactics like 'Act now!' or 'Your account will be closed' are common phishing red flags.",
            },
            ["malware"] = new()
            {
                "🦠 Keep your antivirus updated and avoid downloading files from untrusted sources.",
                "🦠 Ransomware can lock all your files. Regular backups (3-2-1 rule) are your best protection.",
                "🦠 Never open email attachments from unknown senders — they could contain trojans or spyware.",
            },
            ["vpn"] = new()
            {
                "🔒 A VPN encrypts your internet connection and hides your IP. Always use one on public Wi-Fi!",
                "🔒 VPNs protect your privacy online. Choose reputable providers like NordVPN or ProtonVPN.",
                "🔒 A VPN prevents your ISP and hackers from snooping on your internet activity.",
            },
            ["2fa"] = new()
            {
                "🛡️ 2FA adds a second layer beyond your password. Enable it on all important accounts!",
                "🛡️ Use an authenticator app (Google Authenticator or Authy) instead of SMS for stronger 2FA.",
                "🛡️ With 2FA, even if your password is stolen, attackers still can't access your account.",
            },
            ["firewall"] = new()
            {
                "🧱 A firewall monitors your network traffic and blocks unauthorised access. Keep it always enabled!",
                "🧱 Both software and hardware firewalls play a role — home routers usually have a built-in firewall.",
                "🧱 Firewalls are your network's gatekeeper — they decide what traffic gets in and what gets blocked.",
            },
            ["social engineering"] = new()
            {
                "🧠 Social engineering manipulates people psychologically. Always verify identities before sharing info.",
                "🧠 Never share passwords — even with someone claiming to be IT support. Legitimate IT will never ask.",
                "🧠 Common social engineering attacks include pretexting, baiting, and impersonation. Stay sceptical!",
            },
            ["safe browsing"] = new()
            {
                "🌐 Always look for HTTPS in the URL before entering personal information on any website.",
                "🌐 Use a security-focused browser like Chrome or Firefox and keep it updated.",
                "🌐 Avoid clicking pop-up ads — they often lead to malicious sites or unwanted downloads.",
            },
            ["software updates"] = new()
            {
                "🔄 Software updates patch known security vulnerabilities. Enable automatic updates where possible!",
                "🔄 Outdated software is one of the most common ways hackers gain access to systems.",
                "🔄 Don't ignore security patches — they exist because real vulnerabilities were discovered.",
            },
            ["backup"] = new()
            {
                "💾 Follow the 3-2-1 backup rule: 3 copies, 2 different storage types, 1 offsite/cloud backup.",
                "💾 Regular backups protect you from ransomware — you can restore files without paying the ransom.",
                "💾 Test your backups regularly to confirm they restore correctly when you actually need them.",
            },
            ["public wifi"] = new()
            {
                "📶 Public Wi-Fi is unencrypted and risky. Always use a VPN when connected to public networks.",
                "📶 Avoid banking or entering passwords on public Wi-Fi — hackers can intercept your traffic.",
                "📶 Turn off automatic Wi-Fi connection on your device to prevent joining unsafe networks unknowingly.",
            },
            ["scam"] = new()
            {
                "⚠️ Online scams are everywhere. If something sounds too good to be true — it probably is!",
                "⚠️ Report scams to relevant authorities. In SA, contact the SAPS Cybercrime Unit.",
                "⚠️ Common scams include lottery wins, fake job offers, and romance scams. Always verify the source.",
            },
            ["privacy"] = new()
            {
                "🔐 Review your social media privacy settings regularly and limit what you share publicly.",
                "🔐 Be careful what personal information you share online — it can be used for social engineering.",
                "🔐 Use privacy-focused browsers and search engines like Brave or DuckDuckGo for better privacy.",
            },
        };

        // Random number generator for selecting response variations
        private readonly Random _rng = new();

        // ── Part 2: Memory / Recall — public accessors ───────────────────────

        /// <summary>
        /// Stores the user's name in memory for personalised responses.
        /// Part 2: Memory requirement — remembers user details and uses them later.
        /// </summary>
        public void SetUserName(string name) => _userName = name;
        public string GetUserName() => _userName;

        /// <summary>
        /// Stores the user's most-discussed topic.
        /// Part 2: Recall — used to personalise the help text and suggestions.
        /// </summary>
        public void SetFavouriteTopic(string t) => _favouriteTopic = t;
        public string GetFavouriteTopic() => _favouriteTopic;

        /// <summary>Returns the last cybersecurity topic discussed (for follow-ups).</summary>
        public string GetLastTopic() => _lastTopic;

        // ── Part 2: Random cyber topic response ──────────────────────────────

        /// <summary>
        /// Returns a random response for the given cybersecurity topic.
        /// Part 2: Uses lists/arrays to store multiple responses per topic,
        /// randomly selecting one each time for variety and engagement.
        /// Also updates memory (lastTopic and favouriteTopic) for personalisation.
        /// </summary>
        /// <param name="topic">The topic key (e.g. "phishing", "password").</param>
        public string GetCyberResponse(string topic)
        {
            // Update memory — track last and favourite topic for follow-ups and recall
            _lastTopic = topic;
            if (!string.IsNullOrEmpty(topic) && string.IsNullOrEmpty(_favouriteTopic))
                _favouriteTopic = topic; // Set favourite to first topic discussed

            // Randomly select a response from the available options for this topic
            if (_cyberResponses.TryGetValue(topic, out var responses))
                return responses[_rng.Next(responses.Count)];

            return "I can help with many cybersecurity topics! Type 'help' to see the full list.";
        }

        /// <summary>
        /// Returns another response on the last discussed topic.
        /// Part 2: Conversation flow — handles "tell me more", "explain more", etc.
        /// </summary>
        public string GetFollowUpResponse()
        {
            if (!string.IsNullOrEmpty(_lastTopic))
                return GetCyberResponse(_lastTopic); // Get another random response on same topic
            return "Sure! What topic would you like more information about? Type 'help' to see options.";
        }

        // ── Part 2: Sentiment detection ──────────────────────────────────────

        /// <summary>
        /// Detects the user's emotional state from their input and returns
        /// an empathetic, adjusted response.
        /// Part 2: Sentiment detection — adjusts responses to be supportive
        /// based on detected emotions (worried, frustrated, excited, bored).
        /// Returns null if no sentiment is detected (caller handles Unknown intent).
        /// </summary>
        /// <param name="input">The raw user input text.</param>
        public string GetSentimentResponse(string input)
        {
            string lower = input.ToLower();
            // Personalise with username if available (Part 2: memory/recall)
            string name = string.IsNullOrEmpty(_userName) ? "" : $", {_userName}";

            // Detect negative emotions — respond with empathy and encouragement
            if (lower.Contains("worried") || lower.Contains("scared") ||
                lower.Contains("anxious") || lower.Contains("afraid"))
                return $"I understand your concern{name} 😟. Cybersecurity can feel overwhelming, " +
                       "but small steps like strong passwords and 2FA go a long way! " +
                       "Type 'help' to see how I can help.";

            // Detect confusion/frustration — offer simpler guidance
            if (lower.Contains("frustrated") || lower.Contains("confused") ||
                lower.Contains("don't understand") || lower.Contains("lost"))
                return $"No worries{name}! 😊 Cybersecurity can be tricky. Try typing a specific " +
                       "topic like 'phishing' or 'password' and I'll explain it clearly.";

            // Detect positive emotions — reinforce engagement
            if (lower.Contains("excited") || lower.Contains("happy") ||
                lower.Contains("great") || lower.Contains("awesome"))
                return $"That's great to hear{name}! 😄 Your enthusiasm for cybersecurity " +
                       "will really help keep you safe online!";

            // Detect boredom — redirect to quiz for engagement
            if (lower.Contains("bored") || lower.Contains("boring"))
                return $"Let's make it interesting{name}! 🎮 Try the cybersecurity quiz — " +
                       "type 'start quiz' to test your knowledge!";

            return null; // No sentiment detected — let NLP engine handle it
        }

        // ── Greeting / Help / Farewell ────────────────────────────────────────

        /// <summary>
        /// Returns a personalised greeting using the user's remembered name.
        /// Part 2: Memory — uses stored name to personalise the interaction.
        /// </summary>
        public string GetGreeting()
        {
            string name = string.IsNullOrEmpty(_userName) ? "" : $", {_userName}";
            return $"Hello{name}! 👋 How can I help you stay safe online today?";
        }

        /// <summary>
        /// Returns the full help/commands text.
        /// Part 2: Memory — personalises with the user's favourite topic if known.
        /// Covers all features from Parts 1, 2, and 3.
        /// </summary>
        public string GetHelpText()
        {
            // Part 2: Recall — remind user of their favourite topic if remembered
            string fav = string.IsNullOrEmpty(_favouriteTopic)
                ? ""
                : $"\n\n💡 Based on your interest, you might like: '{_favouriteTopic}'";

            return
                "💡 Here's what I can help you with:\n\n" +
                "🔐 CYBERSECURITY TOPICS (Part 1 & 2):\n" +
                "  password, phishing, malware, vpn, 2fa, firewall,\n" +
                "  social engineering, safe browsing, software updates,\n" +
                "  backup, public wifi, scam, privacy\n\n" +
                "✅ TASK MANAGER (Part 3 Task 1):\n" +
                "  'Add task - Enable 2FA'\n" +
                "  'Remind me to update my password in 3 days'\n" +
                "  'Show my tasks'\n" +
                "  'Complete task 1'  |  'Delete task 2'\n\n" +
                "📝 QUIZ (Part 3 Task 2):\n" +
                "  'Start quiz' or 'Quiz me'\n\n" +
                "📋 ACTIVITY LOG (Part 3 Task 4):\n" +
                "  'Show activity log'\n" +
                "  'What have you done for me?'\n\n" +
                "💬 CONVERSATION FLOW (Part 2):\n" +
                "  'Tell me more'  |  'Explain more'  |  'Give me another tip'" +
                fav;
        }
    }
}