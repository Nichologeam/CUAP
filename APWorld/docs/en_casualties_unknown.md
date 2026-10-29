# Casualties: Unknown

## Where is the options page?

The [player options page for this game](Casualties%20Unknown/player-options) contains all the options you need to configure and export a
config file.

## What does randomization do to this game?

You start with only a single layer, and have to unlock the rest through Archipelago. You can specify a specific layer in your yaml, or leave it as random.
Future versions of this game will have additional unlocks, such as crafting recipies.
The goal is to reach a depth of 1500m, or about 5 layers.

## Which items can be in another player's world?

Currently the only randomized items are Layer Unlocks, Recipes and Traps.
Layer Unlocks are self explanitory, they unlock the layer included in the item's name.
Recipes all start locked and must be gotten through the multiworld.
There are only two traps currently implimented: Depressed Trap (lowers mood) and Hearing Loss Trap (gives you hearing loss).

## When the player receives an item, what happens?

Traps instantly take effect, lowering their respective stat or applying their respective debuf.
Layer Unlocks will take effect the next time layers are changed. Keep in mind that the order you play through layers is still random.
Experiment will comment on each item they recieve, as long as they are able to speak. It is recommeded to have a text client handy.

## Using commands in game

The Archipelago mod allows you to send chat messages and commands using the ingame debug console.
At any time, press ` to open the console, then start a command with 'talk '.
Everything after the space will be sent as a chat message. Note that there is no way to see responses.