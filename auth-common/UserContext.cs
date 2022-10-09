using AuthCommon;

namespace AuthCommon {
  public class UserContext {
    private readonly AuthContext authContext;

    public UserContext(AuthContext authContext) {
      this.authContext = authContext;
    }

    public Guid? GetCurrentUserId() {
      return this.authContext.GetJwtToken()?.Id;
    }
  }
}