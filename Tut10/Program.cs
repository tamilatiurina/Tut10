using System.Collections.Immutable;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tut10;
using Tut10.DTO;

var builder = WebApplication.CreateBuilder(args);

var _connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Default connection string not found");

builder.Services.AddDbContext<Tut10Context>(options => options.UseSqlServer(_connectionString));
// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("/api/devices", async (Tut10Context context, CancellationToken token) =>
{
    try
    {
        var ShortInfoDevices = context.Devices.Select(e => new ShortDeviceDto(e.Id, e.Name));
        return Results.Ok(await ShortInfoDevices.ToListAsync(token));
    }
    catch(Exception ex)
    {
        return Results.Problem(new ProblemDetails
        {
            Detail = ex.Message,
            Title = "Server error",
            Instance = "api/devices",
        });

    }
});

app.MapGet("/api/devices/{id}", async (Tut10Context context, CancellationToken token, long id) =>
{
    try
    {
        var device = await context.Devices
            .Include(d => d.DeviceType)
            .Include(d => d.DeviceEmployees)
            .ThenInclude(de => de.Employee)
            .ThenInclude(e => e.Person)
            .FirstOrDefaultAsync(d => d.Id == id, token);

        if (device == null)
            return Results.NotFound();
        
        var activeAssignment = device.DeviceEmployees
            .Where(de => de.ReturnDate == null)
            .OrderByDescending(de => de.IssueDate)
            .FirstOrDefault();

        EmployeeDto? currentEmployee = null;

        if (activeAssignment != null)
        {
            var emp = activeAssignment.Employee;
            currentEmployee = new EmployeeDto(
                emp.Id,
                $"{emp.Person.FirstName} {emp.Person.LastName}"
            );
        }

        var result = new DeviceDto(
            device.Name,
            device.DeviceType?.Name ?? "Unknown",
            device.IsEnabled,
            JsonSerializer.Deserialize<object>(device.AdditionalProperties),
            currentEmployee
        );

        return Results.Ok(result);
    }
    catch(Exception ex)
    {
        return Results.Problem(new ProblemDetails
        {
            Detail = ex.Message,
            Title = "Server error",
            Instance = $"api/devices/{id}",
        });
    }   
});

app.MapPost("/api/devices", async (Tut10Context context, CancellationToken token, CreateDeviceDto deviceDto)=>
{
    try
    {
        var deviceType = await context.DeviceTypes
            .FirstOrDefaultAsync(dt => dt.Name == deviceDto.Type, token);
        
        if (deviceType == null)
        {
            return Results.NotFound($"DeviceType not found.");
        }

        var newDevice = new Device
        {
            Name = deviceDto.Name,
            IsEnabled = deviceDto.IsEnabled,
            AdditionalProperties = deviceDto.AdditionalProperties,
            DeviceTypeId = deviceType.Id
        };
        
        await context.Devices.AddAsync(newDevice, token);
        await context.SaveChangesAsync(token);
        return Results.Created($"/api/employees/{newDevice.Id}", newDevice);
    }
    catch(Exception ex)
    {
        return Results.Problem(new ProblemDetails
        {
            Detail = ex.Message,
            Title = "Cannot create new device",
            Instance = $"api/devices",
        });
    }   
});

app.MapPut("/api/devices/{id}", async (Tut10Context context, CancellationToken token, CreateDeviceDto deviceDto, long id) =>
{
    try
    {
        var device = await context.Devices
            .FirstOrDefaultAsync(d => d.Id == id, token);
        
        if (device == null)
            return Results.NotFound($"Device with ID {id} not found.");
        
        var deviceType = await context.DeviceTypes
            .FirstOrDefaultAsync(dt => dt.Name == deviceDto.Type, token);
        
        if (deviceType == null)
        {
            return Results.NotFound($"DeviceType not found.");
        }
        
        device.Name = deviceDto.Name;
        device.IsEnabled = deviceDto.IsEnabled;
        device.AdditionalProperties = deviceDto.AdditionalProperties;
        device.DeviceTypeId = deviceType.Id;
        
        context.Devices.Update(device);
        await context.SaveChangesAsync(token);
        return Results.Created($"/api/employees/{device.Id}", device);
        
    }
    catch(Exception ex)
    {
        return Results.Problem(new ProblemDetails
        {
            Detail = ex.Message,
            Title = "Cannot update device",
            Instance = $"api/devices",
        });
    }   
    
});

app.MapDelete("/api/devices/{id}", async (Tut10Context context, CancellationToken token, long id) =>
{
    try
    {
        var device = await context.Devices
            .FirstOrDefaultAsync(d => d.Id == id, token);

        if (device == null)
            return Results.NotFound($"Device with ID {id} not found.");

        context.Devices.Remove(device);
        await context.SaveChangesAsync(token);

        return Results.NoContent();
    }
    catch (Exception ex)
    {
        return Results.Problem(new ProblemDetails
        {
            Detail = ex.Message,
            Title = "Cannot delete the device",
            Instance = $"/api/devices/{id}"
        });
    }
});

app.MapGet("/api/employees", async (Tut10Context context, CancellationToken token) =>
{
    try
    {
        var ShortInfoEmployees = context.Employees.Select(e => new ShortEmployeeDto(e.Id, 
            $"{e.Person.FirstName} {e.Person.MiddleName} {e.Person.LastName}"));
        return Results.Ok(await ShortInfoEmployees.ToListAsync(token));
    }
    catch(Exception ex)
    {
        return Results.Problem(new ProblemDetails
        {
            Detail = ex.Message,
            Title = "Server error",
            Instance = "api/employees",
        });

    }
});

app.MapGet("/api/employees/{id}", async (Tut10Context context, CancellationToken token, long id) =>
{
    try
    {
        var employee = await context.Employees
            .Include(e => e.Person)
            .Include(e => e.Position)
            .FirstOrDefaultAsync(e => e.Id == id, token);

        if (employee == null)
            return Results.NotFound();

        var dto = new PersonDto(
            employee.Id,
            employee.Person.PassportNumber,
            employee.Person.FirstName,
            employee.Person.MiddleName,
            employee.Person.LastName,
            employee.Person.PhoneNumber,
            employee.Person.Email,
            employee.Salary,
            new PositionDto(employee.Position.Id, employee.Position.Name),
            employee.HireDate
        );

        return Results.Ok(dto);
    }
    catch(Exception ex)
    {
        return Results.Problem(new ProblemDetails
        {
            Detail = ex.Message,
            Title = "Server error",
            Instance = $"api/devices/{id}",
        });
    }   
});


app.Run();

