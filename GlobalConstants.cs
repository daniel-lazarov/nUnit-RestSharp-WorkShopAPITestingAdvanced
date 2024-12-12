using System;
using System.Net;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace WorkShopAPITestingAdvanced
{
	public static class GlobalConstants
	{
		public const string BaseUrl = "http://localhost:5050/api";

		public static string AuthenticateUser(string email, string password)
		{
			string resource = "";
			if(email  == "admin@gmail.com")
			{
				resource = "user/admin-login";
			}
			else
			{
				resource = "user/login";
			}

			var restClient = new RestClient(BaseUrl);
			var request = new RestRequest(resource, Method.Post);
			request.AddJsonBody(new { email, password });

			var response = restClient.Post(request);

			if(response.StatusCode != HttpStatusCode.OK)
			{
				Assert.Fail($"Authentication failed. Status code {response.StatusCode} with content: {response.Content}");

			};

			var content = JObject.Parse(response.Content);
			return content["token"]?.ToString();


		}

    }
}

