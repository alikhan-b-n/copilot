﻿// Copyright (c) Microsoft. All rights reserved.

using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Experimental.Agents.InternalUtilities.Http;
using Microsoft.SemanticKernel.Experimental.Agents.Exceptions;
using Microsoft.SemanticKernel.Experimental.Agents.Internal;

namespace Microsoft.SemanticKernel.Experimental.Agents.Extensions;

internal static partial class OpenAIRestExtensions
{
    private const string BaseUrl = "https://api.openai.com/v1";
    private const string HeaderNameOpenAIAssistant = "OpenAI-Beta";
    private const string HeaderNameAuthorization = "Authorization";
    private const string HeaderOpenAIValueAssistant = "assistants=v1";

    internal static async Task<TResult> ExecuteGetAsync<TResult>(
        this OpenAIRestContext context,
        string url,
        CancellationToken cancellationToken = default)
    {
        using var request = HttpRequest.CreateGetRequest(url);

        request.Headers.Add(HeaderNameAuthorization, $"Bearer {context.ApiKey}");
        request.Headers.Add(HeaderNameOpenAIAssistant, HeaderOpenAIValueAssistant);

        using var response = await context.GetHttpClient().SendWithSuccessCheckAsync(request, cancellationToken).ConfigureAwait(false);

        var responseBody = await response.Content.ReadAsStringWithExceptionMappingAsync().ConfigureAwait(false);

        // Common case is for failure exception to be raised by REST invocation.
        // Null result is a logical possibility, but unlikely edge case.
        // Might occur due to model alignment issues over time.
        return
            JsonSerializer.Deserialize<TResult>(responseBody) ??
            throw new AgentException($"Null result processing: {typeof(TResult).Name}");
    }

    internal static Task<TResult> ExecutePostAsync<TResult>(
        this OpenAIRestContext context,
        string url,
        CancellationToken cancellationToken = default)
    {
        return context.ExecutePostAsync<TResult>(url, payload: null, cancellationToken);
    }

    internal static async Task<TResult> ExecutePostAsync<TResult>(
        this OpenAIRestContext context,
        string url,
        object? payload,
        CancellationToken cancellationToken = default)
    {
        using var request = HttpRequest.CreatePostRequest(url, payload);

        request.Headers.Add(HeaderNameAuthorization, $"Bearer {context.ApiKey}");
        request.Headers.Add(HeaderNameOpenAIAssistant, HeaderOpenAIValueAssistant);

        using var response = await context.GetHttpClient().SendWithSuccessCheckAsync(request, cancellationToken).ConfigureAwait(false);

        var responseBody = await response.Content.ReadAsStringWithExceptionMappingAsync().ConfigureAwait(false);

        return
            JsonSerializer.Deserialize<TResult>(responseBody) ??
            throw new AgentException($"Null result processing: {typeof(TResult).Name}");
    }

    private static async Task ExecuteDeleteAsync(
        this OpenAIRestContext context,
        string url,
        CancellationToken cancellationToken = default)
    {
        using var request = HttpRequest.CreateDeleteRequest(url);

        request.Headers.Add(HeaderNameAuthorization, $"Bearer {context.ApiKey}");
        request.Headers.Add(HeaderNameOpenAIAssistant, HeaderOpenAIValueAssistant);

        using var response = await context.GetHttpClient().SendWithSuccessCheckAsync(request, cancellationToken).ConfigureAwait(false);
    }
}
