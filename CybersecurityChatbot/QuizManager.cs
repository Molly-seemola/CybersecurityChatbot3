using System.Collections.Generic;
using CybersecurityChatbot.Models;

namespace CybersecurityChatbot
{
    public class QuizManager
    {
        private List<QuizQuestion> _questions;
        private int _currentIndex;
        private int _score;

        public bool IsActive { get; private set; }
        public int TotalQuestions => _questions.Count;
        public int Score => _score;
        public int CurrentIndex => _currentIndex;

        public QuizManager()
        {
            _questions = new List<QuizQuestion>
            {
                // Multiple Choice
                new QuizQuestion(
                    "What does 'phishing' mean?",
                    new List<string> { "A fishing sport", "A cyberattack using fake emails to steal info", "A network protocol", "A firewall technique" },
                    1, "Phishing tricks users via fake emails or websites to steal credentials."),

                new QuizQuestion(
                    "Which of these is the strongest password?",
                    new List<string> { "password123", "YourName2000", "X!9kL#2mQ@wR", "abc123" },
                    2, "Strong passwords use uppercase, lowercase, numbers, and symbols — at least 12 characters."),

                new QuizQuestion(
                    "What does VPN stand for?",
                    new List<string> { "Virtual Private Network", "Very Protected Node", "Verified Public Network", "Virtual Protection Node" },
                    0, "VPN = Virtual Private Network. It encrypts your internet connection."),

                new QuizQuestion(
                    "What is two-factor authentication (2FA)?",
                    new List<string> { "Using two passwords", "A second verification step beyond a password", "Two firewalls", "Double encryption" },
                    1, "2FA adds an extra verification step like an SMS code beyond your password."),

                new QuizQuestion(
                    "What is ransomware?",
                    new List<string> { "Software that speeds up your PC", "A backup tool", "Malware that locks files and demands payment", "An antivirus" },
                    2, "Ransomware encrypts your data and demands a ransom to restore access."),

                new QuizQuestion(
                    "What should you do if you receive an email asking for your password?",
                    new List<string> { "Reply with your password", "Delete the email", "Report it as phishing", "Ignore it" },
                    2, "Reporting phishing emails helps prevent scams and protects others."),

                new QuizQuestion(
                    "Which is safest on public Wi-Fi?",
                    new List<string> { "Online banking without VPN", "Sharing personal info freely", "Using a VPN", "Leaving Bluetooth on" },
                    2, "Always use a VPN on public Wi-Fi to encrypt your traffic."),

                new QuizQuestion(
                    "What does HTTPS indicate on a website?",
                    new List<string> { "The site is popular", "The connection is encrypted and secure", "The site is government-owned", "The site has no ads" },
                    1, "HTTPS means data between you and the site is encrypted using SSL/TLS."),

                new QuizQuestion(
                    "What is social engineering?",
                    new List<string> { "Building social media apps", "Manipulating people to reveal confidential info", "Network engineering", "A coding technique" },
                    1, "Social engineering exploits human psychology rather than technical vulnerabilities."),

                new QuizQuestion(
                    "What is a firewall?",
                    new List<string> { "A physical wall preventing fire", "Software/hardware that monitors network traffic", "A type of antivirus", "An encryption algorithm" },
                    1, "A firewall filters incoming and outgoing network traffic based on security rules."),

                // True / False
                new QuizQuestion(
                    "TRUE or FALSE: You should use the same password for all your accounts.",
                    new List<string> { "True", "False" },
                    1, "FALSE — Reusing passwords means one breach exposes all your accounts.",
                    QuestionType.TrueFalse),

                new QuizQuestion(
                    "TRUE or FALSE: HTTPS websites are always completely safe to use.",
                    new List<string> { "True", "False" },
                    1, "FALSE — HTTPS only encrypts traffic; it doesn't guarantee the site itself is trustworthy.",
                    QuestionType.TrueFalse),

                new QuizQuestion(
                    "TRUE or FALSE: Antivirus software alone is enough to protect your computer.",
                    new List<string> { "True", "False" },
                    1, "FALSE — You also need software updates, strong passwords, 2FA, and safe browsing habits.",
                    QuestionType.TrueFalse),

                new QuizQuestion(
                    "TRUE or FALSE: Regularly updating your software helps protect against cyberattacks.",
                    new List<string> { "True", "False" },
                    0, "TRUE — Updates patch known vulnerabilities that hackers exploit.",
                    QuestionType.TrueFalse),

                new QuizQuestion(
                    "TRUE or FALSE: A strong password must be at least 8 characters with mixed types.",
                    new List<string> { "True", "False" },
                    0, "TRUE — Length and character variety significantly increase password strength.",
                    QuestionType.TrueFalse),
            };
        }

        public void Start() { _currentIndex = 0; _score = 0; IsActive = true; }
        public void Stop() => IsActive = false;

        public QuizQuestion GetCurrentQuestion()
            => _currentIndex < _questions.Count ? _questions[_currentIndex] : null;

        public (bool correct, string explanation) SubmitAnswer(int answerIndex)
        {
            var q = _questions[_currentIndex];
            bool correct = answerIndex == q.CorrectIndex;
            if (correct) _score++;
            _currentIndex++;
            if (_currentIndex >= _questions.Count) IsActive = false;
            return (correct, q.Explanation);
        }

        public string GetFinalFeedback()
        {
            double pct = (double)_score / TotalQuestions * 100;
            if (pct >= 80) return "🏆 Great job! You're a cybersecurity pro!";
            if (pct >= 50) return "👍 Good effort! Keep learning to stay safe online!";
            return "📚 Keep learning to stay safe online! Review the topics and try again.";
        }
    }
}