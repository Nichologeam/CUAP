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
    Layers are individual items with the Reach Depth and Defeat Elder Thornback goals.
    Layers are progressive with all other goals.
    
    Reach Depth: Reach a chosen depth starting from a random or chosen layer. Depth Extenders allow you to go deeper.
    Escape Overgrown Depths: Start in Gravel Lands and goal upon escaping Overgrown Depths. Progressive Layers allow you to go deeper.
                             Note: This excludes all content past Overgrown Depths besides recipes.
    Defeat Elder Thornback: Start in a random layer that isn't Overgrown Depths and goal upon killing an Elder Thornback.
    Complete the Ascent: Retrieve the cargo and ascend to the surface. Any ending counts.
    Escape Containment: Retrieve the cargo, ascend to the surface, return to Earth, and complete the escape sequence.
    Merc: Retrieve the cargo and ascend to the surface with the M.E.R.C. scarf.
    """
    display_name = "Goal Requirement"
    option_reach_depth = 1
    option_escape_overgrown_depths = 2
    option_defeat_elder_thornback = 3
    option_complete_the_ascent = 4
    option_escape_containment = 5
    option_merc = 6
    default = 1

class GoalRequiresAllSawians(Toggle):
    """
    Only has an effect if your goal is not Reach Depth or Escape Overgrown Depths.
    Requries you to reach your set goal with all three playable Sawians.
    Use the `apchecksawians` command in-game to check your progress.
    Pairs well with the Unlock Sawians option.
    """
    display_name = "Goal Requires All Sawians"

class AscentUnlockMode(Choice):
    """
    Only has an effect if your goal is Complete the Ascent, Escape Containment, or Merc.
    How should WHERE WE MUST REACH and CRIMSON ASCENT be unlocked?
    Reaching the cargo without access to CRIMSON ASCENT will loop WHERE WE MUST REACH upon interacting with the drill.

    Dual Unlock: Another Progressive Layer is added that gives access to both WHERE WE MUST REACH and CRIMSON ASCENT.
    Individual: Two Progressive Layers are added, one for WHERE WE MUST REACH and one for CRIMSON ASCENT.
    """
    display_name = "Ascent Unlock Mode"
    option_dual_unlock = 1
    option_individual = 2
    default = 1

class StartingLayer(Choice):
    """
    Only has an effect if your goal is Reach Depth or Defeat Elder Thornback.
    Determines which layer you'll start with.
    You will have to unlock the other layers through the multiworld.
    If your goal is Defeat Elder Thornback, your starting layer cannot be Overgrown Depths.
    You cannot choose any layer past Geothermal Vents.
    """
    display_name = "Starting Layer"
    option_gravel_lands = 1
    option_deeper_gravel_lands = 2
    option_dried_desert = 3
    option_wasteland = 4
    option_overgrown_depths = 5
    option_frozen_chasm = 6
    option_fungal_pools = 7
    option_crystaline_hollow = 8
    option_geothermal_vents = 9
    default = "random"

class GoalDepth(Range):
    """
    Only has an effect if your goal is Reach Depth. Depths are in meters.
    How deep do you have to go in one run to goal? Must be a multiple of 100, and will be automatically rounded if it isn't.
    Each 300m requires a Depth Extender progression item.
    There will be the amount needed +5 in the item pool, unless your goal is 1500m or less, where there will be the exact amount needed.
    """
    display_name = "Goal Depth"
    range_start = 100
    range_end = 5000
    default = 2700 # roughly vanilla depth

class Moodlesanity(Choice):
    """
    Each possible moodle (status effect) is a location. Does not include Anxious and Deceased, as they are impossible to get.
    Disabled: Moodles do not check locations.
    Questboard: Adds a new UI that shows 'quests'. You must get the shown moodle to check its location. More moodles and
                questboard slots are unlocked via items. Each "Progressive Quests" item (11 in total) unlocks 15 moodles.
    Open: All moodle locations are available in sphere 1 (minus a few). Creates VERY LARGE sphere 1. Use with caution.
    With every Moodle enabled, adds 171 locations.
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
    The 'Deadly' preset excludes locations that will almost always kill the Sawian (lung failure, comatose, etc...)
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
    Suicide, Self harm, and the Cargo minigames are never randomized.

    Disabled: Minigames are always unlocked.
    As One: There is only one item that unlocks all minigames.
    Individual: Each of the 11 minigames is an individual item.
    """
    display_name = "Randomize Minigames"
    option_disabled = 1
    option_as_one = 2
    option_individual = 3
    default = 1

class LimbUnlocks(Choice):
    """
    Start with Wooden Peg limbs, with each limb being upgradable through items in the multiworld.
    Each limb has three progressive items, for a total of 45 items.

    Disabled: Vanilla behaviour.
    By Stats: Limbs are given by raw stats. Wood > Living > Steel > P.E.O.
    Living Last: Same order as By Stats, but Living is last. Wood > Steel > P.E.O. > Living
    """
    display_name = "Progressive Limbs"
    option_disabled = 1
    option_by_stats = 2
    option_living_last = 3
    default = 1

class RandomizeCrafting(Toggle):
    """
    If Recipes should be randomized.
    If enabled, 136 of the 137 Recipes become items to recieve.
    The M.E.R.C. scarf Recipe will remain in its vanilla location.
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

