import json
import pkgutil
from enum import IntEnum
from typing import NamedTuple, Optional
from BaseClasses import Location, Item, ItemClassification
from rule_builder.options import OptionFilter
from rule_builder.rules import Has, HasAny, And

rule_cache = {}

class CasualtiesUnknownLocation(Location):
    game = "Casualties Unknown"

class CasualtiesUnknownItem(Item):
    game = "Casualties Unknown"

class LayerType(IntEnum):
    GravelLands = 1
    DeeperGravelLands = 2
    DriedDesert = 3
    Wasteland = 4
    OvergrownDepths = 5

layer_type_to_name = {
    LayerType.GravelLands:              "Gravel Lands Unlock",
    LayerType.DeeperGravelLands:        "Deeper Gravel Lands Unlock",
    LayerType.DriedDesert:              "Dried Desert Unlock",
    LayerType.Wasteland:                "Wasteland Unlock",
    LayerType.OvergrownDepths:          "Overgrown Depths Unlock"
}

class ItemData(NamedTuple):
    ap_code: Optional[int]
    classification: ItemClassification
    count: Optional[int] = 1

class LocData(NamedTuple):
    ap_code: Optional[int]
    region: Optional[str]

def load_recipe_data():
    data = pkgutil.get_data(__name__, "recipes.json")
    return json.loads(data.decode("utf-8"))

def build_recipe_rule(recipe_name: str, recipe: dict, data: dict, world, stack=None):
    from .Options import RandomizeCrafting, Skillsanity

    if stack is None:
        stack = set()

    # --- Cache check ---
    if recipe_name in rule_cache:
        return rule_cache[recipe_name]

    stack.add(recipe_name)

    rules = []

    # --- Require own recipe item if RandomizeRecipes is on ---
    rules.append(
        Has(
            f"{recipe_name.replace('Craftsanity - ', '')} Recipe",
            options=[OptionFilter(RandomizeCrafting, 1)],
            filtered_resolution=True
        )
    )

    # --- INT requirement ---
    int_req = recipe.get("intRequired", 0)
    if int_req > 0:
        rules.append(
            Has(
                "Progressive INT",
                count=int_req,
                options=[OptionFilter(Skillsanity, 1)],
                filtered_resolution=True
            )
        )

    # --- Dependencies ---
    for dep in recipe.get("reliesOn", []):
        dep_location = f"Craftsanity - {dep}"

        if dep_location in stack:
            # skip
            continue

        if dep_location in data["recipes"]:
            rules.append(
                build_recipe_rule(
                    dep_location,
                    data["recipes"][dep_location],
                    data,
                    world,
                    stack
                )
            )
        else:
            rules.append(Has(dep))

    # --- Layers ---
    if "needsLayerAny" in recipe:
        layers = recipe["needsLayerAny"]

        if layers:
            min_layer = min(layers)

            if world.options.GoalRequirement == 2:
                # Progressive layers mode
                required = max(0, min_layer - 1)

                if required > 0:
                    rules.append(Has("Progressive Layer", count=required))
            else:
                # Normal layer unlock items
                layer_items = [
                    layer_type_to_name[layer]
                    for layer in layers
                    if layer in layer_type_to_name
                ]

                if layer_items:
                    rules.append(HasAny(*layer_items))

    final_rule = And(*rules) if rules else True

    rule_cache[recipe_name] = final_rule

    stack.remove(recipe_name)

    return final_rule