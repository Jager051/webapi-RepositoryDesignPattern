using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebAPI.Core.DTOs;
using WebAPI.Core.Entities;
using WebAPI.Core.Interfaces;

namespace WebAPI.Services.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly ICacheService _cacheService;

        public AuthService(IUnitOfWork unitOfWork, IConfiguration configuration, ICacheService cacheService)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _cacheService = cacheService;
        }

        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto loginRequest)
        {
            try
            {
                // Determine if loginRequest.Email is actually an email or username
                var isEmail = loginRequest.Email.Contains("@");
                var cacheKey = isEmail ? $"user:{loginRequest.Email}" : $"user_by_username:{loginRequest.Email}";
                
                // Check cache first
                var cachedUser = await _cacheService.GetAsync<User>(cacheKey);
                
                User? user;
                if (cachedUser != null)
                {
                    user = cachedUser;
                }
                else
                {
                    // Try to find user by email or username
                    user = await _unitOfWork.Repository<User>().FirstOrDefaultAsync(u => 
                        (isEmail ? u.Email == loginRequest.Email : u.Username == loginRequest.Email) && u.IsActive);
                    
                    // Cache user for 15 minutes with multiple keys
                    if (user != null)
                    {
                        await _cacheService.SetAsync($"user:{user.Email}", user, TimeSpan.FromMinutes(15));
                        await _cacheService.SetAsync($"user_by_username:{user.Username}", user, TimeSpan.FromMinutes(15));
                        await _cacheService.SetAsync($"user:{user.Id}", user, TimeSpan.FromMinutes(15));
                    }
                }

                if (user == null || !BCrypt.Net.BCrypt.Verify(loginRequest.Password, user.PasswordHash))
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Invalid email/username or password"
                    };
                }

                var token = GenerateJwtToken(user);
                var userDto = MapToUserDto(user);

                // Cache token for 24 hours
                var tokenCacheKey = $"token:{user.Id}";
                await _cacheService.SetAsync(tokenCacheKey, token, TimeSpan.FromHours(24));

                return new AuthResponseDto
                {
                    Success = true,
                    Token = token,
                    User = userDto,
                    Message = "Login successful"
                };
            }
            catch (Exception ex)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = $"Login failed: {ex.Message}"
                };
            }
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto registerRequest)
        {
            try
            {
                // Check cache first for existing user
                var emailCacheKey = $"user:{registerRequest.Email}";
                var usernameCacheKey = $"user_by_username:{registerRequest.Username}";
                
                var cachedUserByEmail = await _cacheService.GetAsync<User>(emailCacheKey);
                var cachedUserByUsername = await _cacheService.GetAsync<User>(usernameCacheKey);

                if (cachedUserByEmail != null || cachedUserByUsername != null)
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "User with this email or username already exists"
                    };
                }

                // Check database for existing user
                var existingUser = await _unitOfWork.Repository<User>().FirstOrDefaultAsync(u => 
                    u.Email == registerRequest.Email || u.Username == registerRequest.Username);

                if (existingUser != null)
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "User with this email or username already exists"
                    };
                }

                // Create new user
                var user = new User
                {
                    Username = registerRequest.Username,
                    Email = registerRequest.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerRequest.Password),
                    FirstName = registerRequest.FirstName,
                    LastName = registerRequest.LastName,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Repository<User>().AddAsync(user);
                await _unitOfWork.SaveChangesAsync();

                // Cache the new user with multiple keys for fast lookup
                await _cacheService.SetAsync(emailCacheKey, user, TimeSpan.FromMinutes(15));
                await _cacheService.SetAsync(usernameCacheKey, user, TimeSpan.FromMinutes(15));
                await _cacheService.SetAsync($"user:{user.Id}", user, TimeSpan.FromMinutes(15));

                var token = GenerateJwtToken(user);
                var userDto = MapToUserDto(user);

                // Cache token
                var tokenCacheKey = $"token:{user.Id}";
                await _cacheService.SetAsync(tokenCacheKey, token, TimeSpan.FromHours(24));

                return new AuthResponseDto
                {
                    Success = true,
                    Token = token,
                    User = userDto,
                    Message = "Registration successful"
                };
            }
            catch (Exception ex)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = $"Registration failed: {ex.Message}"
                };
            }
        }

        public async Task<bool> ValidateTokenAsync(string token)
        {
            try
            {
                // Check if token is in cache first
                var tokenHash = Convert.ToBase64String(System.Security.Cryptography.SHA256.HashData(Encoding.UTF8.GetBytes(token)));
                var cacheKey = $"valid_token:{tokenHash}";
                
                var cachedValidation = await _cacheService.GetAsync<bool>(cacheKey);
                if (cachedValidation)
                {
                    return true;
                }

                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"] ?? "");

                tokenHandler.ValidateToken(token, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _configuration["Jwt:Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                // Cache validation result for 1 hour
                await _cacheService.SetAsync(cacheKey, true, TimeSpan.FromHours(1));

                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<UserDto?> GetUserFromTokenAsync(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var jwtToken = tokenHandler.ReadJwtToken(token);
                var userIdClaim = jwtToken.Claims.FirstOrDefault(x => x.Type == "userId");

                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                    return null;

                var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId);
                return user != null ? MapToUserDto(user) : null;
            }
            catch
            {
                return null;
            }
        }

        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"] ?? "");
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim("userId", user.Id.ToString()),
                    new Claim("email", user.Email),
                    new Claim("username", user.Username),
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Email, user.Email)
                }),
                Expires = DateTime.UtcNow.AddHours(24),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private async Task InvalidateUserCacheAsync(User user)
        {
            try
            {
                await _cacheService.RemoveAsync($"user:{user.Email}");
                await _cacheService.RemoveAsync($"user_by_username:{user.Username}");
                await _cacheService.RemoveAsync($"user:{user.Id}");
                await _cacheService.RemoveAsync($"token:{user.Id}");
            }
            catch (Exception)
            {
                // Log error if needed
            }
        }

        private static UserDto MapToUserDto(User user)
        {
            return new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            };
        }
    }
}
