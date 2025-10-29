using TorneoUniversitario.Application.DTOs;

namespace LaTroca.Application.Interfaces
{
    public interface IPostService
    {
        Task<PostResponse> CrearPublicacionAsync(string userId, PostRequest request);
        Task<PostResponse> ObtenerPublicacionPorIdAsync(string id);
        Task<List<PostResponse>> ObtenerPublicacionesPorUserIdAsync(string userId);
        Task<List<PostResponse>> ObtenerTodasPublicacionesAsync();
        Task ActualizarPublicacionAsync(string id, string userId, PostRequest request);
        Task EliminarPublicacionAsync(string id, string userId);
    }
}
