using EmployeService.Controllers;
using EmployeService.DTOs.Requests;
using EmployeService.Models.Entities;
using EmployeService.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace EmployeService.Tests.Controllers;

public class EmployeeControllerTests
{
    private readonly Mock<IEmployeeService> _mockService;
    private readonly EmployeeController _controller;

    public EmployeeControllerTests()
    {
        _mockService = new Mock<IEmployeeService>();
        _controller = new EmployeeController(_mockService.Object);
    }

    private static Employee FakeEmployee(Guid? id = null) => new()
    {
        Id = id ?? Guid.NewGuid(),
        FirstName = "John", LastName = "Doe", Email = "john@mail.com",
        Position = "Dev", HireDate = DateTime.Today, DepartmentId = Guid.NewGuid()
    };

    // ======================== CreateEmployee ========================

    [Fact]
    public async Task CreateEmployee_Should_Return_Ok_With_Employee()
    {
        // Arrange
        var dto = new EmployeeCreate("John", "Doe", "john@mail.com", null, DateTime.Today, "Dev", Guid.NewGuid());
        var expected = FakeEmployee();
        _mockService.Setup(s => s.CreateEmployee(dto)).ReturnsAsync(expected);

        // Act
        var result = await _controller.CreateEmployee(dto);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        var employee = Assert.IsType<Employee>(ok.Value);
        Assert.Equal("John", employee.FirstName);
        _mockService.Verify(s => s.CreateEmployee(dto), Times.Once);
    }

    [Fact]
    public async Task CreateEmployee_Should_Return_BadRequest_When_Exception()
    {
        // Arrange
        var dto = new EmployeeCreate("A", "B", "a@b.com", null, DateTime.Today, "Dev", Guid.NewGuid());
        _mockService.Setup(s => s.CreateEmployee(dto)).ThrowsAsync(new Exception("Département introuvable"));

        // Act
        var result = await _controller.CreateEmployee(dto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    // ======================== GetAllEmployees ========================

    [Fact]
    public async Task GetAllEmployees_Should_Return_Ok_With_List()
    {
        // Arrange
        var list = new List<Employee> { FakeEmployee(), FakeEmployee() };
        _mockService.Setup(s => s.GetAllEmployees()).ReturnsAsync(list);

        // Act
        var result = await _controller.GetAllEmployees();

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        var employees = Assert.IsType<List<Employee>>(ok.Value);
        Assert.Equal(2, employees.Count);
    }

    // ======================== GetEmployeeById ========================

    [Fact]
    public async Task GetEmployeeById_Should_Return_Ok_When_Found()
    {
        // Arrange
        var id = Guid.NewGuid();
        var expected = FakeEmployee(id);
        _mockService.Setup(s => s.GetEmployeeById(id)).ReturnsAsync(expected);

        // Act
        var result = await _controller.GetEmployeeById(id);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        var employee = Assert.IsType<Employee>(ok.Value);
        Assert.Equal(id, employee.Id);
    }

    [Fact]
    public async Task GetEmployeeById_Should_Return_NotFound_When_Exception()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockService.Setup(s => s.GetEmployeeById(id)).ThrowsAsync(new Exception("Employé non trouvé"));

        // Act
        var result = await _controller.GetEmployeeById(id);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    // ======================== UpdateEmployee ========================

    [Fact]
    public async Task UpdateEmployee_Should_Return_Ok_When_Succeeds()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new EmployeeUpdate("Updated", "Name", "up@mail.com", null, DateTime.Today, "Lead");
        var expected = FakeEmployee(id);
        _mockService.Setup(s => s.UpdateEmployee(id, dto)).ReturnsAsync(expected);

        // Act
        var result = await _controller.UpdateEmployee(id, dto);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.IsType<Employee>(ok.Value);
    }

    [Fact]
    public async Task UpdateEmployee_Should_Return_NotFound_When_Exception()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new EmployeeUpdate("A", "B", "a@b.com", null, DateTime.Today, "Dev");
        _mockService.Setup(s => s.UpdateEmployee(id, dto)).ThrowsAsync(new Exception("Employé non trouvé"));

        // Act
        var result = await _controller.UpdateEmployee(id, dto);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    // ======================== DeleteEmployee ========================

    [Fact]
    public async Task DeleteEmployee_Should_Return_NoContent_When_Succeeds()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockService.Setup(s => s.DeleteEmployee(id)).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.DeleteEmployee(id);

        // Assert
        Assert.IsType<NoContentResult>(result);
        _mockService.Verify(s => s.DeleteEmployee(id), Times.Once);
    }

    [Fact]
    public async Task DeleteEmployee_Should_Return_NotFound_When_Exception()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockService.Setup(s => s.DeleteEmployee(id)).ThrowsAsync(new Exception("Employé non trouvé"));

        // Act
        var result = await _controller.DeleteEmployee(id);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    // ======================== GetEmployeeByToken ========================

    [Fact]
    public async Task GetEmployeeByToken_Should_Return_Ok_When_Found()
    {
        // Arrange
        var expected = FakeEmployee();
        _mockService.Setup(s => s.GetEmployeeByTokenAsync("valid-token")).ReturnsAsync(expected);

        // Act
        var result = await _controller.GetEmployeeByToken("valid-token");

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.IsType<Employee>(ok.Value);
    }

    [Fact]
    public async Task GetEmployeeByToken_Should_Return_NotFound_When_Invalid()
    {
        // Arrange
        _mockService.Setup(s => s.GetEmployeeByTokenAsync("bad")).ThrowsAsync(new Exception("Token invalide"));

        // Act
        var result = await _controller.GetEmployeeByToken("bad");

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    // ======================== VerifyEmployee ========================

    [Fact]
    public async Task VerifyEmployee_Should_Return_NoContent_When_Succeeds()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockService.Setup(s => s.VerifyEmployeeAsync(id)).Returns(Task.CompletedTask);

        // Act
        var result = await _controller.VerifyEmployee(id);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task VerifyEmployee_Should_Return_NotFound_When_Exception()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockService.Setup(s => s.VerifyEmployeeAsync(id)).ThrowsAsync(new Exception("Employé non trouvé"));

        // Act
        var result = await _controller.VerifyEmployee(id);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }
}
