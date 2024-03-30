#!/bin/bash

## Update Linux distrib
sudo apt-get update

## Install .Net 8 SDK / RT
sudo apt-get install -y dotnet-sdk-8.0 dotnet-runtime-8.0

## Pass git key?

## Enforce chmod 600
chmod 600 ~/.ssh/cmo_testserver

## Clone the repository
cd ~
git clone git@github.com:Salepate/cmo.git
## change branch
cd cmo
git checkout game/mmo_net8
cd Game

## Build Server
dotnet build CMO.Server/CMO.Server.csproj --runtime linux-x64 

## Run Server
./CMO.Server/bin/Debug/net8.0/CMO.Server

## Problems Detected

## Http listener doesnt accept to listen both 127.0.0.1 and * (must be one or the other)
## T3D Must be compiled for linux (binary is not loaded properly otherwise)
## Highprecision clock is windows only and the system doesnt catch error to fallback on lowclock
## UDP use a windows specific IO Control (throw an error)

## --GUI is broken and 