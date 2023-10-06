﻿using Asp.Versioning;
using FoundationaLLM.Common.Models.Orchestration;
using FoundationaLLM.Common.Models.Orchestration.SemanticKernel;
using FoundationaLLM.SemanticKernel.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoundationaLLM.SemanticKernel.API.Controllers
{
    //[Authorize]
    [ApiVersion(1.0)]
    [ApiController]
    [Route("orchestration")]
    public class OrchestrationController : ControllerBase
    {
        private readonly ISemanticKernelService _semanticKernelService;

        public OrchestrationController(
            ISemanticKernelService semanticKernelService)
        {
            _semanticKernelService = semanticKernelService;
        }

        [HttpPost("completion")]
        public async Task<SemanticKernelCompletionResponse> GetCompletion([FromBody] SemanticKernelCompletionRequest request)
        {
            var info = await _semanticKernelService.GetCompletion(request.Prompt, request.MessageHistory);

            return completionResponse;
        }

        [HttpPost("summary")]
        public async Task<SemanticKernelSummaryResponse> GetSummary([FromBody] SemanticKernelSummaryRequest request)
        {
            var info = await _semanticKernelService.GetSummary(request.Prompt);

            return new SemanticKernelSummaryResponse() { Info = info };
        }

        [HttpPost("memory/add")]
        public async Task AddMemory()
        {
            //await _semanticKernelService.AddMemory();

            throw new NotImplementedException();
        }

        [HttpDelete("memory/remove")]
        public async Task RemoveMemory()
        {
            //await _semanticKernelService.RemoveMemory();

            throw new NotImplementedException();
        }
    }
}