# EF Core Migrations

## Generating Migrations

Generate the initial migration using the .NET CLI:

```bash
# Install the EF Core tools (if not already installed)
dotnet tool install --global dotnet-ef

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

## Adding New Migrations

When you modify entities or DbContext configuration:

```bash
dotnet ef migrations add <MigrationName> \
  --project src/CustomerEngagement.Infrastructure \
  --startup-project src/CustomerEngagement.Api \
  --output-dir Persistence/Migrations
```

## Startup Behavior

The application automatically applies migrations on startup via `Database.Migrate()` in `Program.cs`.
If no migrations exist yet (initial development), it falls back to `EnsureCreated()` to bootstrap the schema.

Once you generate the first migration with `dotnet ef migrations add`, the `Migrate()` path
will be used exclusively and `EnsureCreated()` will no longer be needed.

## Rollback

```bash
# Revert to a specific migration
dotnet ef database update <PreviousMigrationName> \
  --project src/CustomerEngagement.Infrastructure \
  --startup-project src/CustomerEngagement.Api

# Remove the last migration (if not yet applied)
dotnet ef migrations remove \
  --project src/CustomerEngagement.Infrastructure \
  --startup-project src/CustomerEngagement.Api
```
