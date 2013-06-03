using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using MI_WorkPlan.ViewModels;
using MI_WorkPlan.Models;

namespace MI_WorkPlan.Controllers
{

    public class SearchCategories
    {
        public int catid { get; set; }
        public string category { get; set; }
    }

    public class HomeController : Controller
    {
        private WebAppsEntities db = new WebAppsEntities();


        public List<ProjectViewModel> GetProjects(bool active = true)
        {
            List<ProjectViewModel> lst = new List<ProjectViewModel>();
            //var plist2 = db.tblMiProjects.ToList();

            var plist = db.tblMiProjects.Where(i => i.Active == active).ToList();

            foreach (tblMiProject p in plist)
            {
                ProjectViewModel vm = new ProjectViewModel
                {
                    CreatedOn = p.CreatedOn,
                    ProjectId = p.projectId,
                    ProjectName = p.ProjectName,
                    UpdatedOn = p.UpdatedOn,
                    Role=p.Role
                };

                if (String.IsNullOrEmpty(p.Notes) == true)
                {
                    vm.Notes = "";
                }
                else
                {
                    vm.Notes = p.Notes.Length > 60 ? p.Notes.Substring(0, 60) + "...." : p.Notes;
                }

                if ((bool)p.Active)
                {
                    vm.Active = "Yes";
                }
                else
                {
                    vm.Active = "No";
                }

                //var query = from t in db.tblMiProjectTasks
                //            where t.projectId == p.projectId                            
                //            select new { t.taskId, t.Progress};

                //var query = db.tblMiProjectTasks.Where(i => i.projectId == p.projectId).AsQueryable();
                
                var query = (from c in db.tblMiProjectTasks
                         where c.projectId == p.projectId
                         select new TaskViewModel { TaskId = c.taskId, WeightedPriority = (double)(1 / (double)c.Priority), 
                             Active=c.Active==true ? "Yes":"No", Progress=c.Progress }).ToList();

                var total = query.Count();
                
                vm.TotalTasks = total;

                if (total > 0)
                {

                    double totalprogress = 0;
                    double totalpriority = 0;
                                        
                    foreach (var q in query)
                    {
                        if (q.Active == "Yes")
                        {
                            totalpriority += q.WeightedPriority;
                        }                    
                    }

                    foreach (var a in query)
                    {
                        if (a.Active == "Yes")
                        { 
                            totalprogress += (double)((double)a.WeightedPriority / (double)totalpriority) * (double)a.Progress;
                        }

                    }

                    int x1 = (int)totalprogress;
                    vm.Progress = x1.ToString() + "%";
                    vm.intProgress = (int)totalprogress;
                }
                else
                {
                    vm.Progress = "0%";
                    vm.intProgress = 0;
                }


                lst.Add(vm);

            }
           
            lst = lst.OrderBy(x => x.ProjectName).ToList();
            return lst;
        }


        //
        // GET: /Home/



        public ActionResult Index()
        {
            List<SearchCategories> items = new List<SearchCategories>();

            items.Add(new SearchCategories { category = "Projects", catid = 1 });
            items.Add(new SearchCategories { category = "Tasks", catid = 2 });
            items.Add(new SearchCategories { category = "All", catid = 3 });

            SelectList a = new SelectList(items, "catid", "category");
            ViewBag.cats = a;
            ViewData["checked"] = string.Empty;
            return View();

        }

        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public PartialViewResult LoadMain()
        {
            ViewData["checked"] = string.Empty;
            return PartialView("ProjectsList", GetProjects());
        }

        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public PartialViewResult ShowInactive()
        {
            var show = Request.QueryString["show"];
            ViewData["checked"] = "checked";
            //return View("Index", GetProjects(false));
            if (show == "all")
            {
                return PartialView("ProjectsList", GetProjects(false));
            }
            else
            {
                return PartialView("ProjectsList", GetProjects(false).Where(c=>c.Role==show));
            }
        }


        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public PartialViewResult HideCompleted()
        {
            var show = Request.QueryString["show"];
            ViewData["completed"] = "completed";
            //return View("Index", GetProjects(false));
            if (show == "all")
            {
                return PartialView("ProjectsList", GetProjects(true).Where(c => c.intProgress < 100));
            }
            else
            {
                return PartialView("ProjectsList", GetProjects(true).Where(c => c.intProgress < 100 && c.Role==show));
            }
        }


        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public PartialViewResult Search()
        {
            var s = Request.QueryString["s"];
            var active = Request.QueryString["active"];
            var completed = Request.QueryString["completed"];
            List<Workplan_Search_Result> sr = new List<Workplan_Search_Result>();
            
            sr = SearchResult(s);

            if (active == "true")
            {
                sr = (from c in sr
                      where c.TaskActive == true
                      select c).ToList();
            }
            else if (active == "false")
            {
                sr = (from c in sr
                      where c.TaskActive == false
                      select c).ToList();
            }            

            if (completed == "true")
            {
                sr = (from c in sr
                      where c.Progress == 100
                      select c).ToList();
            }
            else if(completed=="false")
            {
                sr = (from c in sr
                      where c.Progress < 100
                      select c).ToList();
            }

            foreach (Workplan_Search_Result res in sr)
            {
                if (!string.IsNullOrEmpty(res.TaskNotes) && res.TaskNotes.Length > 60)
                {
                    res.TaskNotes = res.TaskNotes.Substring(0, 55) + "...";
                }

                if (!string.IsNullOrEmpty(res.ProjectNotes) && res.ProjectNotes.Length > 60)
                {
                    res.ProjectNotes = res.ProjectNotes.Substring(0, 55) + "...";
                }
            }

            return PartialView("Search", sr);
        }

