FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /
EXPOSE 80
RUN apt-get update
RUN apt-get -y install libcap2-bin
RUN apt-get clean

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /
COPY src/ src/
RUN dotnet publish "src/Dfe.PlanTech.Web/Dfe.PlanTech.Web.csproj" -c Release -o /out/publish
RUN rm -rf /src

FROM base AS final
WORKDIR /src
RUN useradd -m dotnet
COPY --from=build /out/publish .
RUN chown dotnet:dotnet /src .
RUN setcap CAP_NET_BIND_SERVICE=+ep ./Dfe.PlanTech.Web.dll
USER dotnet
ENTRYPOINT ["dotnet", "Dfe.PlanTech.Web.dll"]