class Craftsanity(Choice):
    """
    Makes crafting each individual recipe in the game a location.
    Will be sphere 1 without additional options like Skillsanity and Randomize Recipes.
    Note: **Item qualities required for crafting are not considered in logic.**
    
    True: Adds 137 locations for all recipes.
    Exclude M.E.R.C. scarf: Adds 136 locations. The M.E.R.C. scarf will not have a location.
    False: No crafting recipes will have locations.
    """
    display_name = "Craftsanity"
    option_true = 1
    option_exclude_MERC_scarf = 2
    option_false = 3
    default = 3

class Skillsanity(Toggle):
    """
    Each skill level up to level 15 is a location, and up to level 18 is an item.
    Adds 45 locations and 54 items.
    """
    display_name = "Skillsanity"

class UnlockBrainchip(Choice):
    """
    If enabled, the Brainchip will become an item in the multiworld.
    Removes Unchipped Trap from the possible trap items if set to anything other than Start With.

    Start With: Vanilla behaviour. You start with the Brainchip.
    As Item: The Brainchip becomes an item in the multiworld.
    Remove Chip: The Brainchip is permanently removed from the game. You will never receive it.
    """
    display_name = "Unlock Brainchip"
    option_start_with = 1
    option_as_item = 2
    option_remove_chip = 3
    default = 1

class UnlockSawians(Toggle):
    """
    Turns the ability to play as each Sawian into an item.
    You will always start with one.

    Note: This option does not take into account if you have unlocked the characters legitimately.
    Enabling this option will give you all Sawians in your vanilla save unless you back it up first.
    """
    display_name = "Unlock Sawians"

class StartingSawian(Choice):
    """
    Only has an effect if Unlock Sawians is on.
    Which Sawian to start the multiworld with.
    """
    display_name = "Starting Sawian"
    option_experiment = 1
    option_milky = 2
    option_dune = 3
    default = "random"

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
    Most items from the demo are supported, but items exclusive to the full game are not.

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

    If Unlock Brainchip is set to anything other than Start With, Unchipped Trap is forced off.
    """
    display_name = "Trap Item Weights"
    valid_keys = ["Depression Trap", "Hearing Loss Trap", "Earthquake Trap", "Reverse Controls Trap", "Sleep Trap", "Unchipped Trap", "Elder Thornback Trap", "Cave Ticks Trap", "Bad Rep Trap", "Disfigured Trap", "Fellow Sawian", "Fragile Items Trap", "Mindwipe Trap", "Pushup Trap", "Temptation Trap", "Trip Trap"]
    default = {item: 50 for item in valid_keys}

@dataclass
class CasualtiesUnknownOptions(PerGameCommonOptions):
    GoalRequirement:       GoalRequirement
    GoalRequiresAllSawians:GoalRequiresAllSawians
    AscentUnlockMode:      AscentUnlockMode
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
    UnlockBrainchip:       UnlockBrainchip
    UnlockSawians:         UnlockSawians
    StartingSawian:        StartingSawian
    ArchipelagoItems:      ArchipelagoItems
    EnergyLink:            EnergyLink
    SleepLink:             SleepLink
    Gifting:               Gifting
    TrapPercentage:        TrapPercentage
    TrapWeights:           TrapWeights
    start_inventory_from_pool:StartInventoryPool

casualties_unknown_option_groups = [
    OptionGroup("Goal", [GoalRequirement, GoalRequiresAllSawians, AscentUnlockMode, StartingLayer, GoalDepth]),
    OptionGroup("Moodlesanity", [Moodlesanity, MoodlesanityExclusions, QuestboardCooldownTime]),
    OptionGroup("Recipes", [RandomizeCrafting, FreeSamples, Craftsanity, Skillsanity, ArchipelagoItems]),
    OptionGroup("Other Items", [RandomizeMinigames, LimbUnlocks, UnlockBrainchip, UnlockSawians, StartingSawian]),
    OptionGroup("Traps", [TrapPercentage, TrapWeights]),
    OptionGroup("Cross-Game Interactions", [EnergyLink, SleepLink, Gifting])
]