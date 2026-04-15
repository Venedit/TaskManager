using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManager.Data;
using TaskManager.Models;

namespace TaskManager.Controllers;

[Authorize]
public class ProjectsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public ProjectsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: /Projects/
    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);

        // Шукаємо тільки ті проєкти, де поточний користувач є учасником
        var myProjects = await _context.Projects
            .Where(p => p.Members!.Any(m => m.UserId == userId))
            .ToListAsync();

        return View(myProjects);
    }

    // GET: /Projects/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: /Projects/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Name,Description")] Project project)
    {
        if (ModelState.IsValid)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            project.CreatedAt = DateTime.UtcNow;

            // Зберігаємо проєкт у БД (це згенерує йому Id)
            _context.Add(project);
            await _context.SaveChangesAsync();

            // ОДРАЗУ робимо творця Власником (Owner)
            var member = new ProjectMember
            {
                ProjectId = project.Id,
                UserId = user.Id,
                Role = ProjectRole.Owner
            };

            _context.Add(member);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
        return View(project);
    }
}