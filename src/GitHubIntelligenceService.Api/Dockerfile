# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/GitHubIntelligenceService.Api/GitHubIntelligenceService.Api.csproj", "src/GitHubIntelligenceService.Api/"]
COPY ["src/GitHubIntelligenceService.Application/GitHubIntelligenceService.Application.csproj", "src/GitHubIntelligenceService.Application/"]
COPY ["src/GitHubIntelligenceService.Domain/GitHubIntelligenceService.Domain.csproj", "src/GitHubIntelligenceService.Domain/"]
COPY ["src/GitHubIntelligenceService.Infrastructure/GitHubIntelligenceService.Infrastructure.csproj", "src/GitHubIntelligenceService.Infrastructure/"]
RUN dotnet restore "src/GitHubIntelligenceService.Api/GitHubIntelligenceService.Api.csproj"
COPY . .
WORKDIR "/src/src/GitHubIntelligenceService.Api"
RUN dotnet build "GitHubIntelligenceService.Api.csproj" -c Release -o /app/build

# Publish Stage
FROM build AS publish
RUN dotnet publish "GitHubIntelligenceService.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Final Stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GitHubIntelligenceService.Api.dll"]
