// Backend.Tests/UserControllerTests.cs
using System.Net;
using System.Net.Http.Json;
using Backend.Dtos;
using FluentAssertions;
using Xunit;

namespace Backend.Tests;

public class UserControllerTests(CustomWebApplicationFactory factory) : IClassFixture<CustomWebApplicationFactory>
{
  private readonly HttpClient _client = factory.CreateClient();

  [Fact]
  public async Task Post_DeveRetornar201_QuandoUsuarioValido()
  {
      var dto = new CreateUserDto("integracao@email.com", "Senha123");

      var response = await _client.PostAsJsonAsync("/api/User", dto);

      response.StatusCode.Should().Be(HttpStatusCode.Created);

      var result = await response.Content.ReadFromJsonAsync<UserResponseDto>();
      result!.Email.Should().Be("integracao@email.com");
  }

  [Fact]
  public async Task Post_DeveRetornar409_QuandoEmailDuplicado()
  {
      var dto = new CreateUserDto("duplicado@email.com", "Senha123");
      await _client.PostAsJsonAsync("/api/User", dto);

      var response = await _client.PostAsJsonAsync("/api/User", dto);

      response.StatusCode.Should().Be(HttpStatusCode.Conflict);
  }

  [Fact]
  public async Task Get_DeveRetornar404_QuandoUsuarioNaoExiste()
  {
      var response = await _client.GetAsync($"/api/User/{Guid.NewGuid()}");

      response.StatusCode.Should().Be(HttpStatusCode.NotFound);
  }

  [Fact]
  public async Task Get_DeveRetornar200EUsuario_QuandoExiste()
  {
      var dto = new CreateUserDto("busca@email.com", "Senha123");
      var createResponse = await _client.PostAsJsonAsync("/api/User", dto);
      var created = await createResponse.Content.ReadFromJsonAsync<UserResponseDto>();

      var response = await _client.GetAsync($"/api/User/{created!.Id}");

      response.StatusCode.Should().Be(HttpStatusCode.OK);
      var result = await response.Content.ReadFromJsonAsync<UserResponseDto>();
      result!.Email.Should().Be("busca@email.com");
  }
}