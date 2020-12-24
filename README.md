# AzurePowerShellCmdlets
A C# library which exposes PowerShell cmdlets that connect and operate resources in Azure.

## About

As of today (Dec 2020), Azure does not provide a native SDK integration with PowerShell. While Azure AZ PowerShell module helps manage Azure resources with control plane operations, there is no module which helps use data plane operations. This library provides a wrapper around the native .NET SDK for Azure components so that it can be exposed as cmdlets for PowerShell.

## Supporting Cmdlets

As of today only two cmdlets are supported. Contributions (PRs) which add support to more cmdlets are most welcome!

1. Get-ServiceBusMessage (Polls for and fetches a message present in a service bus topic, for a particular subscription)
2. Set-AzureTableEntity (Sets the value for a particular column in a row identified with the row and partition key)

## Installation

The release dlls are present in /DLLs folder and can be copied to the desired location and imported using

```
Import-Module ..\path\to\AzurePowerShellCmdlets.dll
```
Note that the `AzurePowerShellCmdlets.dll` is the main dll, while the others are supporting dlls from NuGet modules.

## Development Requirements

* .NET Framework 4.7.2