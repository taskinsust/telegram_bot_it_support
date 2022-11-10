using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telegram.Bot.SCL.Polling.Abstract
{
    public interface IHttpService
    {
        void GetWebApi(string apiUrl);
        void PostWebApi(object data, string apiUrl);
        T GetWebApi<T>(string apiUrl);
        T PostWebApi<T>(object data, string apiUrl);
    }
}
