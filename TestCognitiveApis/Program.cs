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

namespace TestCognitiveApis
{


    public class TextEmotion
    {
        public class Rootobject
        {
            public string type { get; set; }
            public Properties properties { get; set; }
        }

        public class Properties
        {
            public Documents documents { get; set; }
            public Errors errors { get; set; }
        }

        public class Documents
        {
            public string type { get; set; }
            public Items items { get; set; }
        }

        public class Items
        {
            public string type { get; set; }
            public Properties1 properties { get; set; }
        }

        public class Properties1
        {
            public Score score { get; set; }
            public Id id { get; set; }
        }

        public class Score
        {
            public string format { get; set; }
            public string description { get; set; }
            public string type { get; set; }
        }

        public class Id
        {
            public string description { get; set; }
            public string type { get; set; }
        }

        public class Errors
        {
            public string type { get; set; }
            public Items1 items { get; set; }
        }

        public class Items1
        {
            public string type { get; set; }
            public Properties2 properties { get; set; }
        }

        public class Properties2
        {
            public Id1 id { get; set; }
            public Message message { get; set; }
        }

        public class Id1
        {
            public string description { get; set; }
            public string type { get; set; }
        }

        public class Message
        {
            public string description { get; set; }
            public string type { get; set; }
        }
    }

    public class Program
    {
        #region Comment
        //public static string GetJsonResponse(string url)
        //{
        //    HttpWebRequest webrequest = (HttpWebRequest)WebRequest.Create(url);
        //    webrequest.Method = "GET";

        //    // Set the key
        //    webrequest.Headers.Add("Ocp-Apim-Subscription-Key", "2c842aa6c8364a0991f68263caa19603");

        //    // Return the response
        //    HttpWebResponse webresponse = (HttpWebResponse)webrequest.GetResponse();

        //    // Get the json response
        //    Encoding enc = System.Text.Encoding.GetEncoding("utf-8");
        //    string result = (new StreamReader(webresponse.GetResponseStream(), enc)).ReadToEnd();

        //    // Close the webresponse
        //    webresponse.Close();

        //    // Return the json string
        //    return result;
        //}

        //public static async void GetSentiment(string docSentiment)
        //{
        //    var client = new HttpClient();

        //    // Request headers
        //    client.DefaultRequestHeaders
        //            .Accept
        //            .Add(new MediaTypeWithQualityHeaderValue("application/json"));
        //    client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "2c842aa6c8364a0991f68263caa19603");

        //    // Request parameters
        //    var uri = "https://westus.api.cognitive.microsoft.com/text/analytics/v2.0/sentiment";

        //    HttpResponseMessage response;

        //    // Create a basic JSON request body, with a single text 'document'
        //    byte[] byteData = Encoding.UTF8.GetBytes("{\"documents\": [{\"id\": \"1\",\"text\": \"" + docSentiment + "\"}]}");

        //    using (var content = new ByteArrayContent(byteData))
        //    {
        //        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        //        response = await client.PostAsync(uri, content);

        //        using (var stream = await response.Content.ReadAsStreamAsync())
        //        {
        //            Console.WriteLine(stream);
        //            DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(TextEmotion.Rootobject));
        //            object objResponse = jsonSerializer.ReadObject(response.GetResponseStream());
        //            Response jsonResponse
        //            = objResponse as Response;
        //            return jsonResponse;
        //        }


        //    }
        //}
        #endregion

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
                }catch(Exception e)
                {
                    Console.WriteLine(e);
                }
                return null;
            }
        }

    }
}
