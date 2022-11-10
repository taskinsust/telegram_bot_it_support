using Newtonsoft.Json;
using Telegram.Bot.SCL.Auth.Configurations;
using Telegram.Bot.SCL.Auth.Helper;
using Telegram.Bot.SCL.Auth.Models;
using Telegram.Bot.SCL.Auth.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace Telegram.Bot.SCL.Auth
{
    public class TelegramAuthentication : ITelegramAuthentication
    {
        private readonly UserService _userService;
        public TelegramAuthentication()
        {
            _userService = new UserService();
        }

        public bool IsAuthentic { get; set; }

        public string CheckUserCredential(bool isBot = false, string firstName = "", string lastName = "",
            string userName = "", long userId = 0, string contactNo = "")
        {
            try
            {
                if (isBot)
                    return Serializer.Serialize<ResponseResult>(new ResponseResult(false, ErrorMessages.DO_NOT_SUPPORT_BOT_QUERY));
                ResponseResult result = _userService.CheckAuthentication(firstName, lastName, userName, userId, contactNo);
                IsAuthentic = result.IsSuccess;
                return Serializer.Serialize(result);
            }
            catch (Exception e)
            {
                return Serializer.Serialize(new ResponseResult(false, ErrorMessages.ERROR_MESAGE));
            }
        }

        public void LimitCheck(bool isBot, string FirstName = "", string LastName = "", string Username = "")
        {
            throw new NotImplementedException();
        }

        public object UpdateEmail(bool isBot, string firstName, string lastName, string username, long userId, string email)
        {
            try
            {
                if (isBot)
                    return Serializer.Serialize<ResponseResult>(new ResponseResult(false, ErrorMessages.DO_NOT_SUPPORT_BOT_QUERY));
                ResponseResult result = _userService.UpdateMail(firstName, lastName, username, userId, email);
                IsAuthentic = result.IsSuccess;
                return Serializer.Serialize(result);
            }
            catch (Exception e)
            {
                return Serializer.Serialize(new ResponseResult(false, ErrorMessages.ERROR_MESAGE));
            }
        }

        public string UpdateUserCredential(string firstName, string lastName, string username, long userId, string phoneNumber)
        {
            try
            {
                ResponseResult result = _userService.UpdateUserCredential(firstName, lastName, username, userId, phoneNumber);
                return Serializer.Serialize(result);
            }
            catch (Exception e)
            {
                return Serializer.Serialize(new ResponseResult(false, ErrorMessages.ERROR_MESAGE));
            }
        }

        public object UpdateUserProfileEmail(bool isBot, string firstName, string lastName, string username, long userId, string email)
        {
            try
            {
                if (isBot)
                    return Serializer.Serialize<ResponseResult>(new ResponseResult(false, ErrorMessages.DO_NOT_SUPPORT_BOT_QUERY));
                ResponseResult result = _userService.UpdateUserProfileEmail(firstName, lastName, username, userId, email);
                IsAuthentic = result.IsSuccess;
                return Serializer.Serialize(result);
            }
            catch (Exception e)
            {
                return Serializer.Serialize(new ResponseResult(false, ErrorMessages.ERROR_MESAGE));
            }
        }
    }
}
