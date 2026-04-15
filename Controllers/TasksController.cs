using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManager.Data;
using TaskManager.Models;
using TaskStatus = TaskManager.Models.TaskStatus;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TaskManager.Controllers;

[Authorize] // Цей атрибут автоматично перекидатиме незалогінених на сторінку входу
public class TasksController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public TasksController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // Сторінка зі списком задач
    public async Task<IActionResult> Index()
    {
        // Дістаємо всі таски з БД і одразу "приклеюємо" до них дані про творця (щоб вивести Email)
        var tasks = await _context.Tasks
            .Include(t => t.Creator)
            .ToListAsync();

        return View(tasks);
    }

    // GET: /Tasks/Create?projectId=5
    public IActionResult Create(int projectId)
    {
        // Передаємо ID проєкту у форму, щоб вона знала, куди ліпити таску
        ViewBag.ProjectId = projectId;

        // Вибирати виконавців можна тільки серед учасників ЦЬОГО проєкту
        var projectMembers = _context.ProjectMembers
            .Where(pm => pm.ProjectId == projectId)
            .Select(pm => pm.User);

        ViewData["AssigneeId"] = new SelectList(projectMembers, "Id", "Email");
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Title,Description,Deadline,Priority,AssigneeId,ProjectId")] TaskItem task)
    {
        // 1. Вимикаємо перевірку для полів, які ми заповнимо самі, або які є просто об'єктами-зв'язками
        ModelState.Remove("CreatorId");
        ModelState.Remove("Creator");
        ModelState.Remove("Project");
        ModelState.Remove("Assignee");

        // 2. Тепер перевіряємо, чи валідні решта полів (Назва, Дедлайн тощо)
        if (ModelState.IsValid)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Challenge();

            task.CreatorId = userId;
            task.CreatedAt = DateTime.UtcNow;
            task.Status = TaskStatus.New;
            task.Deadline = task.Deadline.ToUniversalTime();

            _context.Add(task);
            await _context.SaveChangesAsync();

            // Повертаємося на сторінку проєкту
            return RedirectToAction("Details", "Projects", new { id = task.ProjectId });
        }

        // 3. ЯКЩО ФОРМА ЗАПОВНЕНА НЕПРАВИЛЬНО (наприклад, забув назву):
        // Нам треба наново завантажити випадаючий список виконавців і ID проєкту, 
        // інакше сторінка впаде при спробі показати форму знову.
        var projectMembers = _context.ProjectMembers
            .Where(pm => pm.ProjectId == task.ProjectId)
            .Select(pm => pm.User);

        ViewData["AssigneeId"] = new SelectList(projectMembers, "Id", "Email", task.AssigneeId);
        ViewBag.ProjectId = task.ProjectId;

        return View(task);
    }

    // GET: /Tasks/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var task = await _context.Tasks.FindAsync(id);
        if (task == null) return NotFound();

        // Завантажуємо ТІЛЬКИ учасників цього проєкту
        var projectMembers = _context.ProjectMembers
            .Where(pm => pm.ProjectId == task.ProjectId)
            .Select(pm => pm.User);

        ViewData["Users"] = new SelectList(projectMembers, "Id", "Email", task.AssigneeId);

        return View(task);
    }

    // POST: /Tasks/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    // ВАЖЛИВО: Додали ProjectId у список Bind
    public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Deadline,Priority,Status,CreatorId,AssigneeId,CreatedAt,ProjectId")] TaskItem task)
    {
        if (id != task.Id) return NotFound();

        // Вимикаємо валідацію об'єктів-зв'язків
        ModelState.Remove("Creator");
        ModelState.Remove("Project");
        ModelState.Remove("Assignee");

        if (ModelState.IsValid)
        {
            try
            {
                // Фікс часу для PostgreSQL
                task.Deadline = task.Deadline.ToUniversalTime();
                task.CreatedAt = task.CreatedAt.ToUniversalTime();

                _context.Update(task);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Tasks.Any(e => e.Id == task.Id)) return NotFound();
                else throw;
            }
            // Після успішного редагування повертаємо на сторінку проєкту!
            return RedirectToAction("Details", "Projects", new { id = task.ProjectId });
        }

        // Якщо помилка - відновлюємо список учасників
        var projectMembers = _context.ProjectMembers
            .Where(pm => pm.ProjectId == task.ProjectId)
            .Select(pm => pm.User);

        ViewData["Users"] = new SelectList(projectMembers, "Id", "Email", task.AssigneeId);
        return View(task);
    }

    // POST: /Tasks/Claim/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Claim(int id)
    {
        var task = await _context.Tasks.FindAsync(id);
        if (task == null) return NotFound();

        var userId = _userManager.GetUserId(User);
        if (userId == null) return Challenge();

        // Перевіряємо, чи є користувач учасником проєкту (захист від несанкціонованого доступу)
        var isMember = await _context.ProjectMembers
            .AnyAsync(pm => pm.ProjectId == task.ProjectId && pm.UserId == userId);

        if (!isMember) return Forbid();

        // Призначаємо задачу на себе
        task.AssigneeId = userId;
        // Одразу змінюємо статус на "В процесі", бо ми її взяли в роботу
        task.Status = TaskStatus.InProgress;

        _context.Update(task);
        await _context.SaveChangesAsync();

        return RedirectToAction("Details", "Projects", new { id = task.ProjectId });
    }

    // POST: /Tasks/Unclaim/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unclaim(int id)
    {
        var task = await _context.Tasks.FindAsync(id);
        if (task == null) return NotFound();

        task.AssigneeId = null; // Прибираємо виконавця
        task.Status = TaskStatus.New; // Повертаємо статус "Нова"

        _context.Update(task);
        await _context.SaveChangesAsync();
        return RedirectToAction("Details", "Projects", new { id = task.ProjectId });
    }

    // POST: /Tasks/UpdateStatus/5?newStatus=Review
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int id, TaskStatus newStatus)
    {
        var task = await _context.Tasks.FindAsync(id);
        if (task == null) return NotFound();

        task.Status = newStatus;

        _context.Update(task);
        await _context.SaveChangesAsync();
        return RedirectToAction("Details", "Projects", new { id = task.ProjectId });
    }
}