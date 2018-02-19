using System;
using System.Collections;
using System.Text;

namespace Immutable.ProjectModel
{
    public static partial class FieldKinds
    {
        private sealed class EnumFieldKind : FieldKind
        {
            public EnumFieldKind(Type type)
            {
                Type = type;
            }

            public override Type Type { get; }

            public override bool HasSuggestions => true;

            public override string Format(Project project, object value)
            {
                var text = Convert.ToString(value);
                return GetDisplayText(text);
            }

            public override bool TryParse(Project project, string text, out object value)
            {
                if (TryParseValue(text, Type, out value))
                    return true;

                try
                {
                    value = Enum.Parse(Type, text);
                    return true;
                }
                catch
                {
                    value = null;
                    return false;
                }
            }

            public override IEnumerable GetSuggestions()
            {
                return Enum.GetValues(Type);
            }

            private static bool TryParseValue(string text, Type enumType, out object result)
            {
                var names = Enum.GetNames(enumType);
                var values = Enum.GetValues(enumType);

                for (var i = 0; i < names.Length; i++)
                {
                    var name = names[i];
                    var displayName = GetDisplayText(name);
                    var value = values.GetValue(i);

                    if (text == name || text == displayName)
                    {
                        result = value;
                        return true;
                    }
                }

                result = default;
                return false;
            }

            private static string GetDisplayText(string text)
            {
                var sb = new StringBuilder(text.Length * 2);
                foreach (var c in text)
                {
                    if (char.IsUpper(c))
                    {
                        if (sb.Length > 0)
                            sb.Append(' ');
                    }

                    sb.Append(c);
                }

                return sb.ToString();
            }
        }
    }
}
