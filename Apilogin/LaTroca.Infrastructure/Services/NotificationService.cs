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
            _firestore = firestore;
            _logger = logger;

            // Inicializar FirebaseApp una sola vez (thread-safe)
            if (!_firebaseInitialized)
            {
                lock (_lock)
                {
                    if (!_firebaseInitialized)
                    {
                        var credentialsPath = Path.Combine(AppContext.BaseDirectory, "la-troca-ed2d2-firebase-adminsdk-fbsvc-efc751c72d.json");

                        if (!File.Exists(credentialsPath))
                        {
                            _logger.LogError($"❌ No se encontró el archivo de credenciales Firebase en: {credentialsPath}");
                            throw new FileNotFoundException("No se encontró el archivo firebase-adminsdk.json", credentialsPath);
                        }

                        FirebaseApp.Create(new AppOptions
                        {
                            Credential = GoogleCredential.FromFile(credentialsPath)
                        });

                        _firebaseInitialized = true;
                        _logger.LogInformation("✅ FirebaseApp inicializado correctamente.");
                    }
                }
            }
        }

        /// <summary>
        /// Envía una notificación push al dispositivo del receptor.
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
                var message = new Message
                {
                    Token = receiverFcmToken,
                    Notification = new Notification
                    {
                        Title = $"{senderName} te ha enviado un mensaje",
                        Body = messageText
                    },
                    Data = new Dictionary<string, string>
                    {
                        { "chatId", chatId },
                        { "senderId", senderId },
                        { "type", "chat_message" }
                    }
                };

                var response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
                _logger.LogInformation($"✅ Notificación enviada correctamente. ResponseId: {response}");
                return true;
            }
            catch (FirebaseMessagingException ex)
            {
                _logger.LogError($"❌ Error FCM ({ex.MessagingErrorCode}): {ex.Message}");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error general al enviar notificación: {ex.Message}");
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
                var userRef = _firestore.Collection("users").Document(userId);
                await userRef.SetAsync(new
                {
                    fcmToken,
                    updatedAt = Timestamp.GetCurrentTimestamp()
                }, SetOptions.MergeAll);

                _logger.LogInformation($"🔄 Token FCM actualizado para usuario {userId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error al actualizar token FCM: {ex.Message}");
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
                var userRef = _firestore.Collection("users").Document(userId);
                await userRef.UpdateAsync(new Dictionary<string, object>
                {
                    { "fcmToken", FieldValue.Delete },
                    { "updatedAt", Timestamp.GetCurrentTimestamp() }
                });

                _logger.LogInformation($"🗑️ Token FCM eliminado para usuario {userId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error al eliminar token FCM: {ex.Message}");
                return false;
            }
        }
    }
}
