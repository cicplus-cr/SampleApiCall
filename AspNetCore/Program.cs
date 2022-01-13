using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using RestSharp;
using System;
using System.Collections.Generic;
using System.IO;

namespace CicPlus.Api.Samples
{
    class Program
    {
        private static IConfiguration config;
        private static string authorizationToken;
        public static void Main(string[] args)
        {
            config = new ConfigurationBuilder().AddJsonFile("appsettings.json").Build();

            authorizationToken = GetAuthorizationToken("[YOUR USER NAME]", "[YOUR PASSWORD]", "[YOUR COMPANY URL SUFFIX]");

            var samples = GetSamples();

            if (samples == null || samples.Count == 0)
            {
                Console.WriteLine("No Pay Statement Samples found");
                return;
            }

            Console.WriteLine("Pay Statement Samples:");
            foreach (PayStatementSample sample in samples)
            {
                Console.WriteLine("Name: " + sample.Name);
                Console.WriteLine("Description: " + sample.Description);
                Console.WriteLine("~");
            }

            string sampleName = samples[0].Name;
            var result = GetSamplePayStatement(sampleName);

            if (result != null)
            {
                string path = "c:/" + sampleName + ".pdf";

                File.WriteAllBytes(path, result);
                Console.WriteLine("Sample Pay Statement can be found at " + path);
            }

        }
        public static string GetAuthorizationToken(string userName, string userPassword, string companyUrlSuffix)
        {
            IRestClient client = new RestClient();

            string authorizationEndPoint = config["AuthorizationServiceEndPoint"];
            var endpoint = string.Concat(authorizationEndPoint, "/api/v1/authorize/token");

            client.BaseUrl = new Uri(endpoint);
            IRestRequest req = new RestRequest(endpoint, Method.POST);

            var userSignIn = new UserSignInModel
            {
                UserName = userName,
                Password = userPassword,
                CompanyUrlSuffix = companyUrlSuffix
            };

            req.AddJsonBody(JsonConvert.SerializeObject(userSignIn));

            IRestResponse<ApiResponse> response = client.Execute<ApiResponse>(req);

            var result = JsonConvert.DeserializeObject<ApiResponse>(response.Content);
            return result?.Data as string;
        }

        public static List<PayStatementSample> GetSamples()
        {
            IRestClient client = new RestClient();

            string payStatementServiceEndPoint = config["PayStatementServiceEndPoint"];
            var endpoint = string.Concat(payStatementServiceEndPoint, "/api/v1/paystatement/admin/samples");

            client.BaseUrl = new Uri(endpoint);
            IRestRequest req = new RestRequest(endpoint, Method.GET);
            req.AddHeader("Authorization", "bearer " + authorizationToken);

            IRestResponse response = client.Execute(req);

            if (response.IsSuccessful)
            {
                var result = JsonConvert.DeserializeObject<ApiResponse>(response.Content);
                return JsonConvert.DeserializeObject<List<PayStatementSample>>(result?.Data?.ToString() ?? string.Empty);
            }
            else
            {
                Console.WriteLine("Error retreiving sample Pay Statements: " + response.Content);
                return null;
            }

        }
        public static byte[] GetSamplePayStatement(string sampleName)
        {
            IRestClient client = new RestClient();

            string payStatementServiceEndPoint = config["PayStatementServiceEndPoint"];
            var endpoint = string.Concat(payStatementServiceEndPoint, "/api/v1/paystatement/admin/samples/pdf");

            client.BaseUrl = new Uri(endpoint);
            IRestRequest req = new RestRequest(endpoint, Method.GET);
            req.AddHeader("Authorization", "bearer " + authorizationToken);
            req.AddParameter("sampleName", sampleName);

            IRestResponse response = client.Execute(req);

            if (response.IsSuccessful)
            {
                return response.RawBytes;
            }
            else
            {
                Console.WriteLine("Error retreiving sample Pay Statement: " + response.Content);
                return null;
            }
        }

        public class UserSignInModel
        {
            public string UserName { get; set; }
            public string Password { get; set; }
            public string CompanyUrlSuffix { get; set; }

        }

        public class ApiResponse
        {
            public bool Success { get; set; }
            public string Message { get; set; }
            public object Data { get; set; }
        }

        public class PayStatementSample
        {
            public string Name { get; set; }
            public string Description { get; set; }
        }
    }
}
