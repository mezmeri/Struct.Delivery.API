using Delivery.Integration;
using Delivery.Integration.Models;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StackExchange.Redis;
using Struct.Delivery.API;
using System.Net.Http.Json;
using System.Text;

namespace Delivery.Integration
{
    public class Product : IClassFixture<WebhookReceiverFactory>
    {
        private readonly WebApplicationFactory<Program> _factory;

        public Product(WebhookReceiverFactory factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task Post_ProductUpdate_ShouldUpdateRedisTimestamp()
        {
            // Arrange
            int maxIteration = 2;
            HttpClient httpClient = _factory.CreateClient();

            var redis = _factory.Services.GetRequiredService<IConnectionMultiplexer>();
            var db = redis.GetDatabase();

            string productId = "2001";
            string rediskey = $"queue:idmap";

            var originalData = await db.HashGetAsync(rediskey, productId);

            JObject json = JObject.Parse(originalData);
            long originalTimestamp = json.Value<long>("Timestamp");

            string jsonOutput;
            using (StreamReader sr = new StreamReader(Path.Combine(Directory.GetCurrentDirectory(), "Resources", "product-update-webhook-data.json")))
            {
                jsonOutput = await sr.ReadToEndAsync();
            }

            // Act
            for (int i = 0; i < maxIteration; i++)
            {
                using (HttpRequestMessage rm = new HttpRequestMessage(HttpMethod.Post, "https://localhost:7053/api/receiver/productUpdate"))
                {
                    rm.Headers.Add("x-event-key", "products:updated");
                    rm.Content = new StringContent(jsonOutput, Encoding.UTF8, "application/json");

                    var response = await httpClient.SendAsync(rm);
                    response.EnsureSuccessStatusCode();
                }
            }

            // Assert
            var updatedData = await db.HashGetAsync(rediskey, productId);
            JObject newJson = JObject.Parse(updatedData);
            long updatedTimestamp = newJson.Value<long>("Timestamp");

            Assert.True(updatedTimestamp > originalTimestamp);

            httpClient.Dispose();
        }
    }
}