using DepartementService.Controllers;
using DepartementService.DTOs.Requests;
using DepartementService.Models;
using DepartementService.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace DepartementService.Tests.Controllers;

public class DepartmentsControllerTests
{
    private readonly Mock<DepartmentService> _mockService;
    private readonly DepartmentsController _controller;

    public DepartmentsControllerTests()
    {
        _mockService = new Mock<DepartmentService>();
        _controller = new DepartmentsController(_mockService.Object);
    }

    // ======================== CreateDepartment ========================

    [Fact]
    public void CreateDepartment_Should_Return_Ok_With_Created_Department()
    {
        // Arrange
        var dto = new DepartmentCreate { Name = "Finance" };
        var expected = new Department { Id = Guid.NewGuid(), Name = "Finance" };
        _mockService.Setup(s => s.CreateDepartment(dto)).Returns(expected);

        // Act
        var result = _controller.CreateDepartment(dto);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var department = Assert.IsType<Department>(ok.Value);
        Assert.NotNull(department);
        Assert.Equal(expected.Id, department.Id);
        Assert.Equal(expected.Name, department.Name);
        _mockService.Verify(s => s.CreateDepartment(dto), Times.Once);
    }

    [Fact]
    public void CreateDepartment_Should_Throw_When_Service_Fails()
    {
        // Arrange
        var dto = new DepartmentCreate { Name = "X" };
        _mockService.Setup(s => s.CreateDepartment(dto)).Throws(new Exception("Erreur"));

        // Act & Assert
        Assert.Throws<Exception>(() => _controller.CreateDepartment(dto));
        _mockService.Verify(s => s.CreateDepartment(dto), Times.Once);
    }

    // ======================== GetDepartmentById ========================

    [Fact]
    public void GetDepartmentById_Should_Return_Ok_When_Department_Exists()
    {
        // Arrange
        var id = Guid.NewGuid();
        var expected = new Department { Id = id, Name = "IT" };
        _mockService.Setup(s => s.FindDepartmentById(id)).Returns(expected);

        // Act
        var result = _controller.GetDepartmentById(id);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var department = Assert.IsType<Department>(ok.Value);
        Assert.NotNull(department);
        Assert.Equal(expected.Id, department.Id);
        Assert.Equal(expected.Name, department.Name);
        _mockService.Verify(s => s.FindDepartmentById(id), Times.Once);
    }

    [Fact]
    public void GetDepartmentById_Should_Return_NotFound_When_Department_Is_Null()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockService.Setup(s => s.FindDepartmentById(id)).Returns((Department?)null);

        // Act
        var result = _controller.GetDepartmentById(id);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
        _mockService.Verify(s => s.FindDepartmentById(id), Times.Once);
    }

    // ======================== GetAllDepartments ========================

    [Fact]
    public void GetAllDepartments_Should_Return_Ok_With_List()
    {
        // Arrange
        var departments = new List<Department>
        {
            new() { Id = Guid.NewGuid(), Name = "RH" },
            new() { Id = Guid.NewGuid(), Name = "IT" }
        };
        _mockService.Setup(s => s.GetAllDepartments()).Returns(departments);

        // Act
        var result = _controller.GetAllDepartments();

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result);
        var list = Assert.IsType<List<Department>>(ok.Value);
        Assert.NotNull(list);
        Assert.Equal(2, list.Count);
        _mockService.Verify(s => s.GetAllDepartments(), Times.Once);
    }

    // ======================== UpdateDepartment ========================

    [Fact]
    public void UpdateDepartment_Should_Return_Ok_When_Update_Succeeds()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new DepartmentUpdate { Name = "Updated" };
        var expected = new Department { Id = id, Name = "Updated" };
        _mockService.Setup(s => s.UpadateDepartment(id, dto)).Returns(expected);

        // Act
        var result = _controller.UpdateDepartment(id, dto);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var department = Assert.IsType<Department>(ok.Value);
        Assert.NotNull(department);
        Assert.Equal(expected.Name, department.Name);
        _mockService.Verify(s => s.UpadateDepartment(id, dto), Times.Once);
    }

    [Fact]
    public void UpdateDepartment_Should_Return_NotFound_When_Exception_Is_Thrown()
    {
        // Arrange
        var id = Guid.NewGuid();
        var dto = new DepartmentUpdate { Name = "X" };
        _mockService.Setup(s => s.UpadateDepartment(id, dto))
                    .Throws(new Exception("Département non trouvé"));

        // Act
        var result = _controller.UpdateDepartment(id, dto);

        // Assert
        var notFound = Assert.IsType<NotFoundObjectResult>(result.Result);
        Assert.NotNull(notFound.Value);
        Assert.Contains("non trouvé", notFound.Value.ToString());
        _mockService.Verify(s => s.UpadateDepartment(id, dto), Times.Once);
    }

    // ======================== DeleteDepartment ========================

    [Fact]
    public void DeleteDepartment_Should_Return_NoContent_When_Delete_Succeeds()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockService.Setup(s => s.DeleteDepartment(id));

        // Act
        var result = _controller.DeleteDepartment(id);

        // Assert
        Assert.IsType<NoContentResult>(result);
        _mockService.Verify(s => s.DeleteDepartment(id), Times.Once);
    }

    [Fact]
    public void DeleteDepartment_Should_Return_NotFound_When_Exception_Is_Thrown()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockService.Setup(s => s.DeleteDepartment(id))
                    .Throws(new Exception("Département non trouvé"));

        // Act
        var result = _controller.DeleteDepartment(id);

        // Assert
        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        Assert.NotNull(notFound.Value);
        Assert.Contains("non trouvé", notFound.Value.ToString());
        _mockService.Verify(s => s.DeleteDepartment(id), Times.Once);
    }
}
