using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace BOTAuthentication.Services
{
    public interface IHttpService
    {
        void GetWebApi(string apiUrl);
        Task<HttpResponseMessage> PostWebApi(object data, string apiUrl);
        //T GetWebApi<T>(string apiUrl);
        //T PostWebApi<T>(object data, string apiUrl);
    }
}
