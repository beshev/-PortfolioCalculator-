namespace PortfolioCalculator.Middlewares
{
    // TODO: This can be changed by registering the user and storing information about the user in persistent storage.
    public class SessionMiddleware
    {
        private readonly RequestDelegate _next;

        public SessionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (string.IsNullOrEmpty(context.Session.GetString(Constants.UserSessionKey)))
            {
                var sessionId = Guid.NewGuid().ToString();
                context.Session.SetString(Constants.UserSessionKey, sessionId);
            }

            await _next(context);
        }
    }
}
