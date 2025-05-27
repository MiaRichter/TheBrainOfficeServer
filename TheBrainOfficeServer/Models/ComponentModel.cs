namespace TheBrainOfficeServer.Models;

public class ComponentModel
{
    public int Id { get; set; }
    public string? ComponentId { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? ComponentType { get; set; }
    public string? Location { get; set; }
    public string? CreatedAt { get; set; }
    public string? UpdatedAt { get; set; }
    public bool IsActive { get; set; }
}