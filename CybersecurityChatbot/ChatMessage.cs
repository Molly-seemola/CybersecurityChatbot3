using System;

namespace CybersecurityChatbot.Models
{
    /// <summary>
    /// Represents a single chat message in the conversation.
    /// Stores the sender (User or Bot), message content, and timestamp.
    /// Part 2: Used for conversation flow and memory tracking.
    /// </summary>
    public class ChatMessage
    {
        /// <summary>The sender of the message — either "User" or "Bot".</summary>
        public string Sender { get; set; }

        /// <summary>The text content of the message.</summary>
        public string Message { get; set; }

        /// <summary>The time the message was sent, formatted as HH:mm.</summary>
        public string Timestamp { get; set; }

        /// <summary>
        /// Creates a new ChatMessage with an automatic timestamp.
        /// </summary>
        /// <param name="sender">Who sent the message ("User" or "Bot").</param>
        /// <param name="message">The message content.</param>
        public ChatMessage(string sender, string message)
        {
            Sender = sender;
            Message = message;
            Timestamp = DateTime.Now.ToString("HH:mm");
        }
    }
}