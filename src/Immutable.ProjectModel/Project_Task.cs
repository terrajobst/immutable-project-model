using System;

namespace Immutable.ProjectModel
{
    public sealed partial class Project
    {
        public Task AddTask(TaskId taskId = default)
        {
            taskId = taskId.CreateIfDefault();

            if (GetTask(taskId) != null)
                throw new ArgumentException($"Project already contains a task with ID {taskId}.");

            var projectData = Data.AddTask(taskId);
            return UpdateProject(projectData).GetTask(taskId);
        }

        public Project RemoveTask(TaskId taskId)
        {
            var project = Data.RemoveTask(taskId);
            return UpdateProject(project);
        }

        internal Task SetTaskField(Task task, TaskField field, object value)
        {
            var project = Data.Set(field, task.Id, value);
            return UpdateProject(project).GetTask(task.Id);
        }
    }
}
