namespace HttpClientApp
{
    
    internal class Program
    {
        static HttpClient httpClient = new();
        static async Task Main(string[] args)
        {
            var content = new StringContent("What is HTTP?");

            using HttpRequestMessage request = new();
            request.Method = HttpMethod.Get;
            request.RequestUri = new Uri("https://www.google.com");
            request.Content = content;

            using HttpResponseMessage response = await httpClient.SendAsync(request);
            Console.WriteLine($"Status: {response.StatusCode}\n");
            Console.WriteLine($"Version: {response.Version.ToString()}\n");
            Console.WriteLine();

            string result = await response.Content.ReadAsStringAsync();
            Console.WriteLine(result);

        }
    }
}
