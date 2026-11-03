using Google.Apis.Auth;

namespace Backend.Services;

public interface IGoogleTokenValidator
{
  Task<GoogleJsonWebSignature.Payload> ValidateAsync(string idToken);
}
