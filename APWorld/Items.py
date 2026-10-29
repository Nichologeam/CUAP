from Options import OptionError
from BaseClasses import Item, ItemClassification

from .Types import ItemData, LayerType, CasualtiesUnknownItem, layer_type_to_name
from typing import TYPE_CHECKING

if TYPE_CHECKING:
    from . import CasualtiesUnknownWorld

def recipe_items_enabled(world: "CasualtiesUnknownWorld") -> bool:
        return bool(world.options.RandomizeRecipes.value)
def skill_items_enabled(world: "CasualtiesUnknownWorld") -> bool:
        return bool(world.options.Skillsanity)
def limb_items_enabled(world: "CasualtiesUnknownWorld") -> bool:
        return bool(world.options.LimbUnlocks)
def questboard_items_enabled(world: "CasualtiesUnknownWorld") -> bool:
        return bool(world.options.Moodlesanity.value == 2)
def minigame_items(world: "CasualtiesUnknownWorld") -> int:
        return int(world.options.RandomizeMinigames.value)

def create_item(world: "CasualtiesUnknownWorld", name: str) -> CasualtiesUnknownItem:
    item_data = item_table.get(name)
    if not item_data:
        raise Exception(f"create_item: unknown item '{name}'")
    
    classification = item_data.classification
    
    if (name.endswith(" Recipe") or name == "Progressive INT") and world.options.Craftsanity:
        classification = ItemClassification.progression
    
    return CasualtiesUnknownItem(
        name=name,
        classification=classification,
        player=world.player,
        code=item_data.ap_code
    )

def create_itempool(world: "CasualtiesUnknownWorld") -> list["CasualtiesUnknownItem"]:
    pool: list[CasualtiesUnknownItem] = []
    player = world.player

    locations = list(world.multiworld.get_locations(player))
    total_locations = len(locations)

    # --- Depth / Layer Progression ---
    if world.options.GoalRequirement == 2:
        for _ in range(4):
            pool.append(create_item(world, "Progressive Layer"))
    else:
        goal_depth = world.options.GoalDepth.value
        layers_needed = (goal_depth + 299) // 300
        depth_extenders_count = max(0, layers_needed - 1)
        if goal_depth > 1500:
            depth_extenders_count += 5

        for _ in range(depth_extenders_count):
            pool.append(create_item(world, "Depth Extender"))

    # --- Layer Unlocks ---
    if world.options.GoalRequirement != 2:
        starting_layer_name = layer_type_to_name[LayerType(world.options.StartingLayer)]
        for layer_name in layers.keys():
            if layer_name != starting_layer_name:
                pool.append(create_item(world, layer_name))

    # --- Recipes ---
    if recipe_items_enabled(world):
        for recipe_name in recipes.keys():
            item = create_item(world, recipe_name)
            pool.append(item)

    # --- Skills ---
    if skill_items_enabled(world):
        for skill_name in skills.keys():
            for _ in range(18):
                item = create_item(world, skill_name)
                pool.append(item)

    # --- Limbs ---
    if limb_items_enabled(world):
        for limb_name in limbs.keys():
            for _ in range(3):
                item = create_item(world, limb_name)
                pool.append(item)

    # --- Quests ---
    if questboard_items_enabled(world):
        for _ in range(10):
            item = create_item(world, "Progressive Quests")
            pool.append(item)
        for _ in range(6):
            item = create_item(world, "Questboard Slot")
            pool.append(item)

    # --- Minigames ---
    if minigame_items(world) == 2:
        item = create_item(world, "Minigames")
        pool.append(item)
    elif minigame_items(world) == 3:
        i = 0
        for minigame in minigames.keys():
            i = i + 1
            if i == 1:
                continue
            item = create_item(world, minigame)
            pool.append(item)

    if total_locations < len(pool):
        raise OptionError(f"""
        ----- Casualties: Unknown -----
        {world.multiworld.player_name[world.player]} has more progression items than locations!
        Progression Items: {len(pool)}, Locations: {total_locations}.
        Change your settings to reduce items or increase locations.
        """)
    
    # --- Remaining items = filler + traps ---
    num_progression_items = len(pool)
    remaining_count = total_locations - num_progression_items
    if remaining_count > 0:
        # Traps
        trap_count = int(remaining_count * (world.options.TrapPercentage / 100))
        trap_count = max(0, min(trap_count, remaining_count))
        filler_count = remaining_count - trap_count

        trap_names = [name for name, data in item_table.items() if data.classification == ItemClassification.trap]
        trap_weights = [world.options.TrapWeights.get(name, 1) for name in trap_names]
        for _ in range(trap_count):
            chosen = world.random.choices(trap_names, weights=trap_weights, k=1)[0]
            pool.append(create_item(world, chosen))

        filler_names = [name for name, data in item_table.items() if data.classification == ItemClassification.filler]
        filler_weights = [junk_weights.get(name, 1) for name in filler_names]
        for _ in range(filler_count):
            chosen = world.random.choices(filler_names, weights=filler_weights, k=1)[0]
            pool.append(create_item(world, chosen))

    return pool

