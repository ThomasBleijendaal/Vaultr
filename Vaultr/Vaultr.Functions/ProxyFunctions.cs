using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace Vaultr.Functions
{
    public class ProxyFunctions
    {
        private static readonly string[] AllowedHeaders = new[]
        {
            "Authorization"
        };

        private readonly ILogger<ProxyFunctions> _logger;
        private readonly HttpClient _httpClient;

        public ProxyFunctions(
            ILogger<ProxyFunctions> log,
            HttpClient httpClient)
        {
            _logger = log;
            _httpClient = httpClient;
        }

        [FunctionName(nameof(ProxyRequestAsync))]
        public async Task<HttpResponseMessage> ProxyRequestAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", "put", "patch", "delete", Route = "{*rest}")] HttpRequest req,
            string rest)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            var keyVault = req.Headers["x-vaultr-keyvault"];

            var query = string.Join("&", req.Query.Select(x => $"{x.Key}={x.Value}"));

            var requestUri = new Uri(new Uri($"https://{keyVault}.vault.azure.net"), $"{req.Path}?{query}");

            var proxyMessage = new HttpRequestMessage(new HttpMethod(req.Method), requestUri);

            _logger.LogWarning(string.Join(", ", req.Headers.Keys));

            foreach (var header in req.Headers.Where(x => AllowedHeaders.Contains(x.Key)))
            {
                proxyMessage.Headers.Add(header.Key, header.Value.ToArray());
            }

            if (req.ContentLength > 0 && !string.IsNullOrWhiteSpace(req.ContentType))
            {
                proxyMessage.Content = new StreamContent(req.Body);
                proxyMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(req.ContentType);
            }

            var response = await _httpClient.SendAsync(proxyMessage);

            return response;
        }
    }
}
