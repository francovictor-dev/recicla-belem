using Backend.Dtos;
using Backend.Data;
using Backend.Services;
using Backend.Enums;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Backend.Tests;

public class UserServiceTests
{
  private static AppDbContext CreateContext()
  {
    var options = new DbContextOptionsBuilder<AppDbContext>()
          .UseInMemoryDatabase(Guid.NewGuid().ToString()) // isola cada teste
          .Options;
    return new AppDbContext(options);
  }

  [Fact]
  public async Task CreateAsync_DeveCriarUsuario_QuandoDadosValidos()
  {
    await using var context = CreateContext();
    var userService = new UserService(context);
    var dto = new CreateUserDto("emailteste@gmail.com", "Senha123");

    var result = await userService.CreateAsync(dto);

    result.Should().NotBeNull();
    result.Email.Should().Be("emailteste@gmail.com");
    result.Id.Should().NotBeEmpty();

    var userInDb = await context.Users.FindAsync(result.Id);
    userInDb.Should().NotBeNull();
  }

  [Fact]
  public async Task CreateAsync_DeveLancarExcecao_QuandoEmailJaExiste()
  {
      await using var context = CreateContext();
      var service = new UserService(context);
      var dto = new CreateUserDto("duplicado@email.com", "Senha123");

      await service.CreateAsync(dto);

      var act = async () => await service.CreateAsync(dto);

      await act.Should().ThrowAsync<InvalidOperationException>()
        .WithMessage("*já cadastrado*");
  }

  [Fact]
  public async Task GetById_DeveRetornarUsuario_QuandoExiste()
  {
      await using var context = CreateContext();
      var service = new UserService(context);
      var dto = new CreateUserDto("novousuario@email.com", "Senha123");
      var user = await service.CreateAsync(dto);
      var result = await service.GetByIdAsync(user.Id);

      result.Should().NotBeNull();
      result!.Id.Should().Be(user.Id);
      result.Email.Should().Be(user.Email);
  }

  [Fact]
  public async Task GetByIdAsync_DeveRetornarNulo_QuandoNaoExiste()
  {
    await using var context = CreateContext();
    var service = new UserService(context);

    var result = await service.GetByIdAsync(Guid.NewGuid());

    result.Should().BeNull();
  }

  [Fact]
  public async Task GetAll_DeveRetornarUsuarios_QuandoExiste()
  {
      await using var context = CreateContext();
      var service = new UserService(context);
      
      var user1 = await service.CreateAsync(new CreateUserDto("novousuario1@email.com", "Senha123"));
      var user2 = await service.CreateAsync(new CreateUserDto("novousuario2@email.com", "Senha123"));

      var result = await service.GetAllAsync();

      result.Should().HaveCount(2);
      result.Select(u => u.Email).Should().Contain([user1.Email, user2.Email]);
  }

  [Fact]
  public async Task Update_DeveRetornarUsuarios_QuandoEstaAtualizado()
  {
      await using var context = CreateContext();
      var service = new UserService(context);
      
      var user = await service.CreateAsync(new CreateUserDto("novousuario1@email.com", "Senha123"));
      var result = await service.UpdateAsync(user.Id, new UpdateUserDto("novousuario2@email.com", false, UserType.Google));
      
      result.Should().NotBeNull();
      result.Email.Should().NotBe("novousuario1@email.com");
      result.IsActived.Should().NotBe(true);
      result.UserType.Should().NotBe(UserType.Email);
  }
  
  [Fact]
  public async Task Update_DeveRetornarException_QuandoNaoExiste()
  {
      await using var context = CreateContext();
      var service = new UserService(context);
      
      var act = async () => await service.UpdateAsync(Guid.NewGuid(), new UpdateUserDto("novousuario2@email.com", false, UserType.Google));

      await act.Should().ThrowAsync<KeyNotFoundException>()
        .WithMessage("*Usuário não encontrado.*");
  }
  [Fact]
  public async Task Delete_DeveRemoverUsuario_QuandoExiste()
  {
    await using var context = CreateContext();
    var service = new UserService(context);
    var criado = await service.CreateAsync(new CreateUserDto("deletar@email.com", "Senha123"));

    await service.DeleteAsync(criado.Id);

    var userNoBanco = await context.Users.FindAsync(criado.Id);
    userNoBanco.Should().BeNull();
  }

  [Fact]
  public async Task Delete_DeveLancarExcecao_QuandoNaoExiste()
  {
    await using var context = CreateContext();
    var service = new UserService(context);

    var act = async () => await service.DeleteAsync(Guid.NewGuid());

    await act.Should().ThrowAsync<KeyNotFoundException>();
  }
}