namespace Client.WeatherUI.Configurations
{
    public class IdentityServerConfiguration
    {
        public string DiscoveryUrl { get; set; }
        public string ClientName { get; set; }
        public string ClientPassword { get; set; }
        public bool UseHttps { get; set; }
    }
}