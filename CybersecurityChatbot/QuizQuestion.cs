using System.Collections.Generic;

namespace CybersecurityChatbot.Models
{
    /// <summary>
    /// Defines the two types of quiz questions supported.
    /// Part 3 Task 2: Mix of MultipleChoice and TrueFalse for variety.
    /// </summary>
    public enum QuestionType
    {
        MultipleChoice, // Standard A/B/C/D question
        TrueFalse       // True or False question
    }

    /// <summary>
    /// Represents a single cybersecurity quiz question.
    /// Stores the question text, answer options, correct answer index,
    /// an explanation for feedback, and the question type.
    /// Part 3 Task 2: Used by QuizManager to run the cybersecurity mini-game.
    /// </summary>
    public class QuizQuestion
    {
        /// <summary>The question text displayed to the user.</summary>
        public string Question { get; set; }

        /// <summary>List of answer options (2 for True/False, 4 for Multiple Choice).</summary>
        public List<string> Options { get; set; }

        /// <summary>Zero-based index of the correct answer in the Options list.</summary>
        public int CorrectIndex { get; set; }

        /// <summary>
        /// Brief explanation shown after answering — reinforces cybersecurity concepts.
        /// Part 3 Task 2: Provides immediate feedback as required by the rubric.
        /// </summary>
        public string Explanation { get; set; }

        /// <summary>Whether this is a multiple choice or true/false question.</summary>
        public QuestionType Type { get; set; }

        /// <summary>
        /// Creates a new quiz question with all required properties.
        /// </summary>
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