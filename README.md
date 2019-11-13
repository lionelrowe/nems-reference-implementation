# NEMS Reference Implementation

The NEMS (National Events Management Service) reference implementation is a .NET Core application.

### NEMS Technical Demonstration

This application is designed to demonstrate interacting with the NHS Digital NEMS service as a:

- Subscriber
- Publisher

The specification for this service can be found at https://developer.nhs.uk/apis/ems-beta/

### NEMS Business Demonstration

You will also find demonstration UIs that aim to show you requests and responses as subscribers and publishers. 

Finally, there will also be a demonstration UI that aims to show you what an event may look like once it has been drawn into a local system via MESH.

### Docker

```sh
# Build Image
docker build -t nemsapi . --no-cache

# Setup container
docker run -it -d -p 8481:5002 --name nemsapiapp nemsapi

# Start container
docker start nemsapiapp

# If error is thrown regarding already existing then run:
docker rm nemsapiapp 

# Stop container
docker stop nemsapiapp
```
