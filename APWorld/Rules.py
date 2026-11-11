from typing import TYPE_CHECKING
from .Options import RandomizeCrafting, Skillsanity
from rule_builder.rules import CanReachLocation, CanReachRegion, Has, HasAny, OptionFilter

if TYPE_CHECKING:
    from . import CasualtiesUnknownWorld

def set_rules(world: "CasualtiesUnknownWorld"):
    player = world.player
    options = world.options
    mw = world.multiworld
    
    if options.GoalRequirement == 1: # Reach Depth: requires reaching the specified depth
        world.set_completion_rule(CanReachLocation(f"Depth Milestone - {options.GoalDepth.value}m"))
    elif options.GoalRequirement == 2: # Escape Overgrown Depths: requires reaching the end of OD
        world.set_completion_rule(CanReachLocation("Depth Milestone - 1500m"))
    elif options.GoalRequirement == 3: # Defeat Elder Thornback: requires killing an elder
        world.set_completion_rule(CanReachLocation("Elder Thornback Defeated!"))
    elif options.GoalRequirement == 4: # Complete the Ascent: requires completing CRIMSON ASCENT
        world.set_completion_rule(CanReachRegion("CRIMSON ASCENT"))
    elif options.GoalRequirement == 5: # Escape Containment: requires completing CRIMSON ASCENT with a firearm and ammo + enough INT to craft them
        world.set_completion_rule(CanReachRegion("CRIMSON ASCENT") 
                                  & Has("Casing Recipe", options=[OptionFilter(RandomizeCrafting, 1)], filtered_resolution = True)
                                  & HasAny("5.56 round Recipe", "9mm round Recipe", "12-Gauge buckshot Recipe", options=[OptionFilter(RandomizeCrafting, 1)], filtered_resolution = True)
                                  & Has("Progressive INT", count = 12, options=[OptionFilter(Skillsanity, 1)], filtered_resolution = True))
    elif options.GoalRequirement == 6: # M.E.R.C.: requires completing CRIMSON ASCENT with the M.E.R.C. scarf + enough INT to craft it
        world.set_completion_rule(CanReachRegion("CRIMSON ASCENT") 
                                  & Has("Progressive INT", count = 18, options=[OptionFilter(Skillsanity, 1)], filtered_resolution = True))