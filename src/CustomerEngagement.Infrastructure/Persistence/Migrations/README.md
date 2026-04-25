# EF Core Migrations

## Workflow (docker-based, no local .NET SDK required)

This repo ships an `ef-tools` service in [docker-compose.yml](../../../../docker-compose.yml)
that runs `dotnet ef` inside an SDK container with the source tree mounted.
Generated migration files land in this directory. A wrapper script at
[scripts/ef.sh](../../../../scripts/ef.sh) prefills the standard `--project`
arguments.

### First-time setup

If you have never generated a migration before, the API has been bootstrapping
its schema via `EnsureCreated()` (see [Program.cs](../../../CustomerEngagement.Api/Program.cs)).
Switching to migrations cleanly requires starting from an empty database — the
migration's `CREATE TABLE` statements will collide with tables EnsureCreated
already created. Drop the dev volume first:

```bash
docker compose down -v
```

Bring postgres up so the tool has something to talk to (the connection isn't
strictly required for `migrations add`, but it is for `database update`):

```bash
docker compose --profile dev up -d postgres
```

Generate the initial migration. The wrapper builds the SDK image (cached
afterwards) and runs `dotnet ef migrations add` with the right project flags:

```bash
scripts/ef.sh migrations add InitialCreate
```

That writes one `<timestamp>_InitialCreate.cs` plus an `AppDbContextModelSnapshot.cs`
into this folder. **Commit them.**

Now bring up the stack normally — the API's startup code calls `MigrateAsync()`
when migrations are present, so the schema gets created on first boot:

```bash
docker compose --profile dev up --build -d
```

### Subsequent schema changes

After editing entities or `AppDbContext`:

```bash
scripts/ef.sh migrations add <DescriptiveName>
docker compose --profile dev up --build -d
```

The new migration applies on the next API startup.

### Other commands

```bash
scripts/ef.sh migrations list           # list applied + pending
scripts/ef.sh database update           # apply pending migrations now
scripts/ef.sh database update <Name>    # roll forward/back to <Name>
scripts/ef.sh migrations remove         # drop the last migration (must be unapplied)
```

For one-off invocations without the wrapper:

```bash
docker compose --profile tools run --rm ef-tools migrations list \
  --project src/CustomerEngagement.Infrastructure \
  --startup-project src/CustomerEngagement.Api
```

## Startup behaviour

[Program.cs](../../../CustomerEngagement.Api/Program.cs) handles bootstrapping:

1. If migration files exist, `Database.MigrateAsync()` applies them.
2. If no migrations are defined yet, falls back to `EnsureCreated()`.
3. As a safety net, if the schema check (does `AspNetRoles` exist?) fails after
   either of the above, it recreates the database via `EnsureDeleted()` +
   `EnsureCreated()` — destructive, so make sure migrations are applying
   cleanly once you switch off EnsureCreated.
