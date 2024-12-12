using System;
using System.Drawing;
using System.Net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace WorkShopAPITestingAdvanced
{
    [TestFixture]
    public class CouponManagementTests
    {
        public class ColorManagementTests
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
            public void CouponLifecycleTest()
            {
                //Get all products from the system
                var getAllProductsRequest = new RestRequest("/product", Method.Get);

                var getAllProductsResponse = restClient.Execute(getAllProductsRequest);

                Assert.IsTrue(getAllProductsResponse.IsSuccessful);

                var products = JArray.Parse(getAllProductsResponse.Content);

                Assert.That(products.Count, Is.GreaterThanOrEqualTo(2));

                //Get two random products from the array by using array index
                var productIds = products.Select(p => p["_id"].ToString()).ToList();

                var firstProductId = productIds[random.Next(productIds.Count)];
                var secondProductId = productIds[random.Next(productIds.Count)];

                //If we have same products we distinct them by generating new random second ID
                while (firstProductId == secondProductId)
                {
                    secondProductId = productIds[random.Next(productIds.Count)];
                }

                //Create new coupon
                string couponName = $"DISCOUNT20_{random.Next(999, 9999)}";
                var createCouponRequest = new RestRequest("/coupon", Method.Post);
                createCouponRequest.AddHeader("Authorization", $"Bearer {adminToken}");
                createCouponRequest.AddJsonBody(new
                {
                    Name = couponName,
                    Expiry = "2024-09-30T23:59:59Z",
                    Discount = 20
                });

                var createCouponResponse = restClient.Execute(createCouponRequest);

                Assert.IsTrue(createCouponResponse.IsSuccessful);

                var couponId = JObject.Parse(createCouponResponse.Content)["_id"]?.ToString();
                Assert.That(couponId, Is.Not.Null.Or.Empty);

                //Create shopping cart and add the two products extracted
                var createCartRequest = new RestRequest("/user/cart", Method.Post);
                createCartRequest.AddHeader("Authorization", $"Bearer {userToken}");
                createCartRequest.AddJsonBody(new
                {
                    cart = new [] {
                        new { _id = firstProductId, count = 1, color = "red"},
                        new { _id = secondProductId, count = 2, color = "blue"}
                    }
                });

                var createCartResponse = restClient.Execute(createCartRequest);

                Assert.IsTrue(createCartResponse.IsSuccessful);

                //Apply the created coupon
                var applyCouponRequest = new RestRequest("/user/cart/applycoupon", Method.Post);
                applyCouponRequest.AddHeader("Authorization", $"Bearer {userToken}");
                applyCouponRequest.AddJsonBody(new
                {
                    Coupon = couponName,
                }) ;

                var applyCouponResponse = restClient.Execute(applyCouponRequest);
                Assert.IsTrue(applyCouponResponse.IsSuccessful);


                //Delete the created coupon
                var deleteCouponRequest = new RestRequest($"/coupon/{couponId}", Method.Delete);
                deleteCouponRequest.AddHeader("Authorization", $"Bearer {adminToken}");

                var deleteCouponResponse = restClient.Execute(deleteCouponRequest);

                Assert.IsTrue(deleteCouponResponse.IsSuccessful);

                //Get the coupon and assert it is deleted
                var verifyRequest = new RestRequest($"/coupon/{couponId}", Method.Get);
                verifyRequest.AddHeader("Authorization", $"Bearer {adminToken}");

                var verifyResponse = restClient.Execute(verifyRequest);

                Assert.That(verifyResponse.Content, Is.Null.Or.EqualTo("null"));

            }

            [Test]
            public void CouponApplicationToOrderTest()
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

                //Create new coupon
                string couponName = $"SAVE10_{random.Next(999, 9999)}";
                var createCouponRequest = new RestRequest("/coupon", Method.Post);
                createCouponRequest.AddHeader("Authorization", $"Bearer {adminToken}");
                createCouponRequest.AddJsonBody(new
                {
                    Name = couponName,
                    Expiry = "2024-09-30T23:59:59Z",
                    Discount = 20
                });

                var createCouponResponse = restClient.Execute(createCouponRequest);

                Assert.IsTrue(createCouponResponse.IsSuccessful);

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
                    Coupon = couponName,
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
            }
        }
    }
}

