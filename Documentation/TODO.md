### TODO:TODO:TODO
### TODO:TODO:TODO
### TODO:TODO:TODO

# CURRENT
    weasel porcupine abilities
    figure out ability previews vs range



# MVP/PROTOTYPE SHIT
    - setup premade level with terrain
    - enemy work
        - write editor/gizmos to preview enemy AI
        - ranged enemies
        - spring weighting and performance review
    - sfx
        - movement
        - taking damage
        - using abilities
        - enemies spawning
        - enemy idles
        - impacts
        - environmental

# EXPANSION STUFF
### New targeting systems + UI for them
    - TARGETING_SELF - follows
    - TARGETING_VECTOR_GROUND - ground targeted vector
    - TARGETING_VECTOR_ENTITY - entity targeted vector

### Casting
    - targeting on cast or on release?
    - channels?
### Ability Stacks
    - cooldown while stacks remain?
    - stack cooldown vs ability cooldown
    - ui?
        - custom border?


# TECH DEBT
    - encounter manager
        - figure out callbacks when enemies are killed
    - use rolling ID numbers in other places that should use it
    - ui for recasts

# NICE STUFF
    - add more granularity for how encounters can get built
        - seperate enemy types in a single wave
        - alternate wave finish conditions (percentage, time, switch)
        - delayed spawns, (initial spawn plus trickle in)
        - seperate spawn points per enemy type
        - randomness????

    - show player stats
    - allow stat upgrades
    - figure out respawning
    - setup the ability to cancel abilities when starting
    - setup abilities to get constant ticks even while not active (split tick)
    - parrys


# IDEAS