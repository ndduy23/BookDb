using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookDb.Models
{
    public class Bookmark
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int DocumentPageId { get; set; }

        public DocumentPage DocumentPage { get; set; }

        [Required]
        public string Url { get; set; }  // Lưu URL trực tiếp

        public string? Title { get; set; } // Tên tài liệu hoặc trang
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


    }
}
