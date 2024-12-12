// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

#nullable disable

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Agents.Protocols.Primitives;
using Microsoft.Agents.Protocols.Serializer;

namespace Microsoft.Agents.Protocols.Connector
{
    internal class UserTokenRestClient(HttpClient client, Uri endpoint) : IUserToken
    {
        private readonly HttpClient _httpClient = client ?? throw new ArgumentNullException(nameof(client));
        private readonly Uri _endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));

        internal HttpRequestMessage CreateGetTokenRequest(string userId, string connectionName, string channelId, string code)
        {
            var request = new HttpRequestMessage();
            request.Method = HttpMethod.Get;

            request.RequestUri = new Uri(_endpoint, $"api/usertoken/GetToken")
                .AppendQuery("userId", userId)
                .AppendQuery("connectionName", connectionName)
                .AppendQuery("channelId", channelId)
                .AppendQuery("code", code);

            request.Headers.Add("Accept", "application/json");
            return request;
        }

        internal HttpRequestMessage CreateExchangeRequest(string userId, string connectionName, string channelId, TokenExchangeRequest body)
        {
            var request = new HttpRequestMessage();
            request.Method = HttpMethod.Post;

            request.RequestUri = new Uri(_endpoint, $"api/usertoken/exchange")
                .AppendQuery("userId", userId)
                .AppendQuery("connectionName", connectionName)
                .AppendQuery("channelId", channelId);

            request.Headers.Add("Accept", "application/json");
            if (body != null)
            {
                request.Content = new StringContent(ProtocolJsonSerializer.ToJson(body), System.Text.Encoding.UTF8, "application/json");
            }
            return request;
        }

        /// <inheritdoc/>
        public async Task<object> ExchangeAsyncAsync(string userId, string connectionName, string channelId, TokenExchangeRequest exchangeRequest, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(userId);
            ArgumentNullException.ThrowIfNullOrEmpty(connectionName);
            ArgumentNullException.ThrowIfNullOrEmpty(channelId);
            ArgumentNullException.ThrowIfNull(exchangeRequest);

            using var message = CreateExchangeRequest(userId, connectionName, channelId, exchangeRequest);
            using var httpResponse = await _httpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
            switch ((int)httpResponse.StatusCode)
            {
                case 200:
                case 400:
                case 404:
                    {
                        return ProtocolJsonSerializer.ToObject<object>(httpResponse.Content.ReadAsStream(cancellationToken));
                    }
                default:
                    throw new HttpRequestException($"ExchangeAsyncAsync {httpResponse.StatusCode}");
            }
        }

        /// <inheritdoc/>
        public async Task<TokenResponse> GetTokenAsync(string userId, string connectionName, string channelId = null, string code = null, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(userId);
            ArgumentNullException.ThrowIfNullOrEmpty(connectionName);

            using var message = CreateGetTokenRequest(userId, connectionName, channelId, code);
            using var httpResponse = await _httpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
            switch ((int)httpResponse.StatusCode)
            {
                case 200:
                    return ProtocolJsonSerializer.ToObject<TokenResponse>(httpResponse.Content.ReadAsStream(cancellationToken));
                case 404:
                    return null;
                default:
                    throw new HttpRequestException($"GetTokenAsync {httpResponse.StatusCode}");
            }
        }

        internal HttpRequestMessage CreateGetAadTokensRequest(string userId, string connectionName, string channelId, AadResourceUrls body)
        {
            var request = new HttpRequestMessage();
            request.Method = HttpMethod.Post;

            request.RequestUri = new Uri(endpoint, $"api/usertoken/GetAadTokens")
                .AppendQuery("userId", userId)
                .AppendQuery("connectionName", connectionName)
                .AppendQuery("channelId", channelId);

            request.Headers.Add("Accept", "application/json");
            if (body != null)
            {
                request.Content = new StringContent(ProtocolJsonSerializer.ToJson(body), System.Text.Encoding.UTF8, "application/json");
            }
            return request;
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyDictionary<string, TokenResponse>> GetAadTokensAsync(string userId, string connectionName, AadResourceUrls aadResourceUrls, string channelId = null, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(userId);
            ArgumentNullException.ThrowIfNullOrEmpty(connectionName);

            using var message = CreateGetAadTokensRequest(userId, connectionName, channelId, aadResourceUrls);
            using var httpResponse = await _httpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
            switch ((int)httpResponse.StatusCode)
            {
                case 200:
                    {
                        return ProtocolJsonSerializer.ToObject<IReadOnlyDictionary<string, TokenResponse>>(httpResponse.Content.ReadAsStream(cancellationToken));
                    }
                default:
                    throw new HttpRequestException($"GetAadTokensAsync {httpResponse.StatusCode}");
            }
        }

        internal HttpRequestMessage CreateSignOutRequest(string userId, string connectionName, string channelId)
        {
            var request = new HttpRequestMessage();
            request.Method = HttpMethod.Delete;

            request.RequestUri = new Uri(endpoint, $"api/usertoken/SignOut")
                .AppendQuery("userId", userId)
                .AppendQuery("connectionName", connectionName)
                .AppendQuery("channelId", channelId);

            request.Headers.Add("Accept", "application/json");
            return request;
        }

        /// <inheritdoc/>
        public async Task<object> SignOutAsync(string userId, string connectionName = null, string channelId = null, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(userId);

            using var message = CreateSignOutRequest(userId, connectionName, channelId);
            using var httpResponse = await _httpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
            switch ((int)httpResponse.StatusCode)
            {
                case 200:
                    {
                        return ProtocolJsonSerializer.ToObject<object>(httpResponse.Content.ReadAsStream(cancellationToken));
                    }
                case 204:
                    return null;
                default:
                    throw new HttpRequestException($"SignOutAsync {httpResponse.StatusCode}");
            }
        }

        internal HttpRequestMessage CreateGetTokenStatusRequest(string userId, string channelId, string include)
        {
            var request = new HttpRequestMessage();
            request.Method = HttpMethod.Get;

            request.RequestUri = new Uri(endpoint, $"api/usertoken/GetTokenStatus")
                .AppendQuery("userId", userId)
                .AppendQuery("channelId", channelId)
                .AppendQuery("include", include);

            request.Headers.Add("Accept", "application/json");
            return request;
        }

        /// <inheritdoc/>
        public async Task<IReadOnlyList<TokenStatus>> GetTokenStatusAsync(string userId, string channelId = null, string include = null, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(userId);

            using var message = CreateGetTokenStatusRequest(userId, channelId, include);
            using var httpResponse = await _httpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
            switch ((int)httpResponse.StatusCode)
            {
                case 200:
                    {
                        return ProtocolJsonSerializer.ToObject<IReadOnlyList<TokenStatus>>(httpResponse.Content.ReadAsStream(cancellationToken));
                    }
                default:
                    throw new HttpRequestException($"GetTokenStatusAsync {httpResponse.StatusCode}");
            }
        }

        internal HttpRequestMessage CreateExchangeTokenRequest(string userId, string connectionName, string channelId, TokenExchangeRequest body)
        {
            var request = new HttpRequestMessage();
            request.Method = HttpMethod.Post;

            request.RequestUri = new Uri(endpoint, $"api/usertoken/exchange")
                .AppendQuery("userId", userId)
                .AppendQuery("connectionName", connectionName)
                .AppendQuery("channelId", channelId);

            request.Headers.Add("Accept", "application/json");
            if (body != null)
            {
                request.Content = new StringContent(ProtocolJsonSerializer.ToJson(body), System.Text.Encoding.UTF8, "application/json");
            }
            return request;
        }

        /// <inheritdoc/>
        public async Task<TokenResponse> ExchangeTokenAsync(string userId, string connectionName, string channelId, TokenExchangeRequest body = null, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNullOrEmpty(userId);
            ArgumentNullException.ThrowIfNullOrEmpty(connectionName);
            ArgumentNullException.ThrowIfNullOrEmpty(channelId);

            using var message = CreateExchangeTokenRequest(userId, connectionName, channelId, body);
            using var httpResponse = await _httpClient.SendAsync(message, cancellationToken).ConfigureAwait(false);
            switch ((int)httpResponse.StatusCode)
            {
                case 200:
                case 404:
                    {
                        return ProtocolJsonSerializer.ToObject<TokenResponse>(httpResponse.Content.ReadAsStream(cancellationToken));
                    }
                default:
                    throw new HttpRequestException($"ExchangeTokenAsync {httpResponse.StatusCode}");
            }
        }
    }
}
