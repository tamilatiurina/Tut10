1. Create appsettings.json as follows:
```
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOURSERVER;Database=YOURDATABASE;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```
2. Create appsettings.Development.json as follows:
```
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

3. The task is small, therefore in purpose of simplicity I chose not to split in several projects.
