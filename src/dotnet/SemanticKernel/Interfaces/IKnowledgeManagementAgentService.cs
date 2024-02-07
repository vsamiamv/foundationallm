﻿using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.SemanticKernel.Core.Models;

namespace FoundationaLLM.SemanticKernel.Core.Interfaces
{
    public interface IKnowledgeManagementAgentService
    {
        /// <summary>
        /// Gets a completion from the Semantic Kernel service.
        /// </summary>
        /// <param name="request">Request object populated from the hub APIs including agent, prompt, data source, and model information.</param>
        /// <returns>Returns a completion response from the orchestration engine.</returns>
        Task<LLMOrchestrationCompletionResponse> GetCompletion(KnowledgeManagementCompletionRequest request);
    }
}
