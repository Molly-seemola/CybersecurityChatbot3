using System;

namespace CybersecurityChatbot
{
    public class ChatMessage
    {
        public string Sender { get; set; } 
        public string Message { get; set; } 
        public string Timestamp { get; set; } 

        public ChatMessage(string sender, string message)
        {
            Sender = sender;
            Message = message;
            Timestamp = DateTime.Now.ToString("HH:mm");
        }
    }
}