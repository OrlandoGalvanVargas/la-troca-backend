using LaTroca.Application.Interfaces;
using LaTroca.Domain.Interfaces;
using TorneoUniversitario.Application.DTOs;
using TorneoUniversitario.Application.Interfaces;
using TorneoUniversitario.Domain.Entities;
using TorneoUniversitario.Domain.Interfaces;

namespace TorneoUniversitario.Application.Services
{
    public class PostService : IPostService
    {
        private readonly IPostRepository _postRepository;
        private readonly IUsuarioRepository _usuarioRepository;
        private readonly ICloudinaryService _cloudinaryService;

        public PostService(
            IPostRepository postRepository,
            IUsuarioRepository usuarioRepository,
            ICloudinaryService cloudinaryService)
        {
            _postRepository = postRepository;
            _usuarioRepository = usuarioRepository;
            _cloudinaryService = cloudinaryService;
        }

        public async Task<PostResponse> CrearPublicacionAsync(string userId, PostRequest request)
        {
            // Validar que el usuario existe y está activo
            var usuario = await _usuarioRepository.GetByIdAsync(userId);
            if (usuario == null || usuario.Status != "active")
                throw new UnauthorizedAccessException("Usuario no encontrado o inactivo.");

            // Validar la solicitud
            if (string.IsNullOrWhiteSpace(request.Titulo))
                throw new ArgumentException("El título es obligatorio.");
            if (string.IsNullOrWhiteSpace(request.Descripcion))
                throw new ArgumentException("La descripción es obligatoria.");
            if (string.IsNullOrWhiteSpace(request.Categoria))
                throw new ArgumentException("La categoría es obligatoria.");
            if (request.Fotos.Length > 3)
                throw new ArgumentException("No se pueden subir más de 3 imágenes por publicación.");

            // Subir imágenes a Cloudinary
            var fotosUrls = new List<string>();
            foreach (var foto in request.Fotos)
            {
                var url = await _cloudinaryService.UploadImageAsync(foto, $"{userId}_{Guid.NewGuid()}");
                fotosUrls.Add(url);
            }

            var publicacion = new Post
            {
                UserId = userId,
                Titulo = request.Titulo,
                Descripcion = request.Descripcion,
                Categoria = request.Categoria,
                FotosUrl = fotosUrls.ToArray(),
                Ubicacion = request.Ubicacion,
                Necesidad = request.Necesidad,
                CreadoEn = DateTime.UtcNow,
                ActualizadoEn = DateTime.UtcNow,
                Estado = "activo"
            };

            await _postRepository.AgregarAsync(publicacion);

            return MapearARespuesta(publicacion);
        }

        public async Task<PostResponse> ObtenerPublicacionPorIdAsync(string id)
        {
            var publicacion = await _postRepository.ObtenerPorIdAsync(id);
            if (publicacion == null)
                throw new ArgumentException("Publicación no encontrada.");
            return MapearARespuesta(publicacion);
        }

        public async Task<List<PostResponse>> ObtenerPublicacionesPorUserIdAsync(string userId)
        {
            var usuario = await _usuarioRepository.GetByIdAsync(userId);
            if (usuario == null)
                throw new ArgumentException("Usuario no encontrado.");

            var publicaciones = await _postRepository.ObtenerPorUserIdAsync(userId);
            return publicaciones.Select(MapearARespuesta).ToList();
        }

        public async Task<List<PostResponse>> ObtenerTodasPublicacionesAsync()
        {
            var publicaciones = await _postRepository.ObtenerTodosAsync();
            return publicaciones.Select(MapearARespuesta).ToList();
        }

        public async Task ActualizarPublicacionAsync(string id, string userId, PostRequest request)
        {
            var publicacion = await _postRepository.ObtenerPorIdAsync(id);
            if (publicacion == null)
                throw new ArgumentException("Publicación no encontrada.");
            if (publicacion.UserId != userId)
                throw new UnauthorizedAccessException("No tienes permiso para modificar esta publicación.");

            // Validar la solicitud
            if (string.IsNullOrWhiteSpace(request.Titulo))
                throw new ArgumentException("El título es obligatorio.");
            if (string.IsNullOrWhiteSpace(request.Descripcion))
                throw new ArgumentException("La descripción es obligatoria.");
            if (string.IsNullOrWhiteSpace(request.Categoria))
                throw new ArgumentException("La categoría es obligatoria.");
            if (request.Fotos.Length > 3)
                throw new ArgumentException("No se pueden subir más de 3 imágenes por publicación.");

            // Subir nuevas imágenes a Cloudinary
            var fotosUrls = new List<string>();
            foreach (var foto in request.Fotos)
            {
                var url = await _cloudinaryService.UploadImageAsync(foto, $"{userId}_{Guid.NewGuid()}");
                fotosUrls.Add(url);
            }

            publicacion.Titulo = request.Titulo;
            publicacion.Descripcion = request.Descripcion;
            publicacion.Categoria = request.Categoria;
            publicacion.FotosUrl = fotosUrls.ToArray();
            publicacion.Ubicacion = request.Ubicacion;
            publicacion.Necesidad = request.Necesidad;
            publicacion.ActualizadoEn = DateTime.UtcNow;

            await _postRepository.ActualizarAsync(publicacion);
        }

        public async Task EliminarPublicacionAsync(string id, string userId)
        {
            var publicacion = await _postRepository.ObtenerPorIdAsync(id);
            if (publicacion == null)
                throw new ArgumentException("Publicación no encontrada.");
            if (publicacion.UserId != userId)
                throw new UnauthorizedAccessException("No tienes permiso para eliminar esta publicación.");

            await _postRepository.EliminarAsync(id);
        }

        private PostResponse MapearARespuesta(Post publicacion)
        {
            return new PostResponse
            {
                Id = publicacion.Id,
                UserId = publicacion.UserId,
                Titulo = publicacion.Titulo,
                Descripcion = publicacion.Descripcion,
                Categoria = publicacion.Categoria,
                FotosUrl = publicacion.FotosUrl,
                Ubicacion = publicacion.Ubicacion,
                Necesidad = publicacion.Necesidad,
                CreadoEn = publicacion.CreadoEn,
                ActualizadoEn = publicacion.ActualizadoEn,
                Estado = publicacion.Estado
            };
        }
    }
}