using System;
using System.Linq;

namespace Immutable.ProjectModel
{
    public sealed partial class Project
    {
        internal Assignment SetAssignmentField(Assignment assignment, AssignmentField field, object value)
        {
            ProjectData project;

            if (field == AssignmentFields.Work)
            {
                project = SetAssignmentWork(Data, assignment.Id, (TimeSpan)value);
            }
            else
            {
                var assignmentData = assignment.Data.SetValue(field, value);
                project = Data.UpdateAssignment(assignmentData);
            }

            return UpdateProject(project).GetAssignment(assignment.Id);
        }

        private ProjectData SetAssignmentWork(ProjectData project, AssignmentId id, TimeSpan value)
        {
            return Scheduler.SetAssignmentWork(project, project.Assignments[id], value);
        }
    }
}
