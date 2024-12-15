FROM mcr.microsoft.com/dotnet/aspnet:9.0-bookworm-slim-amd64 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0-bookworm-slim-amd64 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["mstdnCats.csproj", "./"]
RUN dotnet restore "mstdnCats.csproj"
COPY . .
WORKDIR "/src/"
RUN dotnet build "mstdnCats.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "mstdnCats.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "mstdnCats.dll"]
