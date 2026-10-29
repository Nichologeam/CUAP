from typing import List, Dict
from dataclasses import dataclass
from worlds.AutoWorld import PerGameCommonOptions
from .Locations import moodlesanity_locations
from Options import Choice, OptionGroup, Toggle, Range, ItemDict, OptionList, OptionError, StartInventoryPool

def create_option_groups() -> List[OptionGroup]:
    option_group_list: List[OptionGroup] = []
    for name, options in casualties_unknown_option_groups.items():
        option_group_list.append(OptionGroup(name=name, options=options))

    return option_group_list

class ItemWeights(ItemDict):  # Modified from the ULTRAKILL APWorld
    def __init__(self, value: Dict[str, int]):
        if not value:
            raise OptionError("Casualties Unknown: At least one item weight must be specified!")
        if any(item_count < 0 for item_count in value.values()):
            raise OptionError("Casualties Unknown: Cannot have negative item weight!")
        if sum(value.values()) == 0:
            raise OptionError("Casualties Unknown: At least one item weight must be greater than 0!")
        super(ItemDict, self).__init__(value)

class GoalRequirement(Choice):
    """
    Where is the goal placed?
    Reach Depth: Reach a chosen depth starting from a random or chosen layer. Depth Extenders allow you to go deeper.
    Escape Overgrown Depths: Start in Gravel Lands and goal upon escaping Overgrown Depths. Progressive Layers allow you to go deeper.
    Defeat Elder Thornback: Start in a random layer that isn't Overgrown Depths and goal upon killing an Elder Thornback.
    """
    display_name = "Goal Requirement"
    option_reach_depth = 1
    option_escape_overgrown_depths = 2
    option_defeat_elder_thornback = 3
    default = 1

class StartingLayer(Choice):
    """
    Only has an effect if your goal is Reach Depth or Defeat Elder Thornback.
    Determines which layer you'll start with.
    You will have to unlock the other layers through the multiworld.
    If your goal is Defeat Elder Thornback, your starting layer cannot be Overgrown Depths.
    """
    display_name = "Starting Layer"
    option_gravel_lands = 1
    option_deeper_gravel_lands = 2
    option_dried_desert = 3
    option_wasteland = 4
    option_overgrown_depths = 5
    default = "random"

class GoalDepth(Range):
    """
    Only has an effect if your goal is Reach Depth.
    How deep do you have to go in one run to goal? Must be a multiple of 100, and will be automatically rounded if it isn't.
    Each 300m requires a Depth Extender progression item.
    There will be the amount needed +5 in the item pool, unless your goal is 1500m or less, where there will be the exact amount needed.
    """
    display_name = "Goal Depth"
    range_start = 100
    range_end = 5000
    default = 1500

class Moodlesanity(Choice):
    """
    Each possible moodle (status effect) is a check. Does not include Anxious and Deceased, as they are impossible to get.
    Disabled: Moodles do not send checks.
    Questboard: Adds a new UI that shows 'quests'. You must get the shown moodle to send its check. More moodles and
                questboard slots are unlocked via items. Each "Progressive Quests" item (12 in total) unlocks 15 moodles.
    Open: All moodle locations are available in sphere 1 (minus a few). Creates VERY LARGE sphere 1. Use with caution.
    With every Moodle enabled, adds 166 locations.
    """
    display_name = "Moodlesanity"
    option_disabled = 1
    option_questboard = 2
    option_open = 3
    default = 1

class MoodlesanityExclusions(OptionList):
    """
    What Moodlesanity locations will be excluded? Only has an effect if Moodlesanity is on.

    The 'Minimal' preset excludes only Last Stand, Tachycardia, and Bradycarda
    The 'Deadly' preset excludes locations that will almost always kill Experiment (lung failure, comatose, etc...)
    The 'Difficult' preset excludes locations that rely on a specific item (drug overdose, rapid neuron regeneration sickness, etc...)
    The 'Deadly and Difficult' preset includes everything from the three presets above
    You can specify your own list as well.
    """
    display_name = "Moodlesanity Exclusions"
    valid_keys = {
        key[len("Moodlesanity - "):]  # remove prefix
        for key in moodlesanity_locations.keys()
    } | {"Minimal", "Deadly", "Difficult", "Deadly and Difficult"}
    default = ["Minimal"]

class QuestboardCooldownTime(Range):
    """
    How long should the cooldown between refreshes on the Questboard be? Only has an effect if Moodlesanity is set to Questboard.
    Time is in seconds. Set to 0 to disable the cooldown.
    """
    display_name = "Moodlesanity Questboard Cooldown Time"
    range_start = 0
    range_end = 120
    default = 15

class RandomizeMinigames(Choice):
    """
    Start without the ability to play Minigames (ex: lockpicking, removing shrapnel) until they are received through Archipelago.
    Suicide and self harm minigames are never randomized.

    Disabled: Minigames are always unlocked.
    As One: There is only one item that unlocks all minigames.
    Individual: Each of the 9 minigames is an individual item.
    """
    display_name = "Randomize Minigames"
    option_disabled = 1
    option_as_one = 2
    option_individual = 3
    default = 1

