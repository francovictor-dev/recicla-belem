using Google.Apis.Auth;

namespace Backend.Services;

public class GoogleTokenValidator(IConfiguration configuration) : IGoogleTokenValidator
{
  public async Task<GoogleJsonWebSignature.Payload> ValidateAsync(string idToken)
  {
    if (string.IsNullOrWhiteSpace(idToken)) throw new UnauthorizedAccessException("Token Google inválido.");
    var clientId = configuration["Google:ClientId"];
    if (string.IsNullOrWhiteSpace(clientId)) throw new InvalidOperationException("Client ID Google não configurado.");
    try
    {
      var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, new GoogleJsonWebSignature.ValidationSettings { Audience = [clientId] });
      if (!payload.EmailVerified || string.IsNullOrWhiteSpace(payload.Email) || string.IsNullOrWhiteSpace(payload.Subject))
        throw new UnauthorizedAccessException("A conta Google precisa ter e-mail verificado.");
      return payload;
    }
    catch (InvalidJwtException error)
    {
      throw new UnauthorizedAccessException("Token Google inválido ou expirado.", error);
    }
  }
}
