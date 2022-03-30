# ValHardMode - Hard Mode mod for Valheim
ValHardMode is a mod that is intended to increase the difficulty of Valheim while also including some low-impact quality of life changes.

It is meant to run on a dedicated server and requires all players connecting to that server to install the mod as well. It can also be ran in solo/client-hosted games by adding `VHM` to the end of your world name.

The mod will not load at all when connecting to a server not running the mod or a solo server without the world name suffix.

## Current Features

### Enemies
* Increases overall enemy health and attack damage
* Doubles the chance for enemies to level up
* Reduces the delay between enemy special attacks
* Caps enemy drops at 1 star
* Increases group spawn numbers
* Removes fear of fire from all enemies
* Updates certain drop rates to accomodate for recipe changes

### Bosses
* Increases all boss health, damage and movement speed

### Items
* Increases max stack size of stackable items
* Prevents most non-equippable/consumable items from being teleported
* Increases material cost of most crafting recipes

### Random Events
* Increases potential frequency and chance of random events
* Enables Skeleton & Troll events to happen at any time
* Removes Surtling event
* Adds 2 new random events

### Ships
* Ships now have a max weight and will begin to sink if exceeded
* Increases damage taken by ships from large waves
* Increases the cargo size of Karve

### Player
* Reduces death skill penalty to only reset progress on current level
* Increased range for pieces that Rested status into account for calculating comfort level

### Building
* Increases max capacity of Smelter-like objects
* Fireplace-type objects don't consume fuel
* Increases build range of crafting stations
* Increases crafting station extension placement range
* Increases grow time for planted trees
* Enables Maypole piece for building

### Dedicated Server
* Shared features will only load when connecting to a dedicated server with the same version installed
* Servers running this mod will only allow clients to connect if they also have the same version of the mod installed
* Fixes incorrect data on Steam server browser

## Installation

1. Backup your characters and worlds! Copy `C:\Users\<username>\AppData\LocalLow\IronGate\Valheim` somewhere safe just in case
2. Install [BepInExPack Valheim](https://valheim.thunderstore.io/package/denikson/BepInExPack_Valheim/) (v5.4.602 or higher) on clients and server
3. Download latest release and extract `ValHardMode.dll` to `...\BepInEx\plugins` folder created from previous step
4. Enjoy!


## Known Issues

* When removing the mod, or loading a character that played on a modded server in a non-modded server, item stacks or ore/fuel in Smelter-type objects over the normal max amount will be lost