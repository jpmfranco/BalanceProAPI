FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Copiar desde subcarpeta WebApplication1
COPY WebApplication1/*.csproj ./WebApplication1/
RUN dotnet restore ./WebApplication1/WebApplication1.csproj

COPY WebApplication1/. ./WebApplication1/
RUN dotnet publish ./WebApplication1/WebApplication1.csproj -c Release -o /publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "WebApplication1.dll"]