# TrustedTimestamps

[![Build status](https://ci.appveyor.com/api/projects/status/mm4tx0vigcluebih?svg=true)](https://ci.appveyor.com/project/rwthapp/trustedtimestamps)

> A collection of commandline tools for trusted timestamps

## Table of Contents

* [Install](#install)
* [Usage](#usage)
 * [Creating a Timestamp](#creating-a-timestamp)
 * [Verifying a Timestamp](#verifying-a-timestamp)
 * [Example Run](#example-run)
* [Timestamp Agency](#timestamp-agency)
* [License](#license)

## Install

To install the tool, go to [Releases](https://github.com/pitlabs/TrustedTimestamps/releases) and download the latest release as zip file.

The program can be accessed via the command line.

## Usage

To see a list of all possible commands, enter `-help`. To run the program, the array of commands given to it must always begin with `ts`.

The order of the commands is not important. However all necessary commands have to be entered for every run of the program, and all commands may only be entered once.

### Creating a Timestamp

To create a new Timestamp use the `-query` command. You can give the program the data to be timestamped either as a file path using the `-data <filepath>` command or as a SHA-256
hash using the `-digest <hash>` command. Specify where the timestamp should be saved using the `-out <path>` command. Make sure the file has the .tsr file ending. These commands are
necessary for the program to run.

`-no_nonce` is an optional command that tells the program the timestamp should not contain a nonce.

`-cert` is an optional command that tells the program the signer certificate should be included in the timestamp. While this is optional, it is necessary to later verify the timestamp.

### Verifying a Timestamp

To verify an existing Timestamp use the `-verify` command. The timestamp to be verified is specified using the `-in <filepath>` command, the data that originally was verified is given
to the program either using `-data <filepath>`, `-digest <hash>` or `-queryfile <path>\<file>.tsq`. These commands are necessary.

`-CAfile <path>` is an optional command that can be used to specify a certificate chain to be used in the verification of the timestamp.

`-CApath <path>` is an optional command that can be used to specify a single certificate to be used in the verification of the timestamp.

Make sure to always use only one of these two optional commands.

### Example run

Let's say we want to timestamp a file called `file.pdf` at the path `C:\Example\file.pdf`. To create a timestamp that we later can verify, make sure you are in the same directory
as the ts.exe file, open the command line and type the following:

```
ts.exe ts -query -data C:\Example\file.pdf -out C:\Example\response.tsr -cert
```

Now the program creates a new timestamp for `file.pdf`. To verify the created timestamp, type the following:

```
ts.exe ts -verify -data C:\Example\file.pdf -in C:\Example\response.tsr
```

It is important to note that the timestamp can **only** be verified if it was created using the `-cert` command.

To create and verify a timestamp using a hash type the following:

```
ts.exe ts -query -digest F2E8378C30D317ABFCDDF9E67472C7569CFAF24573095CCEAD2969ADAE665CFF -out C:\Example\response.tsr -cert
ts.exe ts -verify -digest F2E8378C30D317ABFCDDF9E67472C7569CFAF24573095CCEAD2969ADAE665CFF -in C:\Example\response.tsr
```

The program accepts both uppercase and lowercase hex formatting of a hash, however it accepts only SHA-256 hashes.

## Timestamp Agency

At the moment, the Timestamp Agency used for all timestamps is the DFN PKI.

## License

Apache © IT Center RWTH Aachen