class LimbUnlocks(Toggle):
    """
    Start with Experiment's arms being amputated, with them being unlocked progressively through the multiworld.
    Each arm has three progressive items, one for each limb.
    There is NO LOGIC applied to this. Use at your own risk, as you will not be able to break tiles without arms.
    Adds 6 items.
    """
    display_name = "Limb Unlocks"

class RandomizeCrafting(Toggle):
    """
    If Recipes should be randomized.
    If enabled, all 132 Recipes become items to recieve.
    Blueprints will still spawn, but will only give INT experience (unless Archipelago Items is above 0).
    """
    display_name = "Randomize Recipes"

class FreeSamples(Toggle):
    """
    Only has an effect if Randomize Recipes is on.
    Receiving recipes spawns a free copy of the item the recipe makes, but only once.
    This does not work on items that only exist in liquid form.
    """
    display_name = "Free Samples"

class Craftsanity(Toggle):
    """
    Makes crafting each individual recipe in the game a location.
    Will be sphere 1 without additional options like Skillsanity and Randomize Recipes.
    Note: **Item qualities required for crafting are not considered in logic.**
    Adds 132 locations.
    """
    display_name = "Craftsanity"

class Skillsanity(Toggle):
    """
    Each skill level up to level 15 is a location, and up to level 18 is an item.
    Adds 45 locations and 54 items.
    """
    display_name = "Skillsanity"

class ArchipelagoItems(Range):
    """
    How many Archipelago Items can be found throughout the cave system?
    When used, they check a location and give 10 INT Experience.
    They are guarenteed to drop from destroying traps, opening containers, and killing enemies.
    Each Survivor is also guarenteed to have at least one free Archipelago Item in their trades.
    """
    display_name = "Archipelago Items"
    range_start = 0
    range_end = 200
    default = 40

class EnergyLink(Toggle):
    """
    Allow depositing and withdrawling EnergyLink currency for Survivor trading value.
    Deposit a positive amount to add to the pool, and deposit a negative amount to withdrawl.
    No energy is lost on transfer. (1:1 conversion)
    """
    display_name = "EnergyLink Trading"

class SleepLink(Toggle):
    """
    When another player with SleepLink enabled goes to sleep, you will too.
    When you go to sleep, all other players with SleepLink also fall asleep.

    The only other game that is planning to support this is Stardew Valley's Beta, so this is
    mainly meant for servers with more than one Casualties: Unknown game.
    """
    display_name = "SleepLink"

class Gifting(Toggle):
    """
    Allows gifting inventory items between games.
    Sending gifts to other Casualties: Unknown games will give that player the exact same item.
    Sending gifts to any other game will attempt to translate the gift into something similar.
    Most items are supported, but not all of them.

    Use the `apgift` command in-game to send gifts to others.
    """
    display_name = "Gifting"

class TrapPercentage(Range):
    """
    Globally changes how many filler items are replaced with traps. For specific item weights, see the next option.
    Treat as a percentage, with 100(%) being all filler items are traps. Set to 0(%) to disable traps.
    """
    display_name = "Trap Percentage"
    range_start = 0
    range_end = 100
    default = 0

class TrapWeights(ItemWeights):
    """
    Weights of each trap item. To disable all trap items, see the previous option.
    """
    display_name = "Trap Item Weights"
    valid_keys = ["Depression Trap", "Hearing Loss Trap", "Earthquake Trap", "Reverse Controls Trap", "Sleep Trap", "Unchipped Trap", "Elder Thornback Trap", "Cave Ticks Trap", "Bad Rep Trap", "Disfigured Trap", "Fellow Experiment", "Fragile Items Trap", "Mindwipe Trap", "Pushup Trap", "Temptation Trap", "Trip Trap"]
    default = {item: 50 for item in valid_keys}

@dataclass
class CasualtiesUnknownOptions(PerGameCommonOptions):
    GoalRequirement:       GoalRequirement
    StartingLayer:         StartingLayer
    GoalDepth:             GoalDepth
    Moodlesanity:          Moodlesanity
    MoodlesanityExclusions:MoodlesanityExclusions
    QuestboardCooldown:    QuestboardCooldownTime
    RandomizeMinigames:    RandomizeMinigames
    LimbUnlocks:           LimbUnlocks
    RandomizeRecipes:      RandomizeCrafting
    FreeSamples:           FreeSamples
    Craftsanity:           Craftsanity
    Skillsanity:           Skillsanity
    ArchipelagoItems:      ArchipelagoItems
    EnergyLink:            EnergyLink
    SleepLink:             SleepLink
    Gifting:               Gifting
    TrapPercentage:        TrapPercentage
    TrapWeights:           TrapWeights
    start_inventory_from_pool:StartInventoryPool

casualties_unknown_option_groups = [
    OptionGroup("Goal", [GoalRequirement, StartingLayer, GoalDepth]),
    OptionGroup("Moodlesanity", [Moodlesanity, MoodlesanityExclusions, QuestboardCooldownTime]),
    OptionGroup("Recipes", [RandomizeCrafting, FreeSamples, Craftsanity, Skillsanity]),
    OptionGroup("Other Locations", [RandomizeMinigames, LimbUnlocks, ArchipelagoItems]),
    OptionGroup("Traps", [TrapPercentage, TrapWeights]),
    OptionGroup("Cross-Game Interactions", [EnergyLink, SleepLink, Gifting])
]