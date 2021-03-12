using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;

namespace Sarantsev.Shared.ExceptionMiddleware.ResponseDataBuilders
{
    public class ResponseDataBuilder<TE, TR> : ResponseDataBuilderBase<TE>
        where TE : Exception where TR : ProblemDetails, new()
    {
        private Func<TE, int> _statusCodeFunction = GetDefaultStatusCode;
        private readonly List<Action<TR, TE>> _actions = new List<Action<TR, TE>>();

        public ResponseDataBuilder()
        {
            // set the exception.message to the Title by default 
            _actions.Add((r, e) => r.Title = e.Message);
        }

        public ResponseDataBuilder<TE, TR> WithStatusCode(Func<TE, int> statusCodeFunction)
        {
            _statusCodeFunction = statusCodeFunction;
            return this;
        }
        public ResponseDataBuilder<TE, TR> WithStatusCode(int statusCode)
        {
            _statusCodeFunction = (exception) => GetHardcodedValue(statusCode);
            return this;
        }

        public ResponseDataBuilder<TE, TR> WithType(Func<TE, string> typeFunction) =>
            WithAction((r, e) => r.Type = typeFunction(e));

        public ResponseDataBuilder<TE, TR> WithType(string type) => WithAction((r, e) => r.Type = type);

        public ResponseDataBuilder<TE, TR> WithTitle(Func<TE, string> titleFunction) =>
            WithAction((r, e) => r.Title = titleFunction(e));

        public ResponseDataBuilder<TE, TR> WithTitle(string title) => WithAction((r, e) => r.Title = title);

        public ResponseDataBuilder<TE, TR> WithDetail(Func<TE, string> detailFunction) =>
            WithAction((r, e) => r.Detail = detailFunction(e));

        public ResponseDataBuilder<TE, TR> WithDetail(string detail) => WithAction((r, e) => r.Detail = detail);

        public ResponseDataBuilder<TE, TR> WithInstance(Func<TE, string> instanceFunction) =>
            WithAction((r, e) => r.Instance = instanceFunction(e));

        public ResponseDataBuilder<TE, TR> WithInstance(string instance) => WithAction((r, e) => r.Instance = instance);

        public ResponseDataBuilder<TE, TR> WithAction(Action<TR, TE> action)
        {
            _actions.Add(action);
            return this;
        }

        public ResponseDataBuilder<TE, TR> WithExtension(Action<TE, IDictionary<string, object>> action)
            => WithAction((r, e) => action(e, r.Extensions));

        private static TP GetHardcodedValue<TP>(TP t) => t;
        protected override int GetStatusCode(TE exception) => _statusCodeFunction(exception);
        protected override object GetBody(TE exception, ResponseContext options)
        {
            var responseBody = new TR
            {
                Status = GetStatusCode(exception)
            };
            responseBody.Instance = options.HttpContext?.Request?.Path;
            foreach (var action in _actions)
            {
                action(responseBody, exception);
            }
            if (options.IncludeDeveloperData)
            {
                responseBody.Extensions["stackTrace"] = exception.StackTrace;
            }
            return responseBody;
        }
    }
}
