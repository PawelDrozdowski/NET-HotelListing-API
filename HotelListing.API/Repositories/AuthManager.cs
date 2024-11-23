using AutoMapper;
using HotelListing.API.Contracts;
using HotelListing.API.data;
using HotelListing.API.Models.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace HotelListing.API.Repositories
{
    public class AuthManager : IAuthManager
    {
        private IMapper _mapper;
        private UserManager<ApiUser> _userManager;
        private readonly IConfiguration _configuration;
        private ApiUser _user;

        private const string REFRESH_TOKEN_NAME = "RefreshToken";

        public AuthManager(IMapper mapper, UserManager<ApiUser> userManager, IConfiguration configuration)
        {
            _mapper = mapper;
            _userManager = userManager;
            _configuration = configuration;
        }

        public async Task<AuthResponseDto> Login(LoginDto loginDto)
        {
            _user = await _userManager.FindByEmailAsync(loginDto.Email);
            if (_user == null)
            {
                return null;
            }

            bool isValidCredentials = await _userManager.CheckPasswordAsync(_user, loginDto.Password);
            if (isValidCredentials)
            {
                var token = await GenerateToken();
                return new AuthResponseDto 
                { 
                    Token  = token,
                    UserId = _user.Id,
                    RefreshToken = await GenerateRefreshToken()
                };
            }
            else
                return null;
        }

        public async Task<IEnumerable<IdentityError>> Register(ApiUserDto userDto)
        {
            _user = _mapper.Map<ApiUser>(userDto);
            _user.UserName = userDto.Email;

            IdentityResult result = await _userManager.CreateAsync(_user, userDto.Password);

            if(result.Succeeded)
            {
                await _userManager.AddToRoleAsync(_user, "User");
            }

            return result.Errors;
        }

        private async Task<string> GenerateToken()
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:Key"]));
            var credentials = new SigningCredentials(securityKey,SecurityAlgorithms.HmacSha256);
            var roles = await _userManager.GetRolesAsync(_user);
            var roleClaims = roles.Select(x => new Claim(ClaimTypes.Role, x)).ToList();
            var userClaims = await _userManager.GetClaimsAsync(_user);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub,_user.Email),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new(JwtRegisteredClaimNames.Email, _user.Email),
                new("uid",_user.Id)
            }
            .Union(userClaims).Union(roleClaims);

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(int.Parse(_configuration["JwtSettings:DurationInMinutes"])),
                signingCredentials: credentials
                );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<string> GenerateRefreshToken()
        {
            
            string issuer = _configuration["JwtSettings:Issuer"];

            await _userManager.RemoveAuthenticationTokenAsync(_user, issuer, REFRESH_TOKEN_NAME);
            var newRefreshToken = await _userManager.GenerateUserTokenAsync(_user, issuer, REFRESH_TOKEN_NAME);
            var result = await _userManager.SetAuthenticationTokenAsync(_user, issuer, REFRESH_TOKEN_NAME, newRefreshToken);
            return newRefreshToken;
        }

        public async Task<AuthResponseDto> VerifyRefreshToken(AuthResponseDto request)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var tokenContent = jwtTokenHandler.ReadJwtToken(request.Token);
            var username = tokenContent.Claims.ToList().FirstOrDefault(x => x.Type == JwtRegisteredClaimNames.Email)?.Value;
            _user = await _userManager.FindByNameAsync(username);
            if (_user == null || _user.Id != request.UserId)
                return null;
            var isValidRefreshToken = await _userManager
                .VerifyUserTokenAsync(_user, _configuration["JwtSettings:Issuer"], REFRESH_TOKEN_NAME, request.RefreshToken);
            if (isValidRefreshToken)
            {
                var token = await GenerateToken();
                return new AuthResponseDto
                {
                    Token = token,
                    UserId = _user.Id,
                    RefreshToken = await GenerateRefreshToken()
                };
            }

            await _userManager.UpdateSecurityStampAsync(_user);
            return null;
        }
    }
}
