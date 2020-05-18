using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using RandomNumberService.Generator;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace RandomNumberServiceTests
{
    [TestClass]
    public class LargeNumberServiceHealthCheck
    {
        [TestInitialize]
        public void Setup()
        {
            NotificationEndpoint = "https://dkendpoint.free.beeceptor.com/SubmitLargeRandomNumber";
        }

        private string NotificationEndpoint { get; set; }

        [TestMethod]
        public void CheckServiceStatusAsync()
        {
            var healthCheckPassed = CallServiceForLargeNumberNotification(899).Result;
            
            Assert.IsTrue(healthCheckPassed);
        }

        private async Task<bool> CallServiceForLargeNumberNotification(int value)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    var randomNumberMessage = new RandomNumberRequest()
                    {
                        number = value
                    };

                    var jsonString = JsonConvert.SerializeObject(randomNumberMessage);
                    var httpResponse = await httpClient.PostAsync(NotificationEndpoint, new StringContent(jsonString, Encoding.UTF8, "application/json"));

                    if (httpResponse.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        return true;
                    }
                }
            }
            catch
            {
                return false;
            }

            return false;
        }
    }
}
