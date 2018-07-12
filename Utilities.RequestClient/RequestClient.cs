using System;
using System.Net.Http.Headers;
using System.Text;
using Utilities.RequestClient.Base;
using Utilities.RequestClient.Extensions;
using Utilities.RequestClient.Types;

namespace Utilities.RequestClient
{
    /// <summary>
    /// Easiest way to handle rest requests
    /// </summary>
    /// <inheritdoc cref="IDisposable"/>
    public partial class RequestClient : RequestClientBase, IDisposable
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="baseAddress"></param>
        private RequestClient(Uri baseAddress) : base(baseAddress) { }

        #region Settings

        /// <summary>
        /// Set base uri
        /// </summary>
        /// <param name="uriString">Uri as string</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="FormatException"></exception>
        /// <returns>New <see cref="RequestClient"/> object</returns>
        public static RequestClient SetBaseUri(string uriString)
        {
            if (string.IsNullOrWhiteSpace(uriString)) throw new ArgumentNullException(nameof(uriString));

            if (Uri.TryCreate(uriString, UriKind.Absolute, out var uri) == false) throw new FormatException("Uri format is invalid.");

            return SetBaseUri(uri);
        }

        /// <summary>
        /// Set base uri
        /// </summary>
        /// <param name="uri"></param>
        /// <returns>New <see cref="RequestClient"/> object</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static RequestClient SetBaseUri(Uri uri)
        {
            if (uri == null) throw new ArgumentNullException(nameof(uri));

            return new RequestClient(uri);
        }

        /// <summary>
        /// Set encoding
        /// </summary>
        /// <param name="encoding">Request's encoding</param>
        /// <returns></returns>
        public RequestClient SetEncoding(Encoding encoding)
        {
            Encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
            return this;
        }

        /// <summary>
        /// Set media type
        /// </summary>
        /// <param name="mediaType">Request's media type</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public RequestClient SetMediaType(MediaTypes mediaType)
        {
            if (Enum.IsDefined(typeof(MediaTypes), mediaType) == false) throw new ArgumentException($"{nameof(mediaType)} is invalid.");
            Client.DefaultRequestHeaders.Accept.Remove(new MediaTypeWithQualityHeaderValue(MediaType.GetDescription()));
            MediaType = mediaType;
            Client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(MediaType.GetDescription()));
            return this;
        }

        /// <summary>
        /// Set authorizationHeader 
        /// </summary>
        /// <param name="scheme">Authorization scheme</param>
        /// <param name="parameter">Authorization key</param>
        /// <returns></returns>
        public RequestClient SetAuthorizationHeader(string scheme, string parameter)
        {
            Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(scheme, parameter);
            return this;
        }

        /// <summary>
        /// Set basic authorization header 
        /// </summary>
        /// <param name="parameter">Authorization key</param>
        /// <returns></returns>
        public RequestClient SetBasicAuthorizationHeader(string parameter)
        {
            return SetAuthorizationHeader("Basic", parameter);
        }

        /// <summary>
        /// Set bearer authorization header 
        /// </summary>
        /// <param name="parameter">Authorization key</param>
        /// <returns></returns>
        public RequestClient SetBearerAuthorizationHeader(string parameter)
        {
            return SetAuthorizationHeader("Bearer", parameter);
        }

        /// <summary>
        /// Add custom header to http request
        /// </summary>
        /// <param name="key">Header key</param>
        /// <param name="value">Header value</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns></returns>
        public RequestClient AddHeader(string key, string value)
        {
            if (string.IsNullOrEmpty(key)) throw new ArgumentNullException(nameof(key));
            if (value == null) throw new ArgumentNullException(nameof(value));

            Client.DefaultRequestHeaders.Add(key, value);
            return this;
        }

        /// <summary>
        /// Default timeout duration: 5 minutes
        /// Sets timeout duration for request
        /// </summary>
        /// <param name="milliseconds">Timeout duration as milliseconds</param>
        /// <returns></returns>
        public RequestClient SetTimeout(double milliseconds)
        {
            Client.Timeout = TimeSpan.FromMilliseconds(milliseconds);
            return this;
        }

        #endregion

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Dispose()
        {
            Client.Dispose();
            Handler.Dispose();
        }
    }
}