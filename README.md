# Dirt 0.9

# Description

Dirt is a collection of game-oriented libraries written in .Net Standard 2.1
- Compatible with Unity 2021.3.x
- Compatible with .Net 8 projects

# Dirt Collection

## Game
_Client/Server library_

Contains content logic, math operations/structures and global flow structures.

## Simulation
_Client/Server library_

ECS Framework (use structure components array)

## Network
_Client/Server library_

UDP+TCP/HTTP message & event operations and built on top of Simulation. Synchronize Actors, send events, control over replicated actions.

## GameServer
_Server library_

Real time server boilerplate. (will probably be merged with GamePlugin)

## GamePlugin

* Game Server architecture
* Runs multiple simulations (with customizable lifespan) in parallel
* Route Players to simulations
* Define custom player commands, and actor actions
* Runs a webservice and provide a Rest API (undocumented yet) 

# Unity Collection

## Dirt.Unity

* Essentials for global game flow (legacy Dirt.Framework)
* Simulation View binding: pool game objects and attach Actor on spawns
* Unity Log override

This is probably the oldest bit of code in Dirt, I started working on that piece around 2017/2018, without really knowing where I was going back then.
This may disappear at some point or get simplified.

## Dirt.Unity.Network

* Mud Socket wrapper to connect to game server
* Complementary systems for replicated simulation


# Mud Collection

Mud is a really rough wrapper for UDP networking.

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