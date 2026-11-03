using Backend.Services;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Backend.Tests;

public class GoogleTokenValidatorTests
{
  [Theory]
  [InlineData("")]
  [InlineData(" ")]
  public async Task ValidateAsync_DeveRejeitarTokenVazio(string token)
  {
    var validator = new GoogleTokenValidator(new ConfigurationBuilder().Build());
    await Assert.ThrowsAsync<UnauthorizedAccessException>(() => validator.ValidateAsync(token));
  }

  [Fact]
  public async Task ValidateAsync_DeveExigirAudienceConfigurado()
  {
    var validator = new GoogleTokenValidator(new ConfigurationBuilder().Build());
    await Assert.ThrowsAsync<InvalidOperationException>(() => validator.ValidateAsync("token"));
  }
}
