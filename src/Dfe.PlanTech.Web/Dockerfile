FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Dfe.PlanTech.Web/Dfe.PlanTech.Web.csproj", "Dfe.PlanTech.Web/"]
RUN dotnet restore "Dfe.PlanTech.Web/Dfe.PlanTech.Web.csproj"
COPY . .
WORKDIR "/src/Dfe.PlanTech.Web"
RUN dotnet build "Dfe.PlanTech.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Dfe.PlanTech.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Dfe.PlanTech.Web.dll"]
