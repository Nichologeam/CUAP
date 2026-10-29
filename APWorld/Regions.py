import math

from BaseClasses import Region
from .Types import CasualtiesUnknownLocation
from .Locations import location_table, is_valid_location
from .Types import build_recipe_rule
from typing import TYPE_CHECKING
from rule_builder.rules import Has, HasAny, HasAll, True_

if TYPE_CHECKING:
    from . import CasualtiesUnknownWorld

def create_regions(world: "CasualtiesUnknownWorld"):
    menu = create_region(world, "Menu")
    gravellands = create_region_and_connect(world, "Gravel Lands", "Menu -> Gravel Lands", menu)
    deepergravellands = create_region_and_connect(world, "Deeper Gravel Lands", "Menu -> Deeper Gravel Lands", menu)
    drieddesert = create_region_and_connect(world, "Dried Desert", "Menu -> Dried Desert", menu)
    wasteland = create_region_and_connect(world, "Wasteland", "Menu -> Wasteland", menu)
    overgrowndepths = create_region_and_connect(world, "Overgrown Depths", "Menu -> Overgrown Depths", menu)
    starting_layer = world.options.StartingLayer.value
    world.set_rule(menu, True_())

    entrance = next(e for e in menu.exits if e.name == "Menu -> Gravel Lands")
    if world.options.GoalRequirement.value == 2:
        world.set_rule(entrance, True_())
    else:
        if starting_layer == 1:
            world.set_rule(entrance, True_())
        else:
            world.set_rule(entrance, Has("Gravel Lands Unlock"))

    entrance = next(e for e in menu.exits if e.name == "Menu -> Deeper Gravel Lands")
    if world.options.GoalRequirement.value == 2:
        world.set_rule(entrance, Has("Progressive Layer", count = 1))
    else:
        if starting_layer == 2:
            world.set_rule(entrance, True_())
        else:
            world.set_rule(entrance, Has("Deeper Gravel Lands Unlock"))

    entrance = next(e for e in menu.exits if e.name == "Menu -> Dried Desert")
    if world.options.GoalRequirement.value == 2:
        world.set_rule(entrance, Has("Progressive Layer", count = 2))
    else:
        if starting_layer == 3:
            world.set_rule(entrance, True_())
        else:
            world.set_rule(entrance, Has("Dried Desert Unlock"))

    entrance = next(e for e in menu.exits if e.name == "Menu -> Wasteland")
    if world.options.GoalRequirement.value == 2:
        world.set_rule(entrance, Has("Progressive Layer", count = 3))
    else:
        if starting_layer == 4:
            world.set_rule(entrance, True_())
        else:
            world.set_rule(entrance, Has("Wasteland Unlock"))

    entrance = next(e for e in menu.exits if e.name == "Menu -> Overgrown Depths")
    if world.options.GoalRequirement.value == 2:
        world.set_rule(entrance, Has("Progressive Layer", count = 4))
    else:
        if starting_layer == 5:
            world.set_rule(entrance, True_())
        else:
            world.set_rule(entrance, Has("Overgrown Depths Unlock"))

def create_region(world: "CasualtiesUnknownWorld", name: str) -> Region:
    if world.options.GoalRequirement.value != 1:
        max_depth = 1500
    else:
        max_depth = world.options.GoalDepth.value
    max_ap_items = world.options.ArchipelagoItems.value
    reg = Region(name, world.player, world.multiworld)

    hearing_loss_location_names = {
        "Moodlesanity - Impaired hearing",
        "Moodlesanity - Hearing loss",
        "Moodlesanity - Severe hearing loss"
    }

    for (key, data) in location_table.items():
        if data.region != name:
            continue

        if not is_valid_location(world, key):
            continue

        location = CasualtiesUnknownLocation(world.player, key, data.ap_code, reg)

        if key.startswith("Depth Milestone - "):
            milestone_depth = int(key.split(" ")[-1][:-1])

            if milestone_depth > max_depth:
                continue

            if world.options.GoalRequirement.value == 2:
                required_layers = max(0, (milestone_depth + 299) // 300 - 1)
                rule = Has("Progressive Layer", count = required_layers)
            else:
                required_extenders = max(0, (milestone_depth + 299) // 300 - 1)
                rule = Has("Depth Extender", count = required_extenders)
        
        if key.startswith("Archipelago Item "):
            item_number = int(key.split(" ")[-1])

            if item_number > max_ap_items:
                continue

            if world.options.RandomizeMinigames.value == 2:
                free_count = max(15 ,math.floor(max_ap_items * 0.20)) # if 100 items, this is 20 (minimum of 15)
                if item_number > free_count:
                    rule = Has("Minigames")
                    
            elif world.options.RandomizeMinigames.value == 3:
                free_count = max(15 ,math.floor(max_ap_items * 0.20)) # if 100 items, this is 20 (minimum of 15)
                either_count = math.floor((max_ap_items - free_count) * 0.50) # if 100 items, this is 40
                either_cutoff = free_count + either_count # if 100 items, this is 60
                if free_count < item_number <= either_cutoff:
                    rule = HasAny("Lockpicking Minigame", "Keypad Minigame")
                elif item_number > either_cutoff:
                    rule = HasAll("Lockpicking Minigame", "Keypad Minigame")
        
        if key.startswith("Craftsanity - "):
            if key not in world.recipe_data["recipes"]:
                raise Exception(f"Missing recipe data for {key}")
            recipe = world.recipe_data["recipes"][key]
            if recipe.get("impossible"):
                continue
            rule = build_recipe_rule(key, recipe, world.recipe_data, world)

        if world.options.Moodlesanity.value == 2:
            if key in world.questboard_order:
                index = world.questboard_order.index(key)
                required_quests = index // 15

                if required_quests > 0:
                    rule = Has("Progressive Quests", count = required_quests)

        if key in hearing_loss_location_names:
            if not world.options.RandomizeRecipes.value: # If recipes aren't randomized
                hearing_loss_rule = Has("Wasteland Unlock") | Has("Progressive Layer", count = 3)
            else: # If recipes are randomized
                hearing_loss_rule = (
                    # Either the player can make the Drill Repair Kit
                    (HasAll("Drill repair kit Recipe", "Titanium rod Recipe", "Nails Recipe", "Bundle of wires Recipe", "Circuit board Recipe")
                    & HasAny("Wood panel Recipe", "Scrap panel Recipe")
                    & HasAny("Wood cube Recipe", "Scrap cube Recipe"))

                    # Or they have direct access to the Wasteland layer
                    | (Has("Wasteland Unlock") | Has("Progressive Layer", count = 3))
                )

            rule = rule & hearing_loss_rule

        world.set_rule(location, rule)
        reg.locations.append(location)

    world.multiworld.regions.append(reg)
    return reg

def create_region_and_connect(world: "CasualtiesUnknownWorld", name: str, entrancename: str, connected_region: Region) -> Region:
    reg: Region = create_region(world, name)
    connected_region.connect(reg, entrancename)
    return reg