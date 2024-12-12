using System;
namespace WorkShopAPITestingAdvanced
{
    [TestFixture]
    public class ProductManagementTests
    {
        [TestFixture]
        public class OrderManagementTests
        {
            private RestClient restClient;
            private string adminToken;
            private string userToken;
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
                adminToken = GlobalConstants.AuthenticateUser("admin@gmail.com", "admin@gmail.com");
                userToken = GlobalConstants.AuthenticateUser("john.doe@example.com", "password123");
                random = new Random();
            }
            [Test]
            public void ProductLifecycleTest()
            {
                //Create new product
                var createProductRequest = new RestRequest("/product", Method.Post);
                createProductRequest.AddHeader("Authorization", $"Bearer {adminToken}");
                var productTitle = $"ProductTitle_{random.Next(999,9999)}";
                createProductRequest.AddJsonBody(new
                {
                    Title = productTitle,
                    Description = "Description",
                    Slug = productTitle,
                    Price = 9.99,
                    Category = "Category",
                    Brand = "Sony",
                    Quantity = 10,
                });

                var createProductResponse = restClient.Execute(createProductRequest);
                Assert.IsTrue(createProductResponse.IsSuccessful);

                var productId = JObject.Parse(createProductResponse.Content)["_id"]?.ToString();

                //Get the created product
                var getProductRequest = new RestRequest($"/product/{productId}", Method.Get);

                var getProductResponse = restClient.Execute(getProductRequest);

                Assert.IsTrue(getProductResponse.IsSuccessful);
                Assert.That(getProductResponse.Content, Is.Not.Null.Or.Empty);

                //Update the product
                var updateProductRequest = new RestRequest($"/product/{productId}", Method.Put);
                updateProductRequest.AddHeader("Authorization", $"Bearer {adminToken}");
                var updatedProductTitle = $"UpdatedProductTitle_{random.Next(999, 9999)}";
                updateProductRequest.AddJsonBody(new
                {
                    title = updatedProductTitle,
                    description = "Updated Description",
                    price = 19.99,
                });

                var updateProductResponse = restClient.Execute(updateProductRequest);
                Assert.IsTrue(updateProductResponse.IsSuccessful);
            }
        }
    }
}