#!/usr/bin/env python3
"""Check that every id referenced in content/*.yaml exists, and that ids are unique.

Run from the repo root: python3 tools/validate_content.py   (exits non-zero on any error)
"""
import sys
from pathlib import Path

import yaml

ROOT = Path(__file__).resolve().parent.parent
C = {f.stem: yaml.safe_load(f.read_text()) for f in sorted((ROOT / "content").glob("*.yaml"))}
errors = []


def ids(items, where):
    seen = set()
    for it in items:
        if it["id"] in seen:
            errors.append(f"{where}: duplicate id '{it['id']}'")
        seen.add(it["id"])
    return seen


def need(value, pool, where):
    if value not in pool:
        errors.append(f"{where}: unknown id '{value}'")


ranged = ids(C["weapons"]["ranged"], "weapons.ranged")
melee = ids(C["weapons"]["melee"], "weapons.melee")
thrown = ids(C["weapons"]["thrown"], "weapons.thrown")
ids(C["weapons"].get("demolitions", []), "weapons.demolitions")
armour = ids(C["armour"]["armour"], "armour")
kw_weapon = ids(C["keywords"]["weapon"], "keywords.weapon")
kw_armour = ids(C["keywords"]["armour"], "keywords.armour")
kw_model = ids(C["keywords"]["model"], "keywords.model")
gear = ids(C["gear"]["gear"], "gear")
behaviors = ids(C["behaviors"]["behaviors"], "behaviors")
factions = ids(C["factions"]["factions"], "factions")
troops = ids(C["troops"]["troops"], "troops")
elements = ids(C["factions"]["elements"], "elements")
teams = ids(C["rosters"]["teams"], "rosters.teams")
ids(C["rosters"]["starting_forces"], "rosters.starting_forces")

for w in C["weapons"]["ranged"] + C["weapons"]["thrown"] + C["weapons"].get("demolitions", []):
    for k in w.get("keywords", []):
        need(k, kw_weapon, f"weapon {w['id']} keyword")
for a in C["armour"]["armour"]:
    for k in a["keywords"]:
        need(k, kw_armour, f"armour {a['id']} keyword")
for g in C["gear"]["gear"]:
    for k in g.get("grants_keywords", []):
        need(k, kw_model, f"gear {g['id']} grants")
    for k in g.get("weapon_keywords", []):
        need(k, kw_weapon, f"gear {g['id']} weapon keyword")
    if "melee" in g:
        need(g["melee"], melee, f"gear {g['id']} melee")
    if g.get("status") not in ("settled", "proposed"):
        errors.append(f"gear {g['id']}: status must be settled or proposed")

for t in C["troops"]["troops"]:
    w = f"troop {t['id']}"
    need(t["faction"], factions, f"{w} faction")
    if not 0 <= t["q"] <= 3:
        errors.append(f"{w}: q out of range")
    need(t["armour"], armour, f"{w} armour")
    for a in t.get("armour_options", []):
        need(a, armour, f"{w} armour option")
    need(t["weapon"], ranged, f"{w} weapon")
    for o in t.get("weapon_options", []):
        need(o, ranged, f"{w} weapon option")
    if t["melee"] is not None:
        need(t["melee"], melee, f"{w} melee")
    elif "automated" not in t["keywords"]:
        errors.append(f"{w}: only Automated models may have no melee weapon")
    for g in t["grenades"]:
        need(g, thrown, f"{w} grenade")
    if len(t["grenades"]) > 2:
        errors.append(f"{w}: carries more than two grenades")
    for g in t["gear"] + t.get("gear_options", []):
        need(g, gear, f"{w} gear")
    for k in t["keywords"]:
        need(k, kw_model, f"{w} keyword")

for e in C["factions"]["elements"]:
    w = f"element {e['id']}"
    need(e["faction"], factions, f"{w} faction")
    need(e["behavior"], behaviors, f"{w} behavior")
    if e["tier"] not in (1, 2, 3, 4):
        errors.append(f"{w}: tier must be 1-4")
    for part in e["composition"]:
        need(part["troop"], troops, f"{w} troop")
    for alt in e.get("alternatives", []):
        need(alt, troops, f"{w} alternative")

for tm in C["rosters"]["teams"]:
    for s in tm["slots"]:
        need(s["troop"], troops, f"team {tm['id']} slot")
for sf in C["rosters"]["starting_forces"]:
    for tm in sf["teams"]:
        need(tm, teams, f"starting force {sf['id']} team")
    for tr in sf.get("attachments", {}).get("from", []):
        need(tr, troops, f"starting force {sf['id']} attachment")

if errors:
    print("\n".join(errors))
    sys.exit(1)
print(f"Content valid: {len(troops)} troop types, {len(elements)} elements, {len(teams)} teams, {len(gear)} gear items.")
