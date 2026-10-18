# ---- build stage ----
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Restore first, so this layer is cached until the .csproj changes
COPY GameStudioSite.csproj ./
RUN dotnet restore

COPY . .
RUN dotnet publish GameStudioSite.csproj -c Release -o /app/publish --no-restore /p:UseAppHost=false

# ---- runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Folders that must survive restarts, created and owned by the non-root app user
RUN mkdir -p /data /app/wwwroot/images/games /app/wwwroot/images/blog \
    && chown -R $APP_UID /data /app/wwwroot/images/games /app/wwwroot/images/blog

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
USER $APP_UID
ENTRYPOINT ["dotnet", "GameStudioSite.dll"]