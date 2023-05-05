FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Sts.PlanTech.Web/Sts.PlanTech.Web.csproj", "Sts.PlanTech.Web/"]
RUN dotnet restore "Sts.PlanTech.Web/Sts.PlanTech.Web.csproj"
COPY . .
WORKDIR "/src/Sts.PlanTech.Web"
RUN dotnet build "Sts.PlanTech.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Sts.PlanTech.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Sts.PlanTech.Web.dll"]
