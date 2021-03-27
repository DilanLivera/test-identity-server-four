using IdentityModel.Client;
using System.Threading.Tasks;

namespace Client.WeatherUI.Services
{
    public interface IIdentityServerService
    {
        Task<TokenResponse> GetTokenAsync(string scope);
    }
}