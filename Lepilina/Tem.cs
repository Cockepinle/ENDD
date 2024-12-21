namespace Lepilina
{
    public class Tem
    {
        private readonly RequestDelegate _next;

        public Tem(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            string theme = "light"; 
            if (context.Request.Cookies.TryGetValue("Theme", out var cookieTheme))
            {
                theme = cookieTheme;
            }

            context.Items["Theme"] = theme;

            await _next(context);
        }
    }
}
