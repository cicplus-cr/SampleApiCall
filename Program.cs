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

            if (samples != null && samples.Count > 0)
            {
                Console.WriteLine("Pay Statement Samples:");

                foreach (PayStatementSample sample in samples)
                {
                    Console.WriteLine("Name: " + sample.Name);
                    Console.WriteLine("Description: " + sample.Description);
                    Console.WriteLine("~");
                }
            }

            string sampleName = "[SAMPLE NAME]";
            var result = GetSamplePayStatement(sampleName);

            if(result != null)
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
            var endpoint = string.Concat(authorizationEndPoint, "/api/authorize/token");

            client.BaseUrl = new Uri(endpoint);
            IRestRequest req = new RestRequest(endpoint, Method.GET);

            req.AddParameter("userName", userName);
            req.AddParameter("userPassword", userPassword);
            req.AddParameter("companyUrlSuffix", companyUrlSuffix);

            IRestResponse<string> response = client.Execute<string>(req);

            return response.Data;
        }
        public static List<PayStatementSample> GetSamples()
        {
            List<PayStatementSample> result = new List<PayStatementSample>();

            IRestClient client = new RestClient();

            string payStatementServiceEndPoint = config["PayStatementServiceEndPoint"];
            var endpoint = string.Concat(payStatementServiceEndPoint, "/api/PayStatement/samples");

            client.BaseUrl = new Uri(endpoint);
            IRestRequest req = new RestRequest(endpoint, Method.GET);
            req.AddHeader("Authorization", "bearer " + authorizationToken);

            IRestResponse response = client.Execute(req);

            if (response.IsSuccessful)
            {
                result = JsonConvert.DeserializeObject<List<PayStatementSample>>(response.Content);
                return result;
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
            var endpoint = string.Concat(payStatementServiceEndPoint, "/api/PayStatement/samples/pdf");

            client.BaseUrl = new Uri(endpoint);
            IRestRequest req = new RestRequest(endpoint, Method.GET);
            req.AddHeader("Authorization", "bearer " + authorizationToken);
            req.AddParameter("sampleName", sampleName);

            IRestResponse response = client.Execute(req);

            if(response.IsSuccessful)
            {
                return response.RawBytes;
            }
            else
            {
                Console.WriteLine("Error retreiving sample Pay Statement: " + response.Content);
                return null;
            }
        }
        public class PayStatementSample
        {
            public string Name { get; set; }
            public string Description { get; set; }
        }
    }
}
