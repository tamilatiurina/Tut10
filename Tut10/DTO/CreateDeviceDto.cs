using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Tut10.DTO;

public class CreateDeviceDto
{
    [Required]
    public string Name { get; set; }
    
    [Required]
    public string Type { get; set; }
    
    [Required]
    public bool IsEnabled { get; set; }
    
    [Required]
    public string AdditionalProperties { get; set; }
}