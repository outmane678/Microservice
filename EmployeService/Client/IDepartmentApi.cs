using EmployeService.DTOs.Requests;
using Microsoft.AspNetCore.Mvc;
using Refit;
namespace EmployeService.Client;

    public interface IDepartmentAPI
    {
        // Méthodes pour interagir avec l'API des départements
        [Get("/api/Departments/get-departement/{id}")]
        Task<DepartmentDto> GetDepartmentById(Guid id);
    }
