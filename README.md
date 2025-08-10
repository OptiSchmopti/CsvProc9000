<img alt="logo" src="src/CsvProc9000.UI.Wpf/Resources/AppIcon/appicon.png" width="150">

# 🗃 CsvProc9000

[![GitHub license](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![Uses SemVer 2.0.0](https://img.shields.io/badge/Uses%20SemVer-2.0.0-green)](https://semver.org/spec/v2.0.0.html)
[![Latest Release](https://img.shields.io/github/v/release/OptiSchmopti/CsvProc9000?label=latest%20release&sort=semver)](https://github.com/OptiSchmopti/CsvProc9000/releases)
[![codecov](https://codecov.io/gh/OptiSchmopti/CsvProc9000/branch/main/graph/badge.svg?token=AL5PLYOWHU)](https://codecov.io/gh/OptiSchmopti/CsvProc9000)  
[![GitHub stars](https://img.shields.io/github/stars/OptiSchmopti/CsvProc9000?style=social)](https://github.com/OptiSchmopti/CsvProc9000/stargazers)
[![GitHub forks](https://img.shields.io/github/forks/OptiSchmopti/CsvProc9000?style=social)](https://github.com/OptiSchmopti/CsvProc9000/network/members)
[![GitHub watchers](https://img.shields.io/github/watchers/OptiSchmopti/CsvProc9000?style=social)](https://github.com/OptiSchmopti/CsvProc9000/watchers)

**CsvProc9000** is a light-weight, rule-based CSV-Processor. It watches a given Inbox-Folder for you and processes - based on the given rules - any new CSV-File that it catches and outputs it anywhere you want.

## ⚠️ Support

Both the service and the UI application are currently only supported on 64 Bit Windows!

## 🖥️ Installation

### Service

Go to the [Releases](https://github.com/OptiSchmopti/CsvProc9000/releases) and select a Release that you want to use. Generally the lastest release is recommended, because it has all the latest Fixes and Features.  
Attached to the Release you'll find a `CsvProc9000-vX_X_X.zip` which contains:

- `appsettings.json`  - configuration file for CsvProc9000
- `CsvProc9000.exe`   - a self-contained binary containing everything that CsvProc9000 needs to run. No other software is needed
- `CreateService.bat` - a Batch-File which will create and run a Windows-Service named "CsvProc9000" with the Mode "Automatic"

Before running `CreateService.bat` you should make sure that your configuration is the way you want it to be.  
  
Running the `CreateService.bat` requires you to start an admin CMD or PowerShell, navigate to where the bat is and run the bat from inside the admin CMD/PowerShell (if you select "Run as Administrator" from the Contextmenu, the path will be broken and so the bat will fail).  
If there are any issues with starting the service, check out the log-file that is generated in the location where the `CsvProc9000.exe` is, in the folder `logs`. Common issues are:

- Forgot to configure the Inbox/Outbox folders

### UI

A new addition to the _CsvProc9000_ family is the UI. It currently provides an easier way to configure your services configuration.  
  
To install it you simply have to go to the [Releases](https://github.com/OptiSchmopti/CsvProc9000/releases) and select a release that you want to use.  
Generally the lastest release is recommended, because it has all the latest Fixes and Features.  
Attached to the Release you'll find a `CsvProc9000.UI-vX_X_X.zip` which contains the executable of the UI (amongst other files).  
  
Simply extract the files anywhere you want the application to be located, and then double click the `CsvProc9000.UI.exe` file to run it.

## 🖱️ Update

### Service

To update the service you can simply follow these steps:

1. Stop the Service
2. Replace the `.exe` file with the new one
3. Change the Configuration according to any breaking changes, if any
4. Start the Service again

### UI

To update the UI you can simply follow these steps:

1. Close the UI if you haven't already done that
2. Delete the current installation files
3. Download and extract the new files
4. Run the application

## 🛠️ Configuration

⚠️ This part is completely out of date, sorry. I will update this soon(TM), I promise!  
  
CsvProc9000 is heavily dependent on it's configuration, because otherwise it wouldn't know how you'd like it to behave.  
Here's an example configuration with information what everything does:

![image](https://user-images.githubusercontent.com/20710883/138595712-affcaab2-1731-4705-9420-b8e494d9095c.png)

Based on this configuration, here is a simple example on how this configuration would be applied:

![image](https://user-images.githubusercontent.com/20710883/138595164-0270dd5e-3d20-485c-bd13-bab02675a282.png)

On the left side you can see the input and on the right the output.  
Based on the rules, only the 3rd row meets all conditions (`SomeField`=`SomeValue` and `SomeOtherField`=`1337`).  
Because of that all other rows, but the 3rd, are unaffected by any changes. In the 3rd you can see, that in the Output, `SomeOtherField` got changed to the value `42` and a new field `Other Field` with the value `123` has been added. Every other row has an empty value for this new field.  
  
### UI

You can achieve the same with the new UI now too. Simply download and install the application, run it and choose "Config".

## ⌨️ Developing

To develop and work with CsvProc9000 you just need to clone this Repo somewhere on your PC and then open the Solution or the complete Source-Folder (under `src`) with your favorite IDE. No additional tools required. You might need to restore the needed workloads though (use `dotnet restore workload` in the `./src` directory).  
  
Before you can start, you should restore all NuGet-Packages using `dotnet restore` if that's not done for you by your IDE.

## 👋 Want to Contribute?

Cool! We're always welcoming anyone that wants to contribute to this project! Take a look at the [Contributing Guidelines](CONTRIBUTING.md), which helps you get started. You can also look at the [Open Issues](https://github.com/OptiSchmopti/CsvProc9000/issues) for getting more info about current or upcoming tasks.

## 💬 Want to discuss?

If you have any questions, doubts, ideas, problems or you simply want to present your opinions and views, feel free to hop into [Discussions](https://github.com/OptiSchmopti/CsvProc9000/discussions) and write about what you care about. We'd love to hear from you!
