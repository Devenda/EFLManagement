FROM microsoft/dotnet:2.1-aspnetcore-runtime AS base
#FROM microsoft/dotnet:2.1-aspnetcore-runtime-stretch-slim-arm32v7 AS base
WORKDIR /app
EXPOSE 80
ENV TZ=Europe/Brussels
ENV ConnectionString = connectionString

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY EFLManagementAPI/EFLManagementAPI.csproj EFLManagementAPI/
RUN dotnet restore EFLManagementAPI/EFLManagementAPI.csproj
COPY . .
WORKDIR /src/EFLManagementAPI
RUN dotnet build EFLManagementAPI.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish EFLManagementAPI.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "EFLManagementAPI.dll"]
