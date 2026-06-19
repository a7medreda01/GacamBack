using AppBL.IService;
using System.Security.Claims;
using System.Text;

namespace AppPL.Middleware
{
    public class AuditLoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public AuditLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IAuditLogService auditLogService)
        {
            var request = context.Request;
            
            // Log modifications (POST, PUT, DELETE, PATCH) or auth endpoints (login/register)
            bool isMutating = request.Method == "POST" || request.Method == "PUT" || request.Method == "DELETE" || request.Method == "PATCH";
            bool isAuth = request.Path.Value?.ToLower().Contains("/auth/") == true;

            if (isMutating || isAuth)
            {
                request.EnableBuffering();

                string requestBody = string.Empty;
                using (var reader = new StreamReader(request.Body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, leaveOpen: true))
                {
                    requestBody = await reader.ReadToEndAsync();
                    request.Body.Position = 0; // Reset body stream position
                }

                // Proceed with the request pipeline
                await _next(context);

                // Retrieve authenticated user info
                var user = context.User;
                int? userId = null;
                string? email = null;

                if (user.Identity?.IsAuthenticated == true)
                {
                    var idClaim = user.FindFirst(ClaimTypes.NameIdentifier);
                    if (idClaim != null && int.TryParse(idClaim.Value, out var id))
                    {
                        userId = id;
                    }
                    email = user.FindFirst(ClaimTypes.Email)?.Value;
                }

                string action = isAuth ? $"Auth ({request.Method})" : request.Method;
                string tableName = request.Path.Value ?? "System";
                string ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "";

                // Log the request to audit log
                // Avoid logging sensitive passwords in raw text if present in auth body
                string loggedBody = requestBody;
                if (isAuth && requestBody.Contains("\"password\"", StringComparison.OrdinalIgnoreCase))
                {
                    loggedBody = "[REDACTED SENSITIVE DATA]";
                }

                await auditLogService.LogAsync(
                    userId: userId,
                    email: email,
                    action: action,
                    tableName: tableName,
                    recordId: $"Status Code: {context.Response.StatusCode}",
                    oldValues: null,
                    newValues: string.IsNullOrEmpty(loggedBody) ? null : loggedBody,
                    ipAddress: ipAddress
                );
            }
            else
            {
                await _next(context);
            }
        }
    }
}
