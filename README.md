# Dirt 0.9

# Description

Dirt is a collection of game-oriented libraries. Compatible with Unity 2019.3

# Application

## Dirt Server

Standalone implementation of Dirt.GameServer, handle platform-level logic (such as game clock)

# Dirt Collection

## Game
_Client/Server library_

Contains content logic and some mathematics tools.

## Simulation
_Client/Server library_

ECS Framework (use structure components array)

## Network

Shared library. Additional Network Layer (Based on Mud) to allow simulation to be partially (or completely) synchronized through network. Ownership of entities is always decided by the server.
Ownership can also be partial. Supports realtime gameplay.

## GameServer

Server library. Half Monolith that acts as the connector between game and network.

* Runs multiple simulations (with customizable lifespan) in parallel
* Route Players to simulations
* Define custom player commands 
* Runs a webservice and provide a Rest API (undocumented yet) 

# Unity Collection

## Dirt.Unity

Main framework for game prototyping. Also provide Unity oriented helpers to integrate custom ecs.

## Dirt.Unity.Network

Network layer for Unity. Provides a Mud connection utility and ECS systems for actor synchronization.

## Dirt.Unity.Logger

Unity implementation for the logger

_Hard reference to UnityEngine.dll in .csproj (will be fixed someday)_

# Mud Collection

## Mud (Common)
_Client/Server Library_

Main UDP Protocol and serialization logic.

## Mud.Server
_Server Library_

Network Socket Thread handler.

# External Libraries

## NetSerializer

An efficient data serializer created by [@tomba](https://github.com/tomba)
* https://github.com/tomba/netserializer