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
                // Properly sign out
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                
                // Delete authentication cookie
                Response.Cookies.Delete(".AspNetCore.Cookies");
                
                // Add no-cache headers
                Response.Headers.Add("Cache-Control", "no-cache, no-store, must-revalidate");
                Response.Headers.Add("Pragma", "no-cache");
                Response.Headers.Add("Expires", "0");
                
                // Return with JavaScript to ensure proper UI update and navigation
                return Content(@"
                <!DOCTYPE html>
                <html>
                <head>
                    <title>Logging out...</title>
                    <script>
                        function completeLogout() {
                            // Clear any client-side state
                            localStorage.clear();
                            sessionStorage.clear();
                            
                            // Redirect with cache-busting parameter
                            window.location.href = '/?v=' + new Date().getTime();
                        }
                        
                        // Execute immediately
                        completeLogout();
                    </script>
                </head>
                <body>
                    <p>Logging out... Please wait.</p>
                </body>
                </html>
                ", "text/html");
            }
            catch (Exception ex)
            {
                // Log the exception
                return Redirect("/?error=" + Uri.EscapeDataString(ex.Message));
            }
        }
    }
} 