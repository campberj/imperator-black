# 15. Troop values and building troops

*Draft (Phase 7). The costs are Proposed until they have been tested in play.*

Every troop has two values:

- **Troop Value (TV)** is its worth in a fight. It is used to size forces, balance scenarios and compare troops.
- **Requisition** is its price in a campaign (chapter 14): its TV divided by 10, rounded up. Campaign rules may adjust it (a scarce specialist, a robot repaired rather than replaced).

This chapter works out TV from a troop's profile and lets players build troops of their own. The forces are still built like a table of organisation (chapter 9): TV prices the soldiers in a team, it doesn't replace the team.

## How the values work

The baseline is a human with every attribute at 0, Move 5", Wounds 1, Size 2, no armour and no weapon: **TV 10**. Everything else costs according to how much more likely it makes success.

A test is 2d6 + an attribute, 8+. Averaged over the open, −1 and −2, a 0 succeeds 29% of the time, a 1 43%, a 2 57%, a 3 71% and a 4 82%. Each point adds about half of what a zero-rated soldier achieves, so each attribute value has a **multiplier**:

<!-- table:values.multiplier -->
| Attribute value | 0 | 1 | 2 | 3 | 4 |
|---|---|---|---|---|---|
| Multiplier | 1 | 1.5 | 2 | 2.5 | 3 |
<!-- /table -->

A weapon's cost is its base value times the multiplier of the attribute that uses it: Shoot for ranged weapons, Fight for melee weapons. A carbine in the hands of a Shoot 3 elite costs two and a half times what it costs a Shoot 0 conscript, because it hits two and a half times as often. Gear that rolls an attribute (a hacking rig, a trauma kit, grenades, a grenade launcher) scales the same way.

Armour and Wounds multiply the whole total. A model that survives twice as many hits keeps fighting twice as long, and that is worth more than any one piece of kit.

## Working out a troop's value

1. Start at **10**.
2. Add the attribute costs and the Move cost:

<!-- table:values.attribute_costs -->
| Attribute | Cost |
|---|---|
| Physique | 2 per point |
| Nerve | 2 per point |
| Tech | 1 per point |
| Move | 2 per inch above 5" (minus below it) |
<!-- /table -->

3. Add the ranged weapon's cost, under the troop's Shoot. Price the weapon the troop carries; an option swapped in later is priced the same way.
4. Add the melee weapon's cost, under the troop's Fight. A gear item that replaces the melee weapon (a breaching tool) uses its own melee row.
5. Add the gear, grenades and skills. Skills that come with gear (the Medic skill from a trauma kit) are paid for by the gear, not again.
6. Multiply by the **survival multiplier** for its Armour and Wounds.
7. An **Automated** model multiplies by 1.3 more: it ignores Suppression and morale, though a row of 6 or more destroys it.
8. Round to the nearest whole number: that is its TV. Its Requisition is TV ÷ 10, rounded up.

### Ranged weapons

<!-- table:values.weapons_core -->
| Weapon | Shoot 0 | 1 | 2 | 3 | 4 |
|---|---|---|---|---|---|
| Sidearm | 2.2 | 3.3 | 4.4 | 5.5 | 6.6 |
| SMG | 5.9 | 8.9 | 11.8 | 14.8 | 17.7 |
| Carbine | 4.1 | 6.1 | 8.2 | 10.2 | 12.3 |
| Shotgun | 5.3 | 7.9 | 10.6 | 13.2 | 15.9 |
| Rifle | 4.2 | 6.3 | 8.4 | 10.5 | 12.6 |
| Heavy rifle | 4.8 | 7.2 | 9.6 | 12 | 14.4 |
| LMG | 10.6 | 15.9 | 21.2 | 26.5 | 31.8 |
| Compact LMG | 10.4 | 15.6 | 20.8 | 26 | 31.2 |
| DMR | 5.7 | 8.6 | 11.4 | 14.2 | 17.1 |
| AT launcher | 8.2 | 12.3 | 16.4 | 20.5 | 24.6 |
| Auto rifle | 6.7 | 10.1 | 13.4 | 16.8 | 20.1 |
| HMG | 12.5 | 18.8 | 25 | 31.2 | 37.5 |
| Anti-materiel rifle | 6.6 | 9.9 | 13.2 | 16.5 | 19.8 |
<!-- /table -->

