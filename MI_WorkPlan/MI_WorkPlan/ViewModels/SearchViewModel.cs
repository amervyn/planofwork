using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MI_WorkPlan.ViewModels
{
    public class SearchViewModel
    {

        public int ProjectID { get; set; }
        public string TaskName { get; set; }
        public int TaskID { get; set; }
        public string AssignedTo { get; set; }
        public int Progress { get; set; }
        public string ProjectNotes { get; set; }
        public string TaskNotes { get; set; }
        public bool TaskActive { get; set; }
        public bool ProjectActive { get; set; }
        public Nullable<System.DateTime> Deadline { get; set; }

    }
}