FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["src/LibraryManagement.API/LibraryManagement.API.csproj", "src/LibraryManagement.API/"]
COPY ["src/LibraryManagement.Application/LibraryManagement.Application.csproj", "src/LibraryManagement.Application/"]
COPY ["src/LibraryManagement.Domain/LibraryManagement.Domain.csproj", "src/LibraryManagement.Domain/"]
COPY ["src/LibraryManagement.Infrastructure/LibraryManagement.Infrastructure.csproj", "src/LibraryManagement.Infrastructure/"]
RUN dotnet restore "src/LibraryManagement.API/LibraryManagement.API.csproj"
COPY . .
WORKDIR "/src/src/LibraryManagement.API"
RUN dotnet build "LibraryManagement.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "LibraryManagement.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "LibraryManagement.API.dll"]
