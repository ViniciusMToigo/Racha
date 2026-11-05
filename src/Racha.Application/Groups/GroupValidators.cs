using FluentValidation;

namespace Racha.Application.Groups;

public sealed class CreateGroupRequestValidator : AbstractValidator<CreateGroupRequest>
{
}

public sealed class UpdateGroupRequestValidator : AbstractValidator<UpdateGroupRequest>
{
}

public sealed class AddMemberRequestValidator : AbstractValidator<AddMemberRequest>
{
}
