using Telegram.Bot.SCL.Auth.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Telegram.Bot.SCL.Auth
{
    public interface ITicketAPI
    {
       Task<HttpResponseMessage> Insert(TicketEntity ticketEntity);
    }
}
