FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5000
EXPOSE 5001

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["WebAPI/StripeBilling.API.csproj", "WebAPI/"]
COPY ["Core/StripeBilling.Core.csproj", "Core/"]
RUN dotnet restore "WebAPI/StripeBilling.API.csproj"
COPY . .
WORKDIR "/src/WebAPI"
RUN dotnet build "StripeBilling.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "StripeBilling.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:5000
ENTRYPOINT ["dotnet", "StripeBilling.API.dll"]
