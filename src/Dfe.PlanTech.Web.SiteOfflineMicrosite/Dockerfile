FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /
EXPOSE 8080

FROM node:24-alpine AS node-build
WORKDIR /build
COPY src/Dfe.PlanTech.Web.SiteOfflineMicrosite/package*.json ./
RUN npm ci --quiet
COPY src/Dfe.PlanTech.Web.SiteOfflineMicrosite/ ./
RUN npm run build

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /
COPY src/Dfe.PlanTech.Web.SiteOfflineMicrosite/ src/Dfe.PlanTech.Web.SiteOfflineMicrosite/
COPY --from=node-build /build/wwwroot/ src/Dfe.PlanTech.Web.SiteOfflineMicrosite/wwwroot/
RUN dotnet publish "src/Dfe.PlanTech.Web.SiteOfflineMicrosite/Dfe.PlanTech.Web.SiteOfflineMicrosite.csproj" -c Release -o /out/publish && \
    rm -rf /src

FROM base AS final
WORKDIR /src
RUN useradd -m dotnet
COPY --from=build /out/publish .
RUN chown dotnet:dotnet /src .
USER dotnet
ENTRYPOINT ["dotnet", "Dfe.PlanTech.Web.SiteOfflineMicrosite.dll"]

