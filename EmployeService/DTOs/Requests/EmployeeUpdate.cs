using System.ComponentModel.DataAnnotations;
namespace EmployeService.DTOs.Requests;
public record EmployeeUpdate
(
    [Required(ErrorMessage = "Le prénom est obligatoire")]
    [StringLength(100, MinimumLength = 2)]
    string FirstName,
    [Required]
    [StringLength(100)]
    string LastName,
    [Required]
    [EmailAddress]
    string Email,
    [Phone]
    string? Phone,
    [Required]
    DateTime HireDate,
    [Required]
    string Position
);
