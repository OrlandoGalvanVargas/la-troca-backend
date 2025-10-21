using LaTroca.Domain.Interfaces;
using LaTroca.Infrastructure.Data;
using MongoDB.Driver;

using TorneoUniversitario.Domain.Entities;

namespace LaTroca.Infrastructure.Repositories
{
    public class PostRepository : IPostRepository
    {
        private readonly IMongoCollection<Post> _publicaciones;

        public PostRepository(MongoDbContext context)
        {
            _publicaciones = context.Posts;
        }

        public async Task AgregarAsync(Post post)
        {
            await _publicaciones.InsertOneAsync(post);
        }

        public async Task<Post> ObtenerPorIdAsync(string id)
        {
            return await _publicaciones.Find(p => p.Id == id).FirstOrDefaultAsync();
        }

        public async Task<List<Post>> ObtenerPorUserIdAsync(string userId)
        {
            return await _publicaciones.Find(p => p.UserId == userId).ToListAsync();
        }

        public async Task<List<Post>> ObtenerTodosAsync()
        {
            return await _publicaciones.Find(_ => true).ToListAsync();
        }

        public async Task ActualizarAsync(Post post)
        {
            await _publicaciones.ReplaceOneAsync(p => p.Id == post.Id, post);
        }

        public async Task EliminarAsync(string id)
        {
            await _publicaciones.DeleteOneAsync(p => p.Id == id);
        }
    }
}