# Tahap Build menggunakan .NET 10 SDK
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy file csproj dan restore dependencies
COPY ["*.csproj", "./"]
RUN dotnet restore

# Copy seluruh file kode dan lakukan build
COPY . .
RUN dotnet publish -c Release -o /app/publish

# Tahap Runtime (Production) menggunakan .NET 10 ASP.NET
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# WebService memakai port 8000 secara default
ENV ASPNETCORE_HTTP_PORTS=8000
EXPOSE 8000

# Jalankan aplikasi
ENTRYPOINT ["dotnet", "Pomolog.Api.dll"]