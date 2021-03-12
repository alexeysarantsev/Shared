
# Sarantsev.Shared.ExceptionMiddleware

The library provides the middleware to handle exceptions and returns appropriate responses.

### ExceptionMiddleware config

For using ExceptionMiddleware please register it:

1) Add the reference to the submodule project from your WebApi project. 
2) Register middleware in the Starup file:

Please use **UseExceptionMiddleware(IApplicationBuilder)** extension method for services
```c#
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
		...
            app.UseExceptionMiddleware();
		...
        }
```
3) (Optional) Register known exceptions in the **ConfigureServices** method:
```c#
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
			...
            services
                .RegisterException<TransformationSchemasValidationException, TransformationSchemasNotValidResponse>()
                .WithAction((r, e) => e.TransformationSchemaErrors = e.TransformationSchemaErrors)
                .WithStatusCode(StatusCodes.Status422UnprocessableEntity);
            ...
        }
```

### Register exception options

If the midlleware is registered in the Configure method, all unhandled exceptions are caught and the '500' error returns with **ProblemDetails** object in the body. The ProblemDetails.Title is set to exception.Message. But there are some ways to customize both the status code and the response body. 

1) Register the specific exception:

```c#
			...
            services
                .RegisterException<TransformationSchemasValidationException>()
                .WithStatusCode(StatusCodes.Status422UnprocessableEntity);
            ...
```

2) Register the specific exception and response type:

```c#
			...
            services
                .RegisterException<TransformationSchemasValidationException, TransformationSchemasNotValidResponse>()
                .WithStatusCode(StatusCodes.Status422UnprocessableEntity);
            ...
```
Please note that the response type should be an inheritor on the **ProblemDetails** class.

2) Register the specific exception and response type and some custom fields in the response type:

```c#
			...
            services
                .RegisterException<TransformationSchemasValidationException, TransformationSchemasNotValidResponse>()
				.WithAction((r, e) => r.SomeResponseData = e.SomeExceptionData)
                .WithStatusCode(StatusCodes.Status422UnprocessableEntity);
            ...
```
