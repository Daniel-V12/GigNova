using System.Text;
using System.Text.Json;

namespace GigNovaWSClient
{
    // Generic HTTP client wrapper used by every WebApp / WPF controller to call the WS.
    // T is the type we expect back from GET requests (e.g. ApiClient<CatalogViewModel>).
    // The caller sets Scheme / Host / Port / Path, adds parameters, then calls GetAsync /
    // PostAsync / PostAsyncReturn depending on what kind of request they need.
    public class ApiClient<T>
    {
        HttpClient httpClient = GigNovaHttpClient.Instance;
        UriBuilder uriBuilder = new UriBuilder();


        // ============================== URL Builder (Scheme / Host / Port / Path) ==============================

        public string Scheme
        {
            set { this.uriBuilder.Scheme = value; }
        }

        public string Host
        {
            set { this.uriBuilder.Host = value; }
        }

        public int Port
        {
            set { this.uriBuilder.Port = value; }
        }

        public string Path
        {
            set { this.uriBuilder.Path = value; }
        }

        // Appends "key=value" to the URL's query string.
        // The first call writes "?key=value", every later call writes "&key=value".
        public void AddParameter(string key, string value)
        {
            if (this.uriBuilder.Query == string.Empty)
            {
                this.uriBuilder.Query += "?";
            }
            else
            {
                this.uriBuilder.Query += "&";
            }
            this.uriBuilder.Query += $"{key}={value}";
        }


        // ============================== GET ==============================

        // Sends a GET request to the URL we built above. If the response is success (2xx),
        // we parse the JSON body and deserialize it into a T. On any failure we return default(T)
        // (which is null for reference types like view models, 0 for ints, etc.).
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
                        string result = await httpResponse.Content.ReadAsStringAsync();
                        JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions();
                        jsonSerializerOptions.PropertyNameCaseInsensitive = true;
                        T model = JsonSerializer.Deserialize<T>(result, jsonSerializerOptions);
                        return model;
                    }
                    return default(T);
                }
            }
        }


        // ============================== POST (JSON body, returns just success/failure) ==============================

        // Sends a POST request with the model serialized as JSON in the body.
        // Returns true if the server responded with a success status code.
        public async Task<bool> PostAsync(T model)
        {
            using (HttpRequestMessage httpRequest = new HttpRequestMessage())
            {
                httpRequest.Method = HttpMethod.Post;
                httpRequest.RequestUri = this.uriBuilder.Uri;
                string json = JsonSerializer.Serialize<T>(model);
                StringContent content = new StringContent(json, Encoding.UTF8, "application/json");
                httpRequest.Content = content;
                using (HttpResponseMessage responseMessage = await this.httpClient.SendAsync(httpRequest))
                {
                    return responseMessage.IsSuccessStatusCode;
                }
            }
        }


        // ============================== POST (JSON body, returns a deserialized response) ==============================

        // Same as PostAsync(model) but the server is also expected to return JSON, which we
        // deserialize into TResponse. Used by Log In (POST a LoginRequest, get back the person id).
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
                        {
                            return default(TResponse);
                        }
                        JsonSerializerOptions options = new JsonSerializerOptions();
                        options.PropertyNameCaseInsensitive = true;
                        TResponse value = JsonSerializer.Deserialize<TResponse>(result, options);
                        return value;
                    }
                }
                return default(TResponse);
            }
        }


        // ============================== POST (multipart - JSON model + ONE file) ==============================

        // Sends a multipart/form-data POST. The model is serialized to JSON and sent as a "model"
        // part; the file is attached as a "file" part. Used by AddGig/EditGig (photo) and
        // BecomeASeller (avatar).
        public async Task<bool> PostAsync(T model, Stream file, string fileName)
        {
            using (HttpRequestMessage httpRequest = new HttpRequestMessage())
            {
                httpRequest.Method = HttpMethod.Post;
                httpRequest.RequestUri = this.uriBuilder.Uri;

                MultipartFormDataContent multipartFormDataContent = new MultipartFormDataContent();

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


        // ============================== POST (multipart - JSON model + MANY files) ==============================

        // Same as above but takes a list of file streams + a list of file names. Each file
        // becomes its own "file" part. Used by CreateOrderAndPayWithFiles (the buyer can
        // attach up to 5 requirement files to the order).
        public async Task<bool> PostAsync(T model, List<Stream> files, List<string> fileNames)
        {
            using (HttpRequestMessage httpRequest = new HttpRequestMessage())
            {
                httpRequest.Method = HttpMethod.Post;
                httpRequest.RequestUri = this.uriBuilder.Uri;

                MultipartFormDataContent multipartFormDataContent = new MultipartFormDataContent();

                string json = JsonSerializer.Serialize<T>(model);
                StringContent model_content = new StringContent(json);
                multipartFormDataContent.Add(model_content, "model");

                for (int i = 0; i < files.Count; i++)
                {
                    StreamContent streamContent = new StreamContent(files[i]);
                    string fileName = "file";
                    if (fileNames != null && i < fileNames.Count && string.IsNullOrEmpty(fileNames[i]) == false)
                    {
                        fileName = fileNames[i];
                    }
                    multipartFormDataContent.Add(streamContent, "file", fileName);
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
