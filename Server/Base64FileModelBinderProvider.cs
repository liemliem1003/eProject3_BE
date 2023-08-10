using Microsoft.AspNetCore.Mvc.ModelBinding;

public class Base64FileModelBinderProvider : IModelBinderProvider
{
    public IModelBinder GetBinder(ModelBinderProviderContext context)
    {
        if (context.Metadata.ModelType == typeof(IFormFile) && context.BindingInfo.BindingSource != null)
        {
            return new Base64FileModelBinder();
        }

        return null;
    }
}

public class Base64FileModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.FieldName);
        if (valueProviderResult == ValueProviderResult.None)
        {
            return Task.CompletedTask;
        }

        var model = new FormFile(null, 0, valueProviderResult.Length, null, Path.GetFileName(valueProviderResult.FirstValue));

        bindingContext.Result = ModelBindingResult.Success(model);
        return Task.CompletedTask;
    }
}
