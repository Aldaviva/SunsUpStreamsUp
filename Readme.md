🌅 SunsUpStreamsUp
===

[![GitHub Workflow Status](https://img.shields.io/github/actions/workflow/status/Aldaviva/SunsUpStreamsUp/dotnet.yml?branch=master&logo=github)](https://github.com/Aldaviva/SunsUpStreamsUp/actions/workflows/dotnet.yml) [![Testspace](https://img.shields.io/testspace/tests/Aldaviva/Aldaviva:SunsUpStreamsUp/master?passed_label=passing&failed_label=failing&logo=data%3Aimage%2Fsvg%2Bxml%3Bbase64%2CPHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHZpZXdCb3g9IjAgMCA4NTkgODYxIj48cGF0aCBkPSJtNTk4IDUxMy05NCA5NCAyOCAyNyA5NC05NC0yOC0yN3pNMzA2IDIyNmwtOTQgOTQgMjggMjggOTQtOTQtMjgtMjh6bS00NiAyODctMjcgMjcgOTQgOTQgMjctMjctOTQtOTR6bTI5My0yODctMjcgMjggOTQgOTQgMjctMjgtOTQtOTR6TTQzMiA4NjFjNDEuMzMgMCA3Ni44My0xNC42NyAxMDYuNS00NFM1ODMgNzUyIDU4MyA3MTBjMC00MS4zMy0xNC44My03Ni44My00NC41LTEwNi41UzQ3My4zMyA1NTkgNDMyIDU1OWMtNDIgMC03Ny42NyAxNC44My0xMDcgNDQuNXMtNDQgNjUuMTctNDQgMTA2LjVjMCA0MiAxNC42NyA3Ny42NyA0NCAxMDdzNjUgNDQgMTA3IDQ0em0wLTU1OWM0MS4zMyAwIDc2LjgzLTE0LjgzIDEwNi41LTQ0LjVTNTgzIDE5Mi4zMyA1ODMgMTUxYzAtNDItMTQuODMtNzcuNjctNDQuNS0xMDdTNDczLjMzIDAgNDMyIDBjLTQyIDAtNzcuNjcgMTQuNjctMTA3IDQ0cy00NCA2NS00NCAxMDdjMCA0MS4zMyAxNC42NyA3Ni44MyA0NCAxMDYuNVMzOTAgMzAyIDQzMiAzMDJ6bTI3NiAyODJjNDIgMCA3Ny42Ny0xNC44MyAxMDctNDQuNXM0NC02NS4xNyA0NC0xMDYuNWMwLTQyLTE0LjY3LTc3LjY3LTQ0LTEwN3MtNjUtNDQtMTA3LTQ0Yy00MS4zMyAwLTc2LjY3IDE0LjY3LTEwNiA0NHMtNDQgNjUtNDQgMTA3YzAgNDEuMzMgMTQuNjcgNzYuODMgNDQgMTA2LjVTNjY2LjY3IDU4NCA3MDggNTg0em0tNTU3IDBjNDIgMCA3Ny42Ny0xNC44MyAxMDctNDQuNXM0NC02NS4xNyA0NC0xMDYuNWMwLTQyLTE0LjY3LTc3LjY3LTQ0LTEwN3MtNjUtNDQtMTA3LTQ0Yy00MS4zMyAwLTc2LjgzIDE0LjY3LTEwNi41IDQ0UzAgMzkxIDAgNDMzYzAgNDEuMzMgMTQuODMgNzYuODMgNDQuNSAxMDYuNVMxMDkuNjcgNTg0IDE1MSA1ODR6IiBmaWxsPSIjZmZmIi8%2BPC9zdmc%2B)](https://aldaviva.testspace.com/spaces/258390) [![Coveralls](https://img.shields.io/coveralls/github/Aldaviva/SunsUpStreamsUp?logo=coveralls)](https://coveralls.io/github/Aldaviva/SunsUpStreamsUp?branch=master)

*Automatically start an OBS stream when the sun rises, and stop it when the sun sets*

<!-- MarkdownTOC autolink="true" bracket="round" levels="1,2" bullets="1." -->

1. [Prerequisites](#prerequisites)
1. [Installation](#installation)
1. [Configuration](#configuration)
1. [Running](#running)

<!-- /MarkdownTOC -->

## Prerequisites
- [Open Broadcaster Software Studio ≥ 28](https://obsproject.com/download)
- [.NET ≥ 8 Runtime x64 or ARM64](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- Operating system
    - [Linux x64 distribution and version supported by .NET Runtime](https://learn.microsoft.com/en-us/dotnet/core/install/linux) (tested on Fedora Workstation 39)
    - [Windows ≥ 10 or Server ≥ 2012, x64](https://learn.microsoft.com/en-us/dotnet/core/install/windows)
    - [Mac OS ≥ 12, x64 or ARM64](https://learn.microsoft.com/en-us/dotnet/core/install/macos)

## Installation
1. Download the [latest release ZIP file](https://github.com/Aldaviva/SunsUpStreamsUp/releases/latest) for your operating system and CPU architecture.
1. Extract all files from the ZIP file to a directory on your hard drive.
1. On Linux and Mac OS, enable the executable bit on the program in a terminal
    ```sh
    chmod +x SunsUpStreamsUp
    ```

### Updating
1. Download the [latest release ZIP file](https://github.com/Aldaviva/SunsUpStreamsUp/releases/latest) for your operating system and CPU architecture.
1. Extract the executable file (`SunsUpStreamsUp` or `SunsUpStreamsUp.exe`) from the ZIP file to your installation directory. Do not overwrite your existing `appsettings.json`.

## Configuration
1. Launch OBS and go to Tools › WebSocket Server Settings.
1. Ensure **Enable WebSocket Server** is checked.
1. Set a password, or copy the generated password using Show Connect Info › Server Password › Copy.
1. Press OK.
1. In this program's installation directory, open `appsettings.json` in a text editor, and set the following properties in the given sections.

### `geography`
|Name|Values|Description|
|-|-|-|
|`latitude`|[−90.0,90.0]|Decimal degrees of your location north (+) or south (−) of the equator, used to determine the local time of sunrise and sunset|
|`longitude`|(−180.0,180.0]|Decimal degrees of your location east (+) or west (−) of the prime meridian, used to determine the local time of sunrise and sunset|
|`timeZone`|IANA zone ID|Time zone for your location, from [IANA/Olson tzdb](https://en.wikipedia.org/wiki/List_of_tz_database_time_zones) (*e.g.* `"America/Los_Angeles"`), or omit this to use the computer's local zone|
|`minimumSunlightLevel`|[`SunlightLevel`](https://github.com/Aldaviva/SolCalc/blob/master/SolCalc/Data/SunlightLevel.cs)|Stream will be up whenever the sunlight is at least this bright; one of `Night` (always up), `AstronomicalTwilight`, `NauticalTwilight`, `CivilTwilight` (default), or `Daylight`. For example, if you set this to `CivilTwilight`, the stream will start at civil dawn and stop at civil dusk.|

### `stream`
|Name|Values|Description|
|-|-|-|
|`obsHostname`|FQDN or IP address|The hostname of the computer running OBS, defaults to `"localhost"` for when OBS and this program are both installed on the same computer|
|`obsPort`|[1,65535)|TCP port of the OBS WebSocket server, defaults to `4455`|
|`obsPassword`|string|OBS WebSocket server password you set or copied, not URL-encoded, defaults to `""` for if you disabled authentication|

### `logging`
|Name|Values|Description|
|-|-|-|
|`logLevel`|object|You may optionally [change the logging levels of this program](https://learn.microsoft.com/en-us/dotnet/core/extensions/logging?tabs=command-line#configure-logging-without-code)|


## Running
1. Make sure OBS is already running and ready to start a stream.
1. Launch `SunsUpStreamsUp`.
    ```sh
    ./SunsUpStreamsUp
    ```
1. The program will immediately start the stream if the sun is currently up and the stream is stopped.
1. The program will continue running, starting and stopping the stream when the sun rises and sets.
1. To stop this program, press `Ctrl`+`C`.