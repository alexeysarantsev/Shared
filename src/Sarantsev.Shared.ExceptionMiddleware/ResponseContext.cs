using System;
using Microsoft.AspNetCore.Http;

namespace Sarantsev.Shared.ExceptionMiddleware
{
    public class ResponseContext
    {
        public bool IncludeDeveloperData { get; set; }
        public HttpContext HttpContext { get; set; }
    }
}
