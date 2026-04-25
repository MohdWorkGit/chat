FROM mcr.microsoft.com/dotnet/sdk:8.0

# Install dotnet-ef as a global tool. Pin to the same EF Core version
# as the Microsoft.EntityFrameworkCore PackageReference in the projects
# (see CustomerEngagement.Application.csproj — 8.0.0).
RUN dotnet tool install --global dotnet-ef --version 8.0.0

ENV PATH="/root/.dotnet/tools:${PATH}"

WORKDIR /src

ENTRYPOINT ["dotnet", "ef"]
