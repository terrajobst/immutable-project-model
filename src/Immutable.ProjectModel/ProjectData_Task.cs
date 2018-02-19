using System.Collections.Immutable;

namespace Immutable.ProjectModel
{
    internal sealed partial class ProjectData
    {
        public ImmutableDictionary<TaskId, TaskData> Tasks => _tasks;

        public ProjectData WithTasks(ImmutableDictionary<TaskId, TaskData> tasks)
        {
            return With(Information, tasks, Resources, Assignments);
        }

        public ProjectData AddTask(TaskData task)
        {
            return WithTasks(Tasks.Add(task.Id, task));
        }

        public ProjectData RemoveTask(TaskId taskId)
        {
            return WithTasks(Tasks.Remove(taskId));
        }

        public ProjectData UpdateTask(TaskData task)
        {
            return WithTasks(Tasks.SetItem(task.Id, task));
        }
    }
}
