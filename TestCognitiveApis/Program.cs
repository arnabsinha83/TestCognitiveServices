using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using System.Web;
using System.ServiceModel.Web;
using System.Globalization;
using Newtonsoft.Json;

namespace TestCognitiveApis
{
    public class EmotionJson
    {
        public class Rootobject
        {
            public Document[] documents { get; set; }
            public Error[] errors { get; set; }
        }

        public class Document
        {
            public float score { get; set; }
            public string id { get; set; }
        }

        public class Error
        {
            public string id { get; set; }
            public string message { get; set; }
        }
    }

    public class Program
    {
        // Reference: https://text-analytics-demo.azurewebsites.net/Home/SampleCode
        private const string BaseUrl = "https://westus.api.cognitive.microsoft.com/";

        /// <summary>
        /// Your account key goes here.
        /// </summary>
        private const string AccountKey = "2c842aa6c8364a0991f68263caa19603";

        /// <summary>
        /// Maximum number of languages to return in language detection API.
        /// </summary>
        private const int NumLanguages = 1;
        static void Main(string[] args)
        {
            MakeRequests().Wait();
            Console.WriteLine("Hit ENTER to exit...");
            Console.ReadKey();
        }

        static async Task MakeRequests()
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(BaseUrl);

                // Request headers.
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", AccountKey);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // Request body. Insert your text data here in JSON format.
                byte[] byteData = Encoding.UTF8.GetBytes("{\"documents\":[" +
                    "{\"id\":\"1\",\"text\":\"hello world\"}," +
                    "{\"id\":\"2\",\"text\":\"hello foo world\"}," +
                    "{\"id\":\"three\",\"text\":\"hello my world\"},]}");

                // Detect sentiment:
                var uri = "text/analytics/v2.0/sentiment";
                var response = await CallEndpoint(client, uri, byteData);
                var sentimentObj = JsonConvert.DeserializeObject<EmotionJson.Rootobject>(response);
                Console.WriteLine("\nDetect sentiment response:\n" + response);

                // Detect key phrases:
                uri = "text/analytics/v2.0/keyPhrases";
                response = await CallEndpoint(client, uri, byteData);
                Console.WriteLine("\nDetect key phrases response:\n" + response);

                // Detect language:
                var queryString = HttpUtility.ParseQueryString(string.Empty);
                queryString["numberOfLanguagesToDetect"] = NumLanguages.ToString(CultureInfo.InvariantCulture);
                uri = "text/analytics/v2.0/languages?" + queryString;
                response = await CallEndpoint(client, uri, byteData);
                Console.WriteLine("\nDetect language response:\n" + response);


            }
        }

        static async Task<String> CallEndpoint(HttpClient client, string uri, byte[] byteData)
        {
            using (HttpContent content = new ByteArrayContent(byteData))
            {
                try
                {
                    content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                    var response = await client.PostAsync(uri, content);
                    return await response.Content.ReadAsStringAsync();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                return null;
            }
        }

    }
}
