using System.ComponentModel.DataAnnotations;

namespace LaTroca.Application.DTOs
{
    public class SendChatNotificationRequest
    {
        [Required]
        public string ReceiverFcmToken { get; set; }

        [Required]
        public string SenderName { get; set; }

        [Required]
        public string MessageText { get; set; }

        [Required]
        public string ChatId { get; set; }

        [Required]
        public string SenderId { get; set; }
    }

    public class UpdateFcmTokenRequest
    {
        [Required]
        public string FcmToken { get; set; }
    }
}