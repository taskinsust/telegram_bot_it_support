using System;
using System.Collections.Generic;
using System.Text;

namespace Telegram.Bot.SCL.Auth.Configurations
{
    public static class ErrorMessages
    {
        public static string DO_NOT_SUPPORT_BOT_QUERY = "Sorry! We do not process any BOT request!";
        public static string WAITING_FOR_APPROVAL = "Oops! your account has not been approved yet. Our support engineer will activate it shortly.";
        public static string FIRST_TIME_USER_REQUEST = "You are not a registered member. Please share your official contact details (by clicking contact links below) with us so that we can verify that you are an employee of Summit Communications Ltd. ";
        public static string THANK_YOU = "Thank you for sharing your contact with us. Please allow us sometime to verify your identity.";
        public static string ERROR_MESAGE = "Currently processing other requests! please try again later.";
        public static string ACCOUNT_BLOCK = "Your account is temporary block! please contact with IT system & support team.";
        public static string WAITING_FOR_EMAIL_REQUEST = "Almost there! please share your official/SComm email address";
    }
}
