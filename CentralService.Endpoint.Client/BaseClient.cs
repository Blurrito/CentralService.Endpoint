using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Newtonsoft.Json;

namespace CentralService.Endpoint.Client
{
    public abstract class BaseClient
    {
        internal string BaseURL { get; }

        private HttpClient _Client;

        public BaseClient(string BaseURL)
        {
            this.BaseURL = BaseURL;
            _Client = new HttpClient();
            _Client.DefaultRequestHeaders.Accept.Clear();
            _Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        internal async Task<HttpResponseMessage> Get(string RouteValues) => await _Client.GetAsync($"{ BaseURL }{ RouteValues }");

        internal async Task<HttpResponseMessage> Create<TType>(string RouteValues, TType Content)
        {
            HttpContent RequestContent = JsonContent.Create(Content);
            return await _Client.PostAsync($"{ BaseURL }{ RouteValues }", RequestContent);
        }

        internal async Task<HttpResponseMessage> Update<TType>(string RouteValues, TType Content)
        {
            HttpContent RequestContent = JsonContent.Create(Content);
            return await _Client.PutAsync($"{ BaseURL }{ RouteValues }", RequestContent);
        }

        internal async Task Delete(string RouteValues) => await _Client.DeleteAsync($"{ BaseURL }{ RouteValues }");

        internal async Task<TType?> ProcessResponse<TType>(HttpResponseMessage Response) where TType : struct
        {
            string ResponseContent = await Response.Content.ReadAsStringAsync();
            if (Response.Content != null)
            {
                TType Content = JsonConvert.DeserializeObject<TType>(ResponseContent);
                return Content;
            }
            return null;
        }
    }
}
