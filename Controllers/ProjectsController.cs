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

            _context.Add(project);
            await _context.SaveChangesAsync();

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

    // GET: /Projects/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var userId = _userManager.GetUserId(User);

        var project = await _context.Projects
            .Include(p => p.Tasks)
            .Include(p => p.Members!)
                .ThenInclude(m => m.User)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (project == null) return NotFound();

        var memberRecord = project.Members!.FirstOrDefault(m => m.UserId == userId);
        if (memberRecord == null) return Forbid();

        ViewBag.UserRole = memberRecord.Role;

        return View(project);
    }

    // GET: /Projects/AddMember/5
public IActionResult AddMember(int projectId)
{
    ViewBag.ProjectId = projectId;
    return View();
}

// POST: /Projects/AddMember
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> AddMember(int projectId, string email, ProjectRole role)
{
    var user = await _userManager.FindByEmailAsync(email);
    if (user == null)
    {
        ModelState.AddModelError("", "Користувача з таким Email не знайдено.");
        ViewBag.ProjectId = projectId;
        return View();
    }

    // Перевірка, чи він вже не в команді
    var exists = await _context.ProjectMembers
        .AnyAsync(pm => pm.ProjectId == projectId && pm.UserId == user.Id);

    if (exists)
    {
        ModelState.AddModelError("", "Цей користувач вже є учасником проєкту.");
        ViewBag.ProjectId = projectId;
        return View();
    }

    var member = new ProjectMember
    {
        ProjectId = projectId,
        UserId = user.Id,
        Role = role
    };

    _context.Add(member);
    await _context.SaveChangesAsync();

    return RedirectToAction(nameof(Details), new { id = projectId });
}
}