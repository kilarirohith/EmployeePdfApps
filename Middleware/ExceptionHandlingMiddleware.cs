using System.Net;
using System.Text.Json;
using EmployeeCrudPdf.Exceptions;
using EmployeeCrudPdf.Services.Logging;

namespace EmployeeCrudPdf.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IAppLogger _log;
         private static readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web);

        public ExceptionHandlingMiddleware(RequestDelegate next, IAppLogger log) 
        { _next = next; _log = log; }
        public async Task Invoke(HttpContext context)
        {
            try { await _next(context); }
            catch (NotFoundException ex)
            {
                _log.Warn("Global", ex.Message);
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                await context.Response.WriteAsync(ex.Message);
            }
            catch (DuplicateEntityException ex)
            {
                _log.Warn("Global", ex.Message);
                context.Response.StatusCode = (int)HttpStatusCode.Conflict;
                await context.Response.WriteAsync(ex.Message);
            }
            catch (AppException ex)
            {
                _log.Error("Global", ex.Message, ex);
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await context.Response.WriteAsync(ex.Message);
            }
            catch (Exception ex)
            {
                _log.Error("Global", "Unhandled exception", ex);
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                await context.Response.WriteAsync("Internal server error");
            }
        }
    }
}
