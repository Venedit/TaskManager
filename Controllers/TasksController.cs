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

    // Сторінка з формою створення задачі (просто віддає порожню форму)
    public IActionResult Create()
    {
        return View();
    }

    // Метод, який приймає дані після натискання кнопки "Створити"
    [HttpPost]
    [ValidateAntiForgeryToken] // Захист від хакерських атак (CSRF)
    public async Task<IActionResult> Create([Bind("Title,Description,Deadline,Priority")] TaskItem task)
    {
        if (ModelState.IsValid)
        {
            // Знаходимо, який користувач зараз онлайн
            var user = await _userManager.GetUserAsync(User);
            
            if (user == null) return Challenge(); // Якщо щось пішло не так з логіном

            // Заповнюємо системні поля, які користувач не має вводити вручну
            task.CreatorId = user.Id;
            task.CreatedAt = DateTime.UtcNow;
            task.Status = TaskStatus.New;

            _context.Add(task);
            await _context.SaveChangesAsync(); // Зберігаємо в PostgreSQL
            
            return RedirectToAction(nameof(Index)); // Повертаємо користувача на список задач
        }
        return View(task); // Якщо форма заповнена криво, повертаємо її назад з помилками
    }

    // GET: /Tasks/Edit/5 (Віддає форму з поточними даними задачі)
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var task = await _context.Tasks.FindAsync(id);
        if (task == null) return NotFound();

        // Дістаємо всіх користувачів для випадаючого списку "Виконавець"
        ViewData["Users"] = new SelectList(_userManager.Users, "Id", "Email", task.AssigneeId);
        
        return View(task);
    }

    // POST: /Tasks/Edit/5 (Приймає оновлені дані та зберігає в БД)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Deadline,Priority,Status,CreatorId,AssigneeId,CreatedAt")] TaskItem task)
    {
        if (id != task.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(task);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Tasks.Any(e => e.Id == task.Id)) return NotFound();
                else throw;
            }
            return RedirectToAction(nameof(Index));
        }
        
        // Якщо помилка валідації - повертаємо форму і знову завантажуємо список користувачів
        ViewData["Users"] = new SelectList(_userManager.Users, "Id", "Email", task.AssigneeId);
        return View(task);
    }
}