from typing import TYPE_CHECKING
from rule_builder.rules import CanReachLocation

if TYPE_CHECKING:
    from . import CasualtiesUnknownWorld

def set_rules(world: "CasualtiesUnknownWorld"):
    player = world.player
    options = world.options
    mw = world.multiworld
    
    if options.GoalRequirement == 1:
        world.set_completion_rule(CanReachLocation(f"Depth Milestone - {options.GoalDepth.value}m"))
    elif options.GoalRequirement == 2:
        world.set_completion_rule(CanReachLocation("Depth Milestone - 1500m"))
    elif options.GoalRequirement == 3:
        world.set_completion_rule(CanReachLocation("Elder Thornback Defeated!"))