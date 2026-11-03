using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Backend.Dtos;
using Backend.ModelBinders;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Primitives;
using Xunit;

namespace Backend.Tests;

public class InvariantDoubleModelBinderTests
{
  private static ModelBindingContext Context(string[]? values, string culture, string name = "Lat")
  {
    var form = new Dictionary<string, StringValues>();
    if (values is not null) form[name] = new StringValues(values);
    return DefaultModelBindingContext.CreateBindingContext(
      new ActionContext(new DefaultHttpContext(), new RouteData(), new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor()),
      new FormValueProvider(BindingSource.Form, new FormCollection(form), CultureInfo.GetCultureInfo(culture)),
      new EmptyModelMetadataProvider().GetMetadataForType(typeof(double)), null, name);
  }

  [Theory]
  [InlineData("pt-BR")]
  [InlineData("en-US")]
  public async Task BindModelAsync_DevePreservarCoordenadasDoFormularioEmQualquerCultura(string culture)
  {
    var lat = Context(["-1.3639085634032264"], culture);
    var lng = Context(["-48.44998866319657"], culture, "Lng");
    var binder = new InvariantDoubleModelBinder();
    await binder.BindModelAsync(lat); await binder.BindModelAsync(lng);
    Assert.True(lat.Result.IsModelSet); Assert.True(lng.Result.IsModelSet);
    Assert.Equal(-1.3639085634032264, (double)lat.Result.Model!);
    Assert.Equal(-48.44998866319657, (double)lng.Result.Model!);
    var dto = new CreateCollectionPointDto { Lat = (double)lat.Result.Model!, Lng = (double)lng.Result.Model! };
    Assert.True(Validator.TryValidateObject(dto, new ValidationContext(dto), [], true));
  }

  [Theory]
  [InlineData("")]
  [InlineData("abc")]
  [InlineData("-1,3639085634032264")]
  [InlineData("NaN")]
  [InlineData("Infinity")]
  [InlineData("1e999")]
  public async Task BindModelAsync_DeveRejeitarFormatoInvalidoSemLancarExcecao(string raw)
  {
    var context = Context([raw], "pt-BR");
    await new InvariantDoubleModelBinder().BindModelAsync(context);
    Assert.False(context.Result.IsModelSet);
    Assert.False(context.ModelState.IsValid);
  }

  [Fact]
  public async Task BindModelAsync_DeveRejeitarValoresAusentesOuDuplicados()
  {
    foreach (var values in new string[]?[] { null, ["1", "2"] })
    {
      var context = Context(values, "pt-BR");
      await new InvariantDoubleModelBinder().BindModelAsync(context);
      Assert.False(context.ModelState.IsValid);
    }
  }

  [Theory]
  [InlineData(90, 180, true)]
  [InlineData(-90, -180, true)]
  [InlineData(0, 0, true)]
  [InlineData(90.001, 0, false)]
  [InlineData(0, -180.001, false)]
  [InlineData(1e20, 0, false)]
  public void Range_DeveValidarLimitesDoubleSemOverflow(double lat, double lng, bool valid)
  {
    var dto = new CreateCollectionPointDto { Lat = lat, Lng = lng };
    Assert.Equal(valid, Validator.TryValidateObject(dto, new ValidationContext(dto), [], true));
  }
}
