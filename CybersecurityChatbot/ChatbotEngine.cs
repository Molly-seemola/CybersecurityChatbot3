using System;
using System.Collections.Generic;
using CybersecurityChatbot.Models;

namespace CybersecurityChatbot
{
    public class ChatbotEngine
    {
        private string _userName = "";

        private readonly Dictionary<string, string> _cyberResponses = new()
        {
            ["password"] = "🔑 Password Tips:\n• Use at least 12 characters\n• Mix uppercase, lowercase, numbers & symbols\n• Never reuse passwords across sites\n• Use a password manager like Bitwarden",
            ["phishing"] = "🎣 Phishing:\nAttackers send fake emails pretending to be trusted sources to steal your info.\n\n✅ Tips:\n• Check sender email addresses carefully\n• Don't click suspicious links\n• Go directly to official websites when in doubt",
            ["malware"] = "🦠 Malware:\nMalicious software designed to harm your system.\n\n✅ Protection:\n• Keep antivirus updated\n• Don't download from untrusted sources\n• Regularly back up your data",
            ["vpn"] = "🔒 VPN:\nEncrypts your internet connection and hides your IP address.\n\n✅ Use a VPN:\n• On public Wi-Fi\n• To protect your privacy online",
            ["2fa"] = "🛡️ Two-Factor Authentication (2FA):\nAdds a second security layer beyond your password.\n\n✅ Enable 2FA on:\n• Email, banking, and social media accounts\n• Use an authenticator app (e.g. Google Authenticator)",
            ["firewall"] = "🧱 Firewall:\nMonitors network traffic and blocks unauthorized access.\n\n✅ Tips:\n• Always keep your OS firewall enabled\n• Use hardware firewalls for business networks",
            ["social engineering"] = "🧠 Social Engineering:\nAttackers manipulate people psychologically to reveal confidential info.\n\n✅ Stay safe:\n• Be skeptical of unsolicited requests\n• Verify identities before sharing any info\n• Never share passwords — even with 'IT support'",
            ["safe browsing"] = "🌐 Safe Browsing:\n• Look for HTTPS in the URL\n• Avoid clicking pop-up ads\n• Keep your browser updated",
            ["software updates"] = "🔄 Software Updates:\nUpdates patch known security vulnerabilities.\n\n✅ Tips:\n• Enable automatic updates\n• Don't ignore security patches",
            ["backup"] = "💾 Data Backups (3-2-1 Rule):\n• 3 copies of your data\n• 2 different storage types\n• 1 offsite or cloud backup",
            ["public wifi"] = "📶 Public Wi-Fi Safety:\n• Use a VPN on public networks\n• Avoid banking or sensitive logins\n• Turn off file sharing when done",
        };

        public void SetUserName(string name) => _userName = name;
        public string GetUserName() => _userName;

        public string GetCyberResponse(string topic)
            => _cyberResponses.TryGetValue(topic, out var r) ? r
               : "I can help with that! Type 'help' to see all available cybersecurity topics.";

        public string GetGreeting()
            => string.IsNullOrEmpty(_userName)
               ? "Hello! 👋 I'm CyberBot, your cybersecurity assistant. What's your name?"
               : $"Hello again, {_userName}! 👋 How can I help you stay safe online today?";

        public string GetHelpText() =>
            "💡 Things I can help you with:\n\n" +
            "📋 TASKS:\n• 'Add task - Enable 2FA'\n• 'Remind me to update password in 3 days'\n• 'Show my tasks'\n• 'Complete task 1' / 'Delete task 2'\n\n" +
            "📝 QUIZ:\n• 'Start quiz' or 'Quiz me'\n\n" +
            "📋 ACTIVITY LOG:\n• 'Show activity log' or 'What have you done for me?'\n\n" +
            "🔐 CYBER TOPICS:\n• Password, Phishing, Malware, VPN, 2FA,\n  Firewall, Social Engineering, Safe Browsing,\n  Software Updates, Backup, Public Wi-Fi";

        public string? GetSentimentResponse(string input)
        {
            string lower = input.ToLower();
            if (lower.Contains("worried") || lower.Contains("scared") || lower.Contains("anxious"))
                return "I understand your concern 😟. Small steps like strong passwords and 2FA go a long way! Type 'help' to see what I can assist with.";
            if (lower.Contains("frustrated") || lower.Contains("confused"))
                return "No worries! Try typing a specific topic like 'phishing' or 'password' and I'll explain it clearly. 😊";
            return null;
        }
    }
}
