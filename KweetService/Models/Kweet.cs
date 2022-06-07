using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KweetService.Models
{
    public class Kweet
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        /// <summary>
        /// If not null, its a comment on another kweet
        /// </summary>
        public int? KweetId { get; set; }
        [Required]
        public string Message { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime Created { get; set; }
    }
}
