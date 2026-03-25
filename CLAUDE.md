# Project Instructions

## Build Verification Rules

**CRITICAL: Before every commit, verify ALL new or modified .cs files compile correctly.**

1. **Check every `using` directive** — For each type referenced in a file, confirm the namespace where that type is defined and ensure a matching `using` statement exists. Do NOT assume a type is in scope just because it "seems like it should be."

2. **Cross-reference types across projects** — When using a type from another project (e.g., `CampaignMessageEvent` from `Application.Services.Automations` in a `BackgroundJobs` file), explicitly verify the type's namespace by searching the codebase with Grep, then add the correct `using`.

3. **Verify interface method signatures** — Before calling any interface method (e.g., `IEmailSender.SendEmailAsync`), read the interface definition file to confirm the exact method name, parameter order, and parameter types. Do not guess from memory.

4. **Verify entity properties** — Before referencing a property on an entity (e.g., `Contact.AvatarUrl`), confirm it exists by reading the entity file.

5. **Fix ALL errors at once** — Never fix one build error and push. When a build error is found, scan ALL files in the changeset for the same class of error before committing. Batch all fixes into a single commit.

6. **No .NET SDK available in this environment** — `dotnet build` cannot be run. Compensate by being extra rigorous with manual verification of types, namespaces, and signatures before committing.

## Project Structure

- **Backend**: .NET 8 Clean Architecture at `src/`
  - `CustomerEngagement.Api` — ASP.NET Core Web API, controllers, middleware, authorization
  - `CustomerEngagement.Application` — CQRS commands/queries, services, background jobs, DTOs
  - `CustomerEngagement.Core` — Domain entities, enums, events, interfaces
  - `CustomerEngagement.Infrastructure` — EF Core, repositories, external service integrations
  - `CustomerEngagement.Enterprise` — Captain AI, Custom Roles, SAML SSO
- **Frontend**: Angular 17+ at `frontend/`
  - `frontend/dashboard` — Internal management portal
  - `frontend/portal` — Customer self-service help center
  - `frontend/widget` — Embeddable chat widget
- **Tests**: `tests/`
- **Spec**: `PROJECT_SPEC.md` — Full feature specification
