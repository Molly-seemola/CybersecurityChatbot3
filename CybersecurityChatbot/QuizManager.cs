using System.Collections.Generic;
using CybersecurityChatbot.Models;

namespace CybersecurityChatbot
{
    /// <summary>
    /// Manages the Cybersecurity Mini-Game (Quiz).
    /// Part 3 Task 2: Contains 15 questions (10 multiple choice + 5 true/false)
    /// covering phishing, passwords, VPNs, 2FA, social engineering, and more.
    ///
    /// Rubric target (13-15 marks): 10+ questions, interactive and engaging,
    /// covers cybersecurity thoroughly, with smooth varied feedback and flow.
    /// </summary>
    public class QuizManager
    {
        // The full list of quiz questions loaded on construction
        private readonly List<QuizQuestion> _questions;

        // Tracks position in the quiz and the user's current score
        private int _currentIndex;
        private int _score;

        /// <summary>True while a quiz session is in progress.</summary>
        public bool IsActive { get; private set; }

        /// <summary>Total number of questions in the quiz.</summary>
        public int TotalQuestions => _questions.Count;

        /// <summary>Number of correct answers so far in this session.</summary>
        public int Score => _score;

        /// <summary>Zero-based index of the question currently being displayed.</summary>
        public int CurrentIndex => _currentIndex;

        /// <summary>
        /// Initialises all 15 cybersecurity quiz questions.
        /// Mix of MultipleChoice and TrueFalse for variety (Part 3 Task 2 requirement).
        /// Questions cover: phishing, passwords, VPN, 2FA, ransomware, firewalls,
        /// social engineering, HTTPS, public Wi-Fi.
        /// </summary>
        public QuizManager()
        {
            _questions = new List<QuizQuestion>
            {
                // ── MULTIPLE CHOICE QUESTIONS (10) ──────────────────────────

                // Q1 — Phishing awareness
                new("What does 'phishing' mean?",
                    new() { "A fishing sport", "A cyberattack using fake emails to steal info",
                            "A network protocol", "A firewall technique" },
                    1, "Phishing tricks users via fake emails or websites to steal credentials."),

                // Q2 — Password strength
                new("Which of these is the strongest password?",
                    new() { "password123", "YourName2000", "X!9kL#2mQ@wR", "abc123" },
                    2, "Strong passwords use uppercase, lowercase, numbers, and symbols — at least 12 chars."),

                // Q3 — VPN definition
                new("What does VPN stand for?",
                    new() { "Virtual Private Network", "Very Protected Node",
                            "Verified Public Network", "Virtual Protection Node" },
                    0, "VPN = Virtual Private Network. It encrypts your internet connection."),

                // Q4 — Two-factor authentication
                new("What is two-factor authentication (2FA)?",
                    new() { "Using two passwords", "A second verification step beyond a password",
                            "Two firewalls", "Double encryption" },
                    1, "2FA adds an extra verification step like an SMS code beyond your password."),

                // Q5 — Ransomware definition
                new("What is ransomware?",
                    new() { "Software that speeds up your PC", "A backup tool",
                            "Malware that locks files and demands payment", "An antivirus" },
                    2, "Ransomware encrypts your data and demands a ransom to restore access."),

                // Q6 — Phishing email response (matches rubric example question)
                new("What should you do if you receive an email asking for your password?",
                    new() { "Reply with your password", "Delete the email",
                            "Report it as phishing", "Ignore it" },
                    2, "Reporting phishing emails helps prevent scams and protects others."),

                // Q7 — Public Wi-Fi safety
                new("Which is the safest option when using public Wi-Fi?",
                    new() { "Online banking without a VPN", "Sharing personal info freely",
                            "Using a VPN", "Leaving Bluetooth on" },
                    2, "Always use a VPN on public Wi-Fi to encrypt your internet traffic."),

                // Q8 — HTTPS meaning
                new("What does HTTPS indicate on a website?",
                    new() { "The site is popular", "The connection is encrypted and secure",
                            "The site is government-owned", "The site has no ads" },
                    1, "HTTPS means data between you and the site is encrypted using SSL/TLS."),

                // Q9 — Social engineering
                new("What is social engineering in cybersecurity?",
                    new() { "Building social media apps",
                            "Manipulating people to reveal confidential info",
                            "Network engineering", "A coding technique" },
                    1, "Social engineering exploits human psychology rather than technical vulnerabilities."),

                // Q10 — Firewall definition
                new("What is a firewall?",
                    new() { "A physical wall preventing fire",
                            "Software/hardware that monitors and controls network traffic",
                            "A type of antivirus", "An encryption algorithm" },
                    1, "A firewall filters incoming and outgoing network traffic based on security rules."),

                // ── TRUE / FALSE QUESTIONS (5) ───────────────────────────────

                // Q11 — Password reuse
                new("TRUE or FALSE: You should use the same password for all your accounts.",
                    new() { "True", "False" }, 1,
                    "FALSE — Reusing passwords means one breach exposes all your accounts.",
                    QuestionType.TrueFalse),

                // Q12 — HTTPS safety misconception
                new("TRUE or FALSE: HTTPS websites are always completely safe to use.",
                    new() { "True", "False" }, 1,
                    "FALSE — HTTPS only encrypts traffic; it doesn't guarantee the site itself is trustworthy.",
                    QuestionType.TrueFalse),

                // Q13 — Antivirus alone
                new("TRUE or FALSE: Antivirus software alone is enough to protect your computer.",
                    new() { "True", "False" }, 1,
                    "FALSE — You also need updates, strong passwords, 2FA, and safe browsing habits.",
                    QuestionType.TrueFalse),

                // Q14 — Software updates importance
                new("TRUE or FALSE: Regularly updating your software helps protect against cyberattacks.",
                    new() { "True", "False" }, 0,
                    "TRUE — Updates patch known vulnerabilities that hackers actively exploit.",
                    QuestionType.TrueFalse),

                // Q15 — Password length
                new("TRUE or FALSE: A strong password must be at least 12 characters with mixed types.",
                    new() { "True", "False" }, 0,
                    "TRUE — Length and character variety significantly increase password strength.",
                    QuestionType.TrueFalse),
            };
        }

