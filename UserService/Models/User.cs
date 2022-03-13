using System.ComponentModel.DataAnnotations;

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

     

    }
}
