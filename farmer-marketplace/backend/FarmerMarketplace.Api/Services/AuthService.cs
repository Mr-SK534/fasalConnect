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
            // Contract rule: PlatformAdmin cannot self-register
            if (dto.Role == UserRole.PlatformAdmin)
                throw new UnauthorizedAccessException("PlatformAdmin accounts cannot be self-registered.");

            var emailExists = await _context.Users
                .AnyAsync(u => u.Email.ToLower() == dto.Email.ToLower());

            if (emailExists)
                throw new InvalidOperationException("An account with this email already exists.");

            // If Farmer is joining an FPO, validate the FPO exists and is actually an FpoAdmin
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
                Email = dto.Email.ToLower(),
                PasswordHash = _passwordHasher.HashPassword(dto.Password),
                Role = dto.Role,
                Phone = dto.Phone,
                Location = dto.Location,
                PreferredLanguage = dto.PreferredLanguage ?? "en",
                FpoId = dto.FpoId
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var token = _jwtService.GenerateToken(user);

            return new AuthResponseDto
            {
                Token = token,
                User = MapToResponseDto(user)
            };
        }

        public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == dto.Email.ToLower());

            if (user == null || !_passwordHasher.VerifyPassword(dto.Password, user.PasswordHash))
                throw new UnauthorizedAccessException("Invalid email or password.");

            var token = _jwtService.GenerateToken(user);

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

        private static UserResponseDto MapToResponseDto(User user)
        {
            return new UserResponseDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role,
                Phone = user.Phone,
                Location = user.Location,
                PreferredLanguage = user.PreferredLanguage,
                FpoId = user.FpoId
            };
        }
    }
}