using TorneoUniversitario.Domain.Entities;

namespace TorneoUniversitario.Domain.Interfaces
{
    public interface IUsuarioRepository
    {
        Task<Usuario?> GetByEmailAsync(string email);
        Task<Usuario> AddAsync(Usuario usuario);
        Task<Usuario?> GetByIdAsync(string id);
        Task UpdateAsync(Usuario usuario);
    }
}
