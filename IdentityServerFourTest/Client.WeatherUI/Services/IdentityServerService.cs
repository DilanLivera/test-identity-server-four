using Client.WeatherUI.Configurations;
using IdentityModel.Client;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Client.WeatherUI.Services
{
    public class IdentityServerService : IIdentityServerService
    {
        private readonly IOptions<IdentityServerConfiguration> _configuration;
        private readonly IHttpClientFactory _httpClientFactory;

        public IdentityServerService(
            IOptions<IdentityServerConfiguration> configuration,
            IHttpClientFactory httpClientFactory)
        {
            _configuration = configuration;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<TokenResponse> GetTokenAsync(string scope)
        {
            var discoveryDocumentResponse = await GetDiscoveryDocumentAsync();

            var request = new ClientCredentialsTokenRequest
            {
                Address = discoveryDocumentResponse.TokenEndpoint,
                ClientId = _configuration.Value.ClientName,
                ClientSecret = _configuration.Value.ClientPassword,
                Scope = scope
            };

            var client = _httpClientFactory.CreateClient();
            var tokenResponse = await client.RequestClientCredentialsTokenAsync(request);

            if (tokenResponse.IsError)
            {
                throw new Exception(
                    "Unable to get token from IdentityServer", 
                    tokenResponse.Exception);
            }

            return tokenResponse;
        }

        private async Task<DiscoveryDocumentResponse> GetDiscoveryDocumentAsync()
        {
            var client = _httpClientFactory.CreateClient();

            var discoveryDocumentResponse = await client.GetDiscoveryDocumentAsync(
                _configuration.Value.DiscoveryUrl);

            if (discoveryDocumentResponse.IsError)
            {
                throw new Exception(
                    "Unable to get discovery document from IdentityServer",
                    discoveryDocumentResponse.Exception);
            }

            return discoveryDocumentResponse;
        }
    }
}