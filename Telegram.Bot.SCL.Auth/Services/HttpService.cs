using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Telegram.Bot.SCL.Auth.Services
{
    public class HttpService : IHttpService, IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerSettings _serializerSettings;

        public HttpService(JsonSerializerSettings serializerSettings, HttpClient httpClient = null)
        {
            _httpClient = httpClient ?? new HttpClient();
            _httpClient.Timeout = new TimeSpan(0, 10, 0);  //10 second timeout, make this an configuration
            _serializerSettings = serializerSettings;
        }

        public void GetWebApi(string apiUrl)
        {
            try
            {
                var response = _httpClient.GetAsync(apiUrl).Result;
                response.EnsureSuccessStatusCode();
            }
            catch (Exception e)
            {
                throw new Exception("HttpGet failed in HttpService", e);
            }
        }

        //public T GetWebApi<T>(string apiUrl)
        //{
        //    try
        //    {
        //        var response = _httpClient.GetAsync(apiUrl).Result;
        //        //response.EnsureSuccessStatusCode();

        //        var responseString = response.Content.ReadAsStringAsync().Result;

        //        var responseObject = JsonConvert.DeserializeObject<ApiResponse<T>>(responseString, _serializerSettings);
        //        return responseObject.ResultObject;
        //    }
        //    catch (Exception e)
        //    {
        //        throw new Exception("HttpGet failed in HttpService", e);
        //    }
        //}

        public async Task<HttpResponseMessage> PostWebApi(object data, string apiUrl)
        {
            try
            {
                var request = new HttpRequestMessage(HttpMethod.Post, apiUrl);

                using (var content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json"))
                {
                    request.Content = content;

                    return await _httpClient.SendAsync(request);
                    //return response.EnsureSuccessStatusCode();
                }
            }
            catch (Exception e)
            {
                throw new Exception("HttpPost failed in HttpService", e);
            }
        }

        //public T PostWebApi<T>(object data, string apiUrl)
        //{
        //    try
        //    {
        //        var request = new HttpRequestMessage(HttpMethod.Post, apiUrl);

        //        using (var content = new StringContent(JsonConvert.SerializeObject(data, _serializerSettings), Encoding.UTF8, "application/json"))
        //        {
        //            request.Content = content;

        //            var response = _httpClient.SendAsync(request).Result;
        //            response.EnsureSuccessStatusCode();

        //            var responseString = response.Content.ReadAsStringAsync().Result;
        //            var responseObject = JsonConvert.DeserializeObject<ApiResponse<T>>(responseString, _serializerSettings);

        //            return responseObject.ResultObject;

        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        throw new Exception("HttpPost failed in HttpService", e);
        //    }
        //}

        public void Dispose()
        {
            _httpClient.Dispose();
        }
    }
}
