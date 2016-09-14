using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;

namespace Tests
{
    public class EquipoMontajeController : IClassFixture<TestFixture<WebApp.Startup>>
    {
        private readonly HttpClient _client;


        public EquipoMontajeController(TestFixture<WebApp.Startup> fixture)
        {
            _client = fixture.Client;
        }

        [Fact]
        public async Task TestController() 
        {
            // Act
            var response = await _client.GetAsync("/api/EquipoMontaje");

            // Assert
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();                        
            Assert.False(string.IsNullOrWhiteSpace(responseString));
        }

        [Fact]
        public async Task TestInvalidId() 
        {
            // Act
            var response = await _client.GetAsync("/api/EquipoMontaje/invalidId");

            // Assert       
            Assert.True(response.StatusCode == HttpStatusCode.BadRequest);
        }        
    }
}
