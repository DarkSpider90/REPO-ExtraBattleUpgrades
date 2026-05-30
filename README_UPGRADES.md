# Extra Battle Upgrades — Detailed Upgrade Scaling

This document contains the full progression and balancing values for every upgrade included in Extra Battle Upgrades.

The main README focuses on gameplay overview and visual presentation.

This file contains:

* exact scaling values
* cooldown progression
* damage formulas
* healing formulas
* endless scaling behavior

---

# Armor

Reduces incoming damage.

## Scaling

* Level 1 = -5% damage taken
* Level 2 = -10%
* Level 3 = -15%
* Level 4 = -20%
* Level 5 = -25%
* Level 6 = -30%
* Level 7 = -35%
* Level 8 = -40%
* Level 9 = -45%
* Level 10 = -50%

After level 10:

Each additional level grants:
-1% incoming damage

Examples:

* Level 11 = -51%
* Level 12 = -52%
* Level 20 = -60%

Damage reduction is always rounded in the player's favor.

---

# Overcharge

Increases safe enemy hold time before overload.

The upgrade improves:

* overcharge buildup resistance
* overcharge recovery speed
* enemy hold stability before escape attempts

## Scaling

Overcharge buildup and recovery:

* Level 1 = +5%
* Level 2 = +10%
* Level 3 = +15%
* Level 4 = +20%
* Level 5 = +25%
* Level 6 = +30%
* Level 7 = +35%
* Level 8 = +40%
* Level 9 = +45%
* Level 10 = +50%

After level 10:

Each additional level grants:
+1%

Examples:

* Level 11 = +51%
* Level 12 = +52%
* Level 20 = +60%

Enemy hold stability uses a reduced multiplier of the Overcharge bonus to prevent excessively long grabs.

---

# Second Chance

Prevents death and grants temporary invulnerability.

When triggered:

* HP becomes 1
* temporary invulnerability activates
* health UI changes color
* yellow HUD heart icon becomes active
* the effect enters cooldown afterward

Works against:

* lethal enemy damage
* death pits
* void falls

## Invulnerability Duration

* Level 1 = 1 second
* Level 2 = 2 seconds
* Level 3 = 3 seconds
* Level 4 = 4 seconds
* Level 5 = 5 seconds

After level 5:

* duration no longer increases
* additional levels reduce cooldown

## Cooldown Scaling

Base cooldown:
120 seconds

After level 5:

* every level reduces cooldown by 5 seconds

Examples:

* Level 6 = 115 seconds
* Level 7 = 110 seconds
* Level 8 = 105 seconds
* Level 9 = 100 seconds
* Level 10 = 95 seconds

After level 10:

* each additional level reduces cooldown by 0.5 seconds

Examples:

* Level 11 = 94.5 seconds
* Level 12 = 94 seconds
* Level 20 = 90 seconds

---

# Energy Leech

Restores health from direct player-caused damage.

Healing is based on:
ACTUAL enemy HP lost

Only direct player-caused damage counts.

Examples:

* weapon damage
* Shock Grip damage
* tumble launch damage
* collision damage
* smashing enemies into walls
* held-object damage

Invalid damage:

* enemy vs enemy damage
* environmental damage
* uncontrolled physics damage
* death pits
* passive world damage

Healing is only granted to players who actually contributed damage.

Healing is always rounded UP to whole numbers.

Example:

* 100 damage dealt
* 2.5% leech
* restores 3 HP

## Scaling

* Level 1 = 2.5%
* Level 2 = 5%
* Level 3 = 7.5%
* Level 4 = 10%
* Level 5 = 12.5%
* Level 6 = 15%
* Level 7 = 17.5%
* Level 8 = 20%
* Level 9 = 22.5%
* Level 10 = 25%

After level 10:
Each additional level grants:
+1%

Examples:

* Level 11 = 26%
* Level 12 = 27%
* Level 20 = 35%

---

# Shock Grip

Electrocutes stunned airborne enemies while grabbed.

Requirements:

* enemy must be stunned
* enemy must be airborne
* enemy must currently be grabbed

Simple touching does not activate the effect.

The upgrade:

* only affects monsters
* does not damage players

Damage is applied repeatedly while the enemy remains controlled.

## Scaling

* Level 1 = 2 DPS
* Level 2 = 4 DPS
* Level 3 = 6 DPS
* Level 4 = 8 DPS
* Level 5 = 10 DPS
* Level 6 = 12 DPS
* Level 7 = 14 DPS
* Level 8 = 16 DPS
* Level 9 = 18 DPS
* Level 10 = 20 DPS

After level 10:
Each additional level grants:
+1 DPS

Examples:

* Level 11 = 21 DPS
* Level 12 = 22 DPS
* Level 20 = 30 DPS

Shock Grip correctly credits multiplayer damage ownership.

---

# Panic Response

Improves sprint efficiency and grants an emergency movement boost after taking damage.

## Passive Effect

Panic Response passively reduces stamina consumption while sprinting.

## Panic Effect

When the player takes damage, Panic Response can activate a temporary panic state.

While active:

* movement speed is increased
* stamina is restored continuously
* sprinting can be maintained for the duration of the effect

The panic effect has a cooldown before it can activate again.

## Sprint Stamina Scaling

* Level 1 = -5% sprint stamina usage
* Level 2 = -10%
* Level 3 = -15%
* Level 4 = -20%
* Level 5 = -25%
* Level 6 = -30%
* Level 7 = -35%
* Level 8 = -40%
* Level 9 = -45%
* Level 10 = -50%

After level 10:

Each additional level reduces sprint stamina usage by:
+2%

Examples:

* Level 11 = -52%
* Level 12 = -54%
* Level 20 = -70%

## Panic Effect Scaling

The panic effect duration, cooldown, and speed multiplier can be configured in the config file.

---

# Endless Scaling Philosophy

Extra Battle Upgrades is intentionally designed around:

* long multiplayer runs
* escalating shop prices
* diminishing late-game scaling
* preserving R.E.P.O. chaos

Although several upgrades scale endlessly, shop prices increase aggressively and naturally limit excessive progression during normal gameplay.

The goal is:

* rewarding long runs
* enabling fun build experimentation
* preserving balance for standard players
* avoiding instant overpowered gameplay early on
