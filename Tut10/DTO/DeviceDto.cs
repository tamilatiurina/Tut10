namespace Tut10.DTO;

public record DeviceDto(string Name, string TypeName, bool IsEnabled, object AdditionalProperties, EmployeeDto? CurrentEmployee);