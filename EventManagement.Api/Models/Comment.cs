using System;

namespace EventManagement.Api.Models
{
    public class Comment
    {
        public string Text { get; set; }
        public string Author { get; set; }
        public int Rating { get; set; }
        public DateTime DatePosted { get; set; }
    }
}