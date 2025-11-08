using LaTroca.Application.DTOs;
using LaTroca.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace LaTroca.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Todas las rutas requieren autenticación
    public class ChatController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<ChatController> _logger;

        public ChatController(
            INotificationService notificationService,
            ILogger<ChatController> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        /// <summary>
        /// Enviar notificación de mensaje de chat
        /// </summary>
        [HttpPost("send-notification")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> SendChatNotification([FromBody] SendChatNotificationRequest request)
        {
            try
            {
                // Validar datos
                if (string.IsNullOrEmpty(request.ReceiverFcmToken))
                {
                    return BadRequest(new { Message = "El token FCM del receptor es requerido." });
                }

                if (string.IsNullOrEmpty(request.MessageText))
                {
                    return BadRequest(new { Message = "El texto del mensaje es requerido." });
                }

                // Enviar notificación
                var result = await _notificationService.SendChatNotificationAsync(
                    receiverFcmToken: request.ReceiverFcmToken,
                    senderName: request.SenderName,
                    messageText: request.MessageText,
                    chatId: request.ChatId,
                    senderId: request.SenderId
                );

                if (result)
                {
                    return Ok(new { Message = "Notificación enviada exitosamente." });
                }
                else
                {
                    return StatusCode(500, new { Message = "Error al enviar la notificación." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error en SendChatNotification: {ex.Message}");
                return StatusCode(500, new { Message = "Error interno del servidor." });
            }
        }

        /// <summary>
        /// Actualizar el FCM token del usuario
        /// </summary>
        [HttpPost("update-fcm-token")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateFcmToken([FromBody] UpdateFcmTokenRequest request)
        {
            try
            {
                // Obtener userId del token JWT
                var userIdClaim = User.FindFirst("userId")?.Value;

                if (string.IsNullOrEmpty(userIdClaim))
                {
                    return Unauthorized(new { Message = "Usuario no autenticado." });
                }

                if (string.IsNullOrEmpty(request.FcmToken))
                {
                    return BadRequest(new { Message = "El FCM token es requerido." });
                }

                // Actualizar token en Firestore
                var result = await _notificationService.UpdateUserFcmTokenAsync(
                    userId: userIdClaim,
                    fcmToken: request.FcmToken
                );

                if (result)
                {
                    return Ok(new { Message = "FCM token actualizado exitosamente." });
                }
                else
                {
                    return StatusCode(500, new { Message = "Error al actualizar FCM token." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error en UpdateFcmToken: {ex.Message}");
                return StatusCode(500, new { Message = "Error interno del servidor." });
            }
        }

        /// <summary>
        /// Eliminar el FCM token del usuario (logout)
        /// </summary>
        [HttpPost("remove-fcm-token")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RemoveFcmToken()
        {
            try
            {
                // Obtener userId del token JWT
                var userIdClaim = User.FindFirst("userId")?.Value;

                if (string.IsNullOrEmpty(userIdClaim))
                {
                    return Unauthorized(new { Message = "Usuario no autenticado." });
                }

                // Eliminar token de Firestore
                var result = await _notificationService.RemoveUserFcmTokenAsync(userIdClaim);

                if (result)
                {
                    return Ok(new { Message = "FCM token eliminado exitosamente." });
                }
                else
                {
                    return StatusCode(500, new { Message = "Error al eliminar FCM token." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error en RemoveFcmToken: {ex.Message}");
                return StatusCode(500, new { Message = "Error interno del servidor." });
            }
        }
    }
}