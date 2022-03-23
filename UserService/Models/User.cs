using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UserService.Models
{
    /// <summary>
    /// <b>User</b> class used for EF Core to map its landscape for the database
    /// </summary>
    public class User
    {
        /// <summary>
        /// [Key]: Identification number for a User entry in the User table
        /// [Required]: cannot be null
        /// Identification number
        /// </summary>
        [Key]
        [Required]
        public int Id { get; set; }

        [Required]
        public string Username { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTime? Created { get; set; }
    }
}
