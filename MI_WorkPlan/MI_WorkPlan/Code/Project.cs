using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace MI_WorkPlan.Models
{
    partial class tblMiProject
    {
        public string CreatedOnFormatted { get { return this.CreatedOn.ToShortDateString(); } }

        [Required(ErrorMessage="Project Name is required")]
        [StringLength(50, ErrorMessage="Must be under 50 characters")]
        public string ProjName { get { return this.ProjectName; } set { this.ProjectName = ProjName; } }
    }
}