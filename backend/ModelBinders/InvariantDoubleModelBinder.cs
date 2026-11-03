using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Backend.ModelBinders;

public class InvariantDoubleModelBinder : IModelBinder
{
  public Task BindModelAsync(ModelBindingContext context)
  {
    var value = context.ValueProvider.GetValue(context.ModelName);
    if (value != ValueProviderResult.None)
      context.ModelState.SetModelValue(context.ModelName, value);

    if (value.Length != 1 ||
        !double.TryParse(value.FirstValue, NumberStyles.Float, CultureInfo.InvariantCulture, out var number) ||
        !double.IsFinite(number))
    {
      context.ModelState.TryAddModelError(context.ModelName, "Informe uma coordenada válida usando ponto como separador decimal.");
      context.Result = ModelBindingResult.Failed();
      return Task.CompletedTask;
    }

    context.Result = ModelBindingResult.Success(number);
    return Task.CompletedTask;
  }
}
