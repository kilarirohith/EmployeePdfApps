using Microsoft.AspNetCore.Http;
using System.Linq;

namespace EmployeeCrudPdf
{
    public static class HttpContextUserExtensions
    {
        /// <summary>Reads the current user's id from Session (auth_user_id) or JWT "uid" claim.</summary>
        public static int RequireUserId(this HttpContext ctx)
        {
            var sid = ctx.Session?.GetInt32("auth_user_id");
            if (sid.HasValue) return sid.Value;

            var uid = ctx.User?.Claims?.FirstOrDefault(c => c.Type == "uid")?.Value;
            if (int.TryParse(uid, out var id)) return id;

            throw new UnauthorizedAccessException("User is not logged in.");
        }
    }
}
