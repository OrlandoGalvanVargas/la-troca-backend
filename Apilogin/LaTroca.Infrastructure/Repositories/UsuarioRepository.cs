using LaTroca.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using TorneoUniversitario.Domain.Entities;
using TorneoUniversitario.Domain.Interfaces;

namespace TorneoUniversitario.Infrastructure.Repositories
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly MongoDbContext _context;

        public UsuarioRepository(MongoDbContext context)
        {
            _context = context;
        }

        public async Task<Usuario?> GetByEmailAsync(string email)
        {
            return await _context.Usuarios.Find(u => u.Email == email).FirstOrDefaultAsync();
        }

        public async Task<Usuario> AddAsync(Usuario usuario)
        {
            usuario.CreatedAt = DateTime.UtcNow;
            usuario.UpdatedAt = DateTime.UtcNow;
            usuario.Status = "active";

            await _context.Usuarios.InsertOneAsync(usuario);
            return usuario;
        }

        public async Task<Usuario?> GetByIdAsync(string id)
        {
            return await _context.Usuarios.Find(u => u.Id == id).FirstOrDefaultAsync();
        }

        public async Task UpdateAsync(Usuario usuario)
        {
            usuario.UpdatedAt = DateTime.UtcNow;
            await _context.Usuarios.ReplaceOneAsync(u => u.Id == usuario.Id, usuario);
        }
    }
}