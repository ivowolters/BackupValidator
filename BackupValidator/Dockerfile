FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["BackupValidator/BackupValidator.csproj", "BackupValidator/"]
RUN dotnet restore "BackupValidator/BackupValidator.csproj"
COPY . .
WORKDIR "/src/BackupValidator"
RUN dotnet build "BackupValidator.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BackupValidator.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BackupValidator.dll"]
