using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Backend.Controllers;
using Backend.Dtos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Backend.Tests;

public class CoordinateDiagnosticTests
{
  [Fact]
  public void InicializacaoNova_DeveResolverExatamenteOFiltroUsadoNoCreate()
  {
    using var factory = new CustomWebApplicationFactory();
    using var scope = factory.Services.CreateScope();
    var action = typeof(CollectionPointController).GetMethod(nameof(CollectionPointController.Create))!;
    var filter = Assert.Single(action.GetCustomAttributes(typeof(ServiceFilterAttribute), true).Cast<ServiceFilterAttribute>());
    Assert.Equal(typeof(CoordinateDebugFilter), filter.ServiceType);
    Assert.IsType<CoordinateDebugFilter>(scope.ServiceProvider.GetRequiredService(filter.ServiceType));
  }

  [Theory]
  [InlineData("-1.3639085634032264", nameof(CreateCollectionPointDto.Lat))]
  [InlineData("-48.44998866319657", nameof(CreateCollectionPointDto.Lng))]
  public void Range_DeveAceitarCoordenadaCorretaERejeitarValorMalInterpretadoSemOverflow(string raw, string property)
  {
    var styles = NumberStyles.Float | NumberStyles.AllowThousands;
    var invariant = double.Parse(raw, styles, CultureInfo.InvariantCulture);
    var portuguese = double.Parse(raw, styles, CultureInfo.GetCultureInfo("pt-BR"));
    var range = (RangeAttribute)Attribute.GetCustomAttribute(typeof(CreateCollectionPointDto).GetProperty(property)!, typeof(RangeAttribute))!;
    Assert.True(range.IsValid(invariant));
    Assert.True(Math.Abs(portuguese) > int.MaxValue);
    Assert.False(range.IsValid(portuguese));
  }
}
