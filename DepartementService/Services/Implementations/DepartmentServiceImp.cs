using DepartementService.Models;
using DepartementService.DTOs.Requests;
using DepartementService.Services.Interfaces;
using DepartementService.Data;
namespace DepartementService.Services.Implementations;
public class DepartmentServiceImp : DepartmentService
{
    private readonly AppDbContext _context;

    public DepartmentServiceImp(AppDbContext context)
    {
        _context = context;
    }

    // créer un département
    public Department CreateDepartment(DepartmentCreate departmentDto)
    {
        var department = new Department{
            Name = departmentDto.Name
        };
        _context.Departments.Add(department);
        _context.SaveChanges();
        return department;
    }

    // mettre à jour un département
    public Department UpadateDepartment(Guid id, DepartmentUpdate department)
    {
        // cherche le département dans la base
        var existingDepartment = _context.Departments.Find(id);
        if(existingDepartment == null)
        {
            throw new Exception($"Le département avec l'ID {id} n'a pas été trouvé !");
        }
        
            existingDepartment.Name = department.Name;
            _context.SaveChanges();
            return existingDepartment;
    }

    // supprimer un département
    public void DeleteDepartment(Guid id)
    {
        // Cherche le département dans la base
    var department = _context.Departments.Find(id);
        if (department != null)
            {
                _context.Departments.Remove(department);
                _context.SaveChanges();
            }
        else
        {
            throw new Exception($"Le département avec l'ID {id} n'a pas été trouvé !");
        }
    }

    // trouver un département par son id
    public Department? FindDepartmentById(Guid id)
    {
        return _context.Departments.Find(id);
    }

    // afficher tous les départements
    public List<Department> GetAllDepartments()
    {
     return _context.Departments.ToList();   
    }
}