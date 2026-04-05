# ---------- BUILD ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

# copy everything first (important for Razor compile)
COPY . .

# restore
RUN dotnet restore "CAT.AID.Web.csproj"

# publish
RUN dotnet publish "CAT.AID.Web.csproj" \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false


# ---------- RUNTIME ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /app

# fonts for QuestPDF
RUN apt-get update && apt-get install -y \
    fontconfig \
    fonts-dejavu-core \
    fonts-liberation \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "CAT.AID.Web.dll"]
