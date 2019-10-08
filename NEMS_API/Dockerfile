FROM microsoft/aspnetcore-build:2.0 AS build-env

# start in our app directory
WORKDIR /app

# Copy all of solution
COPY . ./

# Restore Solution
RUN dotnet restore NEMS_API.WebApp/NEMS_API.WebApp.csproj

#Start Front-end build
WORKDIR /app/NEMS_API.WebApp
RUN npm install
RUN npm run build

#Start Back-end build
WORKDIR /app
RUN dotnet publish NEMS_API.WebApp/NEMS_API.WebApp.csproj -c Release -o out

# Build runtime image
FROM microsoft/aspnetcore:2.0
WORKDIR /app
COPY --from=build-env /app/NEMS_API.WebApp/out .

# Set Server ENVs
EXPOSE 5002/tcp
#EXPOSE 5544/tcp
ENV ASPNETCORE_URLS http://*:5002
#ENV ASPNETCORE_URLS http://*:5002;https://*:5544
ENV ASPNETCORE_ENVIRONMENT Production

ENTRYPOINT ["dotnet", "NEMS_API.WebApp.dll"]