<!-- table:values.weapons_faction -->
| Weapon | Shoot 0 | 1 | 2 | 3 | 4 |
|---|---|---|---|---|---|
| DC-17 hand blaster (Galactic Republic) | 2.2 | 3.3 | 4.4 | 5.5 | 6.6 |
| Twin DC-17s (Galactic Republic) | 3.6 | 5.4 | 7.2 | 9 | 10.8 |
| DC-15S blaster carbine (Galactic Republic) | 4.1 | 6.1 | 8.2 | 10.2 | 12.3 |
| DC-15A blaster rifle (Galactic Republic) | 4.2 | 6.3 | 8.4 | 10.5 | 12.6 |
| DC-15x sniper rifle (Galactic Republic) | 5.7 | 8.6 | 11.4 | 14.2 | 17.1 |
| DC-17m (blaster) (Galactic Republic) | 4.6 | 6.9 | 9.2 | 11.5 | 13.8 |
| DC-17m (sniper) (Galactic Republic) | 5.7 | 8.6 | 11.4 | 14.2 | 17.1 |
| DC-17m (anti-armour) (Galactic Republic) | 5.7 | 8.6 | 11.4 | 14.2 | 17.1 |
| DP-23 phase-pulse blaster (Galactic Republic) | 5.3 | 7.9 | 10.6 | 13.2 | 15.9 |
| Z-6 rotary blaster (Galactic Republic) | 9.5 | 14.2 | 19 | 23.8 | 28.5 |
| RPS-6 rocket launcher (Galactic Republic) | 8.2 | 12.3 | 16.4 | 20.5 | 24.6 |
| M134 minigun (Anchorage) | 13.4 | 20.1 | 26.8 | 33.5 | 40.2 |
| 20mm autocannon (Anchorage) | 12.9 | 19.4 | 25.8 | 32.2 | 38.7 |
| MAAWS recoilless rifle (Anchorage) | 11.5 | 17.2 | 23 | 28.8 | 34.5 |
| CS/LM5 rotary gun (Hankou) | 10.6 | 15.9 | 21.2 | 26.5 | 31.8 |
| Dual DZJ-08 recoilless (Hankou) | 9.2 | 13.8 | 18.4 | 23 | 27.6 |
| E-5 blaster rifle (Separatist Alliance) | 4.2 | 6.3 | 8.4 | 10.5 | 12.6 |
| E-5C heavy blaster rifle (Separatist Alliance) | 6.7 | 10.1 | 13.4 | 16.8 | 20.1 |
| E-5s sniper rifle (Separatist Alliance) | 5.7 | 8.6 | 11.4 | 14.2 | 17.1 |
| E-60R missile launcher (Separatist Alliance) | 8.2 | 12.3 | 16.4 | 20.5 | 24.6 |
| Wrist blasters (Separatist Alliance) | 6.6 | 9.9 | 13.2 | 16.5 | 19.8 |
| ACM heavy blaster (Separatist Alliance) | 10.6 | 15.9 | 21.2 | 26.5 | 31.8 |
| Wrist rocket launcher (Separatist Alliance) | 7.9 | 11.9 | 15.8 | 19.8 | 23.7 |
| Twin blaster cannons (Separatist Alliance) | 9.5 | 14.2 | 19 | 23.8 | 28.5 |
| Heavy blaster cannon (Separatist Alliance) | 6.7 | 10.1 | 13.4 | 16.8 | 20.1 |
| DT-12 heavy blaster pistol (Separatist Alliance) | 3.7 | 5.6 | 7.4 | 9.2 | 11.1 |
<!-- /table -->

A weapon's base value is the average wound row it deals on a hit against Armour 0, 1 and 2. That is then adjusted for range, Auto N (each target after the first counts 0.6), Explosion (X), and keywords: Handy and Suppressing add to it, while Cumbersome and One per round take away. A launcher with a choice of rounds uses its best round, plus 10% for the choice.

