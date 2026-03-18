using ECommerce.Application.Features.Auth.Commands.Login;
using ECommerce.Application.Features.Auth.Commands.Register;
using ECommerce.Application.DTOs.Auth;
using FluentAssertions;

namespace ECommerce.Tests.Auth;

public class AuthValidatorTests
{
    // ── Register validator ────────────────────────────────────────

    [Fact]
    public async Task RegisterValidator_ValidRequest_Passes()
    {
        var validator = new RegisterRequestValidator();
        var result = await validator.ValidateAsync(new RegisterRequestDto { Username = "alice", Password = "secret123" });
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("", "secret123")]   // empty username
    [InlineData("ab", "secret123")] // username too short (min 3)
    [InlineData("alice", "")]       // empty password
    [InlineData("alice", "123")]    // password too short
    public async Task RegisterValidator_InvalidRequest_Fails(string username, string password)
    {
        var validator = new RegisterRequestValidator();
        var result = await validator.ValidateAsync(new RegisterRequestDto { Username = username, Password = password });
        result.IsValid.Should().BeFalse();
    }

    // ── Login validator ───────────────────────────────────────────

    [Fact]
    public async Task LoginValidator_ValidRequest_Passes()
    {
        var validator = new LoginRequestValidator();
        var result = await validator.ValidateAsync(new LoginRequestDto { Username = "alice", Password = "secret123" });
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("", "secret123")]
    [InlineData("alice", "")]
    public async Task LoginValidator_InvalidRequest_Fails(string username, string password)
    {
        var validator = new LoginRequestValidator();
        var result = await validator.ValidateAsync(new LoginRequestDto { Username = username, Password = password });
        result.IsValid.Should().BeFalse();
    }
}