layers = {
    "Gravel Lands Unlock": ItemData(22318000, ItemClassification.progression),
    "Deeper Gravel Lands Unlock": ItemData(22318001, ItemClassification.progression),
    "Dried Desert Unlock": ItemData(22318002, ItemClassification.progression),
    "Wasteland Unlock": ItemData(22318003, ItemClassification.progression),
    "Overgrown Depths Unlock": ItemData(22318004, ItemClassification.progression)
}

recipes = { # In the order of the game's internal recipe list
    "Foliage rope Recipe": ItemData(22318100, ItemClassification.useful),
    "Foliage Recipe": ItemData(22318101, ItemClassification.useful),
    "String Recipe": ItemData(22318102, ItemClassification.useful),
    "Canvas Recipe": ItemData(22318103, ItemClassification.useful),
    "Wood scraps Recipe": ItemData(22318104, ItemClassification.useful),
    "Ripped dressing Recipe": ItemData(22318105, ItemClassification.useful),
    "Sterilized dressing Recipe": ItemData(22318106, ItemClassification.useful),
    "Bio-chem fluid Recipe": ItemData(22318107, ItemClassification.useful),
    "Opium Recipe": ItemData(22318108, ItemClassification.useful),
    "Morphine Recipe": ItemData(22318109, ItemClassification.useful),
    "Fentanyl Recipe": ItemData(22318110, ItemClassification.useful),
    "Painkillers Recipe": ItemData(22318111, ItemClassification.useful),
    "Neural booster Recipe": ItemData(22318112, ItemClassification.useful),
    "Canteen Recipe": ItemData(22318113, ItemClassification.useful),
    "Foliage bag Recipe": ItemData(22318114, ItemClassification.useful),
    "Sling bag Recipe": ItemData(22318115, ItemClassification.useful),
    "Scrap cube Recipe": ItemData(22318116, ItemClassification.progression),
    "Scrap panel Recipe": ItemData(22318117, ItemClassification.progression),
    "Scrap tube Recipe": ItemData(22318118, ItemClassification.useful),
    "Nails Recipe": ItemData(22318119, ItemClassification.progression),
    "Wood cube Recipe": ItemData(22318120, ItemClassification.progression),
    "Wood panel Recipe": ItemData(22318121, ItemClassification.progression),
    "Stick Recipe": ItemData(22318122, ItemClassification.useful),
    "Flimsy knife Recipe": ItemData(22318123, ItemClassification.useful),
    "Processed copper Recipe": ItemData(22318124, ItemClassification.useful),
    "Bundle of wires Recipe": ItemData(22318125, ItemClassification.progression),
    "Titanium slab Recipe": ItemData(22318126, ItemClassification.useful),
    "Titanium sheet Recipe": ItemData(22318127, ItemClassification.useful),
    "Titanium rod Recipe": ItemData(22318128, ItemClassification.progression),
    "Alien blood Recipe": ItemData(22318129, ItemClassification.useful),
    "Saline Recipe": ItemData(22318130, ItemClassification.useful),
    "Antivenom Recipe": ItemData(22318131, ItemClassification.useful),
    "Blood Recipe": ItemData(22318132, ItemClassification.useful),
    "Makeshift wrench Recipe": ItemData(22318133, ItemClassification.useful),
    "Wrench Recipe": ItemData(22318134, ItemClassification.useful),
    "Crude cleaver Recipe": ItemData(22318135, ItemClassification.useful),
    "Machete Recipe": ItemData(22318136, ItemClassification.useful),
    "Flammable powder Recipe": ItemData(22318137, ItemClassification.useful),
    "Casing Recipe": ItemData(22318138, ItemClassification.useful),
    "9mm round Recipe": ItemData(22318139, ItemClassification.useful),
    "5.56 round Recipe": ItemData(22318140, ItemClassification.useful),
    "12-Gauge buckshot Recipe": ItemData(22318141, ItemClassification.useful),
    "Magazine base Recipe": ItemData(22318142, ItemClassification.useful),
    "Small magazine Recipe": ItemData(22318143, ItemClassification.useful),
    "Rifle magazine Recipe": ItemData(22318144, ItemClassification.useful),
    "Box of 12-Guage Recipe": ItemData(22318145, ItemClassification.useful),
    "Dynamite Recipe": ItemData(22318146, ItemClassification.useful),
    "Large carcass Recipe": ItemData(22318147, ItemClassification.useful),
    "Circuit board Recipe": ItemData(22318148, ItemClassification.progression),
    "Small battery Recipe": ItemData(22318149, ItemClassification.useful),
    "Medium battery Recipe": ItemData(22318150, ItemClassification.useful),
    "Large battery Recipe": ItemData(22318151, ItemClassification.useful),
    "Flashlight Recipe": ItemData(22318152, ItemClassification.useful),
    "Headlamp Recipe": ItemData(22318153, ItemClassification.useful),
    "LCD screen Recipe": ItemData(22318154, ItemClassification.useful),
    "Flexiglass Recipe": ItemData(22318155, ItemClassification.useful),
    "Lightbulb Recipe": ItemData(22318156, ItemClassification.useful),
    "Limb wraps Recipe": ItemData(22318157, ItemClassification.useful),
    "Makeshift lamp Recipe": ItemData(22318158, ItemClassification.useful),
    "Bicycle helmet Recipe": ItemData(22318159, ItemClassification.useful),
    "Riot helmet Recipe": ItemData(22318160, ItemClassification.useful),
    "Makeshift helmet Recipe": ItemData(22318161, ItemClassification.useful),
    "Makeshift digging tool Recipe": ItemData(22318162, ItemClassification.useful),
    "Makeshift rifle Recipe": ItemData(22318163, ItemClassification.useful),
    "Mini laser drill Recipe": ItemData(22318164, ItemClassification.useful),
    "Dressing Recipe": ItemData(22318165, ItemClassification.useful),
    "Lantern Recipe": ItemData(22318166, ItemClassification.useful),
    "Terrain scanner Recipe": ItemData(22318167, ItemClassification.useful),
    "Geiger counter Recipe": ItemData(22318168, ItemClassification.useful),
    "Lockpicking kit Recipe": ItemData(22318169, ItemClassification.useful),
    "Advanced scuba diving gear Recipe": ItemData(22318170, ItemClassification.useful),
    "Pickaxe Recipe": ItemData(22318171, ItemClassification.useful),
    "Scaffolding pack Recipe": ItemData(22318172, ItemClassification.useful),
    "Backpack Recipe": ItemData(22318173, ItemClassification.useful),
    "Wood sandals Recipe": ItemData(22318174, ItemClassification.useful),
    "Tactical boots Recipe": ItemData(22318175, ItemClassification.useful),
    "Tactical gloves Recipe": ItemData(22318176, ItemClassification.useful),
    "Duffel bag Recipe": ItemData(22318177, ItemClassification.useful),
    "Material pouch Recipe": ItemData(22318178, ItemClassification.useful),
    "Belt Recipe": ItemData(22318179, ItemClassification.useful),
    "Trauma rig Recipe": ItemData(22318180, ItemClassification.useful),
    "Bowl of cereal Recipe": ItemData(22318181, ItemClassification.useful),
    "Fat Recipe": ItemData(22318182, ItemClassification.useful),
    "Soap Recipe": ItemData(22318183, ItemClassification.useful),
    "Clotting mush Recipe": ItemData(22318184, ItemClassification.useful),
    "Naltrexone Recipe": ItemData(22318185, ItemClassification.useful),
    "Antidepressants Recipe": ItemData(22318186, ItemClassification.useful),
    "Auto-injector Recipe": ItemData(22318187, ItemClassification.useful),
    "Chest drain Recipe": ItemData(22318188, ItemClassification.useful),
    "Hand crank charger Recipe": ItemData(22318189, ItemClassification.useful),
    "Antiseptic mush Recipe": ItemData(22318190, ItemClassification.useful),
    "Plastic dressing Recipe": ItemData(22318191, ItemClassification.useful),
    "Tourniquet Recipe": ItemData(22318192, ItemClassification.useful),
    "Bone welding tool Recipe": ItemData(22318193, ItemClassification.useful),
    "Tweezers Recipe": ItemData(22318194, ItemClassification.useful),
    "Blood bag Recipe": ItemData(22318195, ItemClassification.useful),
    "Saline bottle Recipe": ItemData(22318196, ItemClassification.useful),
    "Antiseptic Recipe": ItemData(22318197, ItemClassification.useful),
    "Relief cream Recipe": ItemData(22318198, ItemClassification.useful),
    "Splint Recipe": ItemData(22318199, ItemClassification.useful),
    "Bruise kit Recipe": ItemData(22318200, ItemClassification.useful),
    "Carcass splint Recipe": ItemData(22318201, ItemClassification.useful),
    "Makeshift L.R.D. Recipe": ItemData(22318202, ItemClassification.useful),
    "L.R.D. Recipe": ItemData(22318203, ItemClassification.useful),
    "L.R.D. Serum Recipe": ItemData(22318204, ItemClassification.useful),
    "Produce juice Recipe": ItemData(22318205, ItemClassification.useful),
    "Milk Recipe": ItemData(22318206, ItemClassification.useful),
    "Refined juice Recipe": ItemData(22318207, ItemClassification.useful),
    "Drill repair kit Recipe": ItemData(22318208, ItemClassification.progression), # can be used as a hearing loss alternative (and as progression)
    "Procoagulant Recipe": ItemData(22318209, ItemClassification.useful),
    "Spray bottle Recipe": ItemData(22318210, ItemClassification.useful),
    "Syringe Recipe": ItemData(22318211, ItemClassification.useful),
    "Firestarter Recipe": ItemData(22318212, ItemClassification.useful),
    "Campfire Recipe": ItemData(22318213, ItemClassification.useful),
    "Water Recipe": ItemData(22318214, ItemClassification.useful),
    "Charcoal Recipe": ItemData(22318215, ItemClassification.useful),
    "Bread Recipe": ItemData(22318216, ItemClassification.useful),
    "Pancake Recipe": ItemData(22318217, ItemClassification.useful),
    "Rye flour Recipe": ItemData(22318218, ItemClassification.useful),
    "Torch Recipe": ItemData(22318219, ItemClassification.useful),
    "Torch (relight) Recipe": ItemData(22318220, ItemClassification.useful),
    "Nutrient bar Recipe": ItemData(22318221, ItemClassification.useful),
    "Pemmican Recipe": ItemData(22318222, ItemClassification.useful),
    "Foliage meal Recipe": ItemData(22318223, ItemClassification.useful),
    "Burger Recipe": ItemData(22318224, ItemClassification.useful),
    "Soup Recipe": ItemData(22318225, ItemClassification.useful),
    "Ice pack Recipe": ItemData(22318226, ItemClassification.useful),
    "Scarf Recipe": ItemData(22318227, ItemClassification.useful),
    "Titanium pickaxe Recipe": ItemData(22318228, ItemClassification.useful),
    "Titanium machete Recipe": ItemData(22318229, ItemClassification.useful),
    "Titanium multitool Recipe": ItemData(22318230, ItemClassification.useful),
    "Climbing rope Recipe": ItemData(22318231, ItemClassification.useful)
}

