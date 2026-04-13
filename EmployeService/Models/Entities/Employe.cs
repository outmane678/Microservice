using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmployeService.Models.Entities;

[Table("Employees")]
public class Employee
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    [Column("Id")]
    public Guid Id { get; set; }
    [Required]
    [Column("FirstName")]
    [StringLength(100)]
    public string FirstName { get; set; } = string.Empty;
    [Required]
    [Column("LastName")]
    [StringLength(100)]
    public string LastName { get; set; } = string.Empty;
    [Required]
    [Column("Email")]
    [StringLength(255)]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    [Column("Phone")]
    [StringLength(20)]
    public string? Phone { get; set; }
    [Required]
    [Column("HireDate")]
    public DateTime HireDate { get; set; }
    [Column("Position")]
    [Required]
    [StringLength(20)]
    public string Position { get; set; } = string.Empty;

    [Column("IsVerified")]
    public bool IsVerified { get; set; } = false;

    [Column("AccountCreationToken")]
    public string? AccountCreationToken { get; set; }
    [Required]
    [Column("DepartmentId")]
    public Guid DepartmentId { get; set; }
}
