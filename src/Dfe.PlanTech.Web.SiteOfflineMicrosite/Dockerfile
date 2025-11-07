FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /
EXPOSE 8080

FROM node:24-alpine AS node-build
WORKDIR /build
COPY src/Dfe.PlanTech.Web.SiteOfflineMicrosite/package*.json ./
RUN npm ci --quiet --ignore-scripts
COPY src/Dfe.PlanTech.Web.SiteOfflineMicrosite/scripts/ ./scripts/
COPY src/Dfe.PlanTech.Web.SiteOfflineMicrosite/src/ ./src/
RUN npm run build

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY src/Dfe.PlanTech.Web.SiteOfflineMicrosite/*.csproj ./
RUN dotnet restore
COPY src/Dfe.PlanTech.Web.SiteOfflineMicrosite/ ./
COPY --from=node-build /build/wwwroot/ ./wwwroot/
RUN dotnet publish "Dfe.PlanTech.Web.SiteOfflineMicrosite.csproj" \
    -c Release \
    -o /out/publish \
    --no-restore

FROM base AS final
WORKDIR /src
RUN useradd -m dotnet
COPY --from=build /out/publish .
RUN chown dotnet:dotnet /src .
USER dotnet
ENTRYPOINT ["dotnet", "Dfe.PlanTech.Web.SiteOfflineMicrosite.dll"]

