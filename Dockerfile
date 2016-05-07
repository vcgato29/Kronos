FROM lukaszpyrzyk/aspnet-1.0.0-rc1-update2

# copy all files
COPY . /app/

# set workdir
WORKDIR /app

RUN ["dnu", "restore"]

WORKDIR /app/Src/Kronos.Server

ENTRYPOINT ["dnx", "run"]
