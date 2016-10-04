using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Api.Models
{
    public class GuestbookEntryModel
    {
        [Key]
        public int Id { get; set; }

        public string Title { get; set; }

        public string Author { get; set; }

        public DateTime Timestamp { get; set; }

        public string Message { get; set; }
    }
}
