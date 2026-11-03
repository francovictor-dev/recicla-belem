using Backend.Configurations;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Backend.Tests;

public class S3OptionsTests
{
  [Fact]
  public void Configuracao_DevePermitirBindingSemCredenciais()
  {
    var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
    {
      ["S3:Region"] = "sa-east-1", ["S3:Bucket"] = "test-bucket", ["S3:PublicUrl"] = "https://example.com"
    }).Build();
    var options = configuration.GetSection("S3").Get<S3Options>()!;
    Assert.Equal("sa-east-1", options.Region);
    Assert.Equal("test-bucket", options.Bucket);
    Assert.Equal("https://example.com", options.PublicUrl);
  }
}
