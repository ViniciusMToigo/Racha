using System.Globalization;
using Racha.Domain.Groups;

namespace Racha.Domain.Tests;

/// <summary>Fixed ids keep tests deterministic: Alice &lt; Bob &lt; Carol &lt; Dave when ordered.</summary>
internal static class TestData
{
    public static readonly Guid Alice = UserId(1);
    public static readonly Guid Bob = UserId(2);
    public static readonly Guid Carol = UserId(3);
    public static readonly Guid Dave = UserId(4);

    public static readonly DateOnly Today = new(2026, 11, 4);

    public static Guid UserId(int n) => new($"00000000-0000-0000-0000-{n:D12}");

    /// <summary>Creates a group whose first member is the creator.</summary>
    public static Group GroupWith(params Guid[] members)
    {
        var group = Group.Create("Trip to Floripa", members[0]);
        foreach (var member in members.Skip(1))
        {
            group.AddMember(member);
        }

        return group;
    }

    /// <summary>Decimals can't be attribute arguments, so theories pass them as invariant-culture strings.</summary>
    public static decimal Dec(string value) => decimal.Parse(value, CultureInfo.InvariantCulture);
}
