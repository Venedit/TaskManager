using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using TaskManager.Data;
using TaskManager.Models;
using TaskStatus = TaskManager.Models.TaskStatus; // Щоб не плутати статуси

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
        // Перевіряємо, чи користувач зараз авторизований
        if (User.Identity != null && User.Identity.IsAuthenticated)
        {
            var userId = _userManager.GetUserId(User);

            // Рахуємо всі задачі, де він є або Творцем, або Виконавцем
            var myTasks = await _context.Tasks
                .Where(t => t.CreatorId == userId || t.AssigneeId == userId)
                .ToListAsync();

            // ViewBag — це найпростіший спосіб передати дрібні дані з Контролера у View
            ViewBag.Total = myTasks.Count;
            ViewBag.InProgress = myTasks.Count(t => t.Status == TaskStatus.InProgress);
            ViewBag.Completed = myTasks.Count(t => t.Status == TaskStatus.Completed);
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