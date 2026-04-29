using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using TaskManager.Models;
using TaskManager.Services.Interfaces; 

namespace TaskManager.Controllers // Твій namespace
{
    public class HomeController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IProjectService _projectService;
        
        // 1. Додаємо поле для сервісу задач
        private readonly ITaskService _taskService; 

        // 2. Ін'єктуємо ITaskService через конструктор
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

                // 3. Збираємо дані з обох сервісів
                ViewBag.Projects = await _projectService.GetUserProjectsAsync(userId);
                
                // Саме цього рядка не вистачало сторінці, щоб намалювати дедлайни!
                ViewBag.UrgentTasks = await _taskService.GetUrgentTasksAsync(userId); 
            }


            return View();
        }
    }
}