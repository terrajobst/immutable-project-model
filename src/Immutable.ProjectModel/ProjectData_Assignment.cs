using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;

namespace Immutable.ProjectModel
{
    internal sealed partial class ProjectData
    {
        public ImmutableDictionary<AssignmentId, AssignmentData> AssignmentMapping => _assignmentMap;

        public IEnumerable<AssignmentId> Assignments => _assignmentMap.Keys;

        public AssignmentId GetAssignment(TaskId taskId, ResourceId resourcedId)
        {
            Debug.Assert(!taskId.IsDefault);
            Debug.Assert(!resourcedId.IsDefault);

            return _assignmentMap.Keys.SingleOrDefault(a => Get(AssignmentFields.TaskId, a) == taskId &&
                                                          Get(AssignmentFields.ResourceId, a) == resourcedId);
        }

        public IEnumerable<AssignmentId> GetAssignments(TaskId taskId)
        {
            Debug.Assert(!taskId.IsDefault);

            return _assignmentMap.Keys.Where(a => Get(AssignmentFields.TaskId, a) == taskId);
        }

        public IEnumerable<AssignmentId> GetAssignments(ResourceId resourceId)
        {
            Debug.Assert(!resourceId.IsDefault);

            return _assignmentMap.Keys.Where(a => Get(AssignmentFields.ResourceId, a) == resourceId);
        }

        private ProjectData WithAssignmentMap(ImmutableDictionary<AssignmentId, AssignmentData> assignmentMap)
        {
            Debug.Assert(assignmentMap != null);

            return With(_information, _taskMap, _resourceMap, assignmentMap);
        }

        public ProjectData AddAssignment(AssignmentId assignmentId, TaskId taskId, ResourceId resourceId)
        {
            Debug.Assert(!assignmentId.IsDefault);
            Debug.Assert(!taskId.IsDefault);
            Debug.Assert(!resourceId.IsDefault);

            var project = this;
            var numberOfExistingAssignments = project.GetAssignments(taskId).Count();

            // Add assignment

            var assignment = AssignmentData.Create(assignmentId, taskId, resourceId);
            project = project.WithAssignmentMap(_assignmentMap.Add(assignment.Id, assignment));

            // Initialize Assignment.Work and update Task.Work

            if (numberOfExistingAssignments == 0)
            {
                var work = project.Get(TaskFields.Work, taskId);
                if (work == TimeSpan.Zero)
                    work = project.Get(TaskFields.Duration, taskId);

                project = project.SetRaw(TaskFields.Work, taskId, work)
                                 .SetRaw(AssignmentFields.Work, assignmentId, work);
            }
            else
            {
                var taskWork = project.Get(TaskFields.Work, taskId);
                var newTaskWorkHours = taskWork.TotalHours / numberOfExistingAssignments * (numberOfExistingAssignments + 1);
                var newAssignmentWorkHours = newTaskWorkHours - taskWork.TotalHours;

                var newTaskWork = TimeSpan.FromHours(newTaskWorkHours);
                var newAssignmentWork = TimeSpan.FromHours(newAssignmentWorkHours);

                project = project.SetRaw(TaskFields.Work, taskId, newTaskWork)
                                 .SetRaw(AssignmentFields.Work, assignmentId, newAssignmentWork);
            }

            // Initialize Assignment.TaskName and Assignment.ResourceName

            var taskName = project.Get(TaskFields.Name, taskId);
            var resourceName = project.Get(ResourceFields.Name, resourceId);

            project = project.SetRaw(AssignmentFields.TaskName, assignmentId, taskName)
                             .SetRaw(AssignmentFields.ResourceName, assignmentId, resourceName);

            // Update Task.ResourceNames

            project = project.Reset(TaskFields.ResourceNames, taskId);

            return project;
        }

        public ProjectData AddAssignment(AssignmentData assignment)
        {
            Debug.Assert(assignment != null);

            return WithAssignmentMap(_assignmentMap.Add(assignment.Id, assignment));
        }

        public ProjectData RemoveAssignment(AssignmentId assignmentId)
        {
            Debug.Assert(!assignmentId.IsDefault);

            if (!_assignmentMap.ContainsKey(assignmentId))
                return this;

            var taskId = Get(AssignmentFields.TaskId, assignmentId);
            var assignmentWork = Get(AssignmentFields.Work, assignmentId);
            var taskWork = Get(TaskFields.Work, taskId);

            var project = WithAssignmentMap(_assignmentMap.Remove(assignmentId));

            project = project.Set(TaskFields.Work, taskId, taskWork - assignmentWork);
            project = project.Reset(TaskFields.ResourceNames, taskId);

            return project;
        }

        public T Get<T>(AssignmentField<T> field, AssignmentId id)
        {
            Debug.Assert(field != null);
            Debug.Assert(!id.IsDefault);

            return _assignmentMap[id].GetValue(field);
        }

        public ProjectData Set(AssignmentField field, AssignmentId id, object value)
        {
            Debug.Assert(field != null);
            Debug.Assert(!id.IsDefault);

            if (field == AssignmentFields.Work)
            {
               return SetAssignmentWork(this, id, (TimeSpan)value);
            }
            else if (field == AssignmentFields.Units)
            {
                return SetAssignmentUnits(this, id, (double)value);
            }
            else
            {
                return SetRaw(field, id, value);
            }
        }

        public ProjectData SetRaw(AssignmentField field, AssignmentId id, object value)
        {
            Debug.Assert(field != null);
            Debug.Assert(!id.IsDefault);

            var assignment = _assignmentMap[id].SetValue(field, value);
            return WithAssignmentMap(_assignmentMap.SetItem(assignment.Id, assignment));
        }

        private static ProjectData SetAssignmentWork(ProjectData project, AssignmentId id, TimeSpan value)
        {
            var taskId = project.Get(AssignmentFields.TaskId, id);
            var oldTaskWork = project.Get(TaskFields.Work, taskId);
            var oldAssignmentWork = project.Get(AssignmentFields.Work, id);

            var newAssignmentWork = value;
            var newTaskWork = oldTaskWork + newAssignmentWork - oldAssignmentWork;

            return project.SetRaw(TaskFields.Work, taskId, newTaskWork)
                          .SetRaw(AssignmentFields.Work, id, newAssignmentWork);
        }

        private static ProjectData SetAssignmentUnits(ProjectData project, AssignmentId id, double value)
        {
            var taskId = project.Get(AssignmentFields.TaskId, id);

            return project.SetRaw(AssignmentFields.Units, id, value)
                          .Reset(TaskFields.ResourceNames, taskId);
        }
    }
}
