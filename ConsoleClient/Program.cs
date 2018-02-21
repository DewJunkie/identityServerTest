using System;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using IdentityModel.Client;
using Newtonsoft.Json.Linq;
using Serilog;

namespace ConsoleClient
{
    class Program
    {
        private static readonly ILogger Logger;

        static Program()
        {
            Serilog.Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo.Console()
                .CreateLogger();
            Logger = Log.Logger.ForContext<Program>();
        }
        static async Task<int> Main(string[] args)
        {
            Logger.Information("Started");

            var disco = await DiscoveryClient.GetAsync("https://localhost:44372/");
            if (disco.IsError)
            {
                Logger.Error("Discovery Error {Error}", disco.Error);
                return 1;
            }

            // request token
            var tokenClient   = new TokenClient(disco.TokenEndpoint, "client", "secret");
            var tokenResponse = await tokenClient.RequestClientCredentialsAsync("api1");

            if (tokenResponse.IsError)
            {
                Logger.Error("Client Auth Error {Error}", tokenResponse.Error);
                return 1;
            }

            Console.WriteLine(tokenResponse.Json);

            // call api
            var client = new HttpClient();
            client.SetBearerToken(tokenResponse.AccessToken);

            var response = await client.GetAsync("https://localhost:44344/identity");
            if (!response.IsSuccessStatusCode)
            {
                Logger.Error("api error {StatusCode} {Content}", response.StatusCode, await response.Content.ReadAsStringAsync());
                Console.WriteLine(response.StatusCode);
            }
            else
            {
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine(JArray.Parse(content));
            }

            tokenClient = new TokenClient(disco.TokenEndpoint, "ro.client", "secret");
            tokenResponse = await tokenClient.RequestResourceOwnerPasswordAsync("alice", "password", "api1");
            if (tokenResponse.IsError)
            {
                Logger.Error("OwnerPassword request error {Error}", tokenResponse.Error);
            }

            Console.WriteLine(tokenResponse.Json);

            Console.ReadKey();

            return 0;
        }
    }
}
