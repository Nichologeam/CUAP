import math

from BaseClasses import Region
from .Types import CasualtiesUnknownLocation
from .Locations import location_table, is_valid_location
from .Types import build_recipe_rule
from typing import TYPE_CHECKING
from rule_builder.rules import Has, HasAny, HasAnyCount, HasAll, Or, True_

if TYPE_CHECKING:
    from . import CasualtiesUnknownWorld

def create_regions(world: "CasualtiesUnknownWorld"):
    menu = create_region(world, "Menu")
    gravellands = create_region_and_connect(world, "Gravel Lands", "Menu -> Gravel Lands", menu)
    deepergravellands = create_region_and_connect(world, "Deeper Gravel Lands", "Menu -> Deeper Gravel Lands", menu)
    drieddesert = create_region_and_connect(world, "Dried Desert", "Menu -> Dried Desert", menu)
    wasteland = create_region_and_connect(world, "Wasteland", "Menu -> Wasteland", menu)
    overgrowndepths = create_region_and_connect(world, "Overgrown Depths", "Menu -> Overgrown Depths", menu)
    if world.options.GoalRequirement.value != 2: # everything past OD doesn't exist on an Escape Overgrown run
        frozenchasm = create_region_and_connect(world, "Frozen Chasm", "Menu -> Frozen Chasm", menu)
        fungalpools = create_region_and_connect(world, "Fungal Pools", "Menu -> Fungal Pools", menu)
        crystalinehollow = create_region_and_connect(world, "Crystaline Hollow", "Menu -> Crystaline Hollow", menu)
        geothermalvents = create_region_and_connect(world, "Geothermal Vents", "Menu -> Geothermal Vents", menu)
        wherewemustreach = create_region_and_connect(world, "WHERE WE MUST REACH", "Menu -> WHERE WE MUST REACH", menu)
        crimsonascent = create_region_and_connect(world, "CRIMSON ASCENT", "Menu -> CRIMSON ASCENT", menu)
    starting_layer = world.options.StartingLayer.value
    world.set_rule(menu, True_())

    entrance = next(e for e in menu.exits if e.name == "Menu -> Gravel Lands")
    if world.options.GoalRequirement.value in (2, 4, 5, 6):
        world.set_rule(entrance, True_())
    else:
        if starting_layer == 1:
            world.set_rule(entrance, True_())
        else:
            world.set_rule(entrance, Has("Gravel Lands Unlock"))

    entrance = next(e for e in menu.exits if e.name == "Menu -> Deeper Gravel Lands")
    if world.options.GoalRequirement.value in (2, 4, 5, 6):
        world.set_rule(entrance, Has("Progressive Layer", count = 1))
    else:
        if starting_layer == 2:
            world.set_rule(entrance, True_())
        else:
            world.set_rule(entrance, Has("Deeper Gravel Lands Unlock"))

    entrance = next(e for e in menu.exits if e.name == "Menu -> Dried Desert")
    if world.options.GoalRequirement.value in (2, 4, 5, 6):
        world.set_rule(entrance, Has("Progressive Layer", count = 2))
    else:
        if starting_layer == 3:
            world.set_rule(entrance, True_())
        else:
            world.set_rule(entrance, Has("Dried Desert Unlock"))

    entrance = next(e for e in menu.exits if e.name == "Menu -> Wasteland")
    if world.options.GoalRequirement.value in (2, 4, 5, 6):
        world.set_rule(entrance, Has("Progressive Layer", count = 3))
    else:
        if starting_layer == 4:
            world.set_rule(entrance, True_())
        else:
            world.set_rule(entrance, Has("Wasteland Unlock"))

    entrance = next(e for e in menu.exits if e.name == "Menu -> Overgrown Depths")
    if world.options.GoalRequirement.value in (2, 4, 5, 6):
        world.set_rule(entrance, Has("Progressive Layer", count = 4))
    else:
        if starting_layer == 5:
            world.set_rule(entrance, True_())
        else:
            world.set_rule(entrance, Has("Overgrown Depths Unlock"))

    entrance = next(e for e in menu.exits if e.name == "Menu -> Frozen Chasm")
    if world.options.GoalRequirement.value in (4, 5, 6):
        world.set_rule(entrance, Has("Progressive Layer", count = 5))
    else:
        if starting_layer == 6:
            world.set_rule(entrance, True_())
        else:
            world.set_rule(entrance, Has("Frozen Chasm Unlock"))

    entrance = next(e for e in menu.exits if e.name == "Menu -> Fungal Pools")
    if world.options.GoalRequirement.value in (4, 5, 6):
        world.set_rule(entrance, Has("Progressive Layer", count = 6))
    else:
        if starting_layer == 7:
            world.set_rule(entrance, True_())
        else:
            world.set_rule(entrance, Has("Fungal Pools Unlock"))

    entrance = next(e for e in menu.exits if e.name == "Menu -> Crystaline Hollow")
    if world.options.GoalRequirement.value in (4, 5, 6):
        world.set_rule(entrance, Has("Progressive Layer", count = 7))
    else:
        if starting_layer == 8:
            world.set_rule(entrance, True_())
        else:
            world.set_rule(entrance, Has("Crystaline Hollow Unlock"))

    entrance = next(e for e in menu.exits if e.name == "Menu -> Geothermal Vents")
    if world.options.GoalRequirement.value in (4, 5, 6):
        world.set_rule(entrance, Has("Progressive Layer", count = 8))
    else:
        if starting_layer == 9:
            world.set_rule(entrance, True_())
        else:
            world.set_rule(entrance, Has("Geothermal Vents Unlock"))

    entrance = next(e for e in menu.exits if e.name == "Menu -> WHERE WE MUST REACH")
    if world.options.GoalRequirement.value in (4, 5, 6):
        world.set_rule(entrance, Has("Progressive Layer", count = 9))
    else: # It's not possible to have WWMR as a starting layer
        world.set_rule(entrance, Has("WHERE WE MUST REACH Unlock"))

    entrance = next(e for e in menu.exits if e.name == "Menu -> CRIMSON ASCENT")
    if world.options.GoalRequirement.value in (4, 5, 6):
        if world.options.AscentUnlockMode.value == 1:
            world.set_rule(entrance, Has("Progressive Layer", count = 9))
        if world.options.AscentUnlockMode.value == 2:
            world.set_rule(entrance, Has("Progressive Layer", count = 10)) # extra progressive layer
    else:
        world.set_rule(entrance, Has("CRIMSON ASCENT Unlock"))

