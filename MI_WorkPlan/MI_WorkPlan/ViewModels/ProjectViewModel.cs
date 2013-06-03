using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace MI_WorkPlan.ViewModels
{
    public class ProjectViewModel
    {
        public int? ProjectId { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? CreatedOn { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? UpdatedOn { get; set; }
        
        public string ProjectName { get; set; }
        
        public string Notes { get; set; }

        public string Active { get; set; }

        public List<TaskViewModel> ProjectTasks { get; set; }

        public int TotalTasks { get; set; }

        public string Progress { get; set; }

        public int intProgress { get; set; }

        public string Role { get; set; }

        public ProjectViewModel()
        {
            this.ProjectTasks = new List<TaskViewModel>();
            
        }
        
    }

    public class JobRoles
    {
        public string JobRole { get; set; }
    }
}