skills = {
    "Progressive STR": ItemData(22318300, ItemClassification.useful, 18),
    "Progressive RES": ItemData(22318301, ItemClassification.useful, 18),
    "Progressive INT": ItemData(22318302, ItemClassification.useful, 18)
}

minigames = {
    "Minigames": ItemData(22318350, ItemClassification.progression),
    "Amputation Minigame": ItemData(22318351, ItemClassification.useful),
    "Bandage Minigame": ItemData(22318352, ItemClassification.useful),
    "Dislocation Minigame": ItemData(22318353, ItemClassification.useful),
    "Keypad Minigame": ItemData(22318354, ItemClassification.progression),
    "Lockpicking Minigame": ItemData(22318355, ItemClassification.progression),
    "Shrapnel Minigame": ItemData(22318356, ItemClassification.useful),
    "Syringe Minigame": ItemData(22318357, ItemClassification.useful),
    "Manual Defibrilation Minigame": ItemData(22318358, ItemClassification.useful),
    "AED Minigame": ItemData(22318359, ItemClassification.useful)
}

limbs = {
    "Progressive Left Arm": ItemData(22318400, ItemClassification.progression, 3),
    "Progressive Right Arm": ItemData(22318401, ItemClassification.progression, 3)
}

questboard_items = {
    "Progressive Quests": ItemData(22318450, ItemClassification.progression, 10),
    "Questboard Slot": ItemData(22318451, ItemClassification.useful, 3)
}

