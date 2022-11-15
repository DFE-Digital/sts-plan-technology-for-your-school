FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["DfE.PlanTechnologyForYourSchool.WebApp/DfE.PlanTechnologyForYourSchool.WebApp.csproj", "DfE.PlanTechnologyForYourSchool.WebApp/"]
RUN dotnet restore "DfE.PlanTechnologyForYourSchool.WebApp/DfE.PlanTechnologyForYourSchool.WebApp.csproj"
COPY . .
WORKDIR "/src/DfE.PlanTechnologyForYourSchool.WebApp"
RUN dotnet build "DfE.PlanTechnologyForYourSchool.WebApp.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DfE.PlanTechnologyForYourSchool.WebApp.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DfE.PlanTechnologyForYourSchool.WebApp.dll"]
