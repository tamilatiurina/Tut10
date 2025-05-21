namespace Tut10.DTO;

public record PersonDto(
    int Id,
    string PassportNumber,
    string FirstName,
    string? MiddleName,
    string LastName,
    string PhoneNumber,
    string Email,
    decimal Salary,
    PositionDto Position,
    DateTime HireDate
);