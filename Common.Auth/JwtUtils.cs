using System.Text.Json;

namespace Common.Auth {
  public static class JwtTokenExtensions {
    public static string EncodeJwt(this JwtToken token) {
      //yes it is secure trust me
      return System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(JsonSerializer.Serialize<JwtToken>(token)));
    }

    public static JwtToken? DecodeJwt(this string token) {
      try {
        var json = System.Text.Encoding.UTF8.GetString(System.Convert.FromBase64String(token));
        return JsonSerializer.Deserialize<JwtToken>(json);
      }
      catch {
        return null;
      }

    }
  }
}