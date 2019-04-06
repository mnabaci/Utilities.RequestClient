﻿using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
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
        /// Async Http Get request with uri
        /// </summary>
        /// <typeparam name="T">DTO typed object</typeparam>
        /// <param name="uri">Api uri without base uri</param>
        /// <returns>Structed deserialized response</returns>
        public async Task<RequestResult<T>> GetAsync<T>(string uri) where T : class
        {
            try
            {
                using (var response = await Client.GetAsync(GetCompleteUrl(uri)))
                {
                    var responseContent = typeof(T) == typeof(string) ? response.Content.ReadAsStringAsync().Result.Deserialize<T>(SerializationType) :
                                                                        response.Content.ReadAsAsync<T>(new List<MediaTypeFormatter> { MediaTypeFormatter }).Result;
                    return new RequestResult<T> { Result = responseContent, StatusCode = response.StatusCode, ExceptionDetail = response.StatusCode == HttpStatusCode.OK ? string.Empty : responseContent.Serialize() };
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
    }
}