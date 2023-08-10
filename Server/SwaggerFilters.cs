using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;

public class FileUploadOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.MethodInfo.Name == "CreateCompany")
        {
            if (operation.Parameters == null)
                operation.Parameters = new List<OpenApiParameter>();

            // Add the file upload parameter
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "file",
                In = ParameterLocation.Header,
                Description = "Upload file",
                Required = true,
                Schema = new OpenApiSchema
                {
                    Type = "string",
                    Format = "binary"
                }
            });
        }
    }
}
