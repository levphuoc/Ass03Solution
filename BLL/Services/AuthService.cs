using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using DataAccessLayer.Entities;
using DataAccessLayer.Repository.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace BLL.Services
{
    public interface IAuthService
    {
        Task<bool> LoginAsync(string email, string password);
        Task LogoutAsync();
        Task<Member?> GetCurrentUserAsync();
        bool IsInRole(string role);
        Task<bool> UpdateProfileAsync(string companyName, string city, string country, string email, string password);
    }

    public class AuthService : IAuthService
    {
        private readonly IMemberRepository _memberRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthService(IMemberRepository memberRepository, IHttpContextAccessor httpContextAccessor)
        {
            _memberRepository = memberRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<bool> LoginAsync(string email, string password)
        {
            var member = await _memberRepository.GetByEmailAsync(email);
            if (member == null || member.Password != password)
            {
                return false;
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, member.Email),
                new Claim(ClaimTypes.NameIdentifier, member.MemberId.ToString()),
                new Claim("CompanyName", member.CompanyName),
                new Claim(ClaimTypes.Role, member.Role)
            };

            switch (member.Role)
            {
                case "Admin":
                    claims.Add(new Claim(ClaimTypes.Role, "Staff"));
                    claims.Add(new Claim(ClaimTypes.Role, "Deliverer"));
                    claims.Add(new Claim(ClaimTypes.Role, "User"));
                    break;
                case "Staff":
                    claims.Add(new Claim(ClaimTypes.Role, "Deliverer"));
                    claims.Add(new Claim(ClaimTypes.Role, "User"));
                    break;
                case "Deliverer":
                    claims.Add(new Claim(ClaimTypes.Role, "User"));
                    break;
            }

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
            };

            await _httpContextAccessor.HttpContext!.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            return true;
        }

        public async Task LogoutAsync()
        {
            // Only sign out without trying to delete individual cookies
            await _httpContextAccessor.HttpContext!.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }

        public async Task<Member?> GetCurrentUserAsync()
        {
            try
            {
                var email = _httpContextAccessor.HttpContext?.User?.Identity?.Name;
                if (string.IsNullOrEmpty(email))
                {
                    return null;
                }

                return await _memberRepository.GetByEmailAsync(email);
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error in GetCurrentUserAsync: {ex.Message}");
                return null;
            }
        }

        public bool IsInRole(string role)
        {
            return _httpContextAccessor.HttpContext?.User?.IsInRole(role) ?? false;
        }

        public async Task<bool> UpdateProfileAsync(string companyName, string city, string country, string email, string password)
        {
            try
            {
                // Get current user
                var currentUser = await GetCurrentUserAsync();
                if (currentUser == null)
                {
                    Console.WriteLine("UpdateProfileAsync: Current user not found");
                    return false;
                }

                // Store the original email for comparison
                var originalEmail = currentUser.Email;

                // Update user data
                currentUser.CompanyName = companyName;
                currentUser.City = city;
                currentUser.Country = country;
                currentUser.Email = email;
                
                // Only update password if provided
                if (!string.IsNullOrWhiteSpace(password))
                {
                    currentUser.Password = password;
                }

                // Save changes to database
                await _memberRepository.UpdateAsync(currentUser);

                // Always update the authentication cookie to reflect changes
                await _httpContextAccessor.HttpContext!.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

                // Create new claims
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, currentUser.Email),
                    new Claim(ClaimTypes.NameIdentifier, currentUser.MemberId.ToString()),
                    new Claim("CompanyName", currentUser.CompanyName),
                    new Claim(ClaimTypes.Role, currentUser.Role)
                };

                switch (currentUser.Role)
                {
                    case "Admin":
                        claims.Add(new Claim(ClaimTypes.Role, "Staff"));
                        claims.Add(new Claim(ClaimTypes.Role, "Deliverer"));
                        claims.Add(new Claim(ClaimTypes.Role, "User"));
                        break;
                    case "Staff":
                        claims.Add(new Claim(ClaimTypes.Role, "Deliverer"));
                        claims.Add(new Claim(ClaimTypes.Role, "User"));
                        break;
                    case "Deliverer":
                        claims.Add(new Claim(ClaimTypes.Role, "User"));
                        break;
                }

                // Sign in with new claims
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(7)
                };

                await _httpContextAccessor.HttpContext!.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(claimsIdentity),
                    authProperties);

                Console.WriteLine($"Profile updated successfully. Email changed from {originalEmail} to {email}");
                return true;
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error in UpdateProfileAsync: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return false;
            }
        }
    }
} 