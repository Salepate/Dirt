# Dirt 1.0

# Description

Dirt is a collection of game-oriented libraries. Compatible with Unity 2018+

# Application

## Dirt Server

Standalone implementation of Dirt.GameServer, handle platform-level logic (such as game clock)

# Dirt Collection

## Game

Shared library. contains content logic and some mathematics tools.

## Simulation

Shared library. Naive ECS approach, can run most of the game logic with decent cost but cannot be used for massive
amount of entities. Although multiple simulations can be run independently. Might be changed someday.

## Network

Shared library. Additional Network Layer (Based on Mud) to allow simulation to be partially (or completely) synchronized through network. Ownership of entities is always decided by the server.
Ownership can also be partial. Supports realtime gameplay.

## GameServer

Server library. Half Monolith that acts as the connector between game and network.

* Runs multiple simulations (with customizable lifespan) in parallel
* Route Players to simulations
* Define custom player commands 
* Runs a webservice and provide a Rest API (undocumented yet) 

# Mud Collection

## Mud (Common)

Shared Library. Main UDP Protocol and serialization logic.

## Mud.Server

Server Library. Network Socket Thread handler.

# External Libraries

## NetSerializer

Some library found on the internet that provides efficient serizaliation, intended for network operations. Customized for Mud.