# 9. Forces and equipment

## Reading a profile

| Entry | Meaning |
|---|---|
| **Q** | Quality, 0–3 (chapter 1). |
| **Armour** | Armour type, with its value in brackets (chapter 4). |
| **Move** | 6", or 5" in Heavy armour. A dash means it cannot move. |
| **Weapon** | Ranged weapon. Alternatives after a slash may be swapped in when the roster is built. |
| **Kit** | Melee weapon, grenades (two per model), and gear. "Option" gear is chosen per model when the roster is built. |
| **Keywords** | Model traits (table below). Gear can also grant keywords. |

Every model carries a melee weapon. A model with no melee weapon listed (only *Automated* models) cannot start close combat and always loses an exchange it is forced into.

## Building a force

Forces are built from **teams**, like a table of organisation, not from a points list.

- A **team** has fixed role slots. Fill each slot with its troop type and choose any weapon and gear options.
- The proof-of-concept campaign starts with the **Boarding element**: one VAT fireteam plus one attached Pathfinder or Marksman, six models in all. One VAT Assaulter usually carries a breaching charge and the other a trauma kit.
- How the force grows, through requisition, replacements, promotions and new teams, is part of the campaign (Phase 7).

<!-- table:rosters.teams -->
| Team | Models | Role |
|---|---|---|
| Void Assault Team (VAT) fireteam | 1× VAT Team Leader, 2× VAT Assaulter, 1× VAT Gunner, 1× Data Specialist | The boarding fireteam: assault, a close-quarters gunner, and a data specialist. |
| Pathfinder recon team | 1× Pathfinder Team Leader, 2× Pathfinder, 1× Pathfinder SAW Gunner | Recon and fire support with armour-piercing heavy rifles and a SAW. |
| Control team | 2× Control Team Marksman | Two marksmen who hold overwatch over long passages, hangars and open decks. |
<!-- /table -->

## Task Force Boone

<!-- table:troops.tfb -->
| Troop | Role | Q | Armour | Move | Weapon | Kit | Keywords |
|---|---|---|---|---|---|---|---|
| **VAT Team Leader** | Leader | 2 | Void plate (2) | 6" | Carbine / Shotgun | Knife or bayonet, Flashbang, Frag grenade, Jump pack, Low-light optics | Leader, Void-trained |
| **VAT Assaulter** | Assaulter | 1 | Void plate (2) | 6" | Carbine / Shotgun | Knife or bayonet, Flashbang, Frag grenade, Jump pack, Low-light optics; option: Breaching charge or Breaching tool or Trauma kit | Void-trained |
| **VAT Gunner** | Gunner | 1 | Void plate (2) | 6" | Compact LMG | Knife or bayonet, Frag grenade, Smoke grenade, Jump pack, Low-light optics | Void-trained |
| **Data Specialist** | Specialist | 1 | Void plate (2) | 6" | Carbine | Knife or bayonet, Flashbang, Smoke grenade, Jump pack, Low-light optics, Jammer, Hacking rig | Void-trained |
| **Pathfinder Team Leader** | Leader | 2 | Void suit (1) | 6" | Heavy rifle | Knife or bayonet, Frag grenade, Smoke grenade, Low-light optics | Leader, Void-trained |
| **Pathfinder** | Rifleman | 1 | Void suit (1) | 6" | Heavy rifle | Knife or bayonet, Frag grenade, Smoke grenade, Low-light optics; option: Trauma kit or Breaching charge | Void-trained |
| **Pathfinder SAW Gunner** | Gunner | 1 | Void suit (1) | 6" | LMG | Knife or bayonet, Frag grenade, Smoke grenade, Low-light optics | Void-trained |
| **Control Team Marksman** | Marksman | 2 | Void suit (1) | 6" | DMR | Knife or bayonet, Smoke grenade, Frag grenade, Low-light optics | Void-trained |
<!-- /table -->

## Gear

Gear marked **Proposed** adds a rule that is new in this version; it is in play for testing.

