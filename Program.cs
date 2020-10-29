using System;
using System.Net.Http.Headers;
using System.Text;
using System.Net.Http;
using System.Configuration;
using System.Threading.Tasks;
using System.Text.Json;
using System.Collections.Generic;

namespace SampleApiCall
{
    static class Program
    {
        static void Main()
        {
            Console.WriteLine("hit any key to exit...");
            Console.WriteLine("");
            Console.WriteLine("Getting Authorization Token");
            Console.WriteLine("");

            string token = GetToken();

            Console.WriteLine(token);

            Console.WriteLine("");

            Console.WriteLine("Make Request To Authentication Service");

            MakeRequestToAuthenticationService();


            Console.ReadKey();
        }

        static async void MakeRequestToEchoApi()
        {
            var client = new HttpClient();

            //Your Subscription Key
            string subscriptionKey = "1f02e6fc6f594399bfa9e61e61fefeec";

            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            //The URL of the end point you subscribed to
            var url = "https://cicplus.azure-api.net/echo/resource";

            //Echo API body
            string requestBody = "{\"vehicleType\": \"train\", \"maxSpeed\": 125, \"avgSpeed\": 90, \"speedUnit\": \"mph\"}";  
            byte[] byteData = Encoding.UTF8.GetBytes(requestBody);

            using (var content = new ByteArrayContent(byteData))
            {
                HttpResponseMessage response = await client.PostAsync(url, content);

                Console.WriteLine(response.StatusCode.ToString());
                
            }

        }

        static async void MakeRequestToAuthenticationService()
        {
            var authorizationToken = GetToken();

            var client = new HttpClient();

            //Your Subscription Key
            string subscriptionKey = "1f02e6fc6f594399bfa9e61e61fefeec";

            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", authorizationToken);

            client.DefaultRequestHeaders.Referrer = new Uri("http://SampleApiCall");   // required header

            //The URL of the Authentication End Point 
            var url = "https://cicplus.azure-api.net/AuthenticationData/AuthenticationData/UserList";

            //Echo API body
            StringBuilder sb = new StringBuilder();
            sb.Append("{");
            sb.Append("     \"Version\": \"1\",");
            sb.Append("     \"ValidateFlag\": true,");
            sb.Append("     \"AllowNoAuth\": true,");
            sb.Append("     \"CommitFlag\": true,");
            sb.Append("     \"Employee\": [");
            sb.Append("         {");
            sb.Append("             \"EmployeeId\": \"abc\",");
            sb.Append("             \"AuthValue1\": \"abc\",");
            sb.Append("             \"AuthValue2\": \"abc\",");
            sb.Append("             \"EmployeeStatus\": \"A\"");
            sb.Append("         }");
            sb.Append("     ]");
            sb.Append("}");

            string requestBody = sb.ToString();

            using (var content = new StringContent(requestBody, Encoding.UTF8, "application/json"))
            {
                HttpResponseMessage response = await client.PostAsync(url, content);

                string responseBody = await response.Content.ReadAsStringAsync();

                Console.WriteLine(responseBody);

            }
        }


        private static string GetToken()
        {
            //Key and Secret given to you
            string clientKey = "3f7c63b1-9226-4606-8a49-4119c427e457";
            string clientSecret = "ef088700-d8d2-4309-9dd3-19b10c34b961";

            var tokenResponse = GetAuthorizationToken(clientKey, clientSecret);

            return tokenResponse?.access_token;
        }

        private static TokenResponse GetAuthorizationToken(string key, string secret)
        {
            //Your Subscription Key
            string subscriptionKey = "1f02e6fc6f594399bfa9e61e61fefeec";

            //The URL of the Authorization End Point 
            var authorizationApiUrl = "https://cicplus.azure-api.net/authorization/api/authorize";

            string url = string.Format("{0}?grant_type={1}&scope={2}&state={3}&redirect_uri={4}",
                authorizationApiUrl,
                "client_credentials",
                "ACA PAYSTATEMENT EFORM YEAREND",
                DateTime.Now,
                "paperlessemployee.com/api");

            string identifier = string.Format("{0}:{1}", key, secret);
            byte[] bytes = Encoding.ASCII.GetBytes(identifier);
            string clientIdentifier = Convert.ToBase64String(bytes);

            TokenResponse tokenResponse = null;
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(authorizationApiUrl);

                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("basic", clientIdentifier);

                    client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);

                    var postTask = client.PostAsync(url, null);
                    postTask.Wait();

                    var result = postTask.Result;
                    if (result.IsSuccessStatusCode)
                    {
                        var readTask = result.Content.ReadAsStringAsync();
                        readTask.Wait();
                        tokenResponse = JsonSerializer.Deserialize<TokenResponse>(readTask.Result);
                    }
                    else
                    {
                        Console.WriteLine(result.Content.ReadAsStringAsync().Result);
                    }
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(string.Format("GetAuthorizationToken failed: {0}", e.Message));
            }

            return tokenResponse;
        }


    }

    class TokenResponse
    {
        public string access_token { get; set; }
        public string token_type { get; set; }
        public string state { get; set; }

    }

}
