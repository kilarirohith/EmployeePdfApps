using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace EmployeeCrudPdf.Swagger
{
   
    public sealed class CorrelationIdOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            operation.Responses ??= new OpenApiResponses();

            foreach (var resp in operation.Responses.Values)
            {
                resp.Headers ??= new Dictionary<string, OpenApiHeader>();
                if (!resp.Headers.ContainsKey("X-Correlation-Id"))
                {
                    resp.Headers.Add("X-Correlation-Id", new OpenApiHeader
                    {
                        Description = "Correlation id for tracing this request in server logs.",
                        Schema = new OpenApiSchema { Type = "string" }
                    });
                }
            }
        }
    }
}

