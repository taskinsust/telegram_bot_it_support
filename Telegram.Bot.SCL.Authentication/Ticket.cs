using BOTAuthentication.Models;
using BOTAuthentication.Services;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace BOTAuthentication
{
    public class TicketAPI : ITicketAPI
    {
        private readonly IHttpService _httpService;
        public TicketAPI() { _httpService = new HttpService(new JsonSerializerSettings()); }

        public async Task<HttpResponseMessage> Insert(TicketEntity ticketEntity)
        {
            // First check to see if user has already created this ticket or not
            // 

            var parameters = new Dictionary<string, object>
                    {
                        {"TicketType", ticketEntity.TicketType},
                        {"Category", ticketEntity.Category},
                        {"Title", ticketEntity.Title},
                        {"CreatedBy", ticketEntity.FirstName},
                        {"TelegramUserId", ticketEntity.TelegramUserId},
                        {"TicketStatus",1 }
                    };

            var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile($"appsettings.json", true, true);
            var Configuration = builder.Build();
            string baseUrl = Configuration["Settings:baseUrl"];

            try { return await _httpService.PostWebApi(parameters, baseUrl + "ticket/apinew"); }
            catch (Exception e)
            {
                Helper.FileWrite.Example(e);
                throw;
            }
        }
    }
}
