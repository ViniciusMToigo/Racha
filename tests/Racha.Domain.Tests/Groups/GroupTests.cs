using FluentAssertions;
using Racha.Domain.Common;
using Racha.Domain.Groups;
using static Racha.Domain.Tests.TestData;

namespace Racha.Domain.Tests.Groups;

public class GroupTests
{
    [Fact]
    public void Create_AddsCreatorAsOnlyMember()
    {
        var group = Group.Create("Trip to Floripa", Alice);

        group.Id.Should().NotBeEmpty();
        group.Name.Should().Be("Trip to Floripa");
        group.CreatedByUserId.Should().Be(Alice);
        group.Members.Should().ContainSingle()
            .Which.Should().BeEquivalentTo(new { GroupId = group.Id, UserId = Alice });
    }

    [Fact]
    public void Create_TrimsName()
    {
        var group = Group.Create("  House  ", Alice);

        group.Name.Should().Be("House");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithBlankName_Throws(string name)
    {
        var act = () => Group.Create(name, Alice);

        act.Should().Throw<DomainException>().WithMessage("*name*");
    }

    [Fact]
    public void Create_WithNameTooLong_Throws()
    {
        var act = () => Group.Create(new string('a', Group.MaxNameLength + 1), Alice);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_WithEmptyCreator_Throws()
    {
        var act = () => Group.Create("House", Guid.Empty);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void AddMember_AddsUserToGroup()
    {
        var group = Group.Create("House", Alice);

        group.AddMember(Bob);

        group.Members.Select(m => m.UserId).Should().BeEquivalentTo(new[] { Alice, Bob });
        group.IsMember(Bob).Should().BeTrue();
    }

    [Fact]
    public void AddMember_WhenAlreadyMember_Throws()
    {
        var group = Group.Create("House", Alice);

        var act = () => group.AddMember(Alice);

        act.Should().Throw<DomainException>().WithMessage("*already*");
    }

    [Fact]
    public void AddMember_WithEmptyId_Throws()
    {
        var group = Group.Create("House", Alice);

        var act = () => group.AddMember(Guid.Empty);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void IsMember_ForStranger_ReturnsFalse()
    {
        var group = Group.Create("House", Alice);

        group.IsMember(Bob).Should().BeFalse();
    }

    [Fact]
    public void Rename_ChangesName()
    {
        var group = Group.Create("House", Alice);

        group.Rename("  Beach house ");

        group.Name.Should().Be("Beach house");
    }

    [Fact]
    public void Rename_WithBlankName_Throws()
    {
        var group = Group.Create("House", Alice);

        var act = () => group.Rename(" ");

        act.Should().Throw<DomainException>();
    }
}
