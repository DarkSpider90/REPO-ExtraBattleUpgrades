# Extra Battle Upgrades

A combat-focused upgrade pack for R.E.P.O. that expands the game's chaotic survival gameplay with new upgrade builds, enemy control mechanics, sustain systems, and clutch survivability.

## The mod is designed around:

- physics chaos
- risky close-range gameplay
- multiplayer cooperation
- scalable endless progression
- vanilla-style balance philosophy

Instead of turning the game into a generic power fantasy shooter, the upgrades are designed to enhance the existing chaos and reward skilled aggressive play.


### Armor

Reduces incoming damage.

![Preview](https://raw.githubusercontent.com/DarkSpider90/REPO-ExtraBattleUpgrades/main/images/ArmorUpgrade.png)

Each level reduces damage taken by 5%.

Damage reduction is always rounded in the player's favor.

### Overcharge

Increases how long enemies can be held before overload.

![Preview](https://raw.githubusercontent.com/DarkSpider90/REPO-ExtraBattleUpgrades/main/images/OverchargeUpgrade.png)

This upgrade slows overcharge buildup and improves overcharge recovery while grabbing enemies.

The system modifies both:

- overcharge buildup speed
- overcharge recovery behavior

### Second Chance

Prevents death and grants temporary invulnerability when receiving lethal damage.

![Preview](https://raw.githubusercontent.com/DarkSpider90/REPO-ExtraBattleUpgrades/main/images/SecondChanceUpgrade.png)

Prevents death and grants temporary invulnerability after receiving lethal damage.

When activated:

the player's HP becomes 1
temporary invulnerability activates
health UI changes color
a special yellow heart HUD icon appears while the effect is ready
the icon turns gray during cooldown

Works for lethal damage and death pits.

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

Healing is always rounded UP to whole numbers.

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

### Panic Response

An emergency adrenaline survival upgrade that activates after taking damage.

![Preview](https://raw.githubusercontent.com/DarkSpider90/REPO-ExtraBattleUpgrades/main/images/PanicResponseUpgrade.png)

When triggered:

movement speed increases
stamina becomes infinite
stamina UI turns blue
a special pink lightning HUD icon appears while the effect is ready
the icon turns gray during cooldown

Designed for aggressive recovery, escape situations, and chaotic close-range survival.

## Detailed Upgrade Scaling

Full level progression, scaling values, cooldowns, DPS calculations, and balancing details can be found in:

`README_UPGRADES.md`

## Installation

### Mod Manager:

Install with a Thunderstore-compatible mod manager.

### Manual:

1. Install BepInEx.
2. Place `RemainingValueTracker.dll` into:

`BepInEx/plugins/ExtraBattleUpgrades/`

## Credits

Special thanks to SLRUpgradePack By SLR for the original inspiration behind the Armor and Overcharge upgrade concepts.