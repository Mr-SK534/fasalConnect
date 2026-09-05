// backend/FarmerMarketplace.Api/Services/AuthService.cs

using FarmerMarketplace.Api.Data;
using FarmerMarketplace.Api.DTOs;
using FarmerMarketplace.Api.Interfaces;
using FarmerMarketplace.Api.Models;
using FarmerMarketplace.Api.Security;
using Microsoft.EntityFrameworkCore;

namespace FarmerMarketplace.Api.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly PasswordHasher _passwordHasher;
        private readonly JwtService _jwtService;

        public AuthService(AppDbContext context, PasswordHasher passwordHasher, JwtService jwtService)
        {
            _context = context;
            _passwordHasher = passwordHasher;
            _jwtService = jwtService;
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
        {
            if (dto.Role == UserRole.PlatformAdmin)
                throw new UnauthorizedAccessException("PlatformAdmin accounts cannot be self-registered.");

            var phoneExists = await _context.Users.AnyAsync(u => u.Phone == dto.Phone);
            if (phoneExists)
                throw new InvalidOperationException("An account with this phone number already exists.");

            if (!string.IsNullOrWhiteSpace(dto.Email))
            {
                var emailExists = await _context.Users
                    .AnyAsync(u => u.Email != null && u.Email.ToLower() == dto.Email.ToLower());

                if (emailExists)
                    throw new InvalidOperationException("An account with this email already exists.");
            }

            if (dto.FpoId.HasValue)
            {
                var fpoExists = await _context.Users
                    .AnyAsync(u => u.Id == dto.FpoId.Value && u.Role == UserRole.FpoAdmin);

                if (!fpoExists)
                    throw new InvalidOperationException("Invalid FPO reference.");
            }

            var user = new User
            {
                Name = dto.Name,
                Phone = dto.Phone,
                Email = dto.Email?.ToLower(),
                PasswordHash = _passwordHasher.HashPassword(dto.Password),
                Role = dto.Role,
                Location = dto.Location,
                PreferredLanguage = dto.PreferredLanguage ?? "en",
                FpoId = dto.FpoId,
                IsProfileComplete = false
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var (token, _, _) = _jwtService.GenerateToken(user);

            return new AuthResponseDto
            {
                Token = token,
                User = MapToResponseDto(user)
            };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var isEmail = dto.EmailOrPhone.Contains('@');

            var user = isEmail
                ? await _context.Users.FirstOrDefaultAsync(u =>
                    u.Email != null && u.Email.ToLower() == dto.EmailOrPhone.ToLower())
                : await _context.Users.FirstOrDefaultAsync(u => u.Phone == dto.EmailOrPhone);

            if (user == null || !_passwordHasher.VerifyPassword(dto.Password, user.PasswordHash))
                throw new UnauthorizedAccessException("Invalid credentials.");

            var (token, _, _) = _jwtService.GenerateToken(user);

            return new AuthResponseDto
            {
                Token = token,
                User = MapToResponseDto(user)
            };
        }

        public async Task<UserResponseDto> GetMeAsync(Guid userId)
        {
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
                throw new KeyNotFoundException("User not found.");

            return MapToResponseDto(user);
        }

        public async Task LogoutAsync(string jti, DateTime expiresAt)
        {
            var alreadyBlocked = await _context.TokenBlocklist.AnyAsync(t => t.Jti == jti);
            if (alreadyBlocked) return;

            _context.TokenBlocklist.Add(new TokenBlocklist
            {
                Jti = jti,
                ExpiresAt = expiresAt
            });

            await _context.SaveChangesAsync();
        }

        public async Task<AuthResponseDto> RefreshAsync(Guid userId)
        {
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
                throw new KeyNotFoundException("User not found.");

            var (token, _, _) = _jwtService.GenerateToken(user);

            return new AuthResponseDto
            {
                Token = token,
                User = MapToResponseDto(user)
            };
        }

        private static UserResponseDto MapToResponseDto(User user)
        {
            return new UserResponseDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Phone = user.Phone,
                Role = user.Role,
                Location = user.Location,
                PreferredLanguage = user.PreferredLanguage,
                FpoId = user.FpoId,
                IsProfileComplete = user.IsProfileComplete,
                CreatedAt = user.CreatedAt
            };
        }
    }
}