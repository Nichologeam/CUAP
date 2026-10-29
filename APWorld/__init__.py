from BaseClasses import MultiWorld, Item, Tutorial
from worlds.AutoWorld import World, CollectionState, WebWorld
from typing import Dict, Optional, Any
from Options import OptionError

from .Locations import get_location_names, get_total_locations, moodlesanity_locations
from .Items import create_item, create_itempool, item_table, filler_items, junk_weights
from .Options import CasualtiesUnknownOptions, casualties_unknown_option_groups
from .Regions import create_regions
from .Types import LayerType, layer_type_to_name, load_recipe_data, CasualtiesUnknownItem
from .Rules import set_rules

class CasualtiesUnknownWeb(WebWorld):
    theme = "Party"
    
    tutorials = [Tutorial(
        "Multiworld Setup Guide",
        "A guide to setting up Casualties: Unknown for Archipelago."
        "This guide covers single-player, multiworld, and related software.",
        "English",
        "setup_en.md",
        "setup/en",
        ["Nichologeam"]
    )]
    option_groups = casualties_unknown_option_groups

class CasualtiesUnknownWorld(World):
    """
    This is where you describe your game. Pretend you are marketing the game and that people have no clue what it is.
    Or make it silly. Whatever you wish I have no control over you.
    """

    game = "Casualties: Unknown"
    item_name_to_id = {name: data.ap_code for name, data in item_table.items()}
    location_name_to_id = get_location_names()
    options_dataclass = CasualtiesUnknownOptions
    web = CasualtiesUnknownWeb()
    # print("🐀🐀🐀🐀🐀🐀🐀🐀🐀🐀🐀🐀🐀🐀")

    def __init__(self, multiworld: "MultiWorld", player: int):
        super().__init__(multiworld, player)
        self.recipe_data = load_recipe_data()

    def generate_early(self):
        goal_depth = self.options.GoalDepth.value
        if goal_depth % 100 != 0:
            rounded = round(goal_depth / 100) * 100
            print(f"Casualties Unknown: Goal Depth {goal_depth} rounded to {rounded}")
            self.options.GoalDepth.value = rounded

        if self.options.GoalRequirement != 1: # all other goals
            self.options.GoalDepth.value = 1500
        raw_exclusions = set()
        option_minimal = ("Last stand", "Tachycardia", "Bradycardia", "Palpitations")
        option_deadly = ("Comatose", "Severe neurophysiological deterioration", "Neurological damage", "Respiratory arrest", "Lung failure", "Cardiac arrest", "Highly drugged", "Fatal opioid overdose", "Dying of withdrawal", "Catastrophic bleeding", "Life-threatening infection", "Septic shock", "Heatstroke", "Freezing to death", "Severe radiation sickness", "Chernobyl wannabe", "Last stand", "Severe hypoxemia", "Critical hypoxemia", "Ventricular fibrillation", "Palpitations", "Circulatory collapse", "Severe hypotension", "Hypertensive crisis", "Severe hypertension", "Toxic crisis")
        option_difficult = ("Opiated", "Drugged", "Highly drugged", "Fatal opioid overdose", "Opioid craving", "Withdrawal", "Severe withdrawal", "Dying of withdrawal", "Drug overdose", "Bloated", "Hypervolemic", "Critically hypervolemic", "Lethally hypervolemic", "Fat", "Obese", "Last stand", "Tachycardia", "Bradycardia", "Rapid neuron regeneration sickness", "Blind", "Boosted regrowth", "Palpitations", "Critical hypoxemia",  "FOCUSED")
        option_deadly_and_difficult = set(option_deadly) | set(option_difficult)
        for moodle_name in self.options.MoodlesanityExclusions.value:
            if moodle_name.lower() == "minimal":
                raw_exclusions.update(option_minimal)
            elif moodle_name.lower() == "deadly":
                raw_exclusions.update(option_deadly)
            elif moodle_name.lower() == "difficult":
                raw_exclusions.update(option_difficult)
            elif moodle_name.lower() == "deadly and difficult":
                raw_exclusions.update(option_deadly_and_difficult)
            else: # custom (not preset)
                raw_exclusions.add(moodle_name)
        self.moodlesanity_excluded_locations = set()
        short_to_full = {
            key[len("Moodlesanity - "):].lower(): key
            for key in moodlesanity_locations.keys()
        }
        for name in raw_exclusions:
            short_name = name.strip().lower()
            if short_name in short_to_full:
                self.moodlesanity_excluded_locations.add(short_to_full[short_name])
            else:
                raise OptionError(f"""
                ----- Casualties: Unknown -----
                {self.multiworld.player_name[self.player]}'s YAML has an error.
                Unknown Moodle was added to the Moodlesanity Exclusions List: {name}
                Change the Moodlesanity Exclusions setting to fix or remove this Moodle.
                """)
            
        if self.options.Moodlesanity == 2:
            self.questboard_order = self.generate_questboard()
        else:
            self.questboard_order = []

        self.interpret_slot_data(None)

        starting_layer_name = layer_type_to_name[LayerType(self.options.StartingLayer)]
        if self.options.GoalRequirement.value == 3 and starting_layer_name == "Overgrown Depths Unlock":
            print("Casualties Unknown: Cannot have a starting layer of Overgrown Depths with the Defeat Elder Thornback goal. Rerandomizing...")
            self.options.StartingLayer.value = self.random.choice(range(1, 5)) # inclusive, exclusive
            starting_layer_name = layer_type_to_name[LayerType(self.options.StartingLayer)]
        self.multiworld.push_precollected(create_item(self, starting_layer_name))

    def generate_questboard(self) -> list[str]:
        moodle_locations = [location for location in Locations.moodlesanity_locations if location not in self.moodlesanity_excluded_locations]

        self.random.shuffle(moodle_locations)

        if len(moodle_locations) < 15:
            raise OptionError(f"""
            ----- Casualties: Unknown -----
            {self.multiworld.player_name[self.player]}'s YAML has an error.
            Too many moodles were excluded to build a questboard.
            Remove some Moodles from the Moodlesanity Exclusions setting.
            """)

        return moodle_locations    
    
    def set_rules(self):
        set_rules(self)
    
    def create_regions(self):
        create_regions(self)

    def create_items(self) -> list[Item]:
        self.multiworld.itempool += create_itempool(self)

    def create_item(self, name: str) -> CasualtiesUnknownItem:
        return Items.create_item(self, name)
    
    def get_filler_item_name(self) -> str:
        return self.random.choices(
            population=list(filler_items.keys()),
            weights=list(junk_weights.values()),
            k=1
        )[0]
    
    def fill_slot_data(self) -> Dict[str, object]:
        slot_data: Dict[str, object] = {
            "Goal":                self.options.GoalRequirement.value,
            "StartingLayer":       self.options.StartingLayer.value,
            "GoalDepth":           self.options.GoalDepth.value,
            "Moodlesanity":        self.options.Moodlesanity.value,
            "LimbUnlocks":         self.options.LimbUnlocks.value,
            "RandomizeRecipes":    self.options.RandomizeRecipes.value,
            "Skillsanity":         self.options.Skillsanity.value,
            "FreeSamples":         self.options.FreeSamples.value,
            "QuestboardOrder":     self.questboard_order,
            "QuestboardCooldown":  self.options.QuestboardCooldown.value,
            "APItemAmount":        self.options.ArchipelagoItems.value,
            "Craftsanity":         self.options.Craftsanity.value,
            "Minigames":           self.options.RandomizeMinigames.value,
            "EnergyLink":          self.options.EnergyLink.value,
            "Gifting":             self.options.Gifting.value,
            "APWorldVersion":      "v0.12.1",
            
            "Seed": self.multiworld.seed_name,
            "Slot": self.multiworld.player_name[self.player],
            "TotalLocations": get_total_locations(self)
        }
        return slot_data
    
    def interpret_slot_data(self, slot_data: Optional[Dict[str, Any]]) -> Optional[Dict[str, Any]]: # Universal Tracker integration
        if not slot_data \
            and hasattr(self.multiworld, "re_gen_passthrough") \
            and isinstance(self.multiworld.re_gen_passthrough, dict) \
            and "Casualties: Unknown" in self.multiworld.re_gen_passthrough:
                slot_data = self.multiworld.re_gen_passthrough["Casualties: Unknown"]

        if not slot_data:
            return None
        
        self.options.GoalRequirement.value = slot_data["Goal"]
        self.options.StartingLayer.value = slot_data["StartingLayer"]
        self.options.GoalDepth.value = slot_data["GoalDepth"]
        self.options.Moodlesanity.value = slot_data["Moodlesanity"]
        self.options.LimbUnlocks.value = slot_data["LimbUnlocks"]
        self.options.RandomizeRecipes.value = slot_data["RandomizeRecipes"]
        self.options.Skillsanity.value = slot_data["Skillsanity"]
        self.options.FreeSamples.value = slot_data["FreeSamples"]
        self.questboard_order = slot_data["QuestboardOrder"]
        self.options.QuestboardCooldown.value = slot_data["QuestboardCooldown"]
        self.options.ArchipelagoItems.value = slot_data["APItemAmount"]
        self.options.Craftsanity.value = slot_data["Craftsanity"]
        self.options.RandomizeMinigames.value = slot_data["Minigames"]

        return slot_data
    
    def collect(self, state: "CollectionState", item: "Item") -> bool:
        return super().collect(state, item)
    
    def remove(self, state: "CollectionState", item: "Item") -> bool:
        return super().remove(state, item)