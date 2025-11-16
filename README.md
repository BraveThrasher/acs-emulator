# Emulator for Azure Communication Services

Local emulator to run Azure Communication Services client SDKs without having to provision an Azure Communication Services resource.

## Running the emulator

You can run the emulator either by using the CLI tool or by using it by running it in a container e.g. Docker. The containerized version is recommended for most users.

### Containerized version

You can run the emulator in a container using Docker. Make sure you have Docker installed.

First you will need to create a self-signed certificate by running the `createSelfSignedCert.ps1` script located in the `AcsEmulatorAPI` folder. This will generate a `acsEmulator_SelfSigned.pfx` file with password `mypassword`. 

Make note of the full path to this file on your machine. 

Then, pull the latest version of the emulator Docker image:

```bash
docker pull benschalley/acs-emulator:latest
```

To start a container with the emulator run the following command:

```bash
docker run --rm -p 443:443 
  -v /full/path/to/acsEmulator_SelfSigned.pfx:/https/acsEmulator_SelfSigned.pfx:ro 
  -v /full/path/to/local/data:/data
  -e Kestrel__Certificates__Default__Password="the-real-password" 
  -e JwtSigningKey="your-own-jwt-signing-key-if-desired"
  benschalley/acs-emulator:latest
```

It is also possible to start a container taking the Docker compose route. The following is an example `docker-compose.yml` file:

```yaml
services: 
  acs-emulator: 
    image: benschalley/acs-emulator:latest 
    ports: 
      - "443:443" 
    volumes: 
      - /full/path/to/acsEmulator_SelfSigned.pfx:/https/acsEmulator_SelfSigned.pfx:ro 
      - /full/path/to/local/data:/data
    environment: 
      - Kestrel__Certificates__Default__Password=mypassword 
      - JwtSigningKey=your-own-jwt-signing-key-if-desired
    restart: "no"
 ```

Both methods will start the emulator and map port 443 from the container to your localhost. You can then access the emulator API at `https://localhost/`.

#### Volumes

* `/https/acsEmulator_SelfSigned.pfx`: Path to the self-signed certificate file inside the container (read-only) [**REQUIRED!**]

* `/data`: Path to a local folder on your machine where the emulator will store its SQLite database file

#### Configuration

You can configure the emulator by passing environment variables to the container.

