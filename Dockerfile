FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/Flowdo.Web/Flowdo.Web.csproj src/Flowdo.Web/
RUN dotnet restore src/Flowdo.Web/Flowdo.Web.csproj

COPY src/Flowdo.Web/ src/Flowdo.Web/

RUN dotnet publish src/Flowdo.Web/Flowdo.Web.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0

WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://0.0.0.0:10000

EXPOSE 10000

ENTRYPOINT ["dotnet", "Flowdo.Web.dll"]