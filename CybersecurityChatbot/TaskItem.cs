using System;

namespace CybersecurityChatbot.Models
{
    public class TaskItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public bool IsCompleted { get; set; }
        public string ReminderInfo { get; set; }
        public DateTime? ReminderDate { get; set; }
        public string DateAdded { get; set; }

        public TaskItem() { }

        public TaskItem(string title, string description, string reminderInfo = null, DateTime? reminderDate = null)
        {
            Title = title;
            Description = description;
            IsCompleted = false;
            ReminderInfo = reminderInfo;
            ReminderDate = reminderDate;
            DateAdded = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
        }

        public override string ToString()
        {
            string status = IsCompleted ? "✔ Done" : "⏳ Pending";
            string reminder = string.IsNullOrEmpty(ReminderInfo) ? "" : $" | ⏰ {ReminderInfo}";
            return $"[{status}] {Title}{reminder}";
        }
    }
}