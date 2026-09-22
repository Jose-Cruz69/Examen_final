namespace TaskFlow_G2.DTOs;
using System.ComponentModel.DataAnnotations;

public class TaskCreateDto
{
    [Required(ErrorMessage = "El título es obligatorio.")]
    public string Title { get; set; } = string.Empty;

    [Range(1, 3, ErrorMessage = "La prioridad debe estar entre 1 y 3.")]
    public int PriorityLevel { get; set; }

    public string? Notes { get; set; }
}