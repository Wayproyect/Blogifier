FROM mcr.microsoft.com/dotnet/core/aspnet:2.2 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS build
WORKDIR /src
COPY plugins/Common/Common.csproj plugins/Common/
COPY src/App/App.csproj src/App/
COPY src/Core/Core.csproj src/Core/
RUN dotnet restore src/App/App.csproj
COPY . .

WORKDIR /src/src/App
RUN dotnet build App.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish App.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
RUN ls /app/data -la
ENTRYPOINT ["dotnet", "App.dll"]