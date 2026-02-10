using System.ComponentModel.DataAnnotations;
namespace AuthService.Domain.Entities;

    public class Role
    {

      [Key]
      [MaxLength(16)]
     public string Id { get; set; } = string.Empty;
      [Required]
      [MaxLength(50)]
       public string Name { get; set; } = string.Empty;
      [Required]
      [MaxLength(255)]
         public string Description { get; set; } = string.Empty;

// Relación con UserRole
         public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    }
