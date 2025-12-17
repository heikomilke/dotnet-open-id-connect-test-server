FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project file and restore
COPY src/TestOidcServer/TestOidcServer.csproj ./TestOidcServer/
RUN dotnet restore TestOidcServer/TestOidcServer.csproj

# Copy source and build
COPY src/TestOidcServer/ ./TestOidcServer/
RUN dotnet publish TestOidcServer/TestOidcServer.csproj -c Release -o /app/publish

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

# Create config directory
RUN mkdir -p /app/config

ENV ASPNETCORE_URLS=http://+:50000
ENV ConfigPath=/app/config

EXPOSE 50000

ENTRYPOINT ["dotnet", "TestOidcServer.dll"]
