using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace MI_WorkPlan.ViewModels
{
    public class TaskViewModel
    {

        public int? TaskId { get; set; }

        public int? ProjectId { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? CreatedOn { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? UpdatedOn { get; set; }

        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:dd/MM/yyyy}")]
        public DateTime? Deadline { get; set; }

        public string TaskName { get; set; }

        public int? Priority { get; set; }

        public double WeightedPriority { get; set; }

        public int? Progress { get; set; }

        public string Notes { get; set; }

        public string AssignedTo { get; set; }

        public string Active { get; set; }


    }
}