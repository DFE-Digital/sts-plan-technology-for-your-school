FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["PlanTechnologyForYourSchool.Web/PlanTechnologyForYourSchool.Web.csproj", "PlanTechnologyForYourSchool.Web/"]
RUN dotnet restore "PlanTechnologyForYourSchool.Web/PlanTechnologyForYourSchool.Web.csproj"
COPY . .
WORKDIR "/src/PlanTechnologyForYourSchool.Web"
RUN dotnet build "PlanTechnologyForYourSchool.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PlanTechnologyForYourSchool.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PlanTechnologyForYourSchool.Web.dll"]
