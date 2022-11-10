using System;
using System.Collections.Generic;
using System.Text;

namespace Telegram.Bot.SCL.Auth
{
    public interface ITelegramAuthentication
    {
        bool IsAuthentic { get; set; }
        string CheckUserCredential(bool isBot, string FirstName = "", string LastName = "", string Username = "", long userId = 0, string contactNo = "");
        void LimitCheck(bool isBot, string FirstName = "", string LastName = "", string Username = "");
        string UpdateUserCredential(string firstName = "", string lastName = "", string username = "", long userId = 0, string phoneNumber = "");
        object UpdateEmail(bool isBot, string firstName, string lastName, string username, long userId, string email);
        object UpdateUserProfileEmail(bool isBot, string firstName, string lastName, string username, long id, string text);
    }
}
