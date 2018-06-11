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
    /// 
    /// </summary>
    public class RequestClient : IDisposable
    {
        private readonly HttpClientHandler _handler;

        private readonly HttpClient _client;

        private Encoding _encoding = Encoding.UTF8;

        /// <summary>
        /// Default timeout duration as second
        /// </summary>
        private const double DefaultTimeoutDuration = 100;
        private const string TimeoutMessage = "Timeout.";
        private const string BadRequestMessage = "Bad Request.";
        private const string UnknownErrorMessage = "An error has occured.";
        private string _mediaType = "application/json";
        private Uri BaseUri { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="baseAddress"></param>
        public RequestClient(Uri baseAddress)
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
            Uri uri;
            if (Uri.TryCreate(uriString, UriKind.Absolute, out uri) == false) throw new FormatException("Uri format is invalid.");

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
        /// <param name="encoding"></param>
        /// <returns></returns>
        public RequestClient SetEncoding(Encoding encoding)
        {
            _encoding = encoding ?? throw new ArgumentNullException(nameof(encoding));
            return this;
        }

        /// <summary>
        /// Set media type
        /// </summary>
        /// <param name="mediaType"></param>
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
        /// <param name="scheme"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public RequestClient SetAuthorizationHeader(string scheme, string parameter)
        {
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(scheme, parameter);
            return this;
        }

        /// <summary>
        /// Set basic authorization header 
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public RequestClient SetBasicAuthorizationHeader(string parameter)
        {
            return SetAuthorizationHeader("Basic", parameter);
        }

        /// <summary>
        /// Set bearer authorization header 
        /// </summary>
        /// <param name="parameter"></param>
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
        /// <param name="seconds">Timeout duration as seconds</param>
        /// <returns></returns>
        public RequestClient SetTimeout(double seconds)
        {
            _client.Timeout = TimeSpan.FromSeconds(seconds);
            return this;
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual void Dispose()
        {
            _client.Dispose();
            _handler.Dispose();
        }

        /// <summary>
        /// Sync Http Get request with uri
        /// </summary>
        /// <typeparam name="T">DTO typed object</typeparam>
        /// <param name="uri">Api uri without base uri</param>
        /// <returns>Structed deserialized response</returns>
        public RequestResult<T> Get<T>(string uri) where T : class
        {
            try
            {
                var response = Task.Run(() => _client.GetAsync($"{BaseUri}{uri}")).Result;
                var content = Task.Run(() => response.Content.ReadAsStringAsync()).Result;
                var contentObject = content.Deserialize<T>();
                return new RequestResult<T> { Result = contentObject, StatusCode = response.StatusCode, ExceptionDetail = response.StatusCode == HttpStatusCode.OK ? string.Empty : content };
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
        /// <returns>Json string</returns>
        public RequestResult<string> Get(string uri)
        {
            return Get<string>(uri);
        }

        /// <summary>
        /// Async Http Get request with uri.
        /// </summary>
        /// <typeparam name="T">DTO typed object</typeparam>
        /// <param name="uri">Api uri without base uri</param>
        /// <returns>Json string</returns>
        public async Task<RequestResult<T>> GetAsync<T>(string uri) where T : class
        {
            try
            {
                using (var response = await _client.GetAsync($"{BaseUri}{uri}"))
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
        /// Async Http Get request with uri.
        /// </summary>
        /// <param name="uri">Api uri without base uri</param>
        /// <returns>Structed deserialized response</returns>
        public async Task<RequestResult<string>> GetAsync(string uri)
        {
            return await GetAsync<string>(uri);
        }

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
                var content = new StringContent(body.Serialize(), _encoding, _mediaType);
                var response = Task.Run(() => _client.PostAsync($"{BaseUri}{uri}", content)).Result;
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
        /// <returns></returns>
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
        /// <returns></returns>
        public RequestResult<string> Post(string uri, object body)
        {
            return Post<string>(uri, body);
        }

        /// <summary>
        /// Async Post Http request with uri and Json string body.
        /// </summary>
        /// <typeparam name="T">DTO typed object</typeparam>
        /// <param name="uri">Api uri without base uri</param>
        /// <param name="body">Json String</param>
        /// <returns>Structed deserialized response</returns>
        public async Task<RequestResult<T>> PostAsync<T>(string uri, string body) where T : class
        {
            var content = new StringContent(body);

            try
            {
                using (var response = await _client.PostAsync($"{BaseUri}{uri}", content))
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
        /// Async Post Http request with uri and Json string body.
        /// </summary>
        /// <param name="uri">Api uri without base uri</param>
        /// <param name="body">Json String</param>
        /// <returns>JSON string</returns>
        public async Task<RequestResult<string>> PostAsync(string uri, string body)
        {
            return await PostAsync<string>(uri, body);
        }

        /// <summary>
        /// Async Post Http request
        /// </summary>
        /// <typeparam name="T">DTO typed object</typeparam>
        /// <param name="uri">Api uri without base uri</param>
        /// <param name="body">DTO typed object</param>
        /// <returns></returns>
        public async Task<RequestResult<T>> PostAsync<T>(string uri, object body)
            where T : class
        {
            return await PostAsync<T, object>(uri, body);
        }

        /// <summary>
        /// Async Post Http request
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        public async Task<RequestResult<string>> PostAsync(string uri, object body)
        {
            return await PostAsync<string>(uri, body);
        }

        /// <summary>
        /// Async Post Http request with uri and generic typed body. 
        /// Body will be serialized with Json.Net inside the function
        /// </summary>
        /// <typeparam name="T">DTO typed object</typeparam>
        /// <typeparam name="TK">DTO typed object</typeparam>
        /// <param name="uri">Api uri without base uri</param>
        /// <param name="body">Generic typed body.</param>
        /// <returns>Structed deserialized response</returns>
        public async Task<RequestResult<T>> PostAsync<T, TK>(string uri, TK body)
            where T : class
            where TK : class
        {
            return await PostAsync<T>(uri, body.Serialize());
        }

        /// <summary>
        /// Http Put request with uri.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="uri"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        public RequestResult<T> Put<T>(string uri, object body)
            where T : class
        {
            return Put<T, object>(uri, body);
        }

        /// <summary>
        /// Http Put request with uri.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TK"></typeparam>
        /// <param name="uri"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        public RequestResult<T> Put<T, TK>(string uri, TK body)
            where TK : class
            where T : class
        {
            try
            {
                var content = new StringContent(body.Serialize(), _encoding, _mediaType);
                var response = Task.Run(() => _client.PutAsync($"{BaseUri}{uri}", content)).Result;
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
        /// Http Put request with uri.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        public RequestResult<string> Put(string uri, object body)
        {
            return Put<string>(uri, body);
        }

        /// <summary>
        /// Async Put Http request with uri and generic typed body. 
        /// Body will be serialized with Json.Net inside the function
        /// </summary>
        /// <typeparam name="T">DTO typed object</typeparam>
        /// <typeparam name="TK">DTO typed object</typeparam>
        /// <param name="uri">Api uri without base uri</param>
        /// <param name="body">Generic typed body.</param>
        /// <returns>Structed deserialized response</returns>
        public async Task<RequestResult<T>> PutAsync<T, TK>(string uri, TK body)
            where T : class
            where TK : class
        {
            return await PutAsync<T>(uri, body.Serialize());
        }

        /// <summary>
        /// Async Http Put request with uri.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="uri"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        public async Task<RequestResult<T>> PutAsync<T>(string uri, object body)
             where T : class
        {
            return await PutAsync<T, object>(uri, body);
        }

        /// <summary>
        /// Async Http Put request with uri.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="uri"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        public async Task<RequestResult<T>> PutAsync<T>(string uri, string body) where T : class
        {
            var content = new StringContent(body);

            try
            {
                using (var response = await _client.PutAsync($"{BaseUri}{uri}", content))
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
        /// Async Http Put request with uri.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        public async Task<RequestResult<string>> PutAsync(string uri, string body)
        {
            return await PutAsync<string>(uri, body);
        }

        /// <summary>
        /// Async Http Put request with uri.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        public async Task<RequestResult<string>> PutAsync(string uri, object body)
        {
            return await PutAsync<string>(uri, body);
        }

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
                var response = Task.Run(() => _client.DeleteAsync($"{BaseUri}{uri}")).Result;
                var content = Task.Run(() => response.Content.ReadAsStringAsync()).Result;
                var contentObject = content.Deserialize<T>();
                return new RequestResult<T> { Result = contentObject, StatusCode = response.StatusCode, ExceptionDetail = response.StatusCode == HttpStatusCode.OK ? string.Empty : content };
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
        /// <returns>Json string</returns>
        public RequestResult<string> Delete(string uri)
        {
            return Delete<string>(uri);
        }

        /// <summary>
        /// Async Http Delete request with uri.
        /// </summary>
        /// <param name="uri">Api uri without base uri</param>
        /// <returns>Structed deserialized response</returns>
        public async Task<RequestResult<string>> DeleteAsync(string uri)
        {
            return await DeleteAsync<string>(uri);
        }

        /// <summary>
        /// Async Http Delete request with uri.
        /// </summary>
        /// <typeparam name="T">DTO typed object</typeparam>
        /// <param name="uri">Api uri without base uri</param>
        /// <returns>T type object</returns>
        public async Task<RequestResult<T>> DeleteAsync<T>(string uri) where T : class
        {
            try
            {
                using (var response = await _client.DeleteAsync($"{BaseUri}{uri}"))
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
    }
}
