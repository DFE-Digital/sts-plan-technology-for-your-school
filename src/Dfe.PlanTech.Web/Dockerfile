FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /src
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY Dfe.PlanTech.Application .
COPY Dfe.PlanTech.DatabaseUpgrader . 
COPY Dfe.PlanTech.Domain . 
COPY Dfe.PlanTech.Infrastructure.Contentful . 
COPY Dfe.PlanTech.Infrastructure.SignIn . 
COPY Dfe.PlanTech.Web . 
COPY README.md . 
RUN dotnet restore "Dfe.PlanTech.Web/Dfe.PlanTech.Web.csproj"
RUN dotnet build "Dfe.PlanTech.Web/Dfe.PlanTech.Web.csproj" -c Release -o /out/build

FROM build AS publish
RUN dotnet publish "Dfe.PlanTech.Web/Dfe.PlanTech.Web.csproj" -c Release -o /out/publish

FROM base AS final
USER dotnet
WORKDIR /src
COPY --from=publish /out/publish .
ENTRYPOINT ["dotnet", "Dfe.PlanTech.Web.dll"]
