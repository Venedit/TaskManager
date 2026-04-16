using System.ComponentModel.DataAnnotations;
namespace TaskManager.Models;


public enum TaskPriority
{
    [Display(Name = "Низький")]
    Low,
    
    [Display(Name = "Середній")]
    Medium,

    [Display(Name = "Високий")]
    High,
    
    [Display(Name = "Терміново (Високий)")]
    Urgent
}

public enum TaskStatus
{
    [Display(Name = "Нова")]
    New,
    
    [Display(Name = "В процесі")]
    InProgress,
    
    [Display(Name = "На перевірці")] // Додаємо цей статус
    Review,
    
    [Display(Name = "Виконано")]
    Completed
}
public enum ProjectRole
{
    Owner,
    Manager, 
    Developer,
    Viewer
}