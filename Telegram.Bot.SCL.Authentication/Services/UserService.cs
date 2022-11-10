using BOTAuthentication.Configurations;
using BOTAuthentication.DataAccess;
using BOTAuthentication.Enums;
using BOTAuthentication.Helper;
using BOTAuthentication.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Transactions;

namespace BOTAuthentication.Services
{
    public class UserService
    {
        private readonly UserDao _userDao;
        private readonly IHttpService _httpService;
        public UserService()
        {
            _httpService = new HttpService(new JsonSerializerSettings());
            var sqliteConnection = DbHelper.OpenDbConnection();
            _userDao = new UserDao(sqliteConnection);
        }

        internal ResponseResult CheckAuthentication(string firstName, string lastName, string userName, long userId, string contactNo)
        {
            try
            {
                //Helper.FileWrite.Example(new Exception("Call CheckAuthentication"));
                List<UserProfile> userList = _userDao.LoadByUserData(firstName, lastName, userName, userId);
                string defaultemail = "systemgenerate" + Guid.NewGuid().ToString("n").Substring(0, 8) + "@scomm.com";
                var parameters = new Dictionary<string, object>
                    {
                        {"UserName", firstName},
                        {"Email", defaultemail},
                        {"TelegramUserId", userId.ToString()},
                        {"DisplayName", firstName +" "+ lastName},
                        {"Password", "123456"},
                        {"ConfirmPassword", "123456"}
                    };
                var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile($"appsettings.json", true, true);
                var Configuration = builder.Build();
                string baseUrl = Configuration["Settings:baseUrl"];

                if (userList == null || userList.Count <= 0)
                {
                    using (var scope = new TransactionScope())
                    {
                        try
                        {
                            //If this user exists in database 
                            bool success = _userDao.Insert(new UserProfile() { FirstName = firstName, LastName = lastName, UserName = userName, CreationDate = DateTime.Now.ToString(), UserId = userId, ContactNo = contactNo, Email = defaultemail });

                            //Api Call to insert in another table named identity user.
                            _httpService.PostWebApi(parameters, baseUrl + "user/registerapi");
                            scope.Complete();
                            return new ResponseResult(false, ErrorMessages.FIRST_TIME_USER_REQUEST);
                        }
                        catch (Exception e)
                        {
                            Transaction.Current.Rollback();
                            Helper.FileWrite.Example(e);
                            return new ResponseResult(false, ErrorMessages.ERROR_MESAGE);
                        }
                    }
                }
                else if (userList[0].Email.Contains("systemgenerate"))
                {
                    return new ResponseResult(false, ErrorMessages.WAITING_FOR_EMAIL_REQUEST);
                }
                else if (userList.Where(x => x.UserStatus == (int)UserStatusEnum.WaitingForApproval).ToList().Count > 0)
                {
                    return new ResponseResult(false, ErrorMessages.WAITING_FOR_APPROVAL);
                }
                else if (userList.Where(x => x.UserStatus == (int)UserStatusEnum.Block).ToList().Count > 0)
                {
                    return new ResponseResult(false, ErrorMessages.ACCOUNT_BLOCK);
                }

                return new ResponseResult(true, "");
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        internal ResponseResult UpdateUserProfileEmail(string firstName, string lastName, string username, long userId, string email)
        {
            try
            {
                bool success = _userDao.UpdateUserProfileEmail(new UserProfile() { FirstName = firstName, LastName = lastName, UserName = username, CreationDate = DateTime.Now.ToString(), UserId = userId, Email = email });
                if (success)
                    return new ResponseResult(true, ErrorMessages.WAITING_FOR_APPROVAL);
                return new ResponseResult(false, ErrorMessages.ERROR_MESAGE);
            }
            catch (Exception e)
            {
                Helper.FileWrite.Example(e);
                throw e;
            }
        }

        internal ResponseResult UpdateMail(string firstName, string lastName, string username, long userId, string email = "")
        {
            try
            {
                bool success = _userDao.UpdateEmail(new UserProfile() { FirstName = firstName, LastName = lastName, UserName = username, CreationDate = DateTime.Now.ToString(), UserId = userId, Email = email });
                if (success)
                    return new ResponseResult(true, ErrorMessages.WAITING_FOR_APPROVAL);
                return new ResponseResult(false, ErrorMessages.ERROR_MESAGE);
            }
            catch (Exception e)
            {
                Helper.FileWrite.Example(e);
                throw e;
            }
        }

        internal ResponseResult UpdateUserCredential(string firstName, string lastName, string username, long userId, string phonenumber = "")
        {
            try
            {
                bool success = _userDao.Update(new UserProfile() { FirstName = firstName, LastName = lastName, UserName = username, CreationDate = DateTime.Now.ToString(), UserId = userId, ContactNo = phonenumber });
                if (success)
                    return new ResponseResult(true, ErrorMessages.WAITING_FOR_EMAIL_REQUEST);
                return new ResponseResult(false, ErrorMessages.ERROR_MESAGE);
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
