using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Utilities.RequestClient.Result;
using Utilities.Serialization;

namespace Utilities.RequestClient
{
    /// <summary>
    /// Easiest way to handle rest requests
    /// </summary>
    /// <inheritdoc cref="IDisposable"/>
    public class RequestClient : IDisposable
    {
        /// <summary>
        /// Http client handler
        /// </summary>
        private readonly HttpClientHandler _handler;

        /// <summary>
        /// Http client
        /// </summary>
        private readonly HttpClient _client;

        /// <summary>
        /// Request's encoding
        /// </summary>
        private Encoding _encoding = Encoding.UTF8;

        /// <summary>
        /// Default timeout duration as second
        /// </summary>
        private const double DefaultTimeoutDuration = 100;

        /// <summary>
        /// Timeout message
        /// </summary>
        private const string TimeoutMessage = "Timeout.";

        /// <summary>
        /// Bad request message
        /// </summary>
        private const string BadRequestMessage = "Bad Request.";

        /// <summary>
        /// Unknown error message
        /// </summary>
        private const string UnknownErrorMessage = "An error has occured.";

        /// <summary>
        /// Request's media type
        /// </summary>
        private string _mediaType = "application/json";

        /// <summary>
        /// Base uri
        /// </summary>
        private Uri BaseUri { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="baseAddress"></param>
        private RequestClient(Uri baseAddress)
        {
            _handler = new HttpClientHandler();
            _client = new HttpClient(_handler)
            {
                Timeout = TimeSpan.FromMinutes(DefaultTimeoutDuration)
            };
            _client.DefaultRequestHeaders.Clear();
            _client.DefaultRequestHeaders.AcceptLanguage.Add(StringWithQualityHeaderValue.Parse("tr-Tr"));
            _client.DefaultRequestHeaders.AcceptLanguage.Add(StringWithQualityHeaderValue.Parse("en-US"));
            _client.DefaultRequestHeaders.ExpectContinue = false;
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(_mediaType));
            _client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue(_encoding.BodyName));
            BaseUri = baseAddress;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Dispose()
        {
            _client.Dispose();
            _handler.Dispose();
        }

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
            _encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
            return this;
        }

        /// <summary>
        /// Set media type
        /// </summary>
        /// <param name="mediaType">Request's media type</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public RequestClient SetMediaType(string mediaType)
        {
            if (string.IsNullOrEmpty(mediaType)) throw new ArgumentNullException(nameof(mediaType));

            _client.DefaultRequestHeaders.Accept.Remove(new MediaTypeWithQualityHeaderValue(_mediaType));
            _mediaType = mediaType;
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(_mediaType));
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
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(scheme, parameter);
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

