using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Text;
using System.Windows;

using Demo.Services;

using Immutable.ProjectModel;

namespace Demo.Commands
{
    [Export]
    internal sealed class AssertTaskCommand : Command
    {
        private readonly WorkspaceService _workspaceService;
        private readonly Lazy<MainWindow> _mainWindow;

        [ImportingConstructor]
        public AssertTaskCommand(WorkspaceService workspaceService, Lazy<MainWindow> mainWindow)
        {
            _workspaceService = workspaceService;
            _mainWindow = mainWindow;
        }

        public override void Execute(object parameter)
        {
            var project = _workspaceService.Current;
            var fields = _mainWindow.Value.SelectedTaskFields;

            var source = GenerateSourceCode(project, fields);

            Clipboard.SetText(source);
        }

        private static string GenerateSourceCode(Project project, IEnumerable<TaskField> fields)
        {
            var sb = new StringBuilder();
            sb.AppendLine("ProjectAssert.For(project)");

            var isFirst = true;

            foreach (var task in project.Tasks)
            {
                if (isFirst)
                    isFirst = false;
                else
                    sb.AppendLine($".Project");

                sb.AppendLine($"             .ForTask({task.Ordinal})");


                var isFirstField = true;

                foreach (var field in fields)
                {
                    if (isFirstField)
                        isFirstField = false;
                    else
                        sb.AppendLine();

                    var name = field.Name;
                    var value = task.GetValue(field);
                    var valueText = GetValueText(value);

                    sb.Append($"                 .Assert{name}({valueText})");
                }
            }

            sb.Append(";");

            return sb.ToString();
        }

        private static string GetValueText(object value)
        {
            if (value == null)
                return "null";

            var type = value.GetType();

            if (type == typeof(DateTimeOffset))
            {
                var actualValue = (DateTimeOffset)value;
                var year = actualValue.Year;
                var month = actualValue.Month;
                var day = actualValue.Day;
                var hour = actualValue.Hour;
                var minute = actualValue.Minute;
                var second = actualValue.Second;
                return $"new DateTime({year}, {month}, {day}, {hour}, {minute}, {second})";
            }
            else if (type == typeof(TimeSpan))
            {
                var actualValue = (TimeSpan)value;
                var days = TimeConversion.Default.ToUnit(actualValue, TimeUnit.Days);
                return $"ProjectTime.FromDays({days})";
            }
            else if (type == typeof(int))
            {
                return value.ToString();
            }
            else if (type == typeof(bool))
            {
                return (bool)value ? "true" : "false";
            }
            else if (type == typeof(string))
            {
                return $"\"{value.ToString()}\"";
            }

            return $"/* ERROR: '{value}' */";
        }
    }
}
