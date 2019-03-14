@ECHO OFF

REM Build NEMS_API Image
cd ../NEMS_API & docker build -t nemsapi . --no-cache

REM docker run -it -d -p 8481:5002 --name nemsapiapp nemsapi

REM docker start nemsapiapp
REM docker stop nemsapiapp

REM SSH to a running container
REM docker exec -ti nemsapiapp sh


REM docker container rm -f [container_name]

REM Remove all containers
REM docker rm $(docker ps -a -q)

REM List all images
REM docker image ls

REM Remove all images
REM docker rmi $(docker images -f “dangling=true" -q)

REM docker rmi [image_name]
