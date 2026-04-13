using EmployeService.Data;
using EmployeService.Models.Entities;
using Microsoft.EntityFrameworkCore;   
namespace EmployeService.Data;
public class AppDbContext : DbContext
{
     public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Employee> Employees { get; set; }
}