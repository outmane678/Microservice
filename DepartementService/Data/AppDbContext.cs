using Microsoft.EntityFrameworkCore;
using DepartementService.Models;

namespace DepartementService.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Department> Departments { get; set; }
    }
}
