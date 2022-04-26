using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserService.Models
{
    /// <summary>
    /// <b>User</b> class used for EF Core to map its landscape for the database
    /// </summary>
    public class User
    {
        [Key]
        [Required]
        public int Id { get; set; }
        [Required]
        public string Username { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime? Created { get; set; }

        public Guid? VerifyEmailToken { get; set; }

        [ForeignKey("RoleId")]
        public Role Role { get; set; }
    }
}