<!-- table:gear.gear -->
| Gear | Effect | Status |
|---|---|---|
| Breaching charge | One charge. Place with Interact on a door, hatch or thin bulkhead within 1"; detonate later as an action (chapter 6). | — |
| Breaching tool | Force a locked door: Interact, test 8+ (chapter 3). Also counts as a breaching tool or heavy blade in close combat. | — |
| Trauma kit | The carrier has the Medic keyword. | — |
| Hacking rig | The carrier has the Tech keyword and may Hack locked doors and consoles (Interact, test 9+). | — |
| Low-light optics | The carrier has the Low-light keyword: it ignores the darkness penalty. | — |
| Extinguisher | May Interact with a fire cell in contact: test 8+ to put it out (chapter 8). | — |
| Silencer | The carrier's ranged weapon gains the Silenced keyword: its shots are not noise for the alert track. | — |
| Jump pack | Move ignores the extra cost of obstacles and may cross gaps up to half the model's Move. In zero-g, the model may change direction freely during a Move and stop anywhere, even away from a surface (it still counts as unbraced). | **Proposed** |
| Jammer | Enemy Automated models within 6" take -2 to hit, and enemy Hack tests within 6" take -2. | **Proposed** |
<!-- /table -->

## Model keywords

<!-- table:keywords.model -->
| Keyword | Effect |
|---|---|
| *Void-trained* | Ignores the unbraced zero-g penalty; may push off and stop at any surface. |
| *Low-light* | Ignores the darkness penalty. |
| *Tech* | May Hack locked doors and consoles (Interact test 9+). |
| *Medic* | +2 to Medic actions; automatically recovers a row-6 Down model. |
| *Automated* | Never gains Suppression, never tests for breaking, and is not affected by casualty shock or Rally. A wound row of 6 or more destroys it. |
| *Leader* | A Rally made by a Leader, or on a model within 6" and in line of sight of a Leader, removes 2 Suppression instead of 1. The side's highest-Q Leader adds its Q to initiative. |
<!-- /table -->

## Opposition: Freespace Solutions

Freespace Solutions is a privatized debt and salvage corporation that unofficially conducts grey-zone operations for paying patrons; its current patron is the Meridian Directorate. See [the opposition force](../docs/design/opposition-force.md) for its background.

Missions build the enemy force from **elements**, chosen by threat tier. Behavior profiles tell you how each element fights (Phase 3).

<!-- table:factions.threat_tiers -->
| Tier | Name | Draws on |
|---|---|---|
| 1 | Low | Crew and asset security under a recovery captain. |
| 2 | Medium | Adds collections enforcers, salvage techs and automated defences. |
| 3 | High | Adds repossession teams, liquidators and hired guns; a contractor reaction force answers high alert. |
| 4 | Story | Adds Meridian Directorate handlers or deniable operators. |
<!-- /table -->

<!-- table:factions.elements -->
| Element | Faction | Tier | Models | Behavior |
|---|---|---|---|---|
| Salvage crew | Freespace Solutions | 1 | 3× Salvage Hand | Cowed |
| Asset security | Freespace Solutions | 1 | 2× Asset Security Officer, 1× Asset Security Gunner | Sentry |
| Recovery captain | Freespace Solutions | 1 | 1× Recovery Captain | Leader |
| Collections enforcers | Freespace Solutions | 2 | 3× Collections Enforcer | Aggressive |
| Salvage techs | Freespace Solutions | 2 | 2× Salvage Tech | Saboteur |
| Automated defences | Freespace Solutions | 2 | 1× Sentry Turret (or Security Drone) | Automated |
| Repossession team | Freespace Solutions | 3 | 3× Repossession Breacher | Aggressive |
| Liquidators | Freespace Solutions | 3 | 2× Liquidator | Tactical |
| Hired gun | Contractors | 3 | 1× Hired Marksman (or Hired Heavy Gunner) | Attached |
| Contract boarding team | Contractors | 3 | 3× Contract Boarder, 1× Contract Heavy | Tactical — reaction force |
| Contract reaction force | Contractors | 3 | 3× Reaction Force Operator, 1× Reaction Force Gunner | Aggressive — reaction force |
| Directorate handler | Meridian Directorate | 4 | 1× Directorate Handler | Handler |
| Directorate deniable operators | Meridian Directorate | 4 | 3× Directorate Deniable Operator | Tactical |
<!-- /table -->