            _client.DefaultRequestHeaders.Add(key, value);
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
            _client.Timeout = TimeSpan.FromMilliseconds(milliseconds);
            return this;
        }

        #endregion

        #region Get

        /// <summary>
        /// Sync Http Get request with uri
        /// </summary>
        /// <typeparam name="T">DTO typed object</typeparam>
        /// <param name="uri">Api uri without base uri</param>
        /// <returns>Structed deserialized response</returns>
        public RequestResult<T> Get<T>(string uri)
            where T : class
        {
            try
            {
                var response = Task.Run(() => _client.GetAsync(GetCompleteUrl(uri))).Result;
                var responseContent = Task.Run(() => response.Content.ReadAsStringAsync()).Result;
                var responseObject = responseContent.Deserialize<T>();
                return new RequestResult<T> { Result = responseObject, StatusCode = response.StatusCode, ExceptionDetail = response.StatusCode == HttpStatusCode.OK ? string.Empty : responseContent };
            }
            catch (AggregateException ae)
            {
                if (ae.InnerException is HttpRequestException)
                {
                    return new RequestResult<T> { StatusCode = HttpStatusCode.BadRequest, ExceptionDetail = BadRequestMessage, Exception = ae };
                }

                if (ae.InnerException is TaskCanceledException)
                {
                    return new RequestResult<T> { StatusCode = HttpStatusCode.GatewayTimeout, ExceptionDetail = TimeoutMessage, Exception = ae };
                }

                return new RequestResult<T> { StatusCode = HttpStatusCode.RequestTimeout, ExceptionDetail = TimeoutMessage, Exception = ae };
            }
            catch (Exception ex)
            {
                return new RequestResult<T> { StatusCode = HttpStatusCode.InternalServerError, ExceptionDetail = UnknownErrorMessage, Exception = ex };
            }
        }

        /// <summary>
        /// Sync Http Get request with uri
        /// </summary>
        /// <param name="uri">Api uri without base uri</param>
        /// <returns>Result as string</returns>
        public RequestResult<string> Get(string uri)
        {
            return Get<string>(uri);
        }

        #endregion

        #region GetAsync

        /// <summary>
        /// Async Http Get request with uri
        /// </summary>
        /// <typeparam name="T">DTO typed object</typeparam>
        /// <param name="uri">Api uri without base uri</param>
        /// <returns>Structed deserialized response</returns>
        public async Task<RequestResult<T>> GetAsync<T>(string uri) where T : class
        {
            try
            {
                using (var response = await _client.GetAsync(GetCompleteUrl(uri)))
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var responseObject = responseContent.Deserialize<T>();
                    return new RequestResult<T> { Result = responseObject, StatusCode = response.StatusCode, ExceptionDetail = response.StatusCode == HttpStatusCode.OK ? string.Empty : responseContent };
                }
            }
            catch (HttpRequestException ex)
            {
                return new RequestResult<T> { StatusCode = HttpStatusCode.BadRequest, ExceptionDetail = BadRequestMessage, Exception = ex };
            }
            catch (TaskCanceledException ex)
            {
                return new RequestResult<T> { StatusCode = HttpStatusCode.GatewayTimeout, ExceptionDetail = TimeoutMessage, Exception = ex };
            }
            catch (Exception ex)
            {
                return new RequestResult<T> { StatusCode = HttpStatusCode.InternalServerError, ExceptionDetail = UnknownErrorMessage, Exception = ex };
            }
        }

        /// <summary>
        /// Async Http Get request with uri
        /// </summary>
        /// <param name="uri">Api uri without base uri</param>
        /// <returns>Result as string</returns>
        public async Task<RequestResult<string>> GetAsync(string uri)
        {
            return await GetAsync<string>(uri);
        }

        #endregion
        
        #region Post

        /// <summary>
        /// Sync Http Post request with uri and body object
        /// </summary>
        /// <typeparam name="T">DTO typed object</typeparam>
        /// <typeparam name="TK">DTO typed object</typeparam>
        /// <param name="uri">Api uri without base uri</param>
        /// <param name="body">DTO typed object</param>
        /// <returns>Structed deserialized response</returns>
        public RequestResult<T> Post<T, TK>(string uri, TK body)
            where TK : class
            where T : class
        {
            try
            {
                var content = GenerateStringContent(body);
                var response = Task.Run(() => _client.PostAsync(GetCompleteUrl(uri), content)).Result;
                var responseContent = Task.Run(() => response.Content.ReadAsStringAsync()).Result;
                var responseObject = responseContent.Deserialize<T>();
                return new RequestResult<T> { Result = responseObject, StatusCode = response.StatusCode, ExceptionDetail = response.StatusCode == HttpStatusCode.OK ? string.Empty : responseContent };
            }
            catch (AggregateException ae)
            {
                if (ae.InnerException is HttpRequestException)
                {
                    return new RequestResult<T> { StatusCode = HttpStatusCode.BadRequest, ExceptionDetail = BadRequestMessage, Exception = ae };
                }

                if (ae.InnerException is TaskCanceledException)
                {
                    return new RequestResult<T> { StatusCode = HttpStatusCode.GatewayTimeout, ExceptionDetail = TimeoutMessage, Exception = ae };
                }

                return new RequestResult<T> { StatusCode = HttpStatusCode.RequestTimeout, ExceptionDetail = TimeoutMessage, Exception = ae };
            }
            catch (Exception ex)
            {
                return new RequestResult<T> { StatusCode = HttpStatusCode.InternalServerError, ExceptionDetail = UnknownErrorMessage, Exception = ex };
            }
        }

        /// <summary>
        /// Sync Http Post request with uri and body object
        /// </summary>
        /// <typeparam name="T">DTO typed object</typeparam>
        /// <param name="uri">Api uri without base uri</param>
        /// <param name="body">DTO typed object</param>
        /// <returns>Structed deserialized response</returns>
        public RequestResult<T> Post<T>(string uri, object body)
            where T : class
        {
            return Post<T, object>(uri, body);
        }

        /// <summary>
        /// Sync Http Post request with uri and body object
        /// </summary>
        /// <param name="uri">Api uri without base uri</param>
        /// <param name="body">DTO typed object</param>
        /// <returns>Result as string</returns>
        public RequestResult<string> Post(string uri, object body)
        {
            return Post<string>(uri, body);
        }

        /// <summary>
        /// Sync Http Post request with uri
        /// </summary>
        /// <typeparam name="T">DTO typed object</typeparam>
        /// <param name="uri">Api uri without base uri</param>
        /// <returns>Structed deserialized response</returns>
        public RequestResult<T> Post<T>(string uri)
            where T : class
        {
            return Post<T>(uri, null);
        }

        /// <summary>
        /// Sync Http Post request with uri
        /// </summary>
        /// <param name="uri">Api uri without base uri</param>
        /// <returns>Result as string</returns>
        public RequestResult<string> Post(string uri)
        {
            return Post<string>(uri);
        }

        #endregion

        #region PostAsync

        /// <summary>
        /// Async Http Post request with uri and body object
        /// </summary>
        /// <typeparam name="T">DTO typed object</typeparam>
        /// <typeparam name="TK">DTO typed object</typeparam>
        /// <param name="uri">Api uri without base uri</param>
        /// <param name="body">DTO typed object</param>
        /// <returns>Structed deserialized response</returns>
        public async Task<RequestResult<T>> PostAsync<T, TK>(string uri, TK body)
            where T : class
            where TK : class
        {
            try
            {
                var content = GenerateStringContent(body);
                using (var response = await _client.PostAsync(GetCompleteUrl(uri), content))
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var responseObject = responseContent.Deserialize<T>();
                    return new RequestResult<T> { Result = responseObject, StatusCode = response.StatusCode, ExceptionDetail = response.StatusCode == HttpStatusCode.OK ? string.Empty : responseContent };
                }
            }
            catch (HttpRequestException ex)
            {
                return new RequestResult<T> { StatusCode = HttpStatusCode.BadRequest, ExceptionDetail = BadRequestMessage, Exception = ex };
            }
            catch (TaskCanceledException ex)
            {
                return new RequestResult<T> { StatusCode = HttpStatusCode.GatewayTimeout, ExceptionDetail = TimeoutMessage, Exception = ex };
            }
            catch (Exception ex)
            {
                return new RequestResult<T> { StatusCode = HttpStatusCode.InternalServerError, ExceptionDetail = UnknownErrorMessage, Exception = ex };
            }
        }

        /// <summary>
        /// Async Http Post request with uri and body object
        /// </summary>
        /// <typeparam name="T">DTO typed object</typeparam>
        /// <param name="uri">Api uri without base uri</param>
        /// <param name="body">DTO typed object</param>
        /// <returns>Structed deserialized response</returns>
        public async Task<RequestResult<T>> PostAsync<T>(string uri, object body)
            where T : class
        {
            return await PostAsync<T, object>(uri, body);
        }

        /// <summary>
        /// Async Http Post request with uri and body object
        /// </summary>
        /// <param name="uri">Api uri without base uri</param>
        /// <param name="body">DTO typed object</param>
        /// <returns>Result as string</returns>
        public async Task<RequestResult<string>> PostAsync(string uri, object body)
        {
            return await PostAsync<string>(uri, body);
        }

        /// <summary>
        /// Async Http Post request with uri
        /// </summary>
        /// <typeparam name="T">DTO typed object</typeparam>
        /// <param name="uri">Api uri without base uri</param>
        /// <returns>Structed deserialized response</returns>
        public async Task<RequestResult<T>> PostAsync<T>(string uri)
            where T : class
        {
            return await PostAsync<T>(uri, null);
        }

        /// <summary>
        /// Async Http Post request with uri
        /// </summary>
        /// <param name="uri">Api uri without base uri</param>
        /// <returns>Result as string</returns>
        public async Task<RequestResult<string>> PostAsync(string uri)
        {
            return await PostAsync<string>(uri);
        }

        #endregion

        #region Put

        /// <summary>
        /// Sync Http Put request with uri and body object
        /// </summary>
        /// <typeparam name="T">DTO typed object</typeparam>
        /// <typeparam name="TK">DTO typed object</typeparam>
        /// <param name="uri">Api uri without base uri</param>
        /// <param name="body">DTO typed object</param>
        /// <returns>Structed deserialized response</returns>
        public RequestResult<T> Put<T, TK>(string uri, TK body)
            where TK : class
            where T : class
        {
            try
            {
                var content = GenerateStringContent(body);
                var response = Task.Run(() => _client.PutAsync(GetCompleteUrl(uri), content)).Result;
                var responseContent = Task.Run(() => response.Content.ReadAsStringAsync()).Result;
                var responseObject = responseContent.Deserialize<T>();
                return new RequestResult<T> { Result = responseObject, StatusCode = response.StatusCode, ExceptionDetail = response.StatusCode == HttpStatusCode.OK ? string.Empty : responseContent };
            }
            catch (AggregateException ae)
            {
                if (ae.InnerException is HttpRequestException)
                {
                    return new RequestResult<T> { StatusCode = HttpStatusCode.BadRequest, ExceptionDetail = BadRequestMessage, Exception = ae };
                }

                if (ae.InnerException is TaskCanceledException)
                {
                    return new RequestResult<T> { StatusCode = HttpStatusCode.GatewayTimeout, ExceptionDetail = TimeoutMessage, Exception = ae };
                }

                return new RequestResult<T> { StatusCode = HttpStatusCode.RequestTimeout, ExceptionDetail = TimeoutMessage, Exception = ae };
            }
            catch (Exception ex)
            {
                return new RequestResult<T> { StatusCode = HttpStatusCode.InternalServerError, ExceptionDetail = UnknownErrorMessage, Exception = ex };
            }
        }

        /// <summary>
        /// Sync Http Put request with uri and body object
        /// </summary>
        /// <typeparam name="T">DTO typed object</typeparam>
        /// <param name="uri">Api uri without base uri</param>
        /// <param name="body">DTO typed object</param>
        /// <returns>Structed deserialized response</returns>
        public RequestResult<T> Put<T>(string uri, object body)
            where T : class
        {
            return Put<T, object>(uri, body);
        }

        /// <summary>
        /// Sync Http Put request with uri and body object
        /// </summary>
        /// <param name="uri">Api uri without base uri</param>
        /// <param name="body">DTO typed object</param>
        /// <returns>Result as string</returns>
        public RequestResult<string> Put(string uri, object body)
        {
            return Put<string>(uri, body);
        }

        /// <summary>
        /// Sync Http Put request with uri
        /// </summary>
        /// <typeparam name="T">DTO typed object</typeparam>
        /// <param name="uri">Api uri without base uri</param>
        /// <returns>Structed deserialized response</returns>
        public RequestResult<T> Put<T>(string uri)
            where T : class
        {
            return Put<T>(uri, null);
        }

        /// <summary>
        /// Sync Http Put request with uri
        /// </summary>
        /// <param name="uri">Api uri without base uri</param>
        /// <returns>Result as string</returns>
        public RequestResult<string> Put(string uri)
        {
            return Put<string>(uri);
        }

        #endregion

        #region PutAsync

        /// <summary>
        /// Async Http Put request with uri and body object
        /// </summary>
        /// <typeparam name="T">DTO typed object</typeparam>
        /// <typeparam name="TK">DTO typed object</typeparam>
        /// <param name="uri">Api uri without base uri</param>
        /// <param name="body">DTO typed object</param>
        /// <returns>Structed deserialized response</returns>
        public async Task<RequestResult<T>> PutAsync<T, TK>(string uri, TK body)
            where T : class
            where TK : class
        {
            try
            {
                var content = GenerateStringContent(body);
                using (var response = await _client.PutAsync(GetCompleteUrl(uri), content))
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var responseObject = responseContent.Deserialize<T>();
                    return new RequestResult<T> { Result = responseObject, StatusCode = response.StatusCode, ExceptionDetail = response.StatusCode == HttpStatusCode.OK ? string.Empty : responseContent };
                }
            }
            catch (HttpRequestException ex)
            {
                return new RequestResult<T> { StatusCode = HttpStatusCode.BadRequest, ExceptionDetail = BadRequestMessage, Exception = ex };
            }
            catch (TaskCanceledException ex)
            {
                return new RequestResult<T> { StatusCode = HttpStatusCode.GatewayTimeout, ExceptionDetail = TimeoutMessage, Exception = ex };
            }
            catch (Exception ex)
            {
                return new RequestResult<T> { StatusCode = HttpStatusCode.InternalServerError, ExceptionDetail = UnknownErrorMessage, Exception = ex };
            }
        }

        /// <summary>
        /// Async Http Put request with uri and body object
        /// </summary>
        /// <typeparam name="T">DTO typed object</typeparam>
        /// <param name="uri">Api uri without base uri</param>
        /// <param name="body">DTO typed object</param>
        /// <returns>Structed deserialized response</returns>
        public async Task<RequestResult<T>> PutAsync<T>(string uri, object body)
             where T : class
        {
            return await PutAsync<T, object>(uri, body);
        }

        /// <summary>
        /// Async Http Put request with uri and body object
        /// </summary>
        /// <param name="uri">Api uri without base uri</param>
        /// <param name="body">DTO typed object</param>
        /// <returns>Result as string</returns>
        public async Task<RequestResult<string>> PutAsync(string uri, object body)
        {
            return await PutAsync<string>(uri, body);
        }

        /// <summary>
        /// Async Http Put request with uri
        /// </summary>
        /// <typeparam name="T">DTO typed object</typeparam>
        /// <param name="uri">Api uri without base uri</param>
        /// <returns>Structed deserialized response</returns>
        public async Task<RequestResult<T>> PutAsync<T>(string uri)
            where T : class
        {
            return await PutAsync<T>(uri, null);
        }

        /// <summary>
        /// Async Http Put request with uri
        /// </summary>
        /// <param name="uri">Api uri without base uri</param>
        /// <returns>Result as string</returns>
        public async Task<RequestResult<string>> PutAsync(string uri)
        {
            return await PutAsync<string>(uri);
        }

        #endregion

        #region Delete

        /// <summary>
        /// Sync Http Delete request with uri
        /// </summary>
        /// <typeparam name="T">DTO typed object</typeparam>
        /// <param name="uri">Api uri without base uri</param>
        /// <returns>Structed deserialized response</returns>
        public RequestResult<T> Delete<T>(string uri) where T : class
        {
            try
            {
                var response = Task.Run(() => _client.DeleteAsync(GetCompleteUrl(uri))).Result;
                var responseContent = Task.Run(() => response.Content.ReadAsStringAsync()).Result;
                var responseObject = responseContent.Deserialize<T>();
                return new RequestResult<T> { Result = responseObject, StatusCode = response.StatusCode, ExceptionDetail = response.StatusCode == HttpStatusCode.OK ? string.Empty : responseContent };
            }
            catch (AggregateException ae)
            {
                if (ae.InnerException is HttpRequestException)
                {
                    return new RequestResult<T> { StatusCode = HttpStatusCode.BadRequest, ExceptionDetail = BadRequestMessage, Exception = ae };
                }

                if (ae.InnerException is TaskCanceledException)
                {
                    return new RequestResult<T> { StatusCode = HttpStatusCode.GatewayTimeout, ExceptionDetail = TimeoutMessage, Exception = ae };
                }

                return new RequestResult<T> { StatusCode = HttpStatusCode.RequestTimeout, ExceptionDetail = TimeoutMessage, Exception = ae };
            }
            catch (Exception ex)
            {
                return new RequestResult<T> { StatusCode = HttpStatusCode.InternalServerError, ExceptionDetail = UnknownErrorMessage, Exception = ex };
            }
        }

        /// <summary>
        /// Sync Http Delete request with uri
        /// </summary>
        /// <param name="uri">Api uri without base uri</param>
        /// <returns>Result as string</returns>
        public RequestResult<string> Delete(string uri)
        {
            return Delete<string>(uri);
        }

        #endregion

        #region DeleteAsync

        /// <summary>
        /// Async Http Delete request with uri
        /// </summary>
        /// <typeparam name="T">DTO typed object</typeparam>
        /// <param name="uri">Api uri without base uri</param>
        /// <returns>Structed deserialized response</returns>
        public async Task<RequestResult<T>> DeleteAsync<T>(string uri) where T : class
        {
            try
            {
                using (var response = await _client.DeleteAsync(GetCompleteUrl(uri)))
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var responseObject = responseContent.Deserialize<T>();
                    return new RequestResult<T> { Result = responseObject, StatusCode = response.StatusCode, ExceptionDetail = response.StatusCode == HttpStatusCode.OK ? string.Empty : responseContent };
                }
            }
            catch (HttpRequestException ex)
            {
                return new RequestResult<T> { StatusCode = HttpStatusCode.BadRequest, ExceptionDetail = BadRequestMessage, Exception = ex };
            }
            catch (TaskCanceledException ex)
            {
                return new RequestResult<T> { StatusCode = HttpStatusCode.GatewayTimeout, ExceptionDetail = TimeoutMessage, Exception = ex };
            }
            catch (Exception ex)
            {
                return new RequestResult<T> { StatusCode = HttpStatusCode.InternalServerError, ExceptionDetail = UnknownErrorMessage, Exception = ex };
            }
        }

        /// <summary>
        /// Async Http Delete request with uri
        /// </summary>
        /// <param name="uri">Api uri without base uri</param>
        /// <returns>Result as string</returns>
        public async Task<RequestResult<string>> DeleteAsync(string uri)
        {
            return await DeleteAsync<string>(uri);
        }

        #endregion

        #region PrivateMethods

        /// <summary>
        /// Generates string content with given body object
        /// </summary>
        /// <param name="body">Body object</param>
        /// <returns>String content with body object</returns>
        private StringContent GenerateStringContent(object body)
        {
            return new StringContent(body.Serialize(), _encoding, _mediaType);
        }

        /// <summary>
        /// Get complete request url. Concats base uri and given uri
        /// </summary>
        /// <param name="uri">Rest of the full url without base uri</param>
        /// <returns>Complete request url</returns>
        private string GetCompleteUrl(string uri)
        {
            return $"{BaseUri}{uri}";
        }

        #endregion
    }
}
