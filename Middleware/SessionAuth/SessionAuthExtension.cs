namespace WebApplication1.Middleware.SessionAuth
{
    public static class SessionAuthExtension
    {
        public static IApplicationBuilder UseSessionAuth(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SessionAuthMiddleware>();
        }
    }
}