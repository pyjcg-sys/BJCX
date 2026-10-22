using BJCX.Application.Abstractions;
using BJCX.Application.DTOs;
using BJCX.Application.Exceptions;
using BJCX.Domain.Entities;
using BJCX.Domain.Security;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BJCX.Application.Features.Auth;

/// <summary>
/// 登录命令：校验账号密码并签发 JWT。
/// </summary>
public class LoginCommand : IRequest<LoginResultDto>
{
    public string UserName { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;
}

/// <summary>登录参数校验。</summary>
public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.UserName).NotEmpty().WithMessage("用户名不能为空");
        RuleFor(x => x.Password).NotEmpty().WithMessage("密码不能为空");
    }
}

/// <summary>登录处理器：校验用户与密码，签发令牌。</summary>
public class LoginCommandHandler : IRequestHandler<LoginCommand, LoginResultDto>
{
    private readonly IRepository<User> _userRepository;
    private readonly ITokenService _tokenService;

    public LoginCommandHandler(IRepository<User> userRepository, ITokenService tokenService)
    {
        _userRepository = userRepository;
        _tokenService = tokenService;
    }

    public async Task<LoginResultDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.QueryAsNoTracking()
            .FirstOrDefaultAsync(u => u.UserName == request.UserName, cancellationToken);

        // 用户不存在或密码错误统一提示，避免暴露账号是否存在
        if (user is null || !PasswordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new BusinessException("用户名或密码错误");
        }

        if (!user.IsActive)
        {
            throw new BusinessException("账号已停用，请联系管理员");
        }

        var token = _tokenService.GenerateToken(user.Id, user.UserName, user.DisplayName, user.Email);

        return new LoginResultDto
        {
            Token = token,
            UserId = user.Id,
            UserName = user.UserName,
            DisplayName = user.DisplayName,
            Email = user.Email
        };
    }
}
