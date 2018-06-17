using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Utilities.RequestClient.Result;
using Utilities.Serialization;

namespace Utilities.RequestClient
{
    /// <summary>
    /// Easiest way to handle rest requests
    /// </summary>
    /// <inheritdoc cref="IDisposable"/>
    public partial class RequestClient
    {
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
                var response = Task.Run(() => Client.PutAsync(GetCompleteUrl(uri), content)).Result;
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
    }
}