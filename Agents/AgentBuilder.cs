﻿// Copyright (c) Microsoft. All rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.SemanticKernel.Experimental.Agents.Exceptions;
using Microsoft.SemanticKernel.Experimental.Agents.Internal;
using Microsoft.SemanticKernel.Experimental.Agents.Models;

namespace Microsoft.SemanticKernel.Experimental.Agents;

/// <summary>
/// Fluent builder for initializing an <see cref="IAgent"/> instance.
/// </summary>
public partial class AgentBuilder
{
    private readonly AssistantModel _model;
    private readonly KernelPluginCollection _plugins;
    private readonly HashSet<string> _tools;
    private readonly List<string> _fileIds;
    private string? _apiKey;
    private Func<HttpClient>? _httpClientProvider;
    private PromptTemplateConfig? _config;

    /// <summary>
    /// Initializes a new instance of the <see cref="AgentBuilder"/> class.
    /// </summary>
    public AgentBuilder()
    {
        this._model = new AssistantModel();
        this._plugins = new KernelPluginCollection();
        this._tools = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        this._fileIds = new List<string>();
    }

    /// <summary>
    /// Create a <see cref="IAgent"/> instance.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token</param>
    /// <returns>A new <see cref="IAgent"/> instance.</returns>
    public async Task<IAgent> BuildAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(this._model.Model))
        {
            throw new AgentException("Model must be defined for agent.");
        }

        if (string.IsNullOrWhiteSpace(this._apiKey))
        {
            throw new AgentException("ApiKey must be provided for agent.");
        }

        this._model.Tools.AddRange(this._tools.Select(t => new ToolModel { Type = t }));
        this._model.FileIds.AddRange(this._fileIds.Distinct(StringComparer.OrdinalIgnoreCase));

        return
            await Agent.CreateAsync(
                new OpenAIRestContext(this._apiKey!, this._httpClientProvider),
                this._model,
                this._config,
                this._plugins,
                cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Define the OpenAI chat completion service (required).
    /// </summary>
    /// <returns><see cref="AgentBuilder"/> instance for fluid expression.</returns>
    public AgentBuilder WithOpenAIChatCompletion(string model, string apiKey)
    {
        this._apiKey = apiKey;
        this._model.Model = model;

        return this;
    }

    /// <summary>
    /// Provide an httpclient (optional).
    /// </summary>
    /// <returns><see cref="AgentBuilder"/> instance for fluid expression.</returns>
    public AgentBuilder WithHttpClient(HttpClient httpClient)
    {
        this._httpClientProvider ??= () => httpClient;

        return this;
    }

    /// <summary>
    /// Define the agent description (optional).
    /// </summary>
    /// <returns><see cref="AgentBuilder"/> instance for fluid expression.</returns>
    public AgentBuilder WithDescription(string? description)
    {
        this._model.Description = description;

        return this;
    }

    /// <summary>
    /// Define the agent instructions (optional).
    /// </summary>
    /// <returns><see cref="AgentBuilder"/> instance for fluid expression.</returns>
    public AgentBuilder WithInstructions(string instructions)
    {
        this._model.Instructions = instructions;

        return this;
    }

    /// <summary>
    /// Define the agent metadata (optional).
    /// </summary>
    /// <returns><see cref="AgentBuilder"/> instance for fluid expression.</returns>
    public AgentBuilder WithMetadata(string key, object value)
    {
        this._model.Metadata[key] = value;

        return this;
    }

    /// <summary>
    /// Define the agent metadata (optional).
    /// </summary>
    /// <returns><see cref="AgentBuilder"/> instance for fluid expression.</returns>
    public AgentBuilder WithMetadata(IDictionary<string, object> metadata)
    {
        foreach (var kvp in metadata)
        {
            this._model.Metadata[kvp.Key] = kvp.Value;
        }

        return this;
    }

    /// <summary>
    /// Define the agent name (optional).
    /// </summary>
    /// <returns><see cref="AgentBuilder"/> instance for fluid expression.</returns>
    public AgentBuilder WithName(string? name)
    {
        this._model.Name = name;

        return this;
    }

    /// <summary>
    /// Enable the code-interpreter tool with this agent.
    /// </summary>
    /// <returns><see cref="AgentBuilder"/> instance for fluid expression.</returns>
    public AgentBuilder WithCodeInterpreter()
    {
        this._tools.Add(Agent.ToolCodeInterpreter);

        return this;
    }

    /// <summary>
    /// Enable the retrieval tool with this agent.
    /// </summary>
    /// <param name="fileIds">Optional set of uploaded file identifiers.</param>
    /// <returns><see cref="AgentBuilder"/> instance for fluid expression.</returns>
    public AgentBuilder WithRetrieval(params string[] fileIds)
    {
        this._tools.Add(Agent.ToolRetrieval);

        return this.WithFiles(fileIds);
    }

    /// <summary>
    /// Define functions associated with agent instance (optional).
    /// </summary>
    /// <returns><see cref="AgentBuilder"/> instance for fluid expression.</returns>
    public AgentBuilder WithPlugin(KernelPlugin? plugin)
    {
        if (plugin != null)
        {
            this._plugins.Add(plugin);
        }

        return this;
    }

    /// <summary>
    /// Define functions associated with agent instance (optional).
    /// </summary>
    /// <returns><see cref="AgentBuilder"/> instance for fluid expression.</returns>
    public AgentBuilder WithPlugins(IEnumerable<KernelPlugin> plugins)
    {
        this._plugins.AddRange(plugins);

        return this;
    }

    /// <summary>
    /// Associate an uploaded file with the agent, by identifier.
    /// </summary>
    /// <param name="fileId">The uploaded file identifier.</param>
    /// <returns><see cref="AgentBuilder"/> instance for fluid expression.</returns>
    public AgentBuilder WithFile(string fileId)
    {
        if (!string.IsNullOrWhiteSpace(fileId))
        {
            this._fileIds.Add(fileId);
        }

        return this;
    }

    /// <summary>
    /// Associate uploaded files with the agent, by identifier.
    /// </summary>
    /// <param name="fileIds">The uploaded file identifiers.</param>
    /// <returns><see cref="AgentBuilder"/> instance for fluid expression.</returns>
    public AgentBuilder WithFiles(params string[] fileIds)
    {
        if (fileIds.Length > 0)
        {
            this._fileIds.AddRange(fileIds);
        }

        return this;
    }
}
