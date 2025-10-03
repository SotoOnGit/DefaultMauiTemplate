using Common.Enums.Media;
using Common.Models.Account;
using Configuration;
using Configuration.Extensions;
using Configuration.Models;
using Newtonsoft.Json;
using System.Net;

namespace DataLayer.Services
{
    public class NetworkService
    {
        private readonly IDefaultTemplateConfiguration _defaultConfiguration;
        private HttpClient _httpClient;
        private TaskCompletionSource<bool> _refreshTokenTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        public EventHandler LogoutEvent { get; set; }

        public NetworkService(IDefaultTemplateConfiguration defaultConfiguration)
        {
            _defaultConfiguration = defaultConfiguration;
            _httpClient = new HttpClient();
            _refreshTokenTcs.SetResult(true);
        }

        public async Task<ApiResponse> GetAsync(string apiRequest)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, _defaultConfiguration.StaticConfig.ApiEndPoint + apiRequest);
            return await MakeRequest(request).ConfigureAwait(false);
        }

        public async Task<ApiResponse> PostAsync(string apiRequest, object payload)
        {
            var request =
                new HttpRequestMessage(HttpMethod.Post, _defaultConfiguration.StaticConfig.ApiEndPoint + apiRequest);

            request.Content = payload.ToJson();
            return await MakeRequest(request).ConfigureAwait(false);
        }

        public async Task<ApiResponse> PutAsync(string apiRequest, object payload)
        {
            var request =
                new HttpRequestMessage(HttpMethod.Put, _defaultConfiguration.StaticConfig.ApiEndPoint + apiRequest);
            request.Content = payload.ToJson();
            return await MakeRequest(request).ConfigureAwait(false);
        }

        public async Task<ApiResponse> PatchAsync(string apiRequest, object payload)
        {
            var request =
                new HttpRequestMessage(HttpMethod.Patch, _defaultConfiguration.StaticConfig.ApiEndPoint + apiRequest);
            request.Content = payload.ToJson();
            return await MakeRequest(request).ConfigureAwait(false);
        }

        public async Task<ApiResponse> DeleteAsync(string apiRequest, object payload)
        {
            var request =
                new HttpRequestMessage(HttpMethod.Delete, _defaultConfiguration.StaticConfig.ApiEndPoint + apiRequest);
            request.Content = payload.ToJson();
            return await MakeRequest(request).ConfigureAwait(false);
        }

        public async Task<ApiResponse> MakeRequest(HttpRequestMessage request)
        {
            try
            {
                await AddAccessTokenAsync(request);

                var response = await _httpClient.SendAsync(request).ConfigureAwait(false);
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    var signInResult = await RefreshTokensAsync();
                    if (signInResult)
                    {
                        using var newRequest = await CreateRetryRequest(request);
                        await AddAccessTokenAsync(newRequest);
                        response = await _httpClient.SendAsync(newRequest).ConfigureAwait(false);
                    }
                }

                var apiResponse = new ApiResponse
                {
                    StatusCode = response.StatusCode,
                    Data = await response.Content.ReadAsStringAsync().ConfigureAwait(false)
                };

                return apiResponse;
            }
            catch (Exception e)
            {
                return new ApiResponse
                {
                    StatusCode = HttpStatusCode.RequestTimeout,
                    Data = e.Message
                };
            }
        }

        public async Task<ApiResponse> PostFileAsync(string apiRequest, Stream payload, MediaType mediaType, bool isPost = true, string fileName = null)
        {
            try
            {
                var method = isPost ? HttpMethod.Post : HttpMethod.Put;
                using var request = new HttpRequestMessage(method, _defaultConfiguration.StaticConfig.ApiEndPoint + apiRequest);
                request.Content = payload.ToMultipart(mediaType, fileName);

                await AddAccessTokenAsync(request);

                var response = await _httpClient.SendAsync(request).ConfigureAwait(false);

                if (response.StatusCode != HttpStatusCode.Unauthorized)
                {
                    return new ApiResponse
                    {
                        StatusCode = response.StatusCode,
                        Data = await response.Content.ReadAsStringAsync().ConfigureAwait(false)
                    };
                }

                var signInResult = await RefreshTokensAsync();
                if (signInResult)
                {
                    using var newRequest = await CreateRetryRequest(request);
                    await AddAccessTokenAsync(newRequest);
                    response = await _httpClient.SendAsync(newRequest).ConfigureAwait(false);
                }

                return new ApiResponse
                {
                    StatusCode = response.StatusCode,
                    Data = await response.Content.ReadAsStringAsync().ConfigureAwait(false)
                };
            }
            catch (Exception e)
            {
                // ignored
                return new ApiResponse
                {
                    StatusCode = HttpStatusCode.RequestTimeout,
                    Data = e.Message
                };
            }
        }

        private async Task<HttpRequestMessage> CreateRetryRequest(HttpRequestMessage originalRequest)
        {
            var newRequest = new HttpRequestMessage(originalRequest.Method, originalRequest.RequestUri);

            foreach (var header in originalRequest.Headers)
            {
                newRequest.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            if (originalRequest.Content != null)
            {
                var contentBytes = await originalRequest.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                newRequest.Content = new ByteArrayContent(contentBytes);

                foreach (var header in originalRequest.Content.Headers)
                {
                    newRequest.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            foreach (var property in originalRequest.Options)
            {
                newRequest.Options.Set(new HttpRequestOptionsKey<object>(property.Key), property.Value);
            }

            return newRequest;
        }

        private async Task AddAccessTokenAsync(HttpRequestMessage request)
        {
            var currentTcs = _refreshTokenTcs;

            if (!currentTcs.Task.IsCompleted)
                await currentTcs.Task.ConfigureAwait(false);

            request.Headers.Remove("Authorization");
            if (!string.IsNullOrEmpty(_defaultConfiguration.Auth?.AccessToken))
            {
                request.Headers.TryAddWithoutValidation("Authorization", _defaultConfiguration.Auth.AccessToken);
            }
        }

        private async Task<bool> RefreshTokensAsync()
        {
            if (string.IsNullOrEmpty(_defaultConfiguration.Auth?.RefreshToken))
                return false;

            var currentTcs = _refreshTokenTcs;
            if (!currentTcs.Task.IsCompleted)
            {
                try
                {
                    return await currentTcs.Task.ConfigureAwait(false);
                }
                catch
                {
                    return false;
                }
            }

            currentTcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            _refreshTokenTcs = currentTcs;

            try
            {
                var refreshTokenRequest = new RefreshTokenRequest
                {
                    RefreshToken = _defaultConfiguration.Auth.RefreshToken
                };

                var requestUrl = _defaultConfiguration.StaticConfig.ApiEndPoint + RequestDictionary.ApiRequests[BackendRequests.RefreshToken];
                using var request = new HttpRequestMessage(HttpMethod.Put, requestUrl);
                request.Content = refreshTokenRequest.ToJson();

                using var client = new HttpClient();

                var response = await client.SendAsync(request).ConfigureAwait(false);
                if (!response.IsSuccessStatusCode)
                {
                    currentTcs.SetResult(false);
                    LogoutEvent?.Invoke(this, EventArgs.Empty);
                    return false;
                }

                var responseString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                var tokenResponse = JsonConvert.DeserializeObject<TokenResponse>(responseString);

                if (tokenResponse?.Token == null)
                {
                    currentTcs.SetResult(false);
                    LogoutEvent?.Invoke(this, EventArgs.Empty);
                    return false;
                }

                lock (_defaultConfiguration)
                {
                    _defaultConfiguration.Auth = new AuthConfig
                    {
                        AccessToken = tokenResponse.Token,
                        RefreshToken = tokenResponse.RefreshToken
                    };
                }

                currentTcs.SetResult(true);
                return true;
            }
            catch (Exception e)
            {
                currentTcs.SetException(e);
                LogoutEvent?.Invoke(this, EventArgs.Empty);
                return false;
            }
        }
    }
}