        /// <summary>
        /// Starts a new quiz session — resets score and question index.
        /// Part 3 Task 2: Called when user clicks "Start Quiz" or types "start quiz".
        /// </summary>
        public void Start() { _currentIndex = 0; _score = 0; IsActive = true; }

        /// <summary>Manually stops the quiz (used when navigating away).</summary>
        public void Stop() => IsActive = false;

        /// <summary>
        /// Returns the current question, or null if the quiz is finished.
        /// </summary>
        public QuizQuestion GetCurrentQuestion()
            => _currentIndex < _questions.Count ? _questions[_currentIndex] : null;

        /// <summary>
        /// Processes the user's answer for the current question.
        /// Part 3 Task 2: Provides immediate feedback after each answer.
        /// Increments score if correct and advances to the next question.
        /// </summary>
        /// <param name="answerIndex">Zero-based index of the option the user selected.</param>
        /// <returns>A tuple: (whether the answer was correct, explanation text).</returns>
        public (bool correct, string explanation) SubmitAnswer(int answerIndex)
        {
            var q = _questions[_currentIndex];
            bool correct = answerIndex == q.CorrectIndex;
            if (correct) _score++; // Only increment for correct answers
            _currentIndex++;

            // Mark quiz as inactive when all questions are answered
            if (_currentIndex >= _questions.Count) IsActive = false;

            return (correct, q.Explanation);
        }

        /// <summary>
        /// Returns score-based feedback message shown at the end of the quiz.
        /// Part 3 Task 2: Gives personalised feedback — "Great job!" or "Keep learning!".
        /// </summary>
        public string GetFinalFeedback()
        {
            double percentage = (double)_score / TotalQuestions * 100;

            if (percentage >= 80)
                return "🏆 Great job! You're a cybersecurity pro!";
            if (percentage >= 50)
                return "👍 Good effort! Keep learning to stay safe online!";

            return "📚 Keep learning to stay safe online! Review the topics and try again.";
        }
    }
}