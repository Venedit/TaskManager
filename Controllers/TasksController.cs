using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using TaskManager.Models;
using TaskManager.Services.Interfaces;

namespace TaskManager.Controllers
{
    [Authorize]
    public class TasksController : Controller
    {
        private readonly ITaskService _taskService;
        private readonly IProjectService _projectService;
        private readonly UserManager<ApplicationUser> _userManager;

        // Ін'єктуємо обидва сервіси, бо для Edit нам треба завантажити список мемберів проєкту
        public TasksController(ITaskService taskService, IProjectService projectService, UserManager<ApplicationUser> userManager)
        {
            _taskService = taskService;
            _projectService = projectService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Create(int projectId)
        {
            ViewBag.ProjectId = projectId;

            var project = await _projectService.GetProjectDetailsAsync(projectId);

            if (project != null && project.Members != null)
            {
                ViewData["AssigneeId"] = new SelectList(project.Members.Select(m => m.User), "Id", "Email");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Description,Deadline,Priority,AssigneeId,ProjectId")] TaskItem task)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Challenge();

            task.CreatorId = userId;
            task.Status = Models.TaskStatus.New;

            ModelState.Remove("CreatorId");
            ModelState.Remove("Creator");
            ModelState.Remove("Project");
            ModelState.Remove("Assignee");

            if (ModelState.IsValid)
            {

                await _taskService.CreateTaskAsync(task);
                return RedirectToAction("Details", "Projects", new { id = task.ProjectId });
            }

            var project = await _projectService.GetProjectDetailsAsync(task.ProjectId);
            if (project != null && project.Members != null)
            {
                ViewData["AssigneeId"] = new SelectList(project.Members.Select(m => m.User), "Id", "Email", task.AssigneeId);
            }

            ViewBag.ProjectId = task.ProjectId;
            return View(task);
        }

        public async Task<IActionResult> Edit(int id)
        {
            var task = await _taskService.GetTaskByIdAsync(id);
            if (task == null) return NotFound();

            var project = await _projectService.GetProjectDetailsAsync(task.ProjectId);
            if (project != null && project.Members != null)
            {
                ViewData["AssigneeId"] = new SelectList(project.Members.Select(m => m.User), "Id", "Email", task.AssigneeId);
            }

            return View(task);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Deadline,Priority,Status,CreatorId,AssigneeId,CreatedAt,ProjectId")] TaskItem task)
        {
            if (id != task.Id) return NotFound();

            ModelState.Remove("Creator");
            ModelState.Remove("Project");
            ModelState.Remove("Assignee");

            if (ModelState.IsValid)
            {
                var result = await _taskService.UpdateTaskAsync(task);
                if (!result) return NotFound();

                return RedirectToAction("Details", "Projects", new { id = task.ProjectId });
            }

            var project = await _projectService.GetProjectDetailsAsync(task.ProjectId);
            if (project != null && project.Members != null)
            {
                ViewData["AssigneeId"] = new SelectList(project.Members.Select(m => m.User), "Id", "Email", task.AssigneeId);
            }

            return View(task);
        }

        public async Task<IActionResult> Details(int id)
        {
            var task = await _taskService.GetTaskByIdAsync(id);
            if (task == null) return NotFound();

            return View(task);
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Claim(int id)
        {
            var userId = _userManager.GetUserId(User);
            var result = await _taskService.ClaimTaskAsync(id, userId!);

            if (!result) return Forbid();

            var task = await _taskService.GetTaskByIdAsync(id);
            return RedirectToAction("Details", "Projects", new { id = task?.ProjectId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Unclaim(int id)
        {
            await _taskService.UnclaimTaskAsync(id);
            var task = await _taskService.GetTaskByIdAsync(id);
            return RedirectToAction("Details", "Projects", new { id = task?.ProjectId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, Models.TaskStatus newStatus)
        {
            await _taskService.UpdateTaskStatusAsync(id, newStatus);
            var task = await _taskService.GetTaskByIdAsync(id);
            return RedirectToAction("Details", "Projects", new { id = task?.ProjectId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userManager.GetUserId(User);
            var task = await _taskService.GetTaskByIdAsync(id);
            if (task == null) return NotFound();

            var result = await _taskService.DeleteTaskAsync(id, userId!);
            if (!result) return Forbid();

            return RedirectToAction("Details", "Projects", new { id = task.ProjectId });
        }
    }
}