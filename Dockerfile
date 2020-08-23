FROM mcr.microsoft.com/dotnet/core/sdk:3.1

WORKDIR /src
COPY . .

RUN dotnet restore Repository.sln
RUN dotnet build -c Release --no-restore Repository.sln
RUN dotnet test -c Release --no-restore Repository.sln