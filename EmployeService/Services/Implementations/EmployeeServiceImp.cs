using EmployeService.Models;
using EmployeService.Services.Interfaces;
using EmployeService.DTOs.Requests;
using EmployeService.Data;
using EmployeService.Client;
using Microsoft.EntityFrameworkCore;
using EmployeService.Models.Entities;
namespace EmployeService.Services.Implementations;


public class EmployeeServiceImp : IEmployeeService 
{
    private readonly AppDbContext _context;
    private readonly IDepartmentAPI _departmentApi;
    private readonly IEmailSender _emailService;
    public EmployeeServiceImp(AppDbContext context, IDepartmentAPI departmentApi, IEmailSender emailService)
    {
        _context = context;
        _departmentApi = departmentApi;
        _emailService = emailService;
    }

    // créer un employé
    public async Task<Employee> CreateEmployee(EmployeeCreate employeeCreate)
    {
        DepartmentDto departmentDto;
        
        try{
            // Vérifier que le départment existe en appelant l'API des départments
            departmentDto = await _departmentApi.GetDepartmentById(employeeCreate.DepartmentId);
            if (departmentDto == null)
            {
                throw new Exception("Le département spécifié n'existe pas.");
            }
        }
        catch (Exception ex)
        {
            // Gérer les erreurs d'appel à l'API (ex : département non trouvé, problème de connexion, etc.)
            throw new Exception($"Erreur lors de la vérification du département : {ex.Message}");
        }

        // Générer token unique
        string token;
        do {
            token = Guid.NewGuid().ToString();
        } while (await _context.Employees.AnyAsync(e => e.AccountCreationToken == token));

        var employee = new Employee
        {
            
            FirstName = employeeCreate.FirstName,
            LastName = employeeCreate.LastName,
            Email = employeeCreate.Email,
            Phone = employeeCreate.Phone,
            HireDate = employeeCreate.HireDate,
            Position = employeeCreate.Position,
            AccountCreationToken = token,
            IsVerified = false,
            DepartmentId = departmentDto.Id
        };
        _context.Employees.Add(employee);
        await _context.SaveChangesAsync();
        _emailService.SendAccountCreationEmail(employee.Email, token);

        return employee;
    }

    // récupérer un employé par son ID
    public async Task<Employee> GetEmployeeById(Guid id)
    {
        var employee = await _context.Employees.FindAsync(id);
        if(employee == null)
        {
            throw new Exception("Employé non trouvé");
        }
        return employee;
    }

    // récupérer tous les employés
    public async Task<List<Employee>> GetAllEmployees()
    {
        List<Employee> employees = await _context.Employees.ToListAsync();
        return employees;
    }

    // mettre à jour un employé
    public async Task<Employee> UpdateEmployee(Guid id, EmployeeUpdate employeeUpdate)
    {
        var employee = await _context.Employees.FindAsync(id);
        if(employee == null)
        {
            throw new Exception("Employé non trouvé");
        }
        employee.FirstName = employeeUpdate.FirstName;
        employee.LastName = employeeUpdate.LastName;
        employee.Email = employeeUpdate.Email;
        employee.Phone = employeeUpdate.Phone;
        employee.HireDate = employeeUpdate.HireDate;
        employee.Position = employeeUpdate.Position;
        _context.Employees.Update(employee);
        await _context.SaveChangesAsync();
        return employee;
    }

    // supprimer un employé par son ID
    public async Task DeleteEmployee(Guid id)
    {
        var employee = await _context.Employees.FindAsync(id);
        if(employee == null)
        {
            throw new Exception("Employé non trouvé");
        }
        _context.Employees.Remove(employee);
        await _context.SaveChangesAsync();
        
    }

    // =======================
    // GET BY TOKEN
    // =======================

     public async Task<Employee> GetEmployeeByTokenAsync(string token)
    {
        var employee = await _context.Employees
            .FirstOrDefaultAsync(e => e.AccountCreationToken == token);
        
        if (employee == null)
            throw new Exception("Token invalide ou expiré");
        
        return employee;
    }

    // =======================
    // VERIFY
    // =======================
    public async Task VerifyEmployeeAsync(Guid id)
    {
        var employee = await _context.Employees.FindAsync(id);
        if (employee == null)
            throw new Exception("Employé non trouvé");

        employee.IsVerified = true;
        employee.AccountCreationToken = null;
        //  PAS de VerifiedAt
        
        await _context.SaveChangesAsync();
    }
}