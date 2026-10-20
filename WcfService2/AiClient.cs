using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web;
using static System.Net.WebRequestMethods;

namespace WcfService2
{
    public class AiClient
    {
        private static readonly HttpClient http = new HttpClient();// { Timeout = TimeSpan.FromSeconds(30) };
        private readonly string apiKey = ConfigurationManager.AppSettings["AiApiKey"];
        private readonly string model = ConfigurationManager.AppSettings["AiModel"];
        //string apiKey= "AQ.Ab8RN6JuOy_7Dm0iod2qrxk0weVJrvKSBoWFky2BxUflbC52Gw";
        public string Ask(String task)
        {
            string role = "";
            string rules = "Rules:";
            string response_format = "Return JSON only, in this format:{\"category\": \"...\", \"priority\": 1, \"reason\": \"...\"}";
            string prompt = task;
            string url = "https://generativelanguage.googleapis.com/v1beta/models/" + model + ":generateContent";
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Headers.Add("x-goog-api-key", apiKey);
            request.Content = new StringContent(BuildRequestJson(prompt), Encoding.UTF8, "application/json");

            try
            {
                HttpResponseMessage response = http.SendAsync(request).Result;
                if (!response.IsSuccessStatusCode)
                    return null;                       // the service shows an error message

                string body = response.Content.ReadAsStringAsync().Result;
                return ExtractText(body);
            }
            catch (Exception)
            {
                return null;                           // no internet, timeout, bad answer
            }
        }

        // builds: { "contents": [ { "parts": [ { "text": prompt } ] } ] }
        private string BuildRequestJson(string prompt)
        {
            var part = new JObject { ["text"] = prompt };
            var content = new JObject { ["parts"] = new JArray(part) };
            var root = new JObject { ["contents"] = new JArray(content),
                ["generationConfig"] = new JObject { ["responseMimeType"] = "application/json" }
            };
            return root.ToString();
        }
        // reads: candidates[0].content.parts[0].text
        private string ExtractText(string body)
        {
            JObject json = JObject.Parse(body);
            return (string)json.SelectToken("candidates[0].content.parts[0].text");
        }

    }
}