using System;
using EmployeService.Models.Entities;
using EmployeService.DTOs.Requests;
namespace EmployeService.Services.Interfaces;
public interface IEmployeeService
{
    Task<Employee> CreateEmployee(EmployeeCreate employeeCreate);
    Task<Employee> GetEmployeeById(Guid id);
    Task<List<Employee>> GetAllEmployees();
    Task<Employee> UpdateEmployee(Guid id, EmployeeUpdate employeeUpdate);
    Task DeleteEmployee(Guid id);
    Task<Employee> GetEmployeeByTokenAsync(string token);
    Task VerifyEmployeeAsync(Guid id);
}