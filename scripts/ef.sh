#!/usr/bin/env bash
#
# Run `dotnet ef` against the source tree inside docker.
#
# Examples:
#   scripts/ef.sh migrations add InitialCreate
#   scripts/ef.sh migrations list
#   scripts/ef.sh database update
#   scripts/ef.sh migrations remove
#
# The wrapper appends --project / --startup-project so you don't have to
# repeat them every call. For `migrations add` and `migrations script` it
# also appends --output-dir so generated files land in
# src/CustomerEngagement.Infrastructure/Persistence/Migrations.
#
set -euo pipefail

cd "$(dirname "$0")/.."

if [ "$#" -eq 0 ]; then
  echo "Usage: scripts/ef.sh <ef-command> [args...]" >&2
  echo "       e.g. scripts/ef.sh migrations add InitialCreate" >&2
  exit 1
fi

extra_args=()
if [ "${1:-}" = "migrations" ] && { [ "${2:-}" = "add" ] || [ "${2:-}" = "script" ]; }; then
  extra_args+=(--output-dir Persistence/Migrations)
fi

docker compose --profile tools run --rm ef-tools \
  "$@" \
  --project src/CustomerEngagement.Infrastructure \
  --startup-project src/CustomerEngagement.Api \
  "${extra_args[@]}"
