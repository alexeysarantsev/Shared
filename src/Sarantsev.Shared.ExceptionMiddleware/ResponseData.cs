using System;

namespace Sarantsev.Shared.ExceptionMiddleware
{
    /// <summary>
    /// Describes data that is returns in response, i.e. the status code and the body
    /// </summary>
    public class ResponseData
    {
        public object Body { get; set; }
        public int StatusCode { get; set; }
    }
}
