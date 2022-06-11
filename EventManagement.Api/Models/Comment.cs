using System;
using System.ComponentModel.DataAnnotations;

namespace EventManagement.Api.Models
{
    public class Comment
    {
        public int CommentId { get; set; }
        [Required]
        public string Text { get; set; }
        [Required]
        public string Author { get; set; }
        [Required]
        public int Rating { get; set; }
        [Required]
        public DateTime DatePosted { get; set; }
        [Required]
        public int EventId {get; set;}
    }
}