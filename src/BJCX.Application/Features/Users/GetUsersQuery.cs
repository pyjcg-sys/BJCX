using AutoMapper;
using BJCX.Application.Abstractions;
using BJCX.Application.DTOs;
using BJCX.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BJCX.Application.Features.Users;

/// <summary>查询用户列表（用于流程配置中选择审批人）。</summary>
public class GetUsersQuery : IRequest<List<UserDto>>
{
}

/// <summary>用户列表查询处理器。</summary>
public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, List<UserDto>>
{
    private readonly IRepository<User> _userRepository;
    private readonly IMapper _mapper;

    public GetUsersQueryHandler(IRepository<User> userRepository, IMapper mapper)
    {
        _userRepository = userRepository;
        _mapper = mapper;
    }

    public async Task<List<UserDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await _userRepository.QueryAsNoTracking()
            .Where(u => u.IsActive)
            .OrderBy(u => u.Id)
            .ToListAsync(cancellationToken);

        return _mapper.Map<List<UserDto>>(users);
    }
}
