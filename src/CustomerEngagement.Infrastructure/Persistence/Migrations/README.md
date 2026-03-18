# EF Core Migrations

Generate the initial migration using the .NET CLI:

```bash
# From the solution root directory
dotnet ef migrations add InitialCreate \
  --project src/CustomerEngagement.Infrastructure \
  --startup-project src/CustomerEngagement.Api \
  --output-dir Persistence/Migrations

# Apply the migration
dotnet ef database update \
  --project src/CustomerEngagement.Infrastructure \
  --startup-project src/CustomerEngagement.Api
```

Alternatively, apply migrations at startup by adding to `Program.cs`:

```csharp
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}
```
