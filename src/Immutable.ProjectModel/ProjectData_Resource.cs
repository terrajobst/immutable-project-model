using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;

namespace Immutable.ProjectModel
{
    internal sealed partial class ProjectData
    {
        public ImmutableDictionary<ResourceId, ResourceData> ResourceMap => _resourceMap;

        public IEnumerable<ResourceId> Resources => _resourceMap.Keys;

        public IEnumerable<ResourceId> GetResources(string name, bool useInitials = false)
        {
            var field = useInitials ? ResourceFields.Initials : ResourceFields.Name;
            return _resourceMap.Keys.Where(r => string.Equals(Get(field, r), name, StringComparison.OrdinalIgnoreCase));
        }

        public IEnumerable<ResourceId> GetResources(TaskId taskId)
        {
            return _assignmentMap.Keys.Where(a => Get(AssignmentFields.TaskId, a) == taskId)
                                    .Select(a => Get(AssignmentFields.ResourceId, a));
        }

        private ProjectData WithResourceMap(ImmutableDictionary<ResourceId, ResourceData> resourceMap)
        {
            Debug.Assert(resourceMap != null);

            return With(_information, _taskMap, resourceMap, _assignmentMap);
        }

        public ProjectData AddResource(ResourceId resourceId)
        {
            Debug.Assert(!resourceId.IsDefault);
            Debug.Assert(!_resourceMap.ContainsKey(resourceId));

            var resource = ResourceData.Create(resourceId);
            var resources = _resourceMap.Add(resource.Id, resource);
            return WithResourceMap(resources);
        }

        public ProjectData RemoveResource(ResourceId resourceId)
        {
            Debug.Assert(!resourceId.IsDefault);

            var project = this;

            // Avoid cascading errors when we remove resources that don't exist

            if (!_resourceMap.ContainsKey(resourceId))
                return project;

            // Remove assignments

            foreach (var assignmentId in project.GetAssignments(resourceId))
                project = project.RemoveAssignment(assignmentId);

            // Remove resource

            project = project.WithResourceMap(project._resourceMap.Remove(resourceId));

            return project;
        }

        public T Get<T>(ResourceField<T> field, ResourceId id)
        {
            Debug.Assert(field != null);
            Debug.Assert(!id.IsDefault);

            return _resourceMap[id].GetValue(field);
        }

        public ProjectData Set(ResourceField field, ResourceId id, object value)
        {
            Debug.Assert(field != null);
            Debug.Assert(!id.IsDefault);

            if (field == ResourceFields.Name)
            {
                return SetResourceName(this, id, (string)value);
            }
            else if (field == ResourceFields.Initials)
            {
                return SetResourceInitials(this, id, (string)value);
            }
            else
            {
                return SetRaw(field, id, value);
            }
        }

        public ProjectData SetRaw(ResourceField field, ResourceId id, object value)
        {
            Debug.Assert(field != null);
            Debug.Assert(!id.IsDefault);

            var resource = _resourceMap[id].SetValue(field, value);
            return WithResourceMap(_resourceMap.SetItem(resource.Id, resource));
        }

        private static ProjectData SetResourceName(ProjectData project, ResourceId id, string value)
        {
            project = project.SetRaw(ResourceFields.Name, id, value);

            // Update Assignment.ResourceName

            foreach (var assignmentId in project.GetAssignments(id))
                project = project.SetRaw(AssignmentFields.ResourceName, assignmentId, value);

            // Update Task.ResourceNames

            foreach (var taskId in project.GetTasks(id))
                project = project.Reset(TaskFields.ResourceNames, taskId);

            return project;
        }

        private static ProjectData SetResourceInitials(ProjectData project, ResourceId id, string value)
        {
            project = project.SetRaw(ResourceFields.Initials, id, value);

            // Update Task.ResourceInitials

            foreach (var taskId in project.GetTasks(id))
                project = project.Reset(TaskFields.ResourceInitials, taskId);

            return project;
        }
    }
}
