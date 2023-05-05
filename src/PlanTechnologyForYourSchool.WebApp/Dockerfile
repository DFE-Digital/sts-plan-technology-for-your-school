FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["PlanTechnologyForYourSchool.WebApp/PlanTechnologyForYourSchool.WebApp.csproj", "PlanTechnologyForYourSchool.WebApp/"]
RUN dotnet restore "PlanTechnologyForYourSchool.WebApp/PlanTechnologyForYourSchool.WebApp.csproj"
COPY . .
WORKDIR "/src/PlanTechnologyForYourSchool.WebApp"
RUN dotnet build "PlanTechnologyForYourSchool.WebApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PlanTechnologyForYourSchool.WebApp.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PlanTechnologyForYourSchool.WebApp.dll"]
