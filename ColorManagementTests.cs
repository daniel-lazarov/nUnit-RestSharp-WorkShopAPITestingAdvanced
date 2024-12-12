using System;
using System.Drawing;
using System.Net;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace WorkShopAPITestingAdvanced
{
	[TestFixture]
	public class ColorManagementTests
	{
        private RestClient restClient;
        private string token;
        private Random random;

        [TearDown]
        public void Dispose()
        {
            restClient.Dispose();
        }

        [SetUp]
        public void Setup()
        {
            restClient = new RestClient(GlobalConstants.BaseUrl);
            token = GlobalConstants.AuthenticateUser("admin@gmail.com", "admin@gmail.com");
            random = new Random();
        }

        [Test]
        public void ColorLifecycleTest()
        {
            //Make post request to create a color with random title
            var addColorRequest = new RestRequest("/color", Method.Post);
            addColorRequest.AddHeader("Authorization", $"Bearer {token}");
            addColorRequest.AddJsonBody(new
            {
                title = $"Color_{random.Next(999, 9999)}"
            });

            var addColorResponse = restClient.Execute(addColorRequest);

            Assert.That(addColorResponse.IsSuccessful, Is.True, "The creation of the color failed");

            //Extract color ID and make get request by ID
            var colorId = JObject.Parse(addColorResponse.Content)["_id"]?.ToString();
            Assert.That(colorId, Is.Not.Null.Or.Empty);

            var getColorRequest = new RestRequest($"/color/{colorId}", Method.Get);

            var getColorResponse = restClient.Execute(getColorRequest);

            Assert.IsTrue(getColorResponse.IsSuccessful);

            //Delete color
            var deleteColorRequest = new RestRequest($"/color/{colorId}", Method.Delete);
            deleteColorRequest.AddHeader("Authorization", $"Bearer {token}");

            var deleteColorResponse = restClient.Execute(deleteColorRequest);

            Assert.IsTrue(deleteColorResponse.IsSuccessful);

            //Make request by color ID to validate the color is deleted
            var verifyRequest = new RestRequest($"/color/{colorId}", Method.Get);

            var verifyResponse = restClient.Execute(verifyRequest);

            Assert.That(verifyResponse.Content, Is.Null.Or.EqualTo("null"));
        }

        [Test]
        public void ColorLifecycleNegativeTest()
        {
            //Make post request to create with invalid token

            var invalidToken = "invalidToken";

            var addColorRequest = new RestRequest("/color", Method.Post);
            addColorRequest.AddHeader("Authorization", $"Bearer {invalidToken}");
            addColorRequest.AddJsonBody(new
            {
                title = $"Color_{random.Next(999, 9999)}"
            });

            var addColorResponse = restClient.Execute(addColorRequest);

            Assert.IsFalse(addColorResponse.IsSuccessful);
            Assert.That(addColorResponse.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));

            //Get color with invalid ID
            var invalidColorId = "invalidColorId";
            var getColorRequest = new RestRequest($"/color/{invalidColorId}", Method.Get);

            var getColorResponse = restClient.Execute(getColorRequest);

            Assert.IsFalse(getColorResponse.IsSuccessful);
            Assert.That(getColorResponse.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));

            //Delete color with invalid color ID
            var deleteColorRequest = new RestRequest($"/color/{invalidColorId}", Method.Delete);
            deleteColorRequest.AddHeader("Authorization", $"Bearer {invalidToken}");

            var deleteColorResponse = restClient.Execute(deleteColorRequest);

            Assert.IsFalse(deleteColorResponse.IsSuccessful);
            Assert.That(deleteColorResponse.StatusCode, Is.EqualTo(HttpStatusCode.InternalServerError));
        }
    }
}

