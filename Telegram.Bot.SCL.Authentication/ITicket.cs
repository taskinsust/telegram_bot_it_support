using BOTAuthentication.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace BOTAuthentication
{
    public interface ITicketAPI
    {
       Task<HttpResponseMessage> Insert(TicketEntity ticketEntity);
    }
}
