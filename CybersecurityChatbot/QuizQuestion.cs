using System.Collections.Generic;

namespace CybersecurityChatbot.Models
{
    public enum QuestionType { MultipleChoice, TrueFalse }

    public class QuizQuestion
    {
        public string Question { get; set; }
        public List<string> Options { get; set; }
        public int CorrectIndex { get; set; }
        public string Explanation { get; set; }
        public QuestionType Type { get; set; }

        public QuizQuestion(string question, List<string> options, int correctIndex,
                            string explanation, QuestionType type = QuestionType.MultipleChoice)
        {
            Question = question;
            Options = options;
            CorrectIndex = correctIndex;
            Explanation = explanation;
            Type = type;
        }
    }
}
