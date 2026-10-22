FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

# Copy only application source
COPY src/Flowdo.Web/ ./src/Flowdo.Web/

# Restore and publish inside Linux
RUN dotnet publish src/Flowdo.Web/Flowdo.Web.csproj \
    -c Release \
    -o /app/publish

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://0.0.0.0:10000

EXPOSE 10000

ENTRYPOINT ["dotnet", "Flowdo.Web.dll"]