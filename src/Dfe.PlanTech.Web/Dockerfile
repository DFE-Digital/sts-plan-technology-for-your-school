FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /
COPY src/ src/

FROM build AS publish
RUN dotnet publish "src/Dfe.PlanTech.Web/Dfe.PlanTech.Web.csproj" -c Release -o /out/publish

FROM base AS final
WORKDIR /src
RUN useradd -m dotnet
RUN chown dotnet:dotnet /src
USER dotnet
COPY --from=publish /out/publish .
ENTRYPOINT ["dotnet", "Dfe.PlanTech.Web.dll"]
