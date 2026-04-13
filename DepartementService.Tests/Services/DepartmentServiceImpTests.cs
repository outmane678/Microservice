using DepartementService.Data;
using DepartementService.DTOs.Requests;
using DepartementService.Models;
using DepartementService.Services.Implementations;
using Microsoft.EntityFrameworkCore;

namespace DepartementService.Tests.Services;

public class DepartmentServiceImpTests : IDisposable
{
    private const string ConnStr =
        "Server=10.200.0.222,1433;Database=DepartmentDB;User Id=outmane.elkairi;Password=outmane.elkairi@;TrustServerCertificate=True;";

    private readonly AppDbContext _context;
    private readonly DepartmentServiceImp _sut;
    private readonly List<Guid> _createdIds = new();

    public DepartmentServiceImpTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(ConnStr)
            .Options;
        _context = new AppDbContext(options);
        _sut = new DepartmentServiceImp(_context);
    }

    public void Dispose()
    {
        foreach (var id in _createdIds)
        {
            var entity = _context.Departments.Find(id);
            if (entity != null)
                _context.Departments.Remove(entity);
        }
        _context.SaveChanges();
        _context.Dispose();
    }

    private Department Seed(string name = "Test")
    {
        var dept = new Department { Name = name };
        _context.Departments.Add(dept);
        _context.SaveChanges();
        _createdIds.Add(dept.Id);
        return dept;
    }

    // ======================== CreateDepartment ========================

    [Fact]
    public void CreateDepartment_Should_Persist_And_Return_Department()
    {
        // Arrange
        var dto = new DepartmentCreate { Name = "Test_Create" };

        // Act
        var result = _sut.CreateDepartment(dto);
        _createdIds.Add(result.Id);

        // Assert
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("Test_Create", result.Name);
        Assert.NotNull(_context.Departments.Find(result.Id));
    }

    // ======================== UpadateDepartment ========================

    [Fact]
    public void UpadateDepartment_Should_Update_Name_When_Valid_Id()
    {
        // Arrange
        var dept = Seed("Old");

        // Act
        var result = _sut.UpadateDepartment(dept.Id, new DepartmentUpdate { Name = "New" });

        // Assert
        Assert.Equal("New", result.Name);
    }

    [Fact]
    public void UpadateDepartment_Should_Throw_When_Invalid_Id()
    {
        // Arrange & Act & Assert
        var ex = Assert.Throws<Exception>(
            () => _sut.UpadateDepartment(Guid.NewGuid(), new DepartmentUpdate { Name = "X" }));

        Assert.Contains("n'a pas été trouvé", ex.Message);
    }

    // ======================== DeleteDepartment ========================

    [Fact]
    public void DeleteDepartment_Should_Remove_When_Valid_Id()
    {
        // Arrange
        var dept = Seed("ToDelete");
        var id = dept.Id;

        // Act
        _sut.DeleteDepartment(id);
        _createdIds.Remove(id);

        // Assert
        Assert.Null(_context.Departments.Find(id));
    }

    [Fact]
    public void DeleteDepartment_Should_Throw_When_Invalid_Id()
    {
        // Arrange & Act & Assert
        var ex = Assert.Throws<Exception>(() => _sut.DeleteDepartment(Guid.NewGuid()));

        Assert.Contains("n'a pas été trouvé", ex.Message);
    }

    // ======================== FindDepartmentById ========================

    [Fact]
    public void FindDepartmentById_Should_Return_Department_When_Exists()
    {
        // Arrange
        var dept = Seed("Find");

        // Act
        var result = _sut.FindDepartmentById(dept.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Find", result!.Name);
    }

    [Fact]
    public void FindDepartmentById_Should_Return_Null_When_Not_Exists()
    {
        // Arrange & Act
        var result = _sut.FindDepartmentById(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    // ======================== GetAllDepartments ========================

    [Fact]
    public void GetAllDepartments_Should_Return_List()
    {
        // Arrange
        Seed("All_A");
        Seed("All_B");

        // Act
        var result = _sut.GetAllDepartments();

        // Assert
        Assert.True(result.Count >= 2);
    }
}
