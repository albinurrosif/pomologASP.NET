using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pomolog.Api.Data;
using Pomolog.Api.DTOs;
using Pomolog.Api.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Pomolog.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;

        // IConfiguration digunakan untuk membaca appsettings.json (mengambil Secret Key)
        public AuthController(AppDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            // 1. Cek apakah email sudah terdaftar
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (existingUser != null)
                return BadRequest("Email sudah digunakan.");

            // 2. Hash Password menggunakan BCrypt
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            // 3. Simpan User Baru
            var newUser = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                PasswordHash = hashedPassword,
                CreatedAt = DateTime.UtcNow
            };

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Registrasi berhasil!" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            // 1. Cari user berdasarkan Email
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user == null)
                return Unauthorized("Email atau password salah.");

            // 2. Verifikasi Password (Bandingkan input dengan hash di DB)
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
            if (!isPasswordValid)
                return Unauthorized("Email atau password salah.");

            // 3. Jika valid, buatkan Token JWT
            string token = GenerateJwtToken(user);

            return Ok(new { message = "Login berhasil", token });
        }

        // Fungsi bantuan untuk merakit JWT
        private string GenerateJwtToken(User user)
        {
            // Ambil Secret Key dari appsettings.json
            var secretKey = _configuration["JwtSettings:Secret"];
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            // Payload: Data apa saja yang mau dititipkan di dalam token (Klaim)
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("name", user.Name)
            };

            // Rakit tokennya (Berlaku selama 1 Hari)
            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(1),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}