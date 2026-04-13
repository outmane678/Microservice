using DepartementService.Models;
using DepartementService.DTOs.Requests;

namespace DepartementService.Services.Interfaces
{
    public interface DepartmentService
    {
        Department CreateDepartment(DepartmentCreate department);
        Department UpadateDepartment(Guid id, DepartmentUpdate department);
        void DeleteDepartment(Guid id);
        Department? FindDepartmentById(Guid id);
        List<Department> GetAllDepartments();
    }
}