using FluentValidation.TestHelper;
using Racha.Application.Groups;
using Racha.Domain.Groups;

namespace Racha.Application.Tests.Groups;

public class GroupValidatorsTests
{
    [Fact]
    public void CreateGroup_ValidName_HasNoErrors()
    {
        new CreateGroupRequestValidator().TestValidate(new CreateGroupRequest("House"))
            .ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateGroup_BlankName_HasError(string name)
    {
        new CreateGroupRequestValidator().TestValidate(new CreateGroupRequest(name))
            .ShouldHaveValidationErrorFor(r => r.Name);
    }

    [Fact]
    public void CreateGroup_NameTooLong_HasError()
    {
        var request = new CreateGroupRequest(new string('a', Group.MaxNameLength + 1));

        new CreateGroupRequestValidator().TestValidate(request).ShouldHaveValidationErrorFor(r => r.Name);
    }

    [Fact]
    public void UpdateGroup_BlankName_HasError()
    {
        new UpdateGroupRequestValidator().TestValidate(new UpdateGroupRequest(""))
            .ShouldHaveValidationErrorFor(r => r.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    public void AddMember_InvalidEmail_HasError(string email)
    {
        new AddMemberRequestValidator().TestValidate(new AddMemberRequest(email))
            .ShouldHaveValidationErrorFor(r => r.Email);
    }
}
