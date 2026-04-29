using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using TaskManager.Models;
using TaskManager.Services.Interfaces; 

namespace TaskManager.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IProjectService _projectService;
        
        private readonly ITaskService _taskService; 

        public HomeController(
            UserManager<ApplicationUser> userManager, 
            IProjectService projectService,
            ITaskService taskService) 
        {
            _userManager = userManager;
            _projectService = projectService;
            _taskService = taskService;
        }

        public async Task<IActionResult> Index()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = _userManager.GetUserId(User);

                ViewBag.Projects = await _projectService.GetUserProjectsAsync(userId);
                
                ViewBag.UrgentTasks = await _taskService.GetUrgentTasksAsync(userId); 
            }


            return View();
        }
    }
}