﻿using Asp.Versioning;
using FoundationaLLM.Common.Constants;
using FoundationaLLM.Common.Models.Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace FoundationaLLM.Vectorization.API.Controllers
{
    /// <summary>
    /// Provides methods for checking the status of the service.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class StatusController : ControllerBase
    {
        /// <summary>
        /// Returns the status of the Vectorization API service.
        /// </summary>
        [HttpGet(Name = "GetServiceStatus")]
        public IActionResult Get() => new OkObjectResult(new ServiceStatus
        {
            Name = "VectorizationAPI",
            Instance = Environment.GetEnvironmentVariable(EnvironmentVariables.Hostname),
            Version = Environment.GetEnvironmentVariable(EnvironmentVariables.FoundationaLLM_Version),
            Status = ServiceStatuses.Ready
        });

        private static readonly string[] MethodNames = ["GET", "POST", "OPTIONS", "DELETE"];

        /// <summary>
        /// Returns the allowed HTTP methods for the Vectorization API service.
        /// </summary>
        [HttpOptions]
        public IActionResult Options()
        {
            HttpContext.Response.Headers.Append("Allow", MethodNames);

            return Ok();
        }
    }
}
