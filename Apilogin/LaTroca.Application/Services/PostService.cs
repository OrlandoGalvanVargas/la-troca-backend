using LaTroca.Application.Interfaces;
using LaTroca.Domain.Interfaces;
using TorneoUniversitario.Application.DTOs;
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

            // ✅ Usar método asíncrono con información de usuario
            return await MapearARespuestaAsync(publicacion);
        }

        public async Task<PostResponse> ObtenerPublicacionPorIdAsync(string id)
        {
            var publicacion = await _postRepository.ObtenerPorIdAsync(id);
            if (publicacion == null)
                throw new ArgumentException("Publicación no encontrada.");

            // ✅ Usar método asíncrono con información de usuario
            return await MapearARespuestaAsync(publicacion);
        }

        public async Task<List<PostResponse>> ObtenerPublicacionesPorUserIdAsync(string userId)
        {
            var usuario = await _usuarioRepository.GetByIdAsync(userId);
            if (usuario == null)
                throw new ArgumentException("Usuario no encontrado.");

            var publicaciones = await _postRepository.ObtenerPorUserIdAsync(userId);

            // Mapear todas las publicaciones con información del usuario
            var respuestas = new List<PostResponse>();
            foreach (var publicacion in publicaciones)
            {
                respuestas.Add(await MapearARespuestaAsync(publicacion));
            }

            return respuestas;
        }

        public async Task<List<PostResponse>> ObtenerTodasPublicacionesAsync()
        {
            var publicaciones = await _postRepository.ObtenerTodosAsync();

            // Mapear todas las publicaciones con información del usuario
            var respuestas = new List<PostResponse>();
            foreach (var publicacion in publicaciones)
            {
                respuestas.Add(await MapearARespuestaAsync(publicacion));
            }

            return respuestas;
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

        // 🆕 Método actualizado con información del usuario
        private async Task<PostResponse> MapearARespuestaAsync(Post publicacion)
        {
            // Obtener información del usuario
            var usuario = await _usuarioRepository.GetByIdAsync(publicacion.UserId);

            return new PostResponse
            {
                Id = publicacion.Id,
                UserId = publicacion.UserId,
                Titulo = publicacion.Titulo,
                Descripcion = publicacion.Descripcion,
                Categoria = publicacion.Categoria,
                FotosUrl = publicacion.FotosUrl,
                Ubicacion = new UbicacionDto
                {
                    Latitude = publicacion.Ubicacion.Latitude,
                    Longitude = publicacion.Ubicacion.Longitude,
                    Manual = publicacion.Ubicacion.Manual
                },
                Necesidad = publicacion.Necesidad,
                CreadoEn = publicacion.CreadoEn,
                ActualizadoEn = publicacion.ActualizadoEn,
                Estado = publicacion.Estado,

                // 🆕 Incluir información del usuario
                UserInfo = usuario != null ? new UserBasicInfo
                {
                    UserId = usuario.Id,
                    Name = usuario.Name ?? "Usuario",
                    ProfileImageUrl = usuario.ProfilePicUrl ?? ""
                } : new UserBasicInfo
                {
                    UserId = publicacion.UserId,
                    Name = "Usuario Desconocido",
                    ProfileImageUrl = ""
                }
            };
        }
    }
}