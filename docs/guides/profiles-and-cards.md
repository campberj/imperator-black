# Guide: creating unit profiles, weapons and cards

**For:** any agent (or person) adding miniatures to Imperator Black: new troop profiles, weapons, armour, gear, skills, teams and enemy elements, and printing their cards.
**Read first:** [`CLAUDE.md`](../../CLAUDE.md), [`content/README.md`](../../content/README.md) (the file schema), rules chapter 9 (forces) and chapter 15 (troop values).

The short version: **everything is data in `content/*.yaml`**. Never write a profile, weapon or card by hand anywhere else: not in a rules chapter, not in a PDF, not in the app. The tools turn the data into the rulebook tables, the cards, the troop values and the app's database, and they all stay in step only if the data is the one source.

## 1. The workflow

1. Decide what the miniatures are (section 2) and what they carry.
2. Reuse what exists: check `content/weapons.yaml`, `armour.yaml`, `gear.yaml` and `skills.yaml` before adding anything new (section 4).
3. Add the faction (if new), the troops, and their teams or elements (sections 3 and 5).
4. Run the checks and regenerate everything:

```sh
python3 tools/validate_content.py        # ids exist and are unique; dice, rounds, costs, tables all legal
python3 tools/build_content.py           # content/dist/content.json for the app
python3 tools/render_tables.py           # tables in rules/*.md
python3 tools/troop_values.py --reference  # TV of every troop; refreshes the C# test fixture
python3 tools/print_sheets.py            # every card PDF in docs/print/
```

5. Add the troop table to chapter 9 (section 6), and a decision row to `docs/design/decisions.md` (section 8).
6. Check the cards by eye (section 7), then commit. CI fails if the JSON, the rulebook tables or the troop value fixture are stale, or if validation fails.

If the C# tests count troops, teams or gear (`app/tests/Imperator.Core.Tests/ContentLoadingTests.cs`, `CountsMatchPythonValidator`), update the counts to match the validator's summary line.

## 2. Statting a miniature

Profiles are granular: each attribute is its own number, not one quality value. Work from what the model *is* and *carries*. Pillar 2 asks for "under ten minutes with no special rules", so prefer existing kit and skills.

### Attributes

| Attribute | Meaning | Typical |
|---|---|---|
| **Move** | Inches per Move action | 5" heavy kit, 6" most troops, 7" light or scouts, 8" augmented or very fast |
| **Shoot** | Set by the **Shoot scale** (chapter 1) | 0 irregulars, conscripts, crude droids; 1 trained military; 2 professionals; 3 elite; 4 or more legendary |
| **Fight** | Close combat | 0–1 most; 2 assault troops; 3 specialists; 4 legendary duellists |
| **Physique** | Recovery, climbing, falls, throwing | 0–2; 3 only for the augmented or huge |
| **Nerve** | Breaking tests, Find Cover, a Leader's initiative | 1 most; 2 veterans; 3 elite and leaders |
| **Tech** | Hacking, Medic, technical tasks | 0 most; 1–2 specialists |
| **Wounds** | Wounded markers before Down | 1 humans; 2 legendary heroes, augmented elites, man-sized robots; 3 large robots |
| **Size** | 1 small drone, 2 human, 3 large; walkers and vehicles larger | |

Limits for a built troop (chapter 15): Fight, Physique, Nerve and Tech at most Shoot + 1, never above 4 (Physique and Tech 3 unless Augmented or Automated). A named character converted from its source (a Jedi, General Grievous) may break the Shoot + 1 limit where the source clearly does; say so in the chapter 9 note.

**Rank** (`green`, `regular`, `veteran`, `elite`) is a label. It never enters a roll, but enemy elements use their lowest rank for their drill (chapter 10), so set it honestly.

### Robots, droids and drones

