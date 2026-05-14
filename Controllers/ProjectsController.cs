using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Models;
using TaskManager.ViewModels;
using TaskManager.Services.Interfaces;

namespace TaskManager.Controllers
{
    [Authorize]
    public class ProjectsController : Controller
    {
        private readonly IProjectService _projectService;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProjectsController(IProjectService projectService, UserManager<ApplicationUser> userManager)
        {
            _projectService = projectService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Details(int id)
        {
            var project = await _projectService.GetProjectDetailsAsync(id);
            if (project == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            var member = project.Members?.FirstOrDefault(m => m.UserId == userId);

            if (member == null) return Forbid();

            ViewBag.UserRole = member.Role;
            return View(project);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProjectCreateViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var userId = _userManager.GetUserId(User);
            if (userId == null) return Challenge();

            await _projectService.CreateProjectAsync(model, userId);

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = _userManager.GetUserId(User);
            var result = await _projectService.DeleteProjectAsync(id, userId!);

            if (!result) return Forbid();
            return RedirectToAction("Index", "Home");
        }

        public IActionResult AddMember(int projectId)
        {
            ViewBag.ProjectId = projectId;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMember(int projectId, string email, ProjectRole role)
        {
            var currentUserId = _userManager.GetUserId(User); // ДОДАНО

            var result = await _projectService.AddMemberAsync(projectId, email, role, currentUserId!);

            if (!result)
            {
                ModelState.AddModelError("", "Не вдалося додати користувача. Або немає прав, або користувач не знайдений.");
                ViewBag.ProjectId = projectId;
                return View();
            }

            return RedirectToAction(nameof(Details), new { id = projectId });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveMember(int projectId, string userId)
        {
            var currentUserId = _userManager.GetUserId(User);
            await _projectService.RemoveMemberAsync(projectId, userId, currentUserId!);

            return RedirectToAction(nameof(Details), new { id = projectId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRole(int projectId, string userId, ProjectRole newRole)
        {
            var currentUserId = _userManager.GetUserId(User);
            await _projectService.UpdateMemberRoleAsync(projectId, userId, newRole, currentUserId!);

            return RedirectToAction(nameof(Details), new { id = projectId });
        }

        // GET: /Projects/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var project = await _projectService.GetProjectDetailsAsync(id);
            if (project == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            var member = project.Members?.FirstOrDefault(m => m.UserId == userId);

            if (member == null || member.Role != ProjectRole.Owner) return Forbid();

            return View(project);
        }

        // POST: /Projects/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description")] Project project)
        {
            if (id != project.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var userId = _userManager.GetUserId(User);
                var result = await _projectService.UpdateProjectAsync(project, userId!);

                if (!result) return Forbid();
                return RedirectToAction(nameof(Details), new { id = project.Id });
            }
            return View(project);
        }

    }
}