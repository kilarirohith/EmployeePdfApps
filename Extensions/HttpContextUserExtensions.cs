using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace EmployeeCrudPdf
{
    public static class HttpContextUserExtensions
    {
        /// <summary>Reads current user's id from Cookie-auth claims.</summary>
        public static int RequireUserId(this HttpContext ctx)
        {
            var uid = ctx.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(uid, out var id)) return id;

            // (Optional) fallback to Session if you still keep it
            var sid = ctx.Session?.GetInt32("auth_user_id");
            if (sid.HasValue) return sid.Value;

            throw new UnauthorizedAccessException("User is not logged in.");
        }
    }
}
