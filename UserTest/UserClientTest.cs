using System.Net.Http;
using System.Threading.Tasks;
using IdentityModel.Client;
using Xunit;

namespace UserTest
{
    public class UserClientTest
    {
        [Fact]
        public async Task ClientApiTest()
        {
            //get access_token
            var disco = await DiscoveryClient.GetAsync("http://localhost:8000");//授权中心
            var tokenClient = new TokenClient(disco.TokenEndpoint, "Client", "secret");
            var tokenResponse = await tokenClient.RequestClientCredentialsAsync("UserApi");

            var client = new HttpClient();
            client.SetBearerToken(tokenResponse.AccessToken);//add bearer with access_token
            var response = await client.GetAsync("http://localhost:5001/api/Values");//call API with access_token
            var apiResult = response.Content.ReadAsStringAsync().Result;
            Assert.NotEmpty(apiResult);
        }

        [Fact]
        public async Task PasswordApiTests()
        {
            var disco = await DiscoveryClient.GetAsync("http://localhost:8000");
            var tokenClient = new TokenClient(disco.TokenEndpoint, "ro.Client", "secret");
            var tokenResponse = await tokenClient.RequestResourceOwnerPasswordAsync("qwerty", "a123", "UserApi");

            var client = new HttpClient();
            client.SetBearerToken(tokenResponse.AccessToken);//add bearer with access_token
            var response = await client.GetAsync("http://localhost:5001/api/Values");//call API with access_token
            var apiResult = response.Content.ReadAsStringAsync().Result;
            Assert.NotEmpty(apiResult);
        }

    }
}
