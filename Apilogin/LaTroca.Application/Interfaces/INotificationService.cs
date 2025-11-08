using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaTroca.Application.Interfaces
{
    public interface INotificationService
    {
        Task<bool> SendChatNotificationAsync(
            string receiverFcmToken,
            string senderName,
            string messageText,
            string chatId,
            string senderId
        );

        Task<bool> UpdateUserFcmTokenAsync(string userId, string fcmToken);
        Task<bool> RemoveUserFcmTokenAsync(string userId);
    }
}
