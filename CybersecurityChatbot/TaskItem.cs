using System;

namespace CybersecurityChatbot.Models
{
    /// <summary>
    /// Represents a single cybersecurity task created by the user.
    /// Each task includes a title, description, completion status,
    /// an optional reminder, and a date added.
    /// Part 3 Task 1: Tasks are stored and retrieved from the MySQL database.
    /// </summary>
    public class TaskItem
    {
        /// <summary>Auto-incremented primary key from the MySQL database.</summary>
        public int Id { get; set; }

        /// <summary>Short title of the task (e.g. "Enable 2FA").</summary>
        public string Title { get; set; }

        /// <summary>Longer description explaining the task in more detail.</summary>
        public string Description { get; set; }

        /// <summary>
        /// Whether the task has been marked as completed.
        /// Default is false — updated via CRUD in DatabaseHelper.
        /// </summary>
        public bool IsCompleted { get; set; }

        /// <summary>
        /// Human-readable reminder text (e.g. "in 3 days", "tomorrow").
        /// Part 3 Task 1: Optional reminder stored alongside the task in the DB.
        /// </summary>
        public string ReminderInfo { get; set; }

        /// <summary>Parsed DateTime of the reminder if a specific date was provided.</summary>
        public DateTime? ReminderDate { get; set; }

        /// <summary>Date and time the task was originally created.</summary>
        public string DateAdded { get; set; }

        /// <summary>Parameterless constructor needed for database mapping.</summary>
        public TaskItem() { }

        /// <summary>
        /// Creates a new TaskItem with default values.
        /// IsCompleted defaults to false; DateAdded is set to the current time.
        /// </summary>
        public TaskItem(string title, string description,
                        string reminderInfo = null, DateTime? reminderDate = null)
        {
            Title = title;
            Description = description;
            IsCompleted = false;
            ReminderInfo = reminderInfo;
            ReminderDate = reminderDate;
            DateAdded = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
        }

        /// <summary>
        /// Returns a formatted string representation of the task
        /// for display in the TaskListView and chat summary.
        /// </summary>
        public override string ToString()
        {
            string status = IsCompleted ? "✔ Done" : "⏳ Pending";
            string reminder = string.IsNullOrEmpty(ReminderInfo) ? "" : $"  ⏰ {ReminderInfo}";
            return $"[{status}]  {Title}{reminder}";
        }
    }
}