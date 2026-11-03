using Microsoft.AspNetCore.Mvc.Filters;

public class CoordinateDebugFilter(
    ILogger<CoordinateDebugFilter> logger) : IAsyncResourceFilter
{
  public async Task OnResourceExecutionAsync(
      ResourceExecutingContext context,
      ResourceExecutionDelegate next)
  {
    var request = context.HttpContext.Request;

    if (request.HasFormContentType)
    {
      var form = await request.ReadFormAsync();

      logger.LogInformation(
          "Coordenadas recebidas: Lat={Lat}, Lng={Lng}. Cultura={Culture}",
          form["Lat"].ToString(),
          form["Lng"].ToString(),
          System.Globalization.CultureInfo.CurrentCulture.Name);
    }

    await next();
  }
}