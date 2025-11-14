using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Firestore;
using LaTroca.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace LaTroca.Infrastructure.Services
{
    public class NotificationService : INotificationService
    {
        private readonly FirestoreDb _firestore;
        private readonly ILogger<NotificationService> _logger;

        private static readonly object _lock = new();
        private static bool _firebaseInitialized = false;

        public NotificationService(FirestoreDb firestore, ILogger<NotificationService> logger)
        {
            _firestore = firestore ?? throw new ArgumentNullException(nameof(firestore));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Inicializar FirebaseApp una sola vez (thread-safe)
            if (!_firebaseInitialized)
            {
                lock (_lock)
                {
                    if (!_firebaseInitialized)
                    {
                        try
                        {
                            // FirebaseApp ya debería estar inicializado en Program.cs
                            // Solo verificamos si existe
                            if (FirebaseApp.DefaultInstance == null)
                            {
                                _logger.LogWarning("⚠️ FirebaseApp no está inicializado. Se espera que esté configurado en Program.cs");
                            }
                            else
                            {
                                _logger.LogInformation("✅ FirebaseApp ya está inicializado correctamente.");
                            }

                            _firebaseInitialized = true;
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError($"❌ Error verificando Firebase: {ex.Message}");
                            _logger.LogError($"Stack trace: {ex.StackTrace}");
                            throw;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Envía una notificación push al dispositivo del receptor.
        /// IMPORTANTE: Solo usa DATA payload para que onMessageReceived siempre se ejecute.
        /// </summary>
        public async Task<bool> SendChatNotificationAsync(
            string receiverFcmToken,
            string senderName,
            string messageText,
            string chatId,
            string senderId)
        {
            try
            {
                _logger.LogInformation($"📤 Enviando notificación a token: {receiverFcmToken.Substring(0, 20)}...");

                // 🔧 CAMBIO IMPORTANTE: Solo usar DATA payload, NO notification payload
                // Esto asegura que onMessageReceived() siempre se ejecute
                var message = new Message
                {
                    Token = receiverFcmToken,
                    
                    // ❌ REMOVIDO: No usar Notification para que el cliente maneje todo
                    // Notification = new Notification { ... }
                    
                    // ✅ Solo DATA - el cliente Android mostrará la notificación
                    Data = new Dictionary<string, string>
                    {
                        { "chatId", chatId },
                        { "senderId", senderId },
                        { "senderName", senderName },
                        { "messageText", messageText },
                        { "type", "chat_message" }
                    },
                    
                    // Configuración para Android
                    Android = new AndroidConfig
                    {
                        Priority = Priority.High,
                        // ❌ REMOVIDO: AndroidNotification también
                        // Solo usamos Data con Priority alta
                    }
                };

                var response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
                _logger.LogInformation($"✅ Notificación DATA enviada correctamente. ResponseId: {response}");
                _logger.LogInformation($"📊 Datos enviados: chatId={chatId}, sender={senderName}");
                return true;
            }
            catch (FirebaseMessagingException ex)
            {
                _logger.LogError($"❌ Error FCM ({ex.MessagingErrorCode}): {ex.Message}");
                _logger.LogError($"Stack trace: {ex.StackTrace}");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error general al enviar notificación: {ex.Message}");
                _logger.LogError($"Stack trace: {ex.StackTrace}");
                return false;
            }
        }

        /// <summary>
        /// Actualiza o agrega el token FCM del usuario en Firestore.
        /// </summary>
        public async Task<bool> UpdateUserFcmTokenAsync(string userId, string fcmToken)
        {
            try
            {
                _logger.LogInformation($"🔄 Actualizando token FCM para usuario: {userId}");
                _logger.LogInformation($"📱 Token FCM: {fcmToken.Substring(0, 20)}...");

                var userRef = _firestore.Collection("users").Document(userId);

                var data = new Dictionary<string, object>
                {
                    { "fcmToken", fcmToken },
                    { "fcmTokenUpdatedAt", Timestamp.GetCurrentTimestamp() }
                };

                await userRef.SetAsync(data, SetOptions.MergeAll);

                _logger.LogInformation($"✅ Token FCM actualizado exitosamente para usuario {userId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error al actualizar token FCM para usuario {userId}: {ex.Message}");
                _logger.LogError($"Stack trace: {ex.StackTrace}");
                return false;
            }
        }

        /// <summary>
        /// Elimina el token FCM del usuario (por ejemplo, al cerrar sesión).
        /// </summary>
        public async Task<bool> RemoveUserFcmTokenAsync(string userId)
        {
            try
            {
                _logger.LogInformation($"🗑️ Eliminando token FCM para usuario: {userId}");

                var userRef = _firestore.Collection("users").Document(userId);
                await userRef.UpdateAsync(new Dictionary<string, object>
                {
                    { "fcmToken", FieldValue.Delete },
                    { "fcmTokenUpdatedAt", Timestamp.GetCurrentTimestamp() }
                });

                _logger.LogInformation($"✅ Token FCM eliminado exitosamente para usuario {userId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error al eliminar token FCM para usuario {userId}: {ex.Message}");
                _logger.LogError($"Stack trace: {ex.StackTrace}");
                return false;
            }
        }
    }
}
