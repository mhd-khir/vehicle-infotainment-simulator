using System;
using System.Collections;
using Newtonsoft.Json;
using UnityEngine.Networking;

namespace InfotainmentSystem.Data
{
    // Owned by one data source; never shared between simultaneous requests.
    public sealed class JsonHttpClient
    {
        private UnityWebRequest activeRequest; // Only one request at a time; canceling disposes the request.

        /// <summary>
        /// Uses UnityWebRequest to GET JSON data from a URL, parse it, and invoke a callback with the result.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="url"></param>
        /// <param name="parse"></param>
        /// <param name="completed"></param>
        /// <param name="configure"></param>
        /// <returns></returns>
        public IEnumerator Get<T>(string url, Func<string, DataResult<T>> parse,
            Action<DataResult<T>> completed,
            Action<UnityWebRequest> configure = null)
        {
            Cancel(); // Cancel any previous request, disposing it.
            UnityWebRequest request = UnityWebRequest.Get(url);
            activeRequest = request;
            try
            {
                request.timeout = 15;
                configure?.Invoke(request);
                yield return request.SendWebRequest();

                // Cancel() can dispose the request while it is suspended.
                if (!ReferenceEquals(activeRequest, request))
                    yield break;

                DataResult<T> result;
                if (request.result != UnityWebRequest.Result.Success)
                {
                    result = DataResult<T>.Failure(
                        $"Request failed (HTTP {request.responseCode}): {request.error}");
                }
                else
                {
                    try
                    {
                        result = parse(request.downloadHandler.text);
                    }
                    catch (JsonException) // handles both JsonReaderException and JsonSerializationException
                    {
                        result = DataResult<T>.Failure("The service returned invalid JSON.");
                    }
                    catch (ArgumentException) // handles JsonSerializationException for invalid type conversions
                    {
                        result = DataResult<T>.Failure("The service returned invalid data.");
                    }
                }

                // Free network resources before invoking client/UI code.
                Release(request);
                completed(result);
            }
            finally
            {
                Release(request); // Ensure request is disposed even if the coroutine is interrupted.
            }
        }

        /// <summary>
        /// Used to cancel a request that is in progress. The request will be aborted and disposed.
        /// </summary>
        public void Cancel()
        {
            UnityWebRequest request = activeRequest;
            activeRequest = null;
            if (request == null)
                return;

            try { request.Abort(); }
            finally { request.Dispose(); }
        }

        /// <summary>
        /// Used to free network resources after a request is completed or canceled. The request will be disposed.
        /// </summary>
        /// <param name="request"></param>
        private void Release(UnityWebRequest request)
        {
            if (!ReferenceEquals(activeRequest, request))
                return;

            activeRequest = null;
            request.Dispose();
        }
    }
}