### Freespace troops

<!-- table:troops.freespace -->
| Troop | Role | Q | Armour | Move | Weapon | Kit | Keywords |
|---|---|---|---|---|---|---|---|
| **Salvage Hand** | Crew | 0 | None (0) | 6" | Sidearm / SMG | Unarmed / weapon butt, Extinguisher | — |
| **Asset Security Officer** | Guard | 1 | Flak (1) | 6" | Carbine / Shotgun | Knife or bayonet, Flashbang | — |
| **Asset Security Gunner** | Gunner | 1 | Flak (1) | 6" | LMG | Knife or bayonet | — |
| **Collections Enforcer** | Enforcer | 1 | Flak (1) / Composite plate (2) | 6" | Rifle / SMG / Shotgun | Knife or bayonet, Frag grenade | — |
| **Repossession Breacher** | Breacher | 1 | Composite plate (2) | 6" | Shotgun / SMG | Breaching tool or heavy blade, Flashbang, Frag grenade, Breaching charge | — |
| **Liquidator** | Hitter | 2 | Composite plate (2) | 6" | SMG / Rifle | Knife or bayonet, Frag grenade, Flashbang, Silencer, Low-light optics | — |
| **Salvage Tech** | Specialist | 1 | Flak (1) | 6" | Sidearm | Knife or bayonet, Smoke grenade, Hacking rig, Breaching charge | — |
| **Recovery Captain** | Leader | 2 | Composite plate (2) | 6" | SMG / Rifle | Knife or bayonet, Frag grenade, Low-light optics | Leader |
| **Sentry Turret** | Automated | 1 | Composite plate (2) | — | LMG | — | Automated |
| **Security Drone** | Automated | 1 | Flak (1) | 6" | SMG | Low-light optics | Automated, Void-trained |
<!-- /table -->

### Contractors

<!-- table:troops.contractor -->
| Troop | Role | Q | Armour | Move | Weapon | Kit | Keywords |
|---|---|---|---|---|---|---|---|
| **Contract Boarder** | Assaulter | 2 | Void plate (2) | 6" | Carbine / Shotgun | Knife or bayonet, Flashbang, Frag grenade, Breaching charge | Void-trained |
| **Contract Heavy** | Gunner | 2 | Heavy plate (3) | 5" | LMG | Knife or bayonet, Frag grenade | Void-trained |
| **Reaction Force Operator** | Rifleman | 2 | Composite plate (2) | 6" | Rifle / Carbine | Knife or bayonet, Frag grenade, Smoke grenade | — |
| **Reaction Force Gunner** | Gunner | 2 | Composite plate (2) | 6" | LMG | Knife or bayonet, Frag grenade | — |
| **Hired Marksman** | Marksman | 2 | Composite plate (2) | 6" | DMR | Knife or bayonet, Smoke grenade, Low-light optics | — |
| **Hired Heavy Gunner** | Gunner | 2 | Composite plate (2) | 6" | LMG | Knife or bayonet, Frag grenade | — |
<!-- /table -->

### Meridian Directorate

<!-- table:troops.directorate -->
| Troop | Role | Q | Armour | Move | Weapon | Kit | Keywords |
|---|---|---|---|---|---|---|---|
| **Directorate Handler** | Leader | 3 | Void plate (2) | 6" | Carbine | Knife or bayonet, Flashbang, Frag grenade, Silencer, Low-light optics, Hacking rig | Leader, Void-trained |
| **Directorate Deniable Operator** | Assaulter | 2 | Composite plate (2) | 6" | SMG | Breaching tool or heavy blade, Flashbang, Frag grenade, Silencer, Low-light optics | — |
<!-- /table -->

Directorate handlers' independent action, and the hired guns' signature actions, are still to be written.
