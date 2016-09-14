using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using System;
using System.Net.Http;

namespace Tests
{
    /// <summary>
    /// A test fixture which hosts the target project (project we wish to test).
    /// </summary>
    /// <typeparam name="TStartup">Target project's startup type</typeparam>
    public class TestFixture<TStartup> : IDisposable
    {
        public readonly TestServer Server;
        public readonly HttpClient Client;

        public TestFixture()
        {
            Server = new TestServer(new WebHostBuilder()                 
                .UseStartup(typeof(TStartup)));

            Client = Server.CreateClient();          
        }

        public void Dispose()
        {
            Client.Dispose();
            Server.Dispose();
        }        
    }
}