### Melee weapons

<!-- table:values.melee -->
| Melee weapon | Fight 0 | 1 | 2 | 3 | 4 |
|---|---|---|---|---|---|
| Unarmed / weapon butt | 1 | 1.5 | 2 | 2.5 | 3 |
| Knife or bayonet | 2.1 | 3.2 | 4.2 | 5.2 | 6.3 |
| Breaching tool or heavy blade | 2.6 | 3.9 | 5.2 | 6.5 | 7.8 |
| Boarding axe or combat blade | 2.9 | 4.3 | 5.8 | 7.2 | 8.7 |
| Lightsaber | 3.5 | 5.2 | 7 | 8.8 | 10.5 |
| Electrostaff | 2.9 | 4.3 | 5.8 | 7.2 | 8.7 |
| Vibrosword | 2.9 | 4.3 | 5.8 | 7.2 | 8.7 |
<!-- /table -->

### Gear, grenades and skills

<!-- table:values.gear -->
| Item | Cost |
|---|---|
| Breaching charge | 3 |
| Breaching tool | 1 |
| Trauma kit | 2 × Tech multiplier |
| Hacking rig | 2 × Tech multiplier |
| Low-light optics | 1 |
| Extinguisher | 1 |
| Silencer | 2 |
| Jump pack | 3 |
| Grenade launcher | 3 × Shoot multiplier |
| Ballistic shield | 4 |
| Jammer | 4 |
| Mk19 grenade launcher | 5 × Shoot multiplier |
| Mobility and one-way attack package | 4 |
| Flux warhead | 4 |
| Cooperative engagement package | 5 |
| Deflector shield | 4 |
<!-- /table -->

<!-- table:values.grenades -->
| Item | Cost |
|---|---|
| Frag grenade | 1.5 × Physique multiplier |
| Flashbang | 1.5 × Physique multiplier |
| Smoke grenade | 1 |
| Thermal detonator | 3 × Physique multiplier |
<!-- /table -->

<!-- table:values.skills -->
| Skill | Cost |
|---|---|
| Leader | 6 per level |
| Void-trained | 1 per level |
| Low-light | 1 per level |
| Medic | 2 per level × Tech multiplier |
| Marksman | 2 per level × Shoot multiplier |
| Deflect | 8 per level × Fight multiplier |
| Leap | 3 per level |
| Jedi discipline | 6 per level |
| Twin blades | 2 per level × Fight multiplier |
| Scout | 3 per level |
| Shield | 4 per level |
| Layered armour | 3 per level |
| Laser detector | 3 per level |
| Augmented | 4 per level |
| Self-destruct | 3 per level |
| Rolling | 3 per level |
| Battle computer | 5 per level |
<!-- /table -->

### Survival multiplier

From the average number of hits (2d8, AP 0) it takes to put the model Down, compared with an unarmoured, one-Wound human, to the power 0.4.

<!-- table:values.survival -->
|  | Wounds 1 | Wounds 2 | Wounds 3 |
|---|---|---|---|
| Armour 0 | 1.00 | 1.06 | 1.09 |
| Armour 1 | 1.14 | 1.25 | 1.32 |
| Armour 2 | 1.40 | 1.58 | 1.70 |
| Armour 3 | 1.86 | 2.15 | 2.39 |
<!-- /table -->

### Example

A Task Force Boone VAT Assaulter has Shoot 2, Fight 2, Physique 1, Nerve 1, Tech 0, Move 6", Wounds 1, void plate (Armour 2), a carbine, a knife, a flashbang and a frag grenade, a jump pack and low-light optics, and Void-trained.

- Baseline **10**; Physique 1 and Nerve 1: **+4**; Move 6": **+2**.
- Carbine at Shoot 2: **+8.2**. Knife at Fight 2: **+4.2**.
- Flashbang and frag at Physique 1: **+4.5**; jump pack **+3**; low-light optics **+1**; Void-trained **+1**.
- Total 37.9, times 1.40 (Armour 2, Wounds 1): **TV 53**, **Requisition 6**.

## Building a troop

