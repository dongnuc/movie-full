# =========================
# BUILD STAGE
# =========================

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY . .

RUN dotnet restore Movie_StructrueCode.API/Movie_StructureCode.API.csproj

RUN dotnet publish Movie_StructrueCode.API/Movie_StructureCode.API.csproj \
    -c Release \
    -o /app/publish

# =========================
# RUNTIME STAGE
# =========================

FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 8080

ENTRYPOINT ["dotnet", "Movie_StructureCode.API.dll"]