        [OutputCache(NoStore = true, Duration = 0, VaryByParam = "*")]
        public PartialViewResult ShowRole()
        {
            var role = Request.QueryString["role"];
            List<ProjectViewModel> p = new List<ProjectViewModel>();
            if (role == "All")
            {
                p = GetProjects(true);
            }
            else
            {
                p=(from c in GetProjects(true)
                   where c.Role==role
                   select c).ToList();
                
            }
            return PartialView("ProjectsList", p);

        }


        public List<Workplan_Search_Result> SearchResult(string s)
        {

            return db.Workplan_Search(s).ToList();

        }


        //
        // GET: /Home/Details/5

        public ActionResult Details(int id = 0)
        {
            tblMiProject tblmiproject = db.tblMiProjects.Find(id);
            ProjectViewModel p = new ProjectViewModel
            {
                CreatedOn = tblmiproject.CreatedOn,
                ProjectId = tblmiproject.projectId,
                ProjectName = tblmiproject.ProjectName,
                UpdatedOn = tblmiproject.UpdatedOn,
                Notes = tblmiproject.Notes
            };

            if ((bool)tblmiproject.Active)
            {
                p.Active = "Yes";
            }
            else
            {
                p.Active = "No";
            }



            var query = from t in db.tblMiProjectTasks
                        where t.projectId == id
                        select new
                        {
                            t.taskId,
                            t.TaskName,
                            t.Priority,
                            t.Progress,
                            t.CreatedOn,
                            t.UpdatedOn,
                            t.AssignedTo,
                            t.Deadline,
                            t.Notes,
                            t.Active,
                            t.projectId
                        };

            double totalprogress = 0;
            double totalpriority = 0;

            var total = query.Count();

            foreach (var a in query)
            {
                if (total > 0)
                {
                    if ((bool)a.Active)
                    {
                        totalpriority += (double)(1 / (double)a.Priority);
                    }
                }
            }

            if (total > 0)
            {

                foreach (var a in query)
                {
                    TaskViewModel tv = new TaskViewModel
                    {
                        ProjectId = a.projectId,
                        TaskId = a.taskId,
                        TaskName = a.TaskName,
                        Priority = a.Priority,
                        Progress = a.Progress,
                        CreatedOn = a.CreatedOn,
                        UpdatedOn = a.UpdatedOn,
                        AssignedTo = a.AssignedTo,
                        Deadline = a.Deadline,
                        Notes = a.Notes                        
                    };


                    if (String.IsNullOrEmpty(a.Notes) == true)
                    {
                        tv.Notes = "";
                    }
                    else
                    {
                        tv.Notes = a.Notes.Length > 50 ? a.Notes.Substring(0, 50) + "..." : a.Notes;
                    }

                    if ((bool)a.Active)
                    {
                        tv.Active = "Yes";
                        totalprogress += (double)(((double)(1 / (double)a.Priority) / (double)totalpriority)) * (double)a.Progress;
                    }
                    else
                    {
                        tv.Active = "No";
                    }
                                        
                    

                    p.ProjectTasks.Add(tv);
                }

                                
                int x1 = (int)totalprogress;
                p.Progress = x1.ToString() + "%";
                p.intProgress = (int)totalprogress;

            }
            else
            {
                p.Progress = "0%";
                p.intProgress = 0;
            }

            if (tblmiproject == null)
            {
                return HttpNotFound();
            }


            // ---GET PROJECT LIST--

            var projects = (from c in db.tblMiProjects
                            select new ProjectsList { ProjectId = c.projectId, ProjectName = c.ProjectName }).OrderBy(x => x.ProjectName).ToList();
            ViewBag.projects = projects;
            return View(p);
        }


