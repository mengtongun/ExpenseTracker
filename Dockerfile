FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
ARG TARGETARCH

COPY ExpenseTracker.sln ./
COPY src/ExpenseTrackerApi/ExpenseTrackerApi.csproj src/ExpenseTrackerApi/
COPY src/ExpenseTracker.Application/ExpenseTracker.Application.csproj src/ExpenseTracker.Application/
COPY src/ExpenseTracker.Domain/ExpenseTracker.Domain.csproj src/ExpenseTracker.Domain/
COPY src/ExpenseTracker.Infrastructure/ExpenseTracker.Infrastructure.csproj src/ExpenseTracker.Infrastructure/

RUN dotnet restore ExpenseTracker.sln --arch $TARGETARCH

COPY . .

RUN dotnet publish src/ExpenseTrackerApi/ExpenseTrackerApi.csproj \
    --arch $TARGETARCH \
    --configuration Release \
    --output /app/publish \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "ExpenseTrackerApi.dll"]

