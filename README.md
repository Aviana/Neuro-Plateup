# Neuro-Plateup

> [!Warning]
> This mod requires a server that uses the following [API](https://github.com/VedalAI/neuro-game-sdk).

This mod will add virtual players to your game and present the server with actions and feedback in regard to the game state. Only the host requires this mod.


## Setup

1. (Optional) Launch PlateUp! without this mod and create a profile for each bot. The profile name will be the name of the bot. Also adjust player color and accessories on the mirror in the starter lobby. If you skip this profiles with default values will be created for you. You can also still edit profiles by selecting them when starting a single player lobby while using the mod.
2. Download the latest file from the [Releases](https://github.com/Aviana/Neuro-Plateup/releases).
3. Extract and copy the Neuro-Plateup folder to your ../SteamLibrary/steamapps/common/PlateUp/PlateUp/Mods/ directory.
4. Modify the bots.csv file inside the Neuro-Plateup folder to adjust the amount of bots and names. Each line contains one bot. Syntax is <websocket-url>,<profilename>. Unreachable URLs will not spawn a bot. Profilename has a character limit of 20.
5. Launch the game.

## Usage

The bots follow a role system. They can at any time switch to any of the available roles. These roles dictate the actions they are able to make and will set the room they operate in.
The available roles are:

### Chef
Actions for the chef are:
- Prepare food (NYI)
- Extinguish fires
- Clean mess on the floor (kitchen) (NYI)
- Clean plates (if there is no one with the dishwasher role)

### Waiter
Actions for the waiter are:
- Take orders from customers
- Take food from the hatches to the customers
- Clean mess on the floor (not kitchen) (NYI)
- Return dirty plates back to the kitchen

### Dishwasher
- Clean plates
- Clean mess on the floor (kitchen) (NYI)

### Actions for every role
- Stop their current action
- Empty bins
- Go to another player
- Start game (Move to the start area in the franchise lobby)
- Rename the restaurant (NYI)

NYI: The roles are represented in-game through the outfit the bot wears.
Actions like prepare food and take orders will always target the customer group with the least patience.
Every time you encounter a check for consent from all players the bots will automatically agree to it (they do not have free will). However they are able to choose Unlocks(Cards).

## Notes on restaurant building

- DO NOT BLOCK PATHS. This includes chairs. Because occupied chairs are considered an obstacle. And while two half size appliances like the fridge opposite to each other leave a full slot to walk between, it does not for the bots.
- Keep kitchen and dining seperate. The bots are designed to pass things through the hatch that connects dining and kitchen. Always make sure there are enough counters on one side of the hatch to match the amount of guests per group. A hatch the size of 2 can reach 4 tiles on the other side.
- Keep automation simple (see out of scope)

## Out of Scope

Due to the complexity this game offers i have chosen to cut certain aspects of the game out of my implementation.
Those are:
- Automation features: Making the bots understand whatever automated systems were set up during prep phase is too much of a task. However all existing ingredients / food at prep time will be utilized by them.
- Prep / build phase: The bots are meant as companions and are incapable of designing any kind of (especially not an efficient) kitchen.

## Credits

- [dot_avi](https://github.com/Aviana): basically everything in this repository

Thanks to:
- Vedal987: Creator of [Neuro-sama](https://www.twitch.tv/vedal987)
- [Alexvoid](https://github.com/Alexejhero): Creator of the [Neuro Game SDK](https://github.com/VedalAI/neuro-game-sdk)
- [Pasu4](https://github.com/Pasu4), [CoolCat467](https://github.com/CoolCat467): The [Tony](https://github.com/Pasu4/neuro-api-tony) interface for debugging
