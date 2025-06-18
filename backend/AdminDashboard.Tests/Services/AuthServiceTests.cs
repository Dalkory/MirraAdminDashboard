using Api.Exceptions;
using Api.Models;
using Api.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Moq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Tests.Services;

public class AuthServiceTests
{
    private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly AuthService _authService;

    public AuthServiceTests()
    {
        var store = new Mock<IUserStore<ApplicationUser>>();
        _userManagerMock = new Mock<UserManager<ApplicationUser>>(
            store.Object, null, null, null, null, null, null, null, null);

        _configurationMock = new Mock<IConfiguration>();
        _configurationMock.Setup(x => x["JWT:Secret"]).Returns("VeryLongSecretKeyForTestingPurposes");
        _configurationMock.Setup(x => x["JWT:ValidIssuer"]).Returns("test.com");
        _configurationMock.Setup(x => x["JWT:ValidAudience"]).Returns("test.com");

        _authService = new AuthService(_userManagerMock.Object, _configurationMock.Object);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnToken()
    {
        // Arrange
        var user = new ApplicationUser { UserName = "test", Email = "test@test.com" };
        _userManagerMock.Setup(x => x.FindByEmailAsync("test@test.com"))
            .ReturnsAsync(user);
        _userManagerMock.Setup(x => x.CheckPasswordAsync(user, "password"))
            .ReturnsAsync(true);
        _userManagerMock.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { "User" });
        _userManagerMock.Setup(x => x.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _authService.Login("test@test.com", "password");

        // Assert
        result.Should().NotBeNullOrEmpty();

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.ReadJwtToken(result);
        token.Claims.Should().Contain(c => c.Type == ClaimTypes.Email && c.Value == "test@test.com");
    }

    [Fact]
    public async Task Login_WithInvalidCredentials_ShouldThrowUnauthorizedException()
    {
        // Arrange
        _userManagerMock.Setup(x => x.FindByEmailAsync("invalid@test.com"))
            .ReturnsAsync((ApplicationUser)null);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() =>
            _authService.Login("invalid@test.com", "password"));
    }

    [Fact]
    public async Task RefreshToken_WithValidToken_ShouldReturnNewToken()
    {
        // Arrange
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes("VeryLongSecretKeyForTestingPurposes");
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "test") }),
            Expires = DateTime.UtcNow.AddDays(1),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };
        var validToken = tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));

        var user = new ApplicationUser
        {
            UserName = "test",
            Email = "test@test.com",
            RefreshToken = validToken,
            RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(1)
        };

        _userManagerMock.Setup(x => x.FindByNameAsync("test"))
            .ReturnsAsync(user);
        _userManagerMock.Setup(x => x.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(x => x.GetRolesAsync(user))
            .ReturnsAsync(new List<string> { "User" });

        // Act
        var result = await _authService.RefreshToken(validToken);

        // Assert
        result.Should().NotBeNullOrEmpty();
        user.RefreshToken.Should().NotBe(validToken);
    }

    [Fact]
    public async Task RefreshToken_WithExpiredToken_ShouldThrowSecurityTokenException()
    {
        // Arrange
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes("VeryLongSecretKeyForTestingPurposes");

        var now = DateTime.UtcNow;

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "test") }),
            NotBefore = now.AddDays(-2),
            Expires = now.AddDays(-1),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var expiredToken = tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));

        var user = new ApplicationUser
        {
            UserName = "test",
            RefreshToken = expiredToken,
            RefreshTokenExpiryTime = now.AddDays(-1)
        };

        _userManagerMock.Setup(x => x.FindByNameAsync("test"))
            .ReturnsAsync(user);

        // Act & Assert
        await Assert.ThrowsAsync<SecurityTokenException>(() =>
            _authService.RefreshToken(expiredToken));
    }

}