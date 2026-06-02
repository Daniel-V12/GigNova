namespace GigNovaWSClient
{
    // Singleton wrapper around a single HttpClient that the whole app shares.
    // Why singleton: creating a new HttpClient per request leaks sockets / runs out of
    // OS connection handles. .NET's recommended pattern is one shared HttpClient for
    // the application's lifetime; the underlying SocketsHttpHandler handles connection
    // pooling for us.
    //
    // Settings:
    //   - PooledConnectionLifetime = 10 minutes  (recycle each pooled connection after 10
    //     minutes so DNS changes get picked up)
    //   - ConnectTimeout = 15 seconds  (fail fast if the WS isn't reachable)
    public class GigNovaHttpClient
    {
        private static readonly HttpClient httpClient = CreateClient();

        private GigNovaHttpClient() { }

        public static HttpClient Instance
        {
            get { return httpClient; }
        }

        private static HttpClient CreateClient()
        {
            SocketsHttpHandler handler = new SocketsHttpHandler();
            handler.PooledConnectionLifetime = TimeSpan.FromMinutes(10);
            handler.ConnectTimeout = TimeSpan.FromSeconds(15);
            return new HttpClient(handler);
        }
    }
}
