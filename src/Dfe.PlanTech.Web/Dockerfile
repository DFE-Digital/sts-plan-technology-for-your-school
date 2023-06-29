FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /src
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY Dfe.PlanTech.Application /src/Dfe.PlanTech.Application/
COPY Dfe.PlanTech.DatabaseUpgrader /src/Dfe.PlanTech.DatabaseUpgrader/
COPY Dfe.PlanTech.Domain /src/Dfe.PlanTech.Domain/ 
COPY Dfe.PlanTech.Infrastructure.Contentful /src/Dfe.PlanTech.Infrastructure.Contentful/
COPY Dfe.PlanTech.Infrastructure.SignIn /src/Dfe.PlanTech.Infrastructure.SignIn/
COPY Dfe.PlanTech.Web /src/Dfe.PlanTech.Web/
COPY README.md README.md
RUN dotnet restore "Dfe.PlanTech.Web/Dfe.PlanTech.Web.csproj"
RUN dotnet build "Dfe.PlanTech.Web/Dfe.PlanTech.Web.csproj" -c Release -o /out/build

FROM build AS publish
RUN dotnet publish "Dfe.PlanTech.Web/Dfe.PlanTech.Web.csproj" -c Release -o /out/publish

FROM base AS final
USER dotnet
WORKDIR /src
COPY --from=publish /out/publish .
ENTRYPOINT ["dotnet", "Dfe.PlanTech.Web.dll"]
