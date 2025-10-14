using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
