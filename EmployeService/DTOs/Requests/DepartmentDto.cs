
using System.ComponentModel.DataAnnotations;

namespace EmployeService.DTOs.Requests;
public class DepartmentDto{
    [Required]
    public Guid Id { get; set;}
    [Required, StringLength(100), MinLength(2)]
    public string Name { get; set;} = string.Empty;
}
