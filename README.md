# Nyckel

Nyckel is a Key/Value store initially built for storing config in a microservices environment. It's intended to be deployed as a container and it exposes a GUI and an API.

## Features

- Simple REST API
- GUI for managing entries
- String keys / String values
- Option to choose from different backends / stores

## Backlog

- Authentication
- Public Docker image
- Benchmarks
- Multi-node deployment replication

## Run from Dockerfile

### Environment Variables

- `PORT` - Port for the Nyckel host. Default `9997`.
- `BACKEND` - Which backend system to use. Current options: `LiteDb` and `InMemory`. Default `InMemory`.
- `API_KEY` - API Key for authenticating API requests. Must be passed via the `X-API-Key` header. Default: _blank_.

```bash
# Build docker image
docker build . --tag nyckel

# Run docker image
docker run \
-e BACKEND='InMemory' \
-e API_KEY='SuperSecretApiKey' \
-e PORT='9997' \
-p 9997:9997 \
nyckel
```

## Using the API

Please note that the examples are not passing the `X-API-Key` request header.

### Set

```bash
curl --location --request POST 'http://localhost:9997/api/my-key' --data-raw 'my value' -i

HTTP/1.1 200 OK
Content-Type: text/plain
Date: Mon, 20 Jun 2022 15:48:13 GMT
Server: Kestrel
Transfer-Encoding: chunked

my value
```

### Get

```bash
curl --location --request GET 'http://localhost:9997/api/my-key' -i

HTTP/1.1 200 OK
Content-Type: text/plain
Date: Mon, 20 Jun 2022 15:48:13 GMT
Server: Kestrel
Transfer-Encoding: chunked

my value
```

### Delete

```bash
curl --location --request DELETE 'http://localhost:9997/api/my-key' -i

HTTP/1.1 200 OK
Content-Type: text/plain
Date: Mon, 20 Jun 2022 15:48:13 GMT
Server: Kestrel
Transfer-Encoding: chunked

my value
```
