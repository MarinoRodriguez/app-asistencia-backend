FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY AssistantApp.sln ./
COPY AssistantApp.API/AssistantApp.API.csproj AssistantApp.API/
COPY AssistantApp.Shared/AssistantApp.Shared.csproj AssistantApp.Shared/

RUN dotnet restore AssistantApp.API/AssistantApp.API.csproj

COPY AssistantApp.API/ AssistantApp.API/
COPY AssistantApp.Shared/ AssistantApp.Shared/

RUN dotnet publish AssistantApp.API/AssistantApp.API.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
ENV DISABLE_HTTPS_REDIRECT=true

COPY --from=build /app/publish ./

EXPOSE 8080

ENTRYPOINT ["dotnet", "AssistantApp.API.dll"]
