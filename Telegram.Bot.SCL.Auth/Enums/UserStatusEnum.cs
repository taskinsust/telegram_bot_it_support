using System;
using System.Collections.Generic;
using System.Text;

namespace Telegram.Bot.SCL.Auth.Enums
{

    public enum UserStatusEnum
    {
        Approve = 1,
        WaitingForApproval,
        Block,
        NoPermission,
        WaitingForEmail
    }
}
