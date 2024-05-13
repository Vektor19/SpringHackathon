FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 7152
EXPOSE 5197

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["SpringHackathon/SpringHackathon.csproj", "SpringHackathon/"]
RUN dotnet restore "./SpringHackathon/SpringHackathon.csproj"
COPY . .
WORKDIR "/src/SpringHackathon"
RUN dotnet build "./SpringHackathon.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./SpringHackathon.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SpringHackathon.dll"]