        public ActionResult TaskDetails(int id = 0)
        {
            tblMiProjectTask tblmiprojecttask = db.tblMiProjectTasks.Find(id);
            return View(tblmiprojecttask);
        }


        //
        // GET: /Home/Create

        public ActionResult Create()
        {
            List<JobRoles> r = new List<JobRoles>();
            JobRoles role = new JobRoles();
            role.JobRole = "All";
            r.Add(role);
            JobRoles role2 = new JobRoles();
            role2.JobRole = "Analysts";
            r.Add(role2);
            JobRoles role3 = new JobRoles();
            role3.JobRole = "Developers";
            r.Add(role3);
            ViewBag.Roles = r;
            return View();
        }

        //
        // POST: /Home/Create

        [HttpPost]
        public ActionResult Create(tblMiProject tblmiproject)
        {
            if (ModelState.IsValid)
            {
                tblmiproject.CreatedOn = DateTime.Today;
                tblmiproject.UpdatedOn = DateTime.Today;
                db.tblMiProjects.Add(tblmiproject);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(tblmiproject);
        }

        public ActionResult CreateTask(int? projid = 0)
        {
            tblMiProject tblmiproject = db.tblMiProjects.Find(projid);
            if (!projid.HasValue || projid == 0)
            {
                return View("Error");
            }
            else
            {
                tblMiProject proj = db.tblMiProjects.Find(projid);
                ViewData["projectname"] = proj.ProjectName;
                return View();
            }
        }

        [HttpPost]
        public ActionResult CreateTask(tblMiProjectTask task)
        {
            var projid = Request.QueryString["projid"];
            task.projectId = int.Parse(projid);
            db.tblMiProjectTasks.Add(task);
            task.CreatedOn = DateTime.Today;
            task.UpdatedOn = DateTime.Today;
            db.SaveChanges();
            var id = task.projectId;
            return RedirectToAction("Details", new { id = id });
        }

        public ActionResult EditTask(int id = 0, int projid = 0)
        {
            tblMiProjectTask tblmiprojecttask = db.tblMiProjectTasks.Find(id);
            if (tblmiprojecttask == null)
            {
                return HttpNotFound();
            }
            tblMiProject proj = db.tblMiProjects.Find(projid);
            ViewData["projname"] = proj.ProjectName;
            return View(tblmiprojecttask);
        }

        [HttpPost]
        public ActionResult EditTask(tblMiProjectTask tblmiprojecttask, int id)
        {
            if (ModelState.IsValid)
            {
                tblmiprojecttask.projectId = int.Parse(Request.QueryString["projid"]);
                tblmiprojecttask.UpdatedOn = DateTime.Today;
                db.Entry(tblmiprojecttask).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Details", new { id = tblmiprojecttask.projectId });
            }
            return View(tblmiprojecttask);
        }

        //
        // GET: /Home/Edit/5

        public ActionResult Edit(int id = 0)
        {
            tblMiProject tblmiproject = db.tblMiProjects.Find(id);
            if (tblmiproject == null)
            {
                return HttpNotFound();
            }
            return View(tblmiproject);
        }

        //
        // POST: /Home/Edit/5

        [HttpPost]
        public ActionResult Edit(tblMiProject tblmiproject, int id)
        {
            if (ModelState.IsValid)
            {
                db.Entry(tblmiproject).State = EntityState.Modified;
                tblmiproject.UpdatedOn = DateTime.Today;
                db.SaveChanges();
                return RedirectToAction("Details", new { id = tblmiproject.projectId });
            }
            return View(tblmiproject);
        }

        //
        // GET: /Home/Delete/5

        public ActionResult Disable(int id = 0)
        {
            tblMiProject tblmiproject = db.tblMiProjects.Find(id);
            if (tblmiproject == null)
            {
                return HttpNotFound();
            }
            return View("Delete", tblmiproject);
        }

        //
        // POST: /Home/Delete/5

        [HttpPost, ActionName("Disable")]
        public ActionResult DeleteConfirmed(int id)
        {
            tblMiProject tblmiproject = db.tblMiProjects.Find(id);
            //db.tblMiProjects.Remove(tblmiproject);
            tblmiproject.Active = false;
            tblmiproject.UpdatedOn = DateTime.Today;
            db.Entry(tblmiproject).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index");
        }


        public ActionResult DisableTask(int id = 0)
        {
            tblMiProjectTask tblmiprojecttask = db.tblMiProjectTasks.Find(id);
            if (tblmiprojecttask == null)
            {
                return HttpNotFound();
            }
            return View("DeleteTask", tblmiprojecttask);
        }


        [HttpPost, ActionName("DisableTask")]
        public ActionResult DeleteTaskConfirmed(int id)
        {
            tblMiProject proj = db.tblMiProjects.Find(int.Parse(Request.QueryString["projid"]));
            tblMiProjectTask tblmiprojecttask = db.tblMiProjectTasks.Find(id);
            //db.tblMiProjectTasks.Remove(tblmiprojecttask);
            tblmiprojecttask.Active = false;
            tblmiprojecttask.UpdatedOn = DateTime.Today;
            db.Entry(tblmiprojecttask).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Details", new { id = proj.projectId });
        }



        public ActionResult Enable(int id = 0)
        {
            tblMiProject tblmiproject = db.tblMiProjects.Find(id);
            if (tblmiproject == null)
            {
                return HttpNotFound();
            }
            return View("Enable", tblmiproject);
        }

        //
        // POST: /Home/Delete/5

        [HttpPost, ActionName("Enable")]
        public ActionResult EnableConfirmed(int id)
        {
            tblMiProject tblmiproject = db.tblMiProjects.Find(id);
            //db.tblMiProjects.Remove(tblmiproject);
            tblmiproject.Active = true;
            tblmiproject.UpdatedOn = DateTime.Today;
            db.Entry(tblmiproject).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Index");
        }


        public ActionResult EnableTask(int id = 0)
        {
            tblMiProjectTask tblmiprojecttask = db.tblMiProjectTasks.Find(id);
            if (tblmiprojecttask == null)
            {
                return HttpNotFound();
            }
            return View("EnableTask", tblmiprojecttask);
        }


        [HttpPost, ActionName("EnableTask")]
        public ActionResult EnableTaskConfirmed(int id)
        {
            tblMiProject proj = db.tblMiProjects.Find(int.Parse(Request.QueryString["projid"]));
            tblMiProjectTask tblmiprojecttask = db.tblMiProjectTasks.Find(id);            
            tblmiprojecttask.Active = true;
            tblmiprojecttask.UpdatedOn = DateTime.Today;
            db.Entry(tblmiprojecttask).State = EntityState.Modified;
            db.SaveChanges();
            return RedirectToAction("Details", new { id = proj.projectId });
        }

        
        public JsonResult GetProjectList()
        {
            var projects = (from c in db.tblMiProjects
                            select new ProjectsList { ProjectId = c.projectId, ProjectName = c.ProjectName }).OrderBy(x=>x.ProjectName).ToList();

            return Json(projects, JsonRequestBehavior.AllowGet);
        }

        
        public JsonResult MoveProject(int oldid, int newid, int taskid)
        {
            tblMiProjectTask task = db.tblMiProjectTasks.Find(taskid);
            task.projectId = newid;
            db.Entry(task).State = EntityState.Modified;
            db.SaveChanges();
            return Json(new { redirect = true, url = Url.Content("/Home/Details/" + oldid) }); // RedirectToAction("Details", new { id = oldid });
        }

        public JsonResult TaskNotes(int id)
        {
            tblMiProjectTask task = db.tblMiProjectTasks.Find(id);
            string notes = task.Notes;
            string taskname = task.TaskName;
            return Json(new { notes = notes, taskname = taskname }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ProcessActiveToggle(int taskid, bool flag, int projid)
        {
            tblMiProjectTask task = db.tblMiProjectTasks.Find(taskid);
            task.Active = flag;
            db.Entry(task).State = EntityState.Modified;
            db.SaveChanges();
            return Json(new {url = Url.Content("/Home/Details/" + projid) });
        }


        protected override void Dispose(bool disposing)
        {
            db.Dispose();
            base.Dispose(disposing);
        }
    }
}