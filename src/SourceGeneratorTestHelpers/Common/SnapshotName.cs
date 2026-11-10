using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace SourceGeneratorTestHelpers.Common;

internal static class SnapshotName
{
    internal static string Create(Type testClass, string methodName, IReadOnlyList<ParameterInfo> parameters, IReadOnlyList<object?> arguments)
    {
        if (arguments.Count > parameters.Count)
            throw new ArgumentException("The number of test arguments exceeds the number of reflected test parameters.", nameof(arguments));

        // Verify 31.20 includes the immediate enclosing type for nested test classes.
        var className = testClass.IsNested ? $"{testClass.ReflectedType!.Name}.{testClass.Name}" : testClass.Name;
        var name = new StringBuilder(className);
        name.Append('.');
        name.Append(methodName);

        for (var index = 0; index < arguments.Count; index++)
        {
            name.Append('_');
            name.Append(parameters[index].Name);
            name.Append('=');
            AppendParameter(name, arguments[index], true);
        }

        return name.ToString();
    }

    private static void AppendParameter(StringBuilder name, object? value, bool isRoot)
    {
        if (value is null)
        {
            name.Append("null");
            return;
        }

        if (value is string text)
        {
            AppendFileNameText(name, text);
            return;
        }

        if (value is DateOnly date)
        {
            name.Append(date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
            return;
        }

        if (value is TimeOnly time)
        {
            name.Append(time.ToString("h-mm-tt", CultureInfo.InvariantCulture));
            return;
        }

        if (value is DateTime dateTime)
        {
            AppendDateTime(name, dateTime);
            if (dateTime.Kind != DateTimeKind.Unspecified)
                name.Append(dateTime.Kind);
            return;
        }

        if (value is DateTimeOffset dateTimeOffset)
        {
            AppendDateTime(name, dateTimeOffset.DateTime);
            var offset = dateTimeOffset.Offset;
            if (offset >= TimeSpan.Zero)
                name.Append('+');
            name.Append(offset.Hours.ToString("0", CultureInfo.InvariantCulture));
            if (offset.Minutes != 0)
            {
                if (offset > TimeSpan.Zero)
                    name.Append('-');
                name.Append(offset.Minutes.ToString("00", CultureInfo.InvariantCulture));
            }
            return;
        }

        if (value is bool or short or ushort or int or uint or long or ulong or decimal or Half or float or double)
        {
            var formattedValue = Convert.ToString(value, CultureInfo.InvariantCulture);
            name.Append(formattedValue);
            return;
        }

        var valueType = value.GetType();
        if (valueType.IsGenericType && valueType.GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
        {
            var key = valueType.GetProperty("Key")!.GetValue(value);
            var itemValue = valueType.GetProperty("Value")!.GetValue(value);
            AppendParameter(name, key, true);
            name.Append('=');
            AppendParameter(name, itemValue, true);
            return;
        }

        if (value is IEnumerable items)
        {
            var first = true;
            foreach (var item in items)
            {
                if (first)
                {
                    if (!isRoot)
                        name.Append('[');
                    first = false;
                }
                else
                {
                    name.Append(',');
                }
                AppendParameter(name, item, false);
            }

            if (first)
                name.Append("[]");
            else if (!isRoot)
                name.Append(']');
            return;
        }

        var parameterText = value.ToString();
        if (parameterText is null)
            throw new InvalidOperationException($"The test argument of type '{valueType.FullName}' returned null from ToString().");

        AppendFileNameText(name, parameterText);
    }

    private static void AppendDateTime(StringBuilder name, DateTime value)
    {
        var format = value.TimeOfDay == TimeSpan.Zero ? "yyyy-MM-dd"
            : value is { Second: 0, Millisecond: 0 } ? "yyyy-MM-ddTHH-mm"
            : value.Millisecond == 0 ? "yyyy-MM-ddTHH-mm-ss"
            : "yyyy-MM-ddTHH-mm-ss.FFFFFFF";
        name.Append(value.ToString(format, CultureInfo.InvariantCulture));
    }

    private static void AppendFileNameText(StringBuilder name, string text)
    {
        foreach (var character in text)
        {
            var invalidCharacter = character < ' ' || "<>:\"/\\|?*".Contains(character, StringComparison.Ordinal);
            name.Append(invalidCharacter ? '-' : character);
        }
    }
}
