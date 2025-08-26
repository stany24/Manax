# Server

## Installation

### Executable

1. Install [.NET Runtime 8.0](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) on your system
2. Verify the installation
3. Download the latest release from: [releases](https://github.com/stany24/Manax/releases)
4. Run the ManaxServer executable in a console

### systemd

1. Install [.NET Runtime 8.0](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) on your system
2. Verify the installation
3. Download the latest release from: [releases](https://github.com/stany24/Manax/releases)
4. Place this file in /etc/systemd/system/

    [Unit]
    Description=Manax
    
    [Service]
    ExecStart=/path/to/your/manax/server/executable
    Restart=always

5. `sudo systemctl enable manax` to start the service by default on startup
6. `sudo systemctl start manax` to start it now
7. You can see the logs with `journalctl -u manax`

### Docker

1. Install [Docker](https://docs.docker.com/engine/install/) and Docker Compose on your system
2. Verify the installation
3. Download the latest release from: [releases](https://github.com/stany24/Manax/releases)
4. Modify the `compose.yaml` file according to your needs (you really should add at least one volume)
5. Run `docker compose up`

# Application

## Installation

1. Install [.NET Runtime 8.0](https://dotnet.microsoft.com/en-us/download/dotnet/8.0) on your system
2. Verify the installation
3. Download the latest release from [releases](https://github.com/stany24/Manax/releases)
4. Run the ManaxClient executable

## Server Owner

If you're migrating from Komga or Kavita, note this important difference: in Manax, libraries don't specify disk location but serve only as organizational tools. You must use "save-points" to define where series will be saved.

### Initial Setup

1. On your first login as owner, your account will be automatically created
2. Go to the server settings page to add a new save-point
3. Select an empty folder with sufficient free space
4. If using Docker, choose the folder corresponding to `target: /media/save-point` in your `compose.yaml`, if you don't the series will be deleted with the container.
5. Before adding any serie to the server you should go to the server configuration page and check that the chapter configuration fits what you want. The defaults are a good compromise between quality and file size.

**Warning**: Should you change those settings when you have a lot of chapters the conversion of every page in the server will take a while.

**Note:** Creating multiple save-points on the same disk is pointless. If you have multiple disks, you can create one save-point on each.

## User/Administrator

1. Ask your friend for the server address and your account credentials
2. Use this information to connect to the server
3. You can then 