# Extra Battle Upgrades

A combat-focused upgrade pack for R.E.P.O. that expands the game's chaotic survival gameplay with new upgrade builds, enemy control mechanics, sustain systems, and clutch survivability.

## The mod is designed around:

- physics chaos
- risky close-range gameplay
- multiplayer cooperation
- scalable endless progression
- vanilla-style balance philosophy

Instead of turning the game into a generic power fantasy shooter, the upgrades are designed to enhance the existing chaos and reward skilled aggressive play.

### Features

- 5 fully integrated combat upgrades
- Endless scaling progression
- Multiplayer support
- Proper damage ownership tracking
- Physics interaction support
- Shop integration with REPOLib
- Vanilla-style escalating prices
- Custom stat labels
- Fully custom upgrade textures

### Armor

Reduces incoming damage.

![Preview](https://raw.githubusercontent.com/DarkSpider90/REPO-ExtraBattleUpgrades/main/images/ArmorUpgrade.png)

Each level reduces damage taken by 5%.

After level 10, scaling slows down to 1% per level.

Scaling:
- Level 1 = -5% damage taken
- Level 2 = -10%
- Level 3 = -15%
...
- Level 10 = -50%

After level 10:

- Level 11 = -51%
- Level 12 = -52%
etc.

Damage reduction is always rounded in the player's favor.

### Overcharge

Increases how long enemies can be held before overload.

![Preview](https://raw.githubusercontent.com/DarkSpider90/REPO-ExtraBattleUpgrades/main/images/OverchargeUpgrade.png)

This upgrade slows overcharge buildup and improves overcharge recovery while grabbing enemies.

Scaling:
- Level 1 = +10% overcharge capacity
- Level 2 = +20%
- Level 3 = +30%
...
- Level 10 = +100%

After level 10:

- Level 11 = +101%
- Level 12 = +102%
etc.

The system modifies both:

- overcharge buildup speed
- overcharge recovery behavior

### Second Chance

Prevents death and grants temporary invulnerability when receiving lethal damage.

![Preview](https://raw.githubusercontent.com/DarkSpider90/REPO-ExtraBattleUpgrades/main/images/SecondChanceUpgrade.png)

Instead of dying:

- the player's HP becomes 1
- health turns yellow
- temporary invulnerability activates
- the player is launched upward in chaotic ragdoll fashion

The effect also works for death pits and void falls.

After activation, the upgrade enters cooldown.

Invulnerability Duration:
- Level 1 = 1 second
- Level 2 = 2 seconds
- Level 3 = 3 seconds
...
- Level 5 = 5 seconds

After level 5:

- duration no longer increases
- additional levels reduce cooldown instead

Cooldown Scaling:

Base cooldown = 120 seconds

After level 5 - each level reduces cooldown by 5 seconds

Examples:

- Level 6 = 115 seconds
- Level 7 = 110 seconds
...
- Level 10 = 95 seconds

After level 10:

each level reduces cooldown by 0.5 seconds

Examples:

- Level 11 = 69.5 seconds
- Level 12 = 69 seconds

Second Chance intentionally preserves chaos:

- ragdoll remains active
- players may still fall back into pits
- enemies still attack normally

This is not immortality.
It is a last-second emergency survival system.

### Energy Leech

A combat sustain upgrade that restores health from damage dealt.

![Preview](https://raw.githubusercontent.com/DarkSpider90/REPO-ExtraBattleUpgrades/main/images/EnergyLeechUpgrade.png)

Healing is calculated from ACTUAL enemy HP lost.

Only direct player-caused damage counts.

Examples of valid damage:

- weapon damage
- Shock Grip damage
- tumble launch damage
- held object collisions
- smashing enemies into walls
- damage from objects currently held by players

Invalid damage:

- enemies damaging each other
- fall damage without player involvement
- death pits
- world/environment damage
- uncontrolled physics objects

Healing is granted ONLY to players who actually dealt damage.

If multiple players damage the same enemy - all contributing players receive healing

If a player did not contribute damage - they receive nothing

Scaling:

- Level 1 = 2.5%
- Level 2 = 5%
- Level 3 = 7.5%
...
- Level 10 = 25%

After level 10:

each level adds +1%

Examples:

- Level 11 = 26%
- Level 12 = 27%

Healing is always rounded UP to whole numbers.

Example:

100 damage dealt at level 1
2.5 healing
rounded up to 3 HP restored

### Shock Grip

Electrocutes stunned airborne enemies while they are being held.

![Preview](https://raw.githubusercontent.com/DarkSpider90/REPO-ExtraBattleUpgrades/main/images/ShockGripUpgrade.png)

This upgrade ONLY works when:

- the enemy is stunned
- the enemy is airborne
- the enemy is currently being grabbed

Simple touching does NOT activate the effect.

The upgrade:

- only affects monsters
- does NOT damage players

Shock damage is applied repeatedly while the enemy remains airborne and controlled.

Scaling:

- Level 1 = 2 DPS
- Level 2 = 4 DPS
- Level 3 = 6 DPS
...
- Level 10 = 20 DPS

After level 10 - each level adds +1 DPS

Examples:

- Level 11 = 21 DPS
- Level 12 = 22 DPS

Shock Grip uses multiplayer-aware ownership tracking and correctly credits damage to active holders.

## Installation

### Mod Manager:

Install with a Thunderstore-compatible mod manager.

### Manual:

1. Install BepInEx.
2. Place `RemainingValueTracker.dll` into:

`BepInEx/plugins/RemainingValueTracker/`

BepInEx/plugins/ExtraBattleUpgrades/

## Work In Progress

Extra Battle Upgrades is currently in active development.

Planned future features include:
- visual combat effects
- custom particles
- electricity effects for Shock Grip
- impact and protection effects for Armor
- enhanced emergency effects for Second Chance
- improved visual feedback for Energy Leech
- additional combat upgrades
- balancing improvements
- more upgrade textures and polish

The current focus is gameplay mechanics, multiplayer stability, and progression balancing.

## Credits

Special thanks to SLRUpgradePack By SLR for the original inspiration behind the Armor and Overcharge upgrade concepts.