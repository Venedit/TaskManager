using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using TaskManager.Data;
using TaskManager.Models;
using TaskStatus = TaskManager.Models.TaskStatus;

namespace TaskManager.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    // Впроваджуємо залежності (БД та Менеджер користувачів)
    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            var userId = _userManager.GetUserId(User);

            // Отримуємо проєкти користувача
            var projects = await _context.Projects
                .Where(p => p.Members!.Any(m => m.UserId == userId))
                .ToListAsync();

            // Отримуємо 5 найближчих задач, які ще не виконані
            var urgentTasks = await _context.Tasks
                .Include(t => t.Project)
                .Where(t => (t.AssigneeId == userId || t.CreatorId == userId) && t.Status != TaskStatus.Completed)
                .OrderBy(t => t.Deadline)
                .Take(5)
                .ToListAsync();

            ViewBag.Projects = projects;
            ViewBag.UrgentTasks = urgentTasks;
        }

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}