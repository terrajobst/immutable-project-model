using System;
using System.Linq;

namespace Immutable.ProjectModel
{
    public sealed partial class Project
    {
        internal Task SetTaskField(Task task, TaskField field, object value)
        {
            ProjectData project;

            if (field == TaskFields.Ordinal)
            {
                project = SetTaskOrdinal(Data, task.Id, (int)value);
            }
            else if (field == TaskFields.Duration)
            {
                project = SetTaskDuration(Data, task.Id, (TimeSpan)value);
            }
            else if (field == TaskFields.Work)
            {
                project = SetTaskWork(Data, task.Id, (TimeSpan)value);
            }
            else
            {
                var taskData = task.Data.SetValue(field, value);
                project = Data.UpdateTask(taskData);
            }

            return UpdateProject(project).GetTask(task.Id);
        }

        private ProjectData SetTaskOrdinal(ProjectData project, TaskId id, int value)
        {
            if (value < 0 || value >= project.Tasks.Count)
                throw new ArgumentOutOfRangeException(nameof(value));

            var oldOrdinal = project.Tasks[id].Ordinal;
            var newOrdinal = value;

            var orderedTasks = project.Tasks.Values.OrderBy(t => t.Ordinal).ToList();
            var task = orderedTasks[oldOrdinal];
            orderedTasks.RemoveAt(oldOrdinal);
            orderedTasks.Insert(newOrdinal, task);

            var tasks = project.Tasks;

            for (var i = 0; i < orderedTasks.Count; i++)
            {
                var t = orderedTasks[i].SetValue(TaskFields.Ordinal, i);
                tasks = tasks.SetItem(t.Id, t);
            }

            return project.WithTasks(tasks);
        }

        private ProjectData SetTaskDuration(ProjectData project, TaskId id, TimeSpan value)
        {
            return Scheduler.SetTaskDuration(project, project.Tasks[id], value);
        }

        private ProjectData SetTaskWork(ProjectData project, TaskId id, TimeSpan value)
        {
            return Scheduler.SetTaskWork(project, project.Tasks[id], value);
        }
    }
}
