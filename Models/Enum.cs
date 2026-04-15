namespace TaskManager.Models;

public enum TaskPriority
{
    Low, Medium, High, Urgent
}

public enum TaskStatus
{
    New, InProgress, Review, Completed
}
public enum ProjectRole
{
    Owner,
    Manager, 
    Developer,
    Viewer
}