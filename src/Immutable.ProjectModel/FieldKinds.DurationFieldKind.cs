﻿using System;

namespace Immutable.ProjectModel
{
    public static partial class FieldKinds
    {
        private sealed class DurationFieldKind : FieldKind
        {
            public override Type Type => typeof(TimeSpan);

            public override string Format(object value)
            {
                var actualValue = (TimeSpan)value;
                return $"{actualValue.TotalDays:N2} days";
            }

            public override bool TryParse(string text, out object value)
            {
                if (text != null)
                {
                    if (text.EndsWith("days"))
                        text = text.Substring(0, text.Length - 3);

                    text = text.Trim();

                    if (int.TryParse(text, out var actualValue))
                    {
                        value = TimeSpan.FromDays(actualValue);
                        return true;
                    }
                }

                value = null;
                return false;
            }
        }
    }
}