filler_items = {
    "Blood Crystal Shard": ItemData(22318500, ItemClassification.filler, 0),
    "Digestion Crystal Shard": ItemData(22318501, ItemClassification.filler, 0),
    "Emissive Crystal Shard": ItemData(22318502, ItemClassification.filler, 0),
    "Oxygen Crystal Shard": ItemData(22318503, ItemClassification.filler, 0),
    "Relief Crystal Shard": ItemData(22318504, ItemClassification.filler, 0),
    "Soothing Crystal Shard": ItemData(22318505, ItemClassification.filler, 0),
    "Turbulent Crystal Shard": ItemData(22318506, ItemClassification.filler, 0)
}

trap_items = {
    "Depression Trap": ItemData(22318550, ItemClassification.trap),
    "Hearing Loss Trap": ItemData(22318551, ItemClassification.trap),
    "Earthquake Trap": ItemData(22318552, ItemClassification.trap),
    "Reverse Controls Trap": ItemData(22318553, ItemClassification.trap),
    "Sleep Trap": ItemData(22318554, ItemClassification.trap),
    "Unchipped Trap": ItemData(22318555, ItemClassification.trap),
    "Elder Thornback Trap": ItemData(22318556, ItemClassification.trap),
    "Cave Ticks Trap": ItemData(22318557, ItemClassification.trap),
    "Bad Rep Trap": ItemData(22318558, ItemClassification.trap),
    "Disfigured Trap": ItemData(22318559, ItemClassification.trap),
    "Fellow Experiment": ItemData(22318560, ItemClassification.trap),
    "Fragile Items Trap": ItemData(22318561, ItemClassification.trap),
    "Mindwipe Trap": ItemData(22318562, ItemClassification.trap),
    "Pushup Trap": ItemData(22318563, ItemClassification.trap),
    "Temptation Trap": ItemData(22318564, ItemClassification.trap),
    "Trip Trap": ItemData(22318565, ItemClassification.trap)
}

depth_extenders = {
    "Depth Extender": ItemData(22318900, ItemClassification.progression)
}

progressive_layers = {
    "Progressive Layer": ItemData(22318901, ItemClassification.progression)
}

junk_weights = {
    "Blood Crystal Shard": 5,
    "Digestion Crystal Shard": 5,
    "Emissive Crystal Shard": 5,
    "Oxygen Crystal Shard": 5,
    "Relief Crystal Shard": 5,
    "Soothing Crystal Shard": 5,
    "Turbulent Crystal Shard": 5
}

item_table = {
    **layers,
    **recipes,
    **skills,
    **minigames,
    **filler_items,
    **trap_items,
    **depth_extenders,
    **progressive_layers,
    **limbs,
    **questboard_items,
}