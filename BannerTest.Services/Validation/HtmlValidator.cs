using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BannerTest.Services.Validation
{
    public class HtmlValidator : IHtmlValidator
    {
        private readonly HttpClient _apiClient;

        public HtmlValidator(string endpoint)
        {
            _apiClient = new HttpClient { BaseAddress = new Uri(endpoint) };
            _apiClient.DefaultRequestHeaders.UserAgent.ParseAdd("BannerApi");
        }

        public async Task<ValidationResult> Validate(string html)
        {
            var content = new ByteArrayContent(Encoding.UTF8.GetBytes(html));
            content.Headers.ContentType = new MediaTypeHeaderValue("text/html") { CharSet = "utf-8" };

            var resp = await _apiClient.PostAsync("?out=json", content);

            if (resp.IsSuccessStatusCode)
            {
                var respContent = await resp.Content.ReadAsStringAsync();

                var validationResponse = JsonConvert.DeserializeObject<ValidationResponse>(respContent);

                var errorMessages = validationResponse.Messages.Where(m => m.Type == "error").Select(m => m.Message);

                return new ValidationResult { Errors = errorMessages };
            }
            else
            {
                throw new HttpRequestException("HTML validation service is currently unavailable. Please try again later.");
            }
        }

        [DataContract]
        private class ValidationResponse
        {
            [DataMember(Name = "messages")]
            public IEnumerable<ValidationMessage> Messages { get; set; }
        }

        [DataContract]
        private class ValidationMessage
        {
            [DataMember(Name = "message")]
            public string Message { get; set; }

            [DataMember(Name = "type")]
            public string Type { get; set; }
        }
    }
}
