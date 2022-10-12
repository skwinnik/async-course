using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AuthCommon {
  public class AuthorizeAttribute : TypeFilterAttribute {
    public AuthorizeAttribute(params string[] roles) : base(typeof(AuthorizeActionFilter)) {
      this.Arguments = new object[] {
        roles
      };
    }
  }

  public class AuthorizeActionFilter : IAsyncActionFilter {
    private readonly string[] _roles;
    public AuthorizeActionFilter(string[] roles) {
      this._roles = roles;
    }
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next) {
      const string AUTHKEY = "authorization";
      var headers = context.HttpContext.Request.Headers;
      if (headers.ContainsKey(AUTHKEY)) {
        var jwt = headers[AUTHKEY].ToString().DecodeJwt();
        bool isAuthorized = this._roles.Contains(jwt?.Role);
        if (!isAuthorized)
          context.Result = new UnauthorizedResult();
        else
          await next();
      }
      else
        context.Result = new UnauthorizedResult();
    }
  }
}