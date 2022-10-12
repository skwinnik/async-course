using Microsoft.AspNetCore.Http;

namespace AuthCommon {
  public class AuthContext {
    private readonly IHttpContextAccessor httpContextAccessor;

    public AuthContext(IHttpContextAccessor httpContextAccessor) {
      this.httpContextAccessor = httpContextAccessor;
    }

    public JwtToken? GetJwtToken() {
      var header = this.httpContextAccessor?.HttpContext?.Request.Headers["authorization"];
      if (string.IsNullOrEmpty(header)) return null;
      return header?.ToString().DecodeJwt();
    }
  }
}