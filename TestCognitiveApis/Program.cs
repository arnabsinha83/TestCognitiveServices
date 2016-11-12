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
            string[] documents = new string[]
                                    {
                                        //"Hello this is a wonderful day at princeton hackathon. This is a sunny day outside.",
                                        //"Boston is depressing during the winter"
                                        "happy",
                                        "sad",
                                    };
            
            MakeRequests(documents).Wait();
            Console.WriteLine("Hit ENTER to exit...");
            Console.ReadKey();
        }

        static async Task MakeRequests(string [] documents)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = new Uri(BaseUrl);

                // Request headers.
                client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", AccountKey);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                // Request body. Insert your text data here in JSON format.
                StringBuilder sb = new StringBuilder();
                sb.Append("{\"documents\":[");
                for (int i = 0; i < documents.Length; i++)
                {
                    string id = String.Format("\"{0}\"", i + 1);
                    string text = String.Format("\"{0}\"", documents[i]);
                    string s2 = String.Format("\"id\":{0},\"text\":{1}", id, text);
                    string s1 = "{" + s2 + "},";
                    sb.Append(s1);
                }
                sb.Append("]}");

                byte[] byteData = Encoding.UTF8.GetBytes(sb.ToString());

                // Detect sentiment:
                var uri = "text/analytics/v2.0/sentiment";
                var response = await CallEndpoint(client, uri, byteData);
                var sentimentObj = JsonConvert.DeserializeObject<EmotionJson.Rootobject>(response);
                Console.WriteLine("\nDetect sentiment response:\n");
                for(int i=0; i<sentimentObj.documents.Length; i++)
                {
                    Console.WriteLine("id: {0}\tscore: {1}\ttext: {2}", sentimentObj.documents[i].id, sentimentObj.documents[i].score, documents[i]);
                }

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
                content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                var response = await client.PostAsync(uri, content);
                return await response.Content.ReadAsStringAsync();
            }
        }

    }
}
