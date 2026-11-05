using Racha.Application.Users;

namespace Racha.Application.Groups;

public sealed record CreateGroupRequest(string Name);

public sealed record UpdateGroupRequest(string Name);

public sealed record AddMemberRequest(string Email);

public sealed record GroupSummaryDto(Guid Id, string Name, int MemberCount);

public sealed record GroupDto(Guid Id, string Name, Guid CreatedByUserId, IReadOnlyList<UserDto> Members);
