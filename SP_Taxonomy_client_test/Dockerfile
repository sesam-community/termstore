FROM mcr.microsoft.com/dotnet/core/aspnet:3.0.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.0.100 AS build
WORKDIR /src
COPY ["SP_Taxonomy_client_test/SP_Taxonomy_client_test.csproj", "SP_Taxonomy_client_test/"]
RUN dotnet restore "SP_Taxonomy_client_test/SP_Taxonomy_client_test.csproj"
COPY . .
WORKDIR "/src/SP_Taxonomy_client_test"
RUN dotnet build "SP_Taxonomy_client_test.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "SP_Taxonomy_client_test.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "SP_Taxonomy_client_test.dll"]