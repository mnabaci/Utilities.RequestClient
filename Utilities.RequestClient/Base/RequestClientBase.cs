using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Utilities.RequestClient.Extensions;
using Utilities.RequestClient.Types;
using Utilities.Serialization;
using Utilities.Serialization.Options;

namespace Utilities.RequestClient.Base
{
    /// <summary>
    /// Request Client Base
    /// </summary>
    public class RequestClientBase
    {
        /// <summary>
        /// Http client handler
        /// </summary>
        private protected readonly HttpClientHandler Handler;

        /// <summary>
        /// Http client
        /// </summary>
        private protected readonly HttpClient Client;

        /// <summary>
        /// Request's encoding
        /// </summary>
        private protected Encoding Encoding = Encoding.UTF8;

        /// <summary>
        /// Default timeout duration as second
        /// </summary>
        private protected const double DefaultTimeoutDuration = 100;

        /// <summary>
        /// Timeout message
        /// </summary>
        private protected const string TimeoutMessage = "Timeout.";

        /// <summary>
        /// Bad request message
        /// </summary>
        private protected const string BadRequestMessage = "Bad Request.";

        /// <summary>
        /// Unknown error message
        /// </summary>
        private protected const string UnknownErrorMessage = "An error has occured.";

        /// <summary>
        /// Request's media type
        /// </summary>
        private protected MediaTypes MediaType = MediaTypes.Json;

        /// <summary>
        /// Base uri
        /// </summary>
        private protected Uri BaseUri { get; }

        /// <summary>
        /// Serialization type for serialize request and response body
        /// </summary>
        private protected SerializationTypes SerializationType
        {
            get
            {
                switch (MediaType)
                {
                        case MediaTypes.Json:
                            return SerializationTypes.Json;
                        case MediaTypes.Xml:
                            return SerializationTypes.Xml;
                        default:
                            return SerializationTypes.Default;
                }
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="baseUri"></param>
        private protected RequestClientBase(Uri baseUri)
        {
            Handler = new HttpClientHandler();
            Client = new HttpClient(Handler)
            {
                Timeout = TimeSpan.FromMinutes(DefaultTimeoutDuration)
            };
            Client.DefaultRequestHeaders.Clear();
            Client.DefaultRequestHeaders.AcceptLanguage.Add(StringWithQualityHeaderValue.Parse("tr-Tr"));
            Client.DefaultRequestHeaders.AcceptLanguage.Add(StringWithQualityHeaderValue.Parse("en-US"));
            Client.DefaultRequestHeaders.ExpectContinue = false;
            Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaType.GetDescription()));
            Client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue(Encoding.BodyName));
            BaseUri = baseUri;
        }

        /// <summary>
        /// Generates string content with given body object
        /// </summary>
        /// <param name="body">Body object</param>
        /// <returns>String content with body object</returns>
        private protected StringContent GenerateStringContent(object body)
        {
            return new StringContent(body.Serialize(SerializationType), Encoding, MediaType.GetDescription());
        }

        /// <summary>
        /// Get complete request url. Concats base uri and given uri
        /// </summary>
        /// <param name="uri">Rest of the full url without base uri</param>
        /// <returns>Complete request url</returns>
        private protected string GetCompleteUrl(string uri)
        {
            return $"{BaseUri}{uri}";
        }
    }
}