| Configuration Key                        | Description                                         | Required | Default Value                          |
|------------------------------------------|-----------------------------------------------------|----------|----------------------------------------|
| JwtSigningKey                            | Signing key for JWT tokens                          | Yes      | `5fca297d-3350-45ad-a513-44a9d7d8fbfc` |
| Kestrel__Certificates__Default__Password | Password for the HTTPS certificate file             | Yes      | `mypassword`                           |
| ResourcePhoneNumber                      | Phone number to use as the ACS resource phone number| No       | `+1234555000`                          |
| EmulatorDevicePhoneNumber                | Phone number to use for emulator devices            | No       | `+12345556789`                         |
| EventGridSimulatorSystemTopicHostname    | URL for [Azure Event Grid Simulator](https://github.com/pm7y/AzureEventGridSimulator)      | No       | `https://localhost:60101/api/events` |
| EventGridSimulatorSystemTopicCredentials | Credentials for [Azure Event Grid Simulator](https://github.com/pm7y/AzureEventGridSimulator)   | Only when using Azure Event Grid Simulator        | `TheLocal+DevelopmentKey=`           |
| OpenTelemetryEndpointUrl                 | Endpoint URL for OpenTelemetry collector            | No       | (empty)                                |

### CLI tool version

#### Install from GitHub

Make sure you have the .NET Core SDK installed, download the latest [AcsEmulatorCLI Release](https://github.com/DominikMe/acs-emulator/releases) and then run from a terminal:

```dotnetcli
dotnet tool install -g --add-source [downloadFolder] AcsEmulatorCLI --version [nupkgVersion]
```

#### Running the tool

**Usage:**

`acs-emulator [command] [options]`

**Commands:**

| Command            | Description                      |
| ------------------ | -------------------------------- |
|  `--version `        | Show version information
|  `-?, -h, --help`    | Show help and usage information |
|  `run`               | Run the emulator. |
|  `openApi`           | Open the emulator's API in Swagger UI. |
|  `openDB`            | Open the emulator's sqlite database.         |
|  `openUI`            | Open the emulator UI. |
|  `clean`             | Clean all data and reset the emulator state. |
|  `connectionString`  | Get the ACS connection string for the emulator. |
|  `repo`              | Open code repository. |

## Using the emulator

* When running the containerized version with a port mapping to a localhost port other than `443`, adjust the endpoint URL accordingly, e.g. `https://localhost:8443/`

* You can use the `Try it` feature in the Swagger editor under `https://localhost/swagger` to send requests against the API

* Use `"endpoint=https://localhost/;accessKey=pw=="` as your connection string when instantiating Azure Communication Services SDK service clients.

* Use the Identity SDK with the localhost connection string and create users and tokens as usual. Use the created token to instantiate the Chat SDK.

* You can also use endpoint `https://localhost/` to run the Live Preview of the UI library's [Chat composite](https://azure.github.io/communication-ui-library/?path=/story/composites-chat-joinexistingchatthread--join-existing-chat-thread). First, create two users with tokens and use one of the users to create a chat thread with the other user. Then, you can open two tabs side by side and fill in the the respective user, token, thread id and endpoint for each.

* You can also browse the emulator data under `https://localhost/`

![Emulator UI](./AcsEmulator/EmulatorUI.png)

* For inspecting the DB data directly, we recommend to install [DB Browser for SQLite](https://sqlitebrowser.org/) and use it to open the `AcsEmulator.db` file

* Install the generated `acsEmulator_selfSigned.pfx` in your certificate store under Trusted Root Certification Authorities using password `mypassword`. Please uninstall the cert when no longer needed.

## Getting started with code

* Install the `dotnet ef` tool: Run `dotnet tool install --global dotnet-ef` [Link for details](https://docs.microsoft.com/en-us/ef/core/cli/dotnet)

* Run `dotnet ef database update` to update your local copy of the `AcsEmulator.db` in case new migrations were added.

* Build and run the `AcsEmulatorApi` project

* Build the emulator UI by navigating to `acs-emulator-ui`, then run `npm install` and `npm run start`

* To build a new version of the CLI tool, first build the `acs-emulator-ui` with `npm run build` to build production bundles. Then build the `AcsEmulatorCLI` project (in the `Release` flavor)

* For inspecting the DB data directly, we recommend to install [DB Browser for SQLite](https://sqlitebrowser.org/) and use it to open the `AcsEmulator.db` file

* To reset the emulator entirely and clear its data and state, delete the `AcsEmulator.db` file

## Enable real-time notifications for Chat

The URL for establishing the real-time notification channel, is unfortunately hard-coded in the Azure SDK. To enable real-time notifications in the emulator follow these steps (requires rebuilding the emulator from source):

1. Add `127.0.0.1 go.trouter.teams.microsoft.com` to your machine's hosts file to redirect the SDK's hard-coded URL to your localhost.

1. Run `createSelfSignedCert.ps1`

1. Install the generated `acsEmulator_selfSigned.pfx` in your certificate store under Trusted Root Certification Authorities using password `mypassword`. Please uninstall the cert when no longer needed, we're hoping to get rid of the need to install a self-signed cert soon and fully rely on the ASP.NET Core HTTPS development certificate instead.

1. Relaunch your browser to load the newly installed root certificate.

1. Add the following as a top property of `appsettings.json`:

```json
"Kestrel": {
  "Endpoints": {
    "HttpsInlineCertFile": {
      "Url": "https://localhost",
      "Certificate": {
        "Path": "acsEmulator_selfSigned.pfx",
        "Password": "mypassword"
      }
    }
  }
},
```

6. Rebuild and launch `AcsEmulatorAPI`.

7. Restart all browsers to load the new root cert.

8. Run the `AcsEmulatorApi` project to start the emulator.

Now, the Chat SDK can establish a real-time notification channel which is backed by a websocket connection in the emulator's ASP .NET Core service.

## Limitations

* Only a subset of APIs have been implemented so far:
  * `/identities`
  * `/chat`
  * `/email`
  * `/sms`
  * EventGrid events for Sms
* All server/control-plane APIs are unauthenticated (HMAC / AAD is ignored)
* API versioning is ignored, the implemented APIs are trying to be on a recent version
* `/chat` APIs are incomplete
  * token scope `chat` is not enforced
  * read receipts aren't implemented
  * only `messageReceived` and typing real-time notifications are implemented so far
