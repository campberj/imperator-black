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
skills = {s["id"]: s for s in C["skills"]["skills"]}
ids(C["skills"]["skills"], "skills")
ranks = ids(C["core"]["ranks"], "core.ranks")
ATTRS = [a["id"] for a in C["core"]["attributes"]]
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
    for k, lvl in (g.get("grants_skills") or {}).items():
        need(k, skills, f"gear {g['id']} grants")
        if k in skills and not 1 <= lvl <= len(skills[k]["levels"]):
            errors.append(f"gear {g['id']}: {k} level {lvl} out of range")
    for k in g.get("weapon_keywords", []):
        need(k, kw_weapon, f"gear {g['id']} weapon keyword")
    if "melee" in g:
        need(g["melee"], melee, f"gear {g['id']} melee")
    if g.get("status") not in ("settled", "proposed"):
        errors.append(f"gear {g['id']}: status must be settled or proposed")

for t in C["troops"]["troops"]:
    w = f"troop {t['id']}"
    need(t["faction"], factions, f"{w} faction")
    need(t["rank"], ranks, f"{w} rank")
    st = t["stats"]
    if sorted(st) != sorted(ATTRS):
        errors.append(f"{w}: stats must be exactly {ATTRS}")
    automated = "automated" in t["skills"]
    for a, val in st.items():
        if val is None:
            if not automated or a in ("move", "shoot", "wounds", "size"):
                errors.append(f"{w}: {a} may be null only on Automated models (and never move/shoot/wounds/size)")
        elif a == "move" and not 0 <= val <= 10:
            errors.append(f"{w}: move out of range")
        elif a in ("wounds", "size") and not 1 <= val <= 6:
            errors.append(f"{w}: {a} out of range")
        elif a not in ("move", "wounds", "size") and not -1 <= val <= 4:
            errors.append(f"{w}: {a} out of range (-1 to 4)")
    need(t["armour"], armour, f"{w} armour")
    for a in t.get("armour_options", []):
        need(a, armour, f"{w} armour option")
    need(t["weapon"], ranged, f"{w} weapon")
    for o in t.get("weapon_options", []):
        need(o, ranged, f"{w} weapon option")
    if t["melee"] is not None:
        need(t["melee"], melee, f"{w} melee")
    elif "automated" not in t["skills"]:
        errors.append(f"{w}: only Automated models may have no melee weapon")
    for g in t["grenades"]:
        need(g, thrown, f"{w} grenade")
    if len(t["grenades"]) > 2:
        errors.append(f"{w}: carries more than two grenades")
    for g in t["gear"] + t.get("gear_options", []):
        need(g, gear, f"{w} gear")
    for k, lvl in t["skills"].items():
        need(k, skills, f"{w} skill")
        if k in skills and not 1 <= lvl <= len(skills[k]["levels"]):
            errors.append(f"{w}: {k} level {lvl} out of range")

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

B = C["behaviors"]
for bh in B["behaviors"]:
    w = f"behavior {bh['id']}"
    for field in ("engaged", "alerted", "reactions"):
        if not bh.get(field):
            errors.append(f"{w}: needs a non-empty '{field}' list")
    for field in ("close_combat", "investigate", "summary"):
        if not isinstance(bh.get(field), str):
            errors.append(f"{w}: needs '{field}' text")
    for field in ("surrenders", "close_quarters"):
        if not isinstance(bh.get(field), bool):
            errors.append(f"{w}: '{field}' must be true or false")
    if bh.get("status") not in ("settled", "proposed"):
        errors.append(f"{w}: status must be settled or proposed")
drill_ranks = [d["rank"] for d in B["drills"]]
for r in ranks:
    if drill_ranks.count(r) != 1:
        errors.append(f"behaviors.drills: rank '{r}' needs exactly one drill")
ids(B["drills"], "behaviors.drills")
ids(B["noise"], "behaviors.noise")
ids(B["awareness"], "behaviors.awareness")
for section in ("marker", "alarm", "activation_order", "targets", "moving", "close_quarters", "surrender", "tie_breakers"):
    if not B.get(section):
        errors.append(f"behaviors.{section}: missing or empty")

sec = C["core"]["security"]
ids(sec["levels"], "core.security")
for lv in sec["levels"]:
    if not 2 <= lv["target"] <= 12 or lv["successes"] < 1:
        errors.append(f"security level {lv['id']}: target 2-12 and successes 1+")

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
for s in C["skills"]["skills"]:
    if s.get("status") not in ("settled", "proposed"):
        errors.append(f"skill {s['id']}: status must be settled or proposed")

# Layouts (chapter 11).
Ly = C["layouts"]
room_ids = ids(Ly["rooms"], "layouts.rooms")
sec_ids = {s["id"] for s in C["core"]["security"]["levels"]}
haz_ids = ids(Ly["hazards"], "layouts.hazards")
vessel_ids = ids(Ly["vessels"], "layouts.vessels")


def covers_d6(rows, where):
    got = []
    for r in rows:
        lo, _, hi = str(r["roll"]).partition("-")
        got += list(range(int(lo), int(hi or lo) + 1))
    if sorted(got) != [1, 2, 3, 4, 5, 6]:
        errors.append(f"{where}: d6 rows must cover 1-6 exactly once")


for tbl in ("spine_turn", "passage_branch", "junction_branch", "loop_door", "door_state", "hazard_roll", "small_rooms", "vessel_roll"):
    covers_d6(Ly[tbl], f"layouts.{tbl}")
for r in Ly["small_rooms"]:
    need(r["room"], room_ids, "layouts.small_rooms room")
for r in Ly["vessel_roll"]:
    need(r["vessel"], vessel_ids, "layouts.vessel_roll vessel")
for r in Ly["rooms"]:
    if r.get("security"):
        need(r["security"], sec_ids, f"room {r['id']} security")
    for c in r["cover"]:
        if c["type"] not in ("light", "heavy"):
            errors.append(f"room {r['id']}: cover type must be light or heavy")
for v in Ly["vessels"]:
    w = f"vessel {v['id']}"
    covers_d6(v["rooms"], f"{w} rooms")
    for r in v["rooms"]:
        need(r["room"], room_ids, f"{w} room")
    need(v["heart"], room_ids, f"{w} heart")
    for k in ("security", "heart_security"):
        need(v[k], sec_ids, f"{w} {k}")
    for h in v["hazards"]:
        need(h, haz_ids, f"{w} hazard")
    if any(x % 3 for x in v["board"]):
        errors.append(f"{w}: board must be in multiples of 3 inches")
if not errors:
    sys.path.insert(0, str(ROOT / "tools"))
    import generate_layout as gl
    for vid in vessel_ids:
        for seed in range(1, 201):
            deck = gl.Deck(vid, seed).build()
            counts = deck.to_scenario()["tile_counts"]
            if not gl.connected(deck) or any(counts.get(k, 0) > gl.OWNED[k] for k in gl.OWNED):
                errors.append(f"layout {vid} seed {seed}: disconnected or over the tile inventory")
                break

if errors:
    print("\n".join(errors))
    sys.exit(1)
print(f"Content valid: {len(troops)} troop types, {len(elements)} elements, {len(teams)} teams, {len(gear)} gear items, {len(behaviors)} behavior profiles, {len(vessel_ids)} vessels.")
