using System.Net;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Newtonsoft.Json;
using TorneoUniversitario.Application.DTOs;
using Xunit;

namespace LaTroca.Tests.Integration
{
    public class AuthIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly HttpClient _client;

        public AuthIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task Login_ReturnsUnauthorized_WhenInvalidCredentials()
        {
            // Arrange
            var request = new LoginRequest
            {
                Email = "noexiste@test.com",
                Password = "wrongpassword"
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/auth/login", request);

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        [Fact]
        public async Task Register_ReturnsCreated_WhenDataIsValid()
        {
            // Arrange
            var request = new MultipartFormDataContent();
            request.Add(new StringContent("Nombre Prueba"), "Nombre");
            request.Add(new StringContent("testuser@test.com"), "Email");
            request.Add(new StringContent("12345678"), "Password");
            request.Add(new StringContent("USER"), "Rol");
            request.Add(new StringContent("Usuario de prueba para tests"), "Bio");

            // 🚫 Omitir la imagen para que no falle Cloudinary
            // var imageContent = new ByteArrayContent(new byte[] { 1, 2, 3 });
            // imageContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
            // request.Add(imageContent, "ImagenPerfil", "perfil.jpg");

            // Act
            var response = await _client.PostAsync("/api/auth/register", request);
            var content = await response.Content.ReadAsStringAsync();

            Console.WriteLine("🔎 Respuesta del servidor:");
            Console.WriteLine(content);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.Contains("Usuario creado correctamente", content);
        }

        [Fact]
        public async Task Register_ReturnsCreated_WithLocation_WhenDataIsValid()
        {
            // Arrange
            var request = new MultipartFormDataContent();
            request.Add(new StringContent("Nombre Prueba Con Ubicación"), "Nombre");
            request.Add(new StringContent("testuser2@test.com"), "Email");
            request.Add(new StringContent("12345678"), "Password");
            request.Add(new StringContent("USER"), "Rol");
            request.Add(new StringContent("Usuario de prueba con ubicación"), "Bio");
            request.Add(new StringContent("19.432608"), "Location.Latitude");
            request.Add(new StringContent("-99.133209"), "Location.Longitude");
            request.Add(new StringContent("Ciudad de México, Condesa"), "Location.Manual");

            // Act
            var response = await _client.PostAsync("/api/auth/register", request);
            var content = await response.Content.ReadAsStringAsync();

            Console.WriteLine("🔎 Respuesta del servidor:");
            Console.WriteLine(content);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.Contains("Usuario creado correctamente", content);
        }

        [Fact]
        public async Task Logout_ReturnsOk()
        {
            // Act
            var response = await _client.PostAsync("/api/auth/logout", null);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var content = await response.Content.ReadAsStringAsync();
            Assert.Contains("Logout exitoso", content);
        }
    }
}