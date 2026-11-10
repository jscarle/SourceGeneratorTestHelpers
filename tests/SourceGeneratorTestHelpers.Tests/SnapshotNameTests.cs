using System.Collections;
using System.Globalization;
using SourceGeneratorTestHelpers.Common;
using Xunit;
using Assert = Xunit.Assert;

namespace SourceGeneratorTestHelpers.Tests;

public sealed class SnapshotNameTests
{
    public static TheoryData<object?, string> ParameterCases => new()
    {
        { null, "null" },
        { "", "" },
        { "a/b:c?\n", "a-b-c--" },
        { true, "True" },
        { false, "False" },
        { 12, "12" },
        { -12L, "-12" },
        { 12.5m, "12.5" },
        { 12.5d, "12.5" },
        { (Half)12.5, "12.5" },
        { new DateOnly(2026, 11, 10), "2026-11-10" },
        { new TimeOnly(13, 45, 22), "1-45-PM" },
        { new DateTime(2026, 11, 10), "2026-11-10" },
        { new DateTime(2026, 11, 10, 13, 45, 0), "2026-11-10T13-45" },
        { new DateTime(2026, 11, 10, 13, 45, 22, DateTimeKind.Utc), "2026-11-10T13-45-22Utc" },
        { new DateTime(2026, 11, 10, 13, 45, 22, 123), "2026-11-10T13-45-22.123" },
        { new DateTimeOffset(2026, 11, 10, 13, 45, 22, TimeSpan.FromHours(-5)), "2026-11-10T13-45-22-5" },
        { new DateTimeOffset(2026, 11, 10, 0, 0, 0, TimeSpan.FromMinutes(330)), "2026-11-10+5-30" },
        { new DateTimeOffset(2026, 11, 10, 0, 0, 0, TimeSpan.FromMinutes(-330)), "2026-11-10-5-30" },
        { new Guid("11111111-1111-1111-1111-111111111111"), "11111111-1111-1111-1111-111111111111" },
        { DayOfWeek.Tuesday, "Tuesday" },
        { Array.Empty<int>(), "[]" },
        { new[] { 1, 2 }, "1,2" },
        { new[] { new[] { 1, 2 }, new[] { 3 } }, "[1,2],[3]" },
        { new Dictionary<string, int> { { "x/y", 1 } }, "x-y=1" },
        { new DictionaryEntry("x/y", 1), "[x-y, 1]" },
        { new Hashtable { { "x/y", 1 } }, "[x-y, 1]" },
        { new KeyValuePair<string, int>("x/y", 1), "x-y=1" },
        { new ParameterValue(), "custom-value" }
    };

    [Theory]
    [MemberData(nameof(ParameterCases))]
    public void ParameterValues_Create_UsesVerifyDefaultFormattingUnderAnotherCulture(object? value, string expectedValue)
    {
        var originalCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("fr-CA");
            var method = typeof(SnapshotNameTests).GetMethod(nameof(ParameterValues_Create_UsesVerifyDefaultFormattingUnderAnotherCulture))!;
            var parameter = method.GetParameters()[0];

            var name = SnapshotName.Create(typeof(SnapshotNameTests), "Example", [parameter], [value]);

            Assert.Equal($"SnapshotNameTests.Example_value={expectedValue}", name);
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }

    private sealed class ParameterValue
    {
        public override string ToString()
        {
            return "custom/value";
        }
    }
}
