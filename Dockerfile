FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["crud.csproj", "./"]
RUN dotnet restore "./crud.csproj"

COPY . .
RUN dotnet publish "./crud.csproj" --configuration Release --output /app/publish --no-restore /p:UseAppHost=false
RUN rm -f /app/publish/appsettings*.json

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 10000

ENTRYPOINT ["sh", "-c", "dotnet crud.dll --urls http://0.0.0.0:${PORT:-10000}"]
