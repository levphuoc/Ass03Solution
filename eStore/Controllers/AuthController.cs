using BLL.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace eStore.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpGet("login")]
        public async Task<IActionResult> Login(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                return Redirect("/login?error=Email and password are required");
            }

            try
            {
                var result = await _authService.LoginAsync(email, password);
                if (result)
                {
                    return Redirect("/");
                }
                return Redirect("/login?error=Invalid email or password");
            }
            catch (Exception ex)
            {
                // Log the exception
                return Redirect($"/login?error={Uri.EscapeDataString(ex.Message)}");
            }
        }

        [HttpGet("logout")]
        public async Task<IActionResult> Logout()
        {
            try
            {
                // Properly sign out first
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                
                // Clear all cookies to ensure complete logout
                foreach (var cookie in Request.Cookies.Keys)
                {
                    Response.Cookies.Delete(cookie);
                }
                
                // Add no-cache headers to prevent caching
                Response.Headers.Add("Cache-Control", "no-cache, no-store, must-revalidate");
                Response.Headers.Add("Pragma", "no-cache");
                Response.Headers.Add("Expires", "0");
                
                // Redirect with unique timestamp to force reload
                return Redirect("/?v=" + DateTime.Now.Ticks);
            }
            catch (Exception ex)
            {
                // Log the exception
                return Redirect("/?error=" + Uri.EscapeDataString(ex.Message));
            }
        }
    }
} 