A custom troop is built from the baseline, within these limits, then priced as above.

1. **Class sets Shoot** (the Shoot scale, chapter 1): irregulars and conscripts 0, trained military 1, professionals 2, elite units 3, legendary heroes 4 or more.
2. **Other attributes:** Fight, Physique, Nerve and Tech may each be at most the class's Shoot + 1, and never above 4 (Physique and Tech at most 3, unless the troop is Augmented or Automated).
3. **Move:** 5" to 7" for humans; up to 8" for the Augmented or very lightly equipped; Automated models by their chassis.
4. **Wounds:** 1 for humans. Legendary heroes and Augmented elites may have 2. Automated models: 1 for Size 1, 2 for Size 2, 3 for Size 3.
5. **Rank** follows the class and experience (chapter 1). It is a label; it costs nothing.
6. **Kit:** any armour, weapons, gear and grenades from the core lists. A faction's own kit (marked with its faction in chapter 9) only for troops of that faction.
7. **Skills:** any from the list. One Leader per team.
8. **Automated** models have no Nerve or Tech (dashes) and the Automated skill.

**Extending it.** Robots and drones on the table are Automated troops and are priced like any other troop. Vehicles and walkers will add their own survival table (hull, facings) when the vehicle rules are written, and use the same weapon costs through their crew's Shoot. Off-table assets (strikes, surveillance, air defence) are priced as campaign support, not TV (see `docs/design/air-and-void.md`).

## Troop values

Every profile in chapter 9, with its default loadout.

### Generic forces

<!-- table:values.tv_generic -->
| Troop | TV | Requisition |
|---|---|---|
| **Hostage** | 11 | 2 |
| **Officer** | 45 | 5 |
| **Line Trooper** | 32 | 4 |
| **Support Gunner** | 43 | 5 |
| **Anti-armour Trooper** | 39 | 4 |
| **Specialist** | 31 | 4 |
| **Marksman** | 43 | 5 |
| **Assault Trooper** | 56 | 6 |
| **Heavy Infantry Sergeant** | 125 | 13 |
| **Heavy Infantry** | 110 | 11 |
<!-- /table -->

### Task Force Boone

<!-- table:values.tv_tfb -->
| Troop | TV | Requisition |
|---|---|---|
| **VAT Team Leader** | 66 | 7 |
| **VAT Assaulter** | 53 | 6 |
| **VAT Gunner** | 71 | 8 |
| **Data Specialist** | 60 | 6 |
| **Pathfinder Team Leader** | 51 | 6 |
| **Pathfinder** | 42 | 5 |
| **Pathfinder SAW Gunner** | 55 | 6 |
| **Control Team Marksman** | 48 | 5 |
<!-- /table -->

### Galactic Republic

<!-- table:values.tv_republic -->
| Troop | TV | Requisition |
|---|---|---|
| **Clone Sergeant** | 48 | 5 |
| **Clone Trooper** | 39 | 4 |
| **Clone Heavy Trooper** | 46 | 5 |
| **Clone Trooper Marksman** | 47 | 5 |
| **ARC Trooper Leader** | 64 | 7 |
| **ARC Trooper** | 52 | 6 |
| **ARF Trooper** | 43 | 5 |
| **Clone Commando Leader** | 88 | 9 |
| **Clone Commando** | 79 | 8 |
| **Commander Cody** | 62 | 7 |
| **Obi-Wan Kenobi** | 110 | 11 |
| **Ahsoka Tano** | 80 | 8 |
| **Jedi Guardian** | 67 | 7 |
<!-- /table -->

### Anchorage

<!-- table:values.tv_anchorage -->
| Troop | TV | Requisition |
|---|---|---|
| **Prime** | 79 | 8 |
| **Scout** | 66 | 7 |
| **Grenadier** | 60 | 6 |
| **Systems Operator** | 54 | 6 |
| **Hound CCT** | 144 | 15 |
<!-- /table -->

### Hankou

