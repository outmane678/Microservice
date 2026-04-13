using EmployeService.Client;
using EmployeService.Data;
using EmployeService.DTOs.Requests;
using EmployeService.Models.Entities;
using EmployeService.Services.Implementations;
using EmployeService.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace EmployeService.Tests.Services;

public class EmployeeServiceImpTests : IDisposable
{
    private const string ConnStr =
        "Server=10.200.0.222,1433;Database=EmployeeDB;User Id=outmane.elkairi;Password=outmane.elkairi@;TrustServerCertificate=True;";

    private readonly AppDbContext _context;
    private readonly Mock<IDepartmentAPI> _api;
    private readonly Mock<IEmailSender> _email;
    private readonly EmployeeServiceImp _sut;
    private readonly List<Guid> _createdIds = new();

    public EmployeeServiceImpTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(ConnStr)
            .Options;
        _context = new AppDbContext(options);
        _api = new Mock<IDepartmentAPI>();
        _email = new Mock<IEmailSender>();
        _sut = new EmployeeServiceImp(_context, _api.Object, _email.Object);
    }

    public void Dispose()
    {
        foreach (var id in _createdIds)
        {
            var entity = _context.Employees.Find(id);
            if (entity != null)
                _context.Employees.Remove(entity);
        }
        _context.SaveChanges();
        _context.Dispose();
    }

    private Employee Seed(string firstName = "Test")
    {
        var emp = new Employee
        {
            FirstName = firstName, LastName = "Unit", Email = $"{Guid.NewGuid():N}@test.com",
            Position = "Dev", HireDate = DateTime.Today, DepartmentId = Guid.NewGuid()
        };
        _context.Employees.Add(emp);
        _context.SaveChanges();
        _createdIds.Add(emp.Id);
        return emp;
    }

    // ======================== CreateEmployee ========================

    [Fact]
    public async Task CreateEmployee_Should_Persist_And_Send_Email()
    {
        // Arrange
        var deptId = Guid.NewGuid();
        _api.Setup(a => a.GetDepartmentById(deptId))
            .ReturnsAsync(new DepartmentDto { Id = deptId, Name = "IT" });
        var dto = new EmployeeCreate("John", "Doe", "john_test@unit.com", null, DateTime.Today, "Dev", deptId);

        // Act
        var result = await _sut.CreateEmployee(dto);
        _createdIds.Add(result.Id);

        // Assert
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("John", result.FirstName);
        Assert.False(result.IsVerified);
        Assert.NotNull(result.AccountCreationToken);
        _email.Verify(e => e.SendAccountCreationEmail("john_test@unit.com", It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task CreateEmployee_Should_Throw_When_Department_Not_Found()
    {
        // Arrange
        var deptId = Guid.NewGuid();
        _api.Setup(a => a.GetDepartmentById(deptId)).ThrowsAsync(new Exception("Not found"));

        // Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(
            () => _sut.CreateEmployee(new EmployeeCreate("A", "B", "a@b.com", null, DateTime.Today, "Dev", deptId)));

        Assert.Contains("Erreur lors de la vérification du département", ex.Message);
    }

    // ======================== GetEmployeeById ========================

    [Fact]
    public async Task GetEmployeeById_Should_Return_Employee_When_Exists()
    {
        // Arrange
        var emp = Seed();

        // Act
        var result = await _sut.GetEmployeeById(emp.Id);

        // Assert
        Assert.Equal(emp.Id, result.Id);
    }

    [Fact]
    public async Task GetEmployeeById_Should_Throw_When_Not_Found()
    {
        // Arrange & Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _sut.GetEmployeeById(Guid.NewGuid()));
    }

    // ======================== GetAllEmployees ========================

    [Fact]
    public async Task GetAllEmployees_Should_Return_List()
    {
        // Arrange
        Seed("A");
        Seed("B");

        // Act
        var result = await _sut.GetAllEmployees();

        // Assert
        Assert.True(result.Count >= 2);
    }

    // ======================== UpdateEmployee ========================

    [Fact]
    public async Task UpdateEmployee_Should_Modify_Fields()
    {
        // Arrange
        var emp = Seed();
        var dto = new EmployeeUpdate("Updated", "Name", "up@unit.com", null, DateTime.Today, "Lead");

        // Act
        var result = await _sut.UpdateEmployee(emp.Id, dto);

        // Assert
        Assert.Equal("Updated", result.FirstName);
        Assert.Equal("Lead", result.Position);
    }

    [Fact]
    public async Task UpdateEmployee_Should_Throw_When_Not_Found()
    {
        // Arrange & Act & Assert
        await Assert.ThrowsAsync<Exception>(
            () => _sut.UpdateEmployee(Guid.NewGuid(), new EmployeeUpdate("A", "B", "a@b.com", null, DateTime.Today, "Dev")));
    }

    // ======================== DeleteEmployee ========================

    [Fact]
    public async Task DeleteEmployee_Should_Remove_Entity()
    {
        // Arrange
        var emp = Seed();
        var id = emp.Id;

        // Act
        await _sut.DeleteEmployee(id);
        _createdIds.Remove(id);

        // Assert
        Assert.Null(await _context.Employees.FindAsync(id));
    }

    [Fact]
    public async Task DeleteEmployee_Should_Throw_When_Not_Found()
    {
        // Arrange & Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _sut.DeleteEmployee(Guid.NewGuid()));
    }

    // ======================== GetEmployeeByTokenAsync ========================

    [Fact]
    public async Task GetEmployeeByTokenAsync_Should_Return_Employee()
    {
        // Arrange
        var token = "test_tok_" + Guid.NewGuid().ToString()[..8];
        var emp = new Employee
        {
            FirstName = "Token", LastName = "Test", Email = "token@unit.com",
            Position = "Dev", HireDate = DateTime.Today,
            DepartmentId = Guid.NewGuid(), AccountCreationToken = token
        };
        _context.Employees.Add(emp);
        await _context.SaveChangesAsync();
        _createdIds.Add(emp.Id);

        // Act
        var result = await _sut.GetEmployeeByTokenAsync(token);

        // Assert
        Assert.Equal(emp.Id, result.Id);
    }

    [Fact]
    public async Task GetEmployeeByTokenAsync_Should_Throw_For_Invalid_Token()
    {
        // Arrange & Act & Assert
        var ex = await Assert.ThrowsAsync<Exception>(() => _sut.GetEmployeeByTokenAsync("bad_token"));
        Assert.Contains("Token invalide", ex.Message);
    }

    // ======================== VerifyEmployeeAsync ========================

    [Fact]
    public async Task VerifyEmployeeAsync_Should_Set_Verified_And_Clear_Token()
    {
        // Arrange
        var emp = new Employee
        {
            FirstName = "Verify", LastName = "Test", Email = "verify@unit.com",
            Position = "Dev", HireDate = DateTime.Today,
            DepartmentId = Guid.NewGuid(), AccountCreationToken = "vtok", IsVerified = false
        };
        _context.Employees.Add(emp);
        await _context.SaveChangesAsync();
        _createdIds.Add(emp.Id);

        // Act
        await _sut.VerifyEmployeeAsync(emp.Id);

        // Assert
        var reloaded = await _context.Employees.FindAsync(emp.Id);
        Assert.True(reloaded!.IsVerified);
        Assert.Null(reloaded.AccountCreationToken);
    }

    [Fact]
    public async Task VerifyEmployeeAsync_Should_Throw_When_Not_Found()
    {
        // Arrange & Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _sut.VerifyEmployeeAsync(Guid.NewGuid()));
    }
}
