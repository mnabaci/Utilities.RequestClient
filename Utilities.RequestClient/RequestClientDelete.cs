﻿using System;
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
        /// Sync Http Delete request with uri
        /// </summary>
        /// <typeparam name="T">DTO typed object</typeparam>
        /// <param name="uri">Api uri without base uri</param>
        /// <returns>Structed deserialized response</returns>
        public RequestResult<T> Delete<T>(string uri) where T : class
        {
            try
            {
                var response = Client.DeleteAsync(GetCompleteUrl(uri)).Result;
                var responseContent = response.Content.ReadAsStringAsync().Result;
                var responseObject = responseContent.Deserialize<T>(SerializationType);
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
    }
}