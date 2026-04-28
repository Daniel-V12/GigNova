using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Schema;

namespace GigNovaWSClient
{
    public class ApiClient<T>
    {
        HttpClient httpClient = GigNovaHttpClient.Instance;
        UriBuilder uriBuilder =new UriBuilder();
        public string Scheme
        {
            set
            {
                this.uriBuilder.Scheme = value;
            }
        }

        public string Host
        {
            set
            {
                this.uriBuilder.Host = value;
            }
        }

        public int Port
        {
            set
            {
                this.uriBuilder.Port = value;
            }
        }

        public string Path
        {
            set
            {
                this.uriBuilder.Path = value;
            }
        }

        public void AddParameter(string key, string value)
        {
            if(this.uriBuilder.Query == string.Empty)
            {
                this.uriBuilder.Query += "?";
            }
            else
            {
                this.uriBuilder.Query += "&";
            }
            this.uriBuilder.Query += $"{key}={value}";
        }

        //נשתמש בפונקציה זו כאשר אנחנו רוצים לקבל נתונים ממסד נתונים
        public async Task<T> GetAsync()
        {
            using (HttpRequestMessage httpRequest = new HttpRequestMessage())
            {
                httpRequest.Method = HttpMethod.Get;
                httpRequest.RequestUri = this.uriBuilder.Uri;
                using (HttpResponseMessage httpResponse = await this.httpClient.SendAsync(httpRequest))
                {
                    if (httpResponse.IsSuccessStatusCode)
                    {
                        string result =await httpResponse.Content.ReadAsStringAsync();
                        JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions();
                        jsonSerializerOptions.PropertyNameCaseInsensitive = true;
                        T model = JsonSerializer.Deserialize<T>(result , jsonSerializerOptions);
                        return model;
                    }
                    return  default(T);
                }
            }
        }


        public async Task<bool> PostAsync(T model)
        {
            using (HttpRequestMessage httpRequest = new HttpRequestMessage())
            {
                httpRequest.Method = HttpMethod.Post;
                httpRequest.RequestUri = this.uriBuilder.Uri;
                string json = JsonSerializer.Serialize<T>(model);
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                httpRequest.Content = content;
                using (HttpResponseMessage responseMessage =
                      await this.httpClient.SendAsync(httpRequest))
                {
                    return responseMessage.IsSuccessStatusCode == true;

                }

            }
        }

        public async Task<bool> PostAsync(T model, Stream file,string fileName)
        {
            using (HttpRequestMessage httpRequest = new HttpRequestMessage())
            {
                httpRequest.Method = HttpMethod.Post;
                httpRequest.RequestUri = this.uriBuilder.Uri;
                MultipartFormDataContent multipartFormDataContent  = new MultipartFormDataContent();
                string json = JsonSerializer.Serialize<T>(model);
                StringContent model_content = new StringContent(json);
                multipartFormDataContent.Add(model_content, "model");
                StreamContent streamContent = new StreamContent(file);
                multipartFormDataContent.Add(streamContent, "file", fileName);
                httpRequest.Content = multipartFormDataContent;
                using (HttpResponseMessage responseMessage = await this.httpClient.SendAsync(httpRequest))
                {
                    return responseMessage.IsSuccessStatusCode;
                }
            }
        }

        public async Task<TResponse> PostAsyncReturn<TRequest, TResponse>(TRequest model)
        {

            using (HttpRequestMessage httpRequest = new HttpRequestMessage())
            {
                httpRequest.Method = HttpMethod.Post;
                httpRequest.RequestUri = this.uriBuilder.Uri;
                string json = JsonSerializer.Serialize<TRequest>(model);
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                httpRequest.Content = content;
                using (HttpResponseMessage httpResponse = await this.httpClient.SendAsync(httpRequest))
                {
                    if (httpResponse.IsSuccessStatusCode)
                    {
                        string result = await httpResponse.Content.ReadAsStringAsync();
                        if (string.IsNullOrWhiteSpace(result))
                            return default(TResponse);
                        JsonSerializerOptions options = new JsonSerializerOptions();
                        options.PropertyNameCaseInsensitive = true;
                        TResponse value = JsonSerializer.Deserialize<TResponse>(result, options);
                        return value;
                    }
                }
                return default(TResponse);

            }
        }

        public async Task<bool> PostAsync(T model, List<Stream> files)
        {
            using (HttpRequestMessage httpRequest = new HttpRequestMessage())
            {
                httpRequest.Method = HttpMethod.Post;
                httpRequest.RequestUri = this.uriBuilder.Uri;
                MultipartFormDataContent multipartFormDataContent = new MultipartFormDataContent();
                string json = JsonSerializer.Serialize<T>(model);
                StringContent model_content = new StringContent(json);
                multipartFormDataContent.Add(model_content, "model");
                foreach(Stream fileStream in files)
                {
                    StreamContent streamContent = new StreamContent(fileStream);
                    multipartFormDataContent.Add(streamContent, "file", "file");
                }
                httpRequest.Content = multipartFormDataContent;
                using (HttpResponseMessage responseMessage = await this.httpClient.SendAsync(httpRequest))
                {
                    return responseMessage.IsSuccessStatusCode;
                }
            }
        }









    }
}