<!-- table:values.tv_hankou -->
| Troop | TV | Requisition |
|---|---|---|
| **Team Leader** | 76 | 8 |
| **Rifleman** | 61 | 7 |
| **Automatic Rifleman** | 69 | 7 |
| **Pioneer** | 70 | 7 |
| **ZQ44 CCT** | 84 | 9 |
<!-- /table -->

### Freespace Solutions

<!-- table:values.tv_freespace -->
| Troop | TV | Requisition |
|---|---|---|
| **Salvage Hand** | 19 | 2 |
| **Asset Security Officer** | 31 | 4 |
| **Asset Security Gunner** | 39 | 4 |
| **Collections Enforcer** | 36 | 4 |
| **Repossession Breacher** | 63 | 7 |
| **Liquidator** | 59 | 6 |
| **Salvage Tech** | 30 | 3 |
| **Recovery Captain** | 61 | 7 |
| **Sentry Turret** | 29 | 3 |
| **Security Drone** | 37 | 4 |
<!-- /table -->

### Contractors

<!-- table:values.tv_contractor -->
| Troop | TV | Requisition |
|---|---|---|
| **Contract Boarder** | 64 | 7 |
| **Contract Heavy** | 112 | 12 |
| **Reaction Force Operator** | 49 | 5 |
| **Reaction Force Gunner** | 66 | 7 |
| **Hired Marksman** | 55 | 6 |
| **Hired Heavy Gunner** | 76 | 8 |
| **Borz Bratva Trooper** | 60 | 6 |
| **Borz Bratva HMG Gunner** | 72 | 8 |
| **Borz Cyka Powered Trooper** | 116 | 12 |
| **Borz Cyka Bravo Breacher** | 116 | 12 |
| **Ubiytsa Controller** | 59 | 6 |
| **Ubiytsa Shock Trooper** | 55 | 6 |
| **Reaction Force Controller** | 68 | 7 |
| **Reaction Force Grenadier** | 57 | 6 |
| **Reaction Force Engineer** | 55 | 6 |
| **Harlow Veteran** | 53 | 6 |
| **Springbok AI** | 70 | 7 |
| **Cricket Drone** | 23 | 3 |
<!-- /table -->

### UN Force Command

<!-- table:values.tv_un -->
| Troop | TV | Requisition |
|---|---|---|
| **Battalion Controller** | 61 | 7 |
| **3rd Battalion Rifleman** | 49 | 5 |
| **3rd Battalion Data Specialist** | 61 | 7 |
| **3rd Battalion AT Specialist** | 60 | 6 |
| **Pointman** | 61 | 7 |
| **Pointman SAW** | 88 | 9 |
| **UTG Mother** | 115 | 12 |
| **UTG Assaulter** | 82 | 9 |
| **UTG Grenadier** | 93 | 10 |
| **UTG Marksman** | 93 | 10 |
| **Golem** | 64 | 7 |
<!-- /table -->

### Meridian Directorate

<!-- table:values.tv_directorate -->
| Troop | TV | Requisition |
|---|---|---|
| **Directorate Handler** | 102 | 11 |
| **Directorate Deniable Operator** | 68 | 7 |
<!-- /table -->

### Separatist Alliance

<!-- table:values.tv_separatist -->
| Troop | TV | Requisition |
|---|---|---|
| **B1 Battle Droid** | 22 | 3 |
| **B1 Heavy Weapons Droid** | 26 | 3 |
| **B2 Super Battle Droid** | 46 | 5 |
| **B2 Heavy Weapons Droid** | 57 | 6 |
| **Droideka** | 68 | 7 |
| **BX Commando Droid** | 57 | 6 |
| **IG-100 MagnaGuard** | 52 | 6 |
| **T-series Tactical Droid** | 35 | 4 |
| **Kraken** | 63 | 7 |
| **General Grievous** | 142 | 15 |
| **DSD1 Dwarf Spider Droid** | 90 | 9 |
<!-- /table -->

The ranking was checked against the simulator's duel scores (`docs/analysis/troop-value-calibration.md`): rank correlation 0.87 across the 73 profiles in the simulator report. The largest differences are where the duel can't see a troop's worth: Leaders (command), gunners and launchers (several targets), and pistol-armed troops, who are weak at duel range by design.