def create_region(world: "CasualtiesUnknownWorld", name: str) -> Region:
    if world.options.GoalRequirement.value in (2, 3):
        max_depth = 1500
    elif world.options.GoalRequirement.value in (4, 5, 6):
        max_depth = 2700
    else:
        max_depth = world.options.GoalDepth.value
    max_ap_items = world.options.ArchipelagoItems.value
    reg = Region(name, world.player, world.multiworld)

    hearing_loss_location_names = {
        "Moodlesanity - Impaired hearing",
        "Moodlesanity - Hearing loss",
        "Moodlesanity - Severe hearing loss"
    }

    brainchip_needed_moodles = { # list of all moodles that require having the brainchip
        "Moodlesanity - High pressure",
        "Moodlesanity - Very high pressure",
        "Moodlesanity - Cognitive impairment",
        "Moodlesanity - Neurological damage",
        "Moodlesanity - Severe neurophysiological deterioration",
        "Moodlesanity - Comatose",
        "Moodlesanity - Stroke",
        "Moodlesanity - Arrhythmia",
        "Moodlesanity - Ventricular tachycardia",
        "Moodlesanity - Ventricular fibrillation",
        "Moodlesanity - Mild hypotension",
        "Moodlesanity - Hypotension",
        "Moodlesanity - Severe hypotension",
        "Moodlesanity - Circulatory collapse",
        "Moodlesanity - Mild hypertension",
        "Moodlesanity - Hypertension",
        "Moodlesanity - Severe hypertension",
        "Moodlesanity - Hypertensive crisis",
        "Moodlesanity - Hemothorax",
        "Moodlesanity - Pulmonary embolism",
        "Moodlesanity - Mild hypoxemia",
        "Moodlesanity - Hypoxemia",
        "Moodlesanity - Severe hypoxemia",
        "Moodlesanity - Critical hypoxemia",
        "Moodlesanity - Uncomfortable",
        "Moodlesanity - Radiation sickness",
        "Moodlesanity - Severe radiation sickness",
        "Moodlesanity - Chernobyl wannabe",
        "Moodlesanity - Opiated",
        "Moodlesanity - Drugged",
        "Moodlesanity - Highly drugged",
        "Moodlesanity - Fatal opioid overdose",
        "Moodlesanity - Opioid craving",
        "Moodlesanity - Withdrawal",
        "Moodlesanity - Severe withdrawal",
        "Moodlesanity - Dying of withdrawal",
        "Moodlesanity - Stimulated",
        "Moodlesanity - Concussed",
        "Moodlesanity - Drug overdose",
        "Moodlesanity - Internal bleeding",
        "Moodlesanity - Fractured neck",
        "Moodlesanity - Fractured ribs",
        "Moodlesanity - Dislocated spine",
        "Moodlesanity - Infection",
        "Moodlesanity - Sepsis",
        "Moodlesanity - Severe sepsis",
        "Moodlesanity - Septic shock",
        "Moodlesanity - Loose screws",
        "Moodlesanity - Immunocompromised",
        "Moodlesanity - Immunocompetent",
        "Moodlesanity - Boosted regrowth",
        "Moodlesanity - Underweight",
        "Moodlesanity - Skinny",
        "Moodlesanity - Chubby",
        "Moodlesanity - Overweight",
        "Moodlesanity - Energized",
        "Moodlesanity - Rapid neuron regeneration sickness",
        "Moodlesanity - Bad sleep",
        "Moodlesanity - Adrenaline"
    }

    living_limb_needed_moodles = {
        "Moodlesanity - Mild hypotension",
        "Moodlesanity - Hypotension",
        "Moodlesanity - Severe hypotension",
        "Moodlesanity - Circulatory collapse",
        "Moodlesanity - Mild hypertension",
        "Moodlesanity - Hypertension",
        "Moodlesanity - Severe hypertension",
        "Moodlesanity - Hypertensive crisis",
        "Moodlesanity - Mild toxicosis",
        "Moodlesanity - Toxicosis",
        "Moodlesanity - Severe toxicosis",
        "Moodlesanity - Toxic crisis",
        "Moodlesanity - Discomfort",
        "Moodlesanity - Pain",
        "Moodlesanity - Severe pain",
        "Moodlesanity - Agony",
        "Moodlesanity - Shock",
        "Moodlesanity - Minor bleeding",
        "Moodlesanity - Bleeding",
        "Moodlesanity - Heavy bleeding",
        "Moodlesanity - Catastrophic bleeding",
        "Moodlesanity - Infection",
        "Moodlesanity - Painful infection",
        "Moodlesanity - Severe infection",
        "Moodlesanity - Life-threatening infection",
        "Moodlesanity - Sepsis",
        "Moodlesanity - Severe sepsis",
        "Moodlesanity - Septic shock",
        "Moodlesanity - Dislocated joint"
    }

    for (key, data) in location_table.items():
        if data.region != name:
            continue

        if not is_valid_location(world, key):
            continue

        location = CasualtiesUnknownLocation(world.player, key, data.ap_code, reg)
        rule = True_()

        if key.startswith("Depth Milestone - "):
            milestone_depth = int(key.split(" ")[-1][:-1])

            if milestone_depth > max_depth:
                continue

            if world.options.GoalRequirement.value in (2, 4, 5, 6):
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

        if key in brainchip_needed_moodles:
            if world.options.UnlockBrainchip.value == 2: # as item
                brainchip_rule = Has("Brainchip")
                rule = rule & brainchip_rule
            elif world.options.UnlockBrainchip.value == 3: # removed
                continue # This location is impossible to check

        if key in living_limb_needed_moodles:
            if world.options.LimbUnlocks.value == 2: # by stats
                continue # A player could get too many progressive limbs, making these locations impossible to check. remove them
            elif world.options.LimbUnlocks.value == 3: # living last
                living_limb_rule = HasAnyCount({
                    "Progressive Head": 3,
                    "Progressive Thorax": 3,
                    "Progressive Abdomen": 3,
                    "Progressive Upper Right Arm": 3,
                    "Progressive Right Forearm": 3,
                    "Progressive Right Hand": 3,
                    "Progressive Upper Left Arm": 3,
                    "Progressive Left Forearm": 3,
                    "Progressive Left Hand": 3,
                    "Progressive Right Thigh": 3,
                    "Progressive Right Crus": 3,
                    "Progressive Right Foot": 3,
                    "Progressive Left Thigh": 3,
                    "Progressive Left Crus": 3,
                    "Progressive Left Foot": 3
                    })
                if key in ("Moodlesanity - Sepsis", "Moodlesanity - Severe sepsis", "Moodlesanity - Septic shock"):
                    if world.options.GoalRequirement.value in (2, 4, 5, 6):
                        extra_sepsis_rule = Or(living_limb_rule, Has("Progressive Layer", count = 5)) # access to layer 6
                    else:
                        extra_sepsis_rule = Or(living_limb_rule, HasAny("Frozen Chasm Unlock", "Fungal Pools Unlock")) # layers 6 or 7 (not funny, don't laugh)
                    rule = rule & extra_sepsis_rule
                else:
                    rule = rule & living_limb_rule

        if key in hearing_loss_location_names:
            if not world.options.RandomizeRecipes.value: # If recipes aren't randomized
                hearing_loss_rule = Has("Wasteland Unlock") | Has("Progressive Layer", count = 3)
            else: # If recipes are randomized
                hearing_loss_rule = (
                    # Either the player can make the Drill Repair Kit
                    (HasAll("Drill repair kit Recipe", "Nails Recipe", "Bundle of wires Recipe", "Scrap panel Recipe"))
                    # Or they have direct access to the Wasteland layer
                    | (HasAny("Wasteland Unlock", "WHERE WE MUST REACH Unlock") | Has("Progressive Layer", count = 3))
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