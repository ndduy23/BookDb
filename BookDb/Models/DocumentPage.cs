using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BookDb.Models
{
    public class DocumentPage
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Document")]
        public int DocumentId { get; set; }
        public Document? Document { get; set; }

        public int PageNumber { get; set; }

        // Nội dung text (tùy chọn)
        public string? TextContent { get; set; }

        // File vật lý chứa nội dung (ảnh/pdf) – mỗi trang một file
        public string? FilePath { get; set; }

        // Loại nội dung (image/png, application/pdf…)
        [MaxLength(100)]
        public string? ContentType { get; set; }
        public Bookmark? Bookmark { get; set; }

    }
}
