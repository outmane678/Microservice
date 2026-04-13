using System.ComponentModel.DataAnnotations;
namespace DepartementService.DTOs.Requests;
public class DepartmentCreate
{
     [Required, StringLength(100)]
    public string Name { get; set; } = null!;
}