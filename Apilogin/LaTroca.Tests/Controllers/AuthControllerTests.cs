using Xunit;
using Moq;
using Microsoft.AspNetCore.Mvc;
using TorneoUniversitario.API.Controllers;
using TorneoUniversitario.Application.DTOs;
using TorneoUniversitario.Application.Interfaces;
using System.Threading.Tasks;
namespace LaTroca.Tests.Controllers
{
    public class AuthControllerTests
    {
        private readonly Mock<IAuthService> _authServiceMock;
        private readonly AuthController _controller;

        public AuthControllerTests()
        {
            _authServiceMock = new Mock<IAuthService>();
            _controller = new AuthController(_authServiceMock.Object);
        }

        [Fact]
        public async Task Login_ReturnsOk_WhenCredentialsAreValid()
        {
            // Arrange
            var request = new LoginRequest { Email = "test@test.com", Password = "123456" };
            var response = new LoginResponse { Token = "fake-jwt", UserId = "1" };

            _authServiceMock
                .Setup(s => s.LoginAsync(request))
                .ReturnsAsync(response);

            // Act
            var result = await _controller.Login(request);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnedResponse = Assert.IsType<LoginResponse>(okResult.Value);
            Assert.Equal("fake-jwt", returnedResponse.Token);
        }

        [Fact]
        public async Task Login_ReturnsBadRequest_WhenArgumentExceptionThrown()
        {
            // Arrange
            var request = new LoginRequest { Email = "invalid", Password = "" };

            _authServiceMock
                .Setup(s => s.LoginAsync(request))
                .ThrowsAsync(new ArgumentException("Datos inválidos"));

            // Act
            var result = await _controller.Login(request);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("Datos inválidos", badRequest.Value);
        }

        [Fact]
        public async Task Register_ReturnsCreated_WhenSuccessful()
        {
            // Arrange
            var request = new RegisterRequest { Email = "user@test.com", Password = "123456" };

            _authServiceMock
                .Setup(s => s.RegisterAsync(request))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Register(request);

            // Assert
            var created = Assert.IsType<ObjectResult>(result);
            Assert.Equal(201, created.StatusCode);
        }

        [Fact]
        public async Task Logout_ReturnsOk()
        {
            // Arrange
            _authServiceMock.Setup(s => s.LogoutAsync()).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.Logout();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Contains("Logout exitoso", okResult.Value.ToString());
        }
    }
}