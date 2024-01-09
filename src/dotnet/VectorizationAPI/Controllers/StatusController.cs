﻿using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

namespace FoundationaLLM.Vectorization.API.Controllers
{
    /// <summary>
    /// Provides methods for checking the status of the service.
    /// </summary>
    [ApiVersion(1.0)]
    [ApiController]
    [Route("[controller]")]
    public class StatusController : ControllerBase
    {
        /// <summary>
        /// Returns the status of the Vectorization API service.
        /// </summary>
        [HttpGet(Name = "GetServiceStatus")]
        public IActionResult Get() => Ok();

        /// <summary>
        /// Returns the allowed HTTP methods for the Vectorization API service.
        /// </summary>
        [HttpOptions]
        public IActionResult Options()
        {
            HttpContext.Response.Headers.Add("Allow", new[] { "GET", "POST", "OPTIONS", "DELETE" });

            return Ok();
        }
    }
}
