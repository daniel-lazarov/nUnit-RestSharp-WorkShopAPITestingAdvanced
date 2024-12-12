using System;
using System.Drawing;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace WorkShopAPITestingAdvanced
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
        public void ComplexOrderLifecycleTest()
        {
            //Get all products from the system
            var getAllProductsRequest = new RestRequest("/product", Method.Get);

            var getAllProductsResponse = restClient.Execute(getAllProductsRequest);

            Assert.IsTrue(getAllProductsResponse.IsSuccessful);

            var products = JArray.Parse(getAllProductsResponse.Content);

            Assert.That(products.Count, Is.GreaterThan(0));

            //Get the id of the first product
            string productId = products.First()["_id"]?.ToString();
            Assert.That(productId, Is.Not.Null.Or.Empty);

            //Create shopping cart and add the product extracted
            var createCartRequest = new RestRequest("/user/cart", Method.Post);
            createCartRequest.AddHeader("Authorization", $"Bearer {userToken}");
            createCartRequest.AddJsonBody(new
            {
                cart = new[] {
                        new { _id = productId, count = 3, color = "orange"},
                    }
            });

            var createCartResponse = restClient.Execute(createCartRequest);

            Assert.IsTrue(createCartResponse.IsSuccessful);

            //Apply the created coupon
            var applyCouponRequest = new RestRequest("/user/cart/applycoupon", Method.Post);
            applyCouponRequest.AddHeader("Authorization", $"Bearer {userToken}");
            applyCouponRequest.AddJsonBody(new
            {
                Coupon = "BLACKFRIDAY",
            });

            var applyCouponResponse = restClient.Execute(applyCouponRequest);
            Assert.IsTrue(applyCouponResponse.IsSuccessful);

            //Place the order with the applied coupon
            var placeOrderRequest = new RestRequest("/user/cart/cash-order", Method.Post);
            placeOrderRequest.AddHeader("Authorization", $"Bearer {userToken}");
            placeOrderRequest.AddJsonBody(JsonConvert.SerializeObject(new
            {
                COD = true,
                couponApplied = true
            }));

            var placeOrderResponse = restClient.Execute(placeOrderRequest);

            Assert.That(placeOrderResponse.IsSuccessful, Is.True);

            //Get All user orders
            var getUserOrdersRequest = new RestRequest("/user/get-orders", Method.Get);
            getUserOrdersRequest.AddHeader("Authorization", $"Bearer {userToken}");

            var getUserOrdersResponse = restClient.Execute(getUserOrdersRequest);
            Assert.IsTrue(getUserOrdersResponse.IsSuccessful);

            //Get order ID
            var orderId = JObject.Parse(getUserOrdersResponse.Content)["_id"]?.ToString();

            //Update the order and cancel it
            var updateOrderRequest = new RestRequest($"/user/order/update-order/{orderId}", Method.Put);
            updateOrderRequest.AddHeader("Authorization", $"Bearer {adminToken}");
            updateOrderRequest.AddJsonBody(new
            {
                Status = "Cancelled",
            });

            var updateOrderResponse = restClient.Execute(updateOrderRequest);

            Assert.IsTrue(updateOrderResponse.IsSuccessful);

        }
    }
}