- Give them the **Automated** skill (`automated: 1`) and set `nerve: null` and `tech: null`: Automated models never test those, and the cards print a dash.
- Automated models ignore Suppression and morale but are destroyed on a wound row of 6 or more, so don't also give them high Wounds unless they are big (Size 3).
- A cheap mass-produced droid is Shoot 0 or 1; a combat robot with fire control is Shoot 2.
- Self-destructing drones use **Self-destruct** (`self_destruct: 1`, a 2" explosion; level 2 is 3").

### Converting from another game

Keep a short note of the method in the chapter 9 section and in the decision row (see the BLKOUT, Star Wars: Legion and Near Present notes in chapter 9 for examples):

- Map their shooting quality to the Shoot scale, not their numbers one-for-one.
- Map their movement to ours with a fixed rule (Near Present: their range less 2").
- Map their armour or save to Armour 0–3 (3 is powered or heavy plate).
- Turn their special rules into existing skills, gear or weapon keywords where you can. Don't carry over their army rules, command abilities or cards.
- No occult, demonic or supernatural abilities, ever. Space-wizard powers become trained abilities (see the Jedi in chapter 9) or are left out.

## 3. Troop entries

In `content/troops.yaml`, grouped by faction under a comment header. Schema: [`content/README.md`](../../content/README.md).

```yaml
  - id: sep_b1_droid             # snake_case, faction prefix, never changed once used
    faction: separatist          # an id from factions.yaml
    name: B1 Battle Droid        # what the card shows
    role: Rifleman               # descriptive
    class: trooper               # leader, trooper, specialist or character (chapter 9): who it is in a fireteam
    rank: green                  # a label (drill for enemies)
    stats: { move: 5, shoot: 0, fight: 0, physique: 1, nerve: null, tech: null, wounds: 1, size: 2 }
    armour: none                 # an id from armour.yaml (armour_options: [...] for alternatives)
    weapon: e5_blaster           # a ranged weapon id, or null (then it must have a melee weapon: it cannot Shoot)
    weapon_options: [e5_heavy]   # alternatives printed as "or ..." on the card
    melee: unarmed               # a melee weapon id (every model has one; unarmed is fine)
    grenades: []                 # up to two thrown ids
    gear: []                     # gear ids carried
    gear_options: []             # one choice per model, printed as "Choose one"
    skills: { automated: 1 }     # skill id: level
    miniatures: "Star Wars: Legion B1 Battle Droids"   # printed in italics at the foot of the card
```

- **Default loadout:** `weapon`, `armour` and `gear` are what the model carries by default; its Troop Value is worked out for that loadout. Options are alternatives the player picks per model.
- **Gear that grants a skill** (a trauma kit gives Medic): don't also list the skill.
- **Proposed rules:** a new skill or gear item starts `status: proposed`; the card shows "[proposed]" beside it.

## 4. Weapons, armour, gear and skills

### Ranged weapons (`weapons.yaml`, `ranged`)

```yaml
  - { id: e5_blaster, faction: separatist, name: E-5 blaster rifle, damage: 2d8, short_range: 24, ap: 0, keywords: [] }
```

| Field | Rules |
|---|---|
| `damage` | A matched pair `2d4`, `2d6`, `2d8`, `2d10`, `2d12`, or a half-step `d6+d8`, `d8+d10`, `d10+d12` (chapter 1). Nothing else; no flat bonuses. Pistols 2d6, carbines and rifles 2d8, heavy rifles and DMRs 2d8–2d10, shotguns, anti-armour and anti-materiel 2d12. |
| `short_range` | Inches before the −1 penalty (and −2 beyond double). Pistols 8, SMGs 12, carbines 18, rifles 24–30, marksman rifles 40–48, launchers 30–36. |
| `range_penalty` | Optional `[beyond, double]` for weapons that fall off harder (shotguns `[-2, -3]`). |
| `ap` | 0 most; 1 heavy rifles, AT; 2 anti-materiel, autocannon, Lightsaber-class melee. |
| `auto` | The N in *Auto N* (with the `auto` keyword): 2 SMGs and auto rifles; 3 LMGs and rotary guns; 4 miniguns. |
| `explosion` | The X in *Explosion (X)*, a radius in inches, for weapons that always explode. |
| `round`, `rounds` | Launchers with a choice of rounds: `round` names the weapon's own round (e.g. `Anti-armour`); `rounds` lists the others: `[{ id: he, name: High explosive, damage: d8+d10, ap: 0, explosion: 2 }]`. |
| `keywords` | From `keywords.yaml`: `handy`, `auto`, `suppressing`, `one_per_round`, `cumbersome`, `explosion`, `confined_bonus`, `breach`, `silenced`. |
| `faction` | Set it for kit only that faction uses (it is then listed with that faction in chapter 9, not in the core weapon tables). Leave it off for generic kit any force may carry. |

**Prefer core weapons.** A clone's rifle and a droid's rifle can both be a 2d8, 24–30" rifle; give the faction its own named weapon only when it differs in numbers, or the name matters to the players (as with the Republic's blasters). Check the cost in chapter 15: a new weapon's base value comes out of the formula automatically.

### Melee and thrown

- `melee`: `{ id, name, damage, ap }`. Unarmed 2d4, knife 2d8, heavy blade 2d10, combat blade 2d10 AP 1, lightsaber 2d12 AP 2.
- `thrown`: `{ id, name, throw_range, blast_radius, damage, ap, keywords: [explosion, confined_bonus] }` for damaging grenades, or `effect` instead of damage for flashbangs and smoke. `blast_radius` is its Explosion (X).

### Armour, gear and skills

- **Armour** (`armour.yaml`): value 0–3 and keywords (`sealed`, `heavy`). Reuse: none 0, flak 1, void suit 1 (sealed), composite 2, void plate 2 (sealed), heavy 3 (sealed, heavy). Add a faction armour only if it needs its own name.
- **Gear** (`gear.yaml`): `{ id, name, text, grants_skills, status }`. Rules text is plain second-person-free prose, British spelling, inches written `\"`.
- **Skills** (`skills.yaml`): `{ id, name, status, levels: [...] }`; each level's text adds to the levels below it.
- **Every new gear item, grenade and skill needs a cost** in `content/values.yaml` (`gear`, `grenades`, `skills`); the validator fails without one. Cost like the existing entries: a flat number, or `per: shoot` (or another attribute) when it rolls that attribute. Skills that are worth more in better hands scale too (`marksman: { cost: 2, per: shoot }`).

## 5. Factions, teams and elements

- **Faction** (`factions.yaml`, `factions`): `{ id, name, side: player | enemy, summary }`. Add its card colour to `FACTION_COLOUR` in `tools/print_sheets.py` (a darkish hex colour that white text reads on).
- **Classes:** give every troop a `class`. Leaders lead a fireteam; troopers are its core; specialists have a job (gunners, medics, data specialists, engineers, drone operators, marksmen, anti-armour, robots and drones: the army decides); characters (named characters, Jedi, commanders, hired guns) join a fireteam or act alone.
- **Standard fireteams** come in two lists, one per use, and each has a `type` (`line`, `assault` or `support`, from `rosters.yaml` `fireteam_types`; or `character` for a character alone). The validator checks each fits its type: how many leaders, troopers and specialists, the most models, at most one character.
- **Teams** (`rosters.yaml`, `teams`) for player factions: `{ id, name, faction, type, summary, slots: [{ troop, count }] }`. Build from the miniatures' real organisation (a squad of a leader, riflemen and a gunner).
- **Elements** (`factions.yaml`, `elements`) for enemy factions, so missions and the solo opponent can use them: `{ id, name, faction, type, tier, behavior, composition: [{ troop, count }] }`. An `alternatives` troop replaces one model of its class. `behavior` is one of the profiles in `behaviors.yaml`: `sentry`, `tactical`, `aggressive`, `cowed`, `saboteur`, `leader`, `automated`, `attached`, `handler`. `tier` is the threat tier 1–4 it belongs to.
- A faction used both ways (a player force that can also be fought) can have both teams and elements.

## 6. Rules chapter and print

- **Chapter 9:** add a section for a new faction with a short paragraph (what it is, the Shoot scale, any conversion notes), then the generated tables:

```markdown
<!-- table:troops.<faction> -->
<!-- /table -->

<!-- table:kit.<faction> -->       (only if it has faction weapons or armour)
<!-- /table -->
```

  Run `render_tables.py` to fill them. Never edit between the markers by hand.
- **Chapter 15:** add `<!-- table:values.tv_<faction> -->` / `<!-- /table -->` under "Troop values" for its TV table.
- **Cards:** `tools/print_sheets.py` prints cards by faction. Opposition factions go into `ImperatorBlack_unit-cards.pdf` (the `unit_cards()` default list); player factions with their own sheet get a line in `__main__`, for example `unit_cards(("separatist",), "separatist-cards.pdf")`. Every PDF is named `ImperatorBlack_<name>.pdf`; the tool adds the prefix.
- **Scenario and mission cards** come from the same profiles through `tools/print_scenario.py`; no extra work.

## 7. The card layout

Cards are **3.5" × 5"**, four to a US Letter page with cut marks, drawn by `draw_card()` and `card_flowables()` in `tools/print_sheets.py`. Don't lay cards out by hand; change the content, or the drawing code if the layout itself must change. From top to bottom:

| Part | Content |
|---|---|
| **Header band** | Faction colour. The troop's `name` (bold), then "Rank Role · Faction name". Scenario cards prefix model numbers ("1, 2. Asset Security Officer"). |
| **Stat line** | Boxes for MOV, SHT, FGT, PHY, NRV, TEC, W, SZ and ARM (shaded). A dash for a `null` attribute. Below it, the armour name and any armour options. |
| **Weapon table** | Columns Weapon, Dmg, Short, AP. The default weapon in bold; each option in italics as "or …". Under each weapon, a small line with its keywords (Auto N, Explosion (X)) and any special range penalty. Launcher rounds follow as "Anti-armour above, or high explosive round" with their own line. Then the melee weapon "(close combat)". A model with no ranged weapon shows "No ranged weapon: cannot Shoot". |
| **Grenades** | One line. |
| **Gear** | Each gear item with its rules text; "[proposed]" if new. Gear options as "Choose one: A / B □ □ □". On Automated models, every option's text is printed in full (robots' modules), with the text of any skill it grants. |
| **Skills** | Each skill with its level and full text; "[proposed]" if new. |
| **Element** | Enemy factions only: which elements the troop belongs to, with tier and behavior. |
| **Footer** | The `miniatures` text in italics, and "Imperator Black · content v…". |

Text shrinks to fit if a card is crowded. **Check by eye:** render the page (`pdftoppm -r 70 -png file.pdf out`) and make sure nothing is too small to read. If a card is crowded, shorten gear and skill text, or move a rarely used option off the profile; don't add a second card per troop.

## 8. Before you finish

- [ ] `validate_content.py` passes and its summary counts match the C# test counts.
- [ ] `build_content.py`, `render_tables.py`, `troop_values.py --reference` and `print_sheets.py` all run; commit their output.
- [ ] Chapter 9 has the new section and tables; chapter 15 lists the faction's troop values.
- [ ] Each new weapon, gear item or skill has a cost in `values.yaml`, and new rules are `status: proposed`.
- [ ] A decision row in `docs/design/decisions.md` (section 6, equipment and force building): what was added, the conversion method, and **Proposed — playtest**.
- [ ] The cards look right at 100% scale, every PDF name starts with `ImperatorBlack_`, and no artwork, logos or text was copied from another company's product.
- [ ] Run `python3 tools/troop_values.py` and sanity-check the new troops' TV against similar existing troops (a trained rifleman about 30–40, a professional about 45–55, an elite trooper 80+).
