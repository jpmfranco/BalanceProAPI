FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

COPY WebApplication1/WebApplication1.csproj ./WebApplication1/
RUN dotnet restore ./WebApplication1/WebApplication1.csproj

COPY WebApplication1/. ./WebApplication1/
RUN dotnet publish ./WebApplication1/WebApplication1.csproj -c Release -o /publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
# No correr como root: el puerto 8080 (>1024) no lo requiere y la app
# no escribe nada al disco. La imagen ya trae el usuario 'app'.
USER app
ENTRYPOINT ["dotnet", "WebApplication1.dll"]