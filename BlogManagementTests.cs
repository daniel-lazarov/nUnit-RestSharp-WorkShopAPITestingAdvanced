using System;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace WorkShopAPITestingAdvanced
{
	[TestFixture]
	public class BlogManagementTests
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
		public void BlogPostLifecycleTest()
		{
			//Create blog post
			var createBlogPostRequest = new RestRequest("/blog", Method.Post);
			createBlogPostRequest.AddHeader("Authorization", $"Bearer {token}");
			createBlogPostRequest.AddJsonBody(new
			{
				Title = $"BlogTitle_{random.Next(999, 9999).ToString()}",
				Description = "Description",
				Category = "SomeCategory"
			});

			var createBlogResponse = restClient.Execute(createBlogPostRequest);

			string blogId = JObject.Parse(createBlogResponse.Content)["id"]?.ToString();

			Assert.That(createBlogResponse.IsSuccessful, Is.True);
			Assert.That(blogId, Is.Not.Null.Or.Empty);

			//Update blog post
			var updateRequest = new RestRequest($"/blog/{blogId}", Method.Put);
			updateRequest.AddHeader("Authorization", $"Bearer {token}");
			updateRequest.AddJsonBody(new
			{
				Title = $"UpdatedTitle_{random.Next(999, 9999)}",
				Description = $"UpdatedDescription",
			});

			var updateResponse = restClient.Execute(updateRequest);

			Assert.That(updateResponse.IsSuccessful, Is.True);

			//Delete Blog Post
			var deleteRequest = new RestRequest($"/blog/{blogId}", Method.Delete);
			deleteRequest.AddHeader("Authorization", $"Bearer {token}");

			var deleteResponse = restClient.Execute(deleteRequest);

			Assert.That(deleteResponse.IsSuccessful, Is.True);

			//Make get request to check if blog post is deleted
			var verifyRequest = new RestRequest($"/blog/{blogId}", Method.Get);

			var verifyResponse = restClient.Execute(verifyRequest);

			Assert.That(verifyResponse.Content, Is.Null.Or.EqualTo("null"));
		}

	}
}

