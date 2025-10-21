using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TorneoUniversitario.Domain.Entities;

namespace LaTroca.Domain.Interfaces
{
    public interface IPostRepository
    {
        Task AgregarAsync(Post post);
        Task<Post> ObtenerPorIdAsync(string id);
        Task<List<Post>> ObtenerPorUserIdAsync(string userId);
        Task<List<Post>> ObtenerTodosAsync();
        Task ActualizarAsync(Post post);
        Task EliminarAsync(string id);
    }
}
