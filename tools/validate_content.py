#!/usr/bin/env python3
"""Check that every id referenced in content/*.yaml exists, and that ids are unique.

Run from the repo root: python3 tools/validate_content.py   (exits non-zero on any error)
Mirrored in app/src/Imperator.Core/ContentValidator.cs (same checks, same messages); keep the two in step.
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

# Damage dice: a matched pair (2d4 to 2d12) or a half-step mixed pair (decision 6.14).
import re as _re
DAMAGE_OK = {f"2d{n}" for n in (4, 6, 8, 10, 12)} | {"d6+d8", "d8+d10", "d10+d12"}
for grp in ("ranged", "melee", "thrown"):
    for w in C["weapons"][grp]:
        if w.get("damage") and w["damage"] not in DAMAGE_OK:
            errors.append(f"weapon {w['id']}: damage '{w['damage']}' is not a pair or a half-step (d6+d8, d8+d10, d10+d12)")
for w in C["weapons"]["ranged"]:
    if w.get("explosion") is not None and not (isinstance(w["explosion"], int) and 1 <= w["explosion"] <= 6):
        errors.append(f"weapon {w['id']}: explosion must be a radius of 1 to 6 inches")
    for r in w.get("rounds", []):
        if r.get("damage") not in DAMAGE_OK:
            errors.append(f"weapon {w['id']} round {r.get('id')}: bad damage")
        if r.get("explosion") is not None and not (isinstance(r["explosion"], int) and 1 <= r["explosion"] <= 6):
            errors.append(f"weapon {w['id']} round {r.get('id')}: explosion must be 1 to 6")
    if w.get("rounds") and not w.get("round"):
        errors.append(f"weapon {w['id']}: a weapon with rounds names its own first round ('round')")
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
    if t["weapon"] is not None:
        need(t["weapon"], ranged, f"{w} weapon")
    elif t["melee"] is None:
        errors.append(f"{w}: a model with no ranged weapon needs a melee weapon")
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
# Fireteams (decision 6.18): classes on troops, types on standard fireteams (teams and elements).
FT = {t["id"]: t for t in C["rosters"]["fireteam_types"]}
CLASSES = {c["id"] for c in C["rosters"]["classes"]}
troop_class = {t["id"]: t.get("class") for t in C["troops"]["troops"]}
for tid, cls in troop_class.items():
    if cls not in CLASSES:
        errors.append(f"troop {tid}: class must be one of {sorted(CLASSES)}")


def check_fireteam(where, ftype, parts):
    """parts: (troop, count). A type is a fireteam type, or 'character' for a character acting alone."""
    counts = {c: 0 for c in CLASSES}
    for troop, n in parts:
        if troop in troop_class:
            counts[troop_class[troop]] = counts.get(troop_class[troop], 0) + n
    if ftype == "character":
        if counts["character"] != 1 or sum(counts.values()) != 1:
            errors.append(f"{where}: type character is one character acting alone")
        return
    if ftype not in FT:
        errors.append(f"{where}: type must be one of {sorted(FT)} or character")
        return
    t = FT[ftype]
    if counts["character"] > 1:
        errors.append(f"{where}: at most one character")
    for cls, key in (("leader", "leader"), ("trooper", "troopers"), ("specialist", "specialists")):
        lo, hi = t[key]
        if not lo <= counts[cls] <= hi:
            errors.append(f"{where}: {counts[cls]} {cls}(s), but a {ftype} fireteam takes {lo} to {hi}")
    if counts["leader"] + counts["trooper"] + counts["specialist"] > t["max_models"]:
        errors.append(f"{where}: more than {t['max_models']} models (characters not counted)")


for tm in C["rosters"]["teams"]:
    check_fireteam(f"team {tm['id']}", tm.get("type"), [(s["troop"], s["count"]) for s in tm["slots"]])
for e in C["factions"]["elements"]:
    check_fireteam(f"element {e['id']}", e.get("type"), [(p["troop"], p["count"]) for p in e["composition"]])
    for alt in e.get("alternatives", []):
        # An alternative replaces one model: of the same class if the element has one, else of its first part.
        parts = [(p["troop"], p["count"]) for p in e["composition"]]
        i = next((k for k, (tr, _) in enumerate(parts) if troop_class.get(tr) == troop_class.get(alt)), 0)
        swapped = [(tr, n - (1 if k == i else 0)) for k, (tr, n) in enumerate(parts)] + [(alt, 1)]
        check_fireteam(f"element {e['id']} with {alt}", e.get("type"), swapped)

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
# Land terrain (chapter 12).
Tr = C["terrain"]
ids(Tr["types"], "terrain.types")
env_ids = ids(Tr["environments"], "terrain.environments")
for e in Tr["environments"]:
    covers_d6(e["table"], f"terrain environment {e['id']}")
covers_d6(Tr["environment_roll"], "terrain.environment_roll")
covers_d6(Tr["conditions"], "terrain.conditions")
for r in Tr["environment_roll"]:
    need(r["environment"], env_ids, "terrain.environment_roll environment")
terrain_ids = {t["id"] for t in Tr["types"]}
for e in Tr["environments"]:
    for w, pieces in [(f"terrain environment {e['id']} fill", e.get("fill_pieces"))] + \
                     [(f"terrain environment {e['id']} roll {r['roll']}", r.get("pieces")) for r in e["table"]]:
        if pieces is None:
            errors.append(f"{w}: needs 'pieces' (the generator's footprint; [] for open ground)")
            continue
        for pc in pieces:
            need(pc.get("type"), terrain_ids, f"{w} piece type")
            if not (0 < pc.get("w", 0) <= 12 and 0 < pc.get("h", 0) <= 12):
                errors.append(f"{w}: piece '{pc.get('label')}' must fit a 12\" sector")
    if not e.get("fill_pieces"):
        errors.append(f"terrain environment {e['id']}: the fill must have cover")
for x in Tr["types"]:
    for k in ("cover", "move", "sight"):
        if not x.get(k):
            errors.append(f"terrain type {x['id']}: needs '{k}'")
for x in C["behaviors"].get("terms", []):
    if not x.get("term") or not x.get("text"):
        errors.append("behaviors.terms: each needs term and text")

# Missions (chapter 13).
M = C["missions"]
mission_ids = ids(M["missions"], "missions.missions")
covers_d6(M["mission_roll"], "missions.mission_roll")
covers_d6(M["tier_roll"], "missions.tier_roll")
covers_d6(M["complications"], "missions.complications")
for r in M["mission_roll"]:
    need(r["mission"], mission_ids, "missions.mission_roll mission")
for z in M["clock"]["sizes"]:
    if z["die"] not in ("d4", "d6", "d8") or not isinstance(z["limit"], int) or z["limit"] <= M["clock"]["first_check_in"]:
        errors.append(f"missions.clock size {z['id']}: needs a die (d4/d6/d8) and a whole-minute limit past the first check-in")
special = {c["id"] for c in M["contact_results"]}
opp_ids = ids(M.get("oppositions", []), "missions.oppositions")
all_tiers = [("", tr) for tr in M["tiers"]] + [(f"{o['id']} ", tr) for o in M.get("oppositions", []) for tr in o["tiers"]]
for o in M.get("oppositions", []):
    if sorted(t["tier"] for t in o["tiers"]) != [1, 2, 3, 4]:
        errors.append(f"missions.oppositions {o['id']}: needs tiers 1 to 4")
for pre, tr in all_tiers:
    w = f"mission {pre}tier {tr['tier']}"
    covers_d6(tr["table"], w)
    for r in tr["table"]:
        if r["result"] not in special:
            need(r["result"], elements, f"{w} contact result")
    need(tr["reinforcement"], elements, f"{w} reinforcement")
    need(tr["hvt"], troops, f"{w} hvt")
for r in M["tier_roll"]:
    if r["tier"] not in [t["tier"] for t in M["tiers"]]:
        errors.append(f"missions.tier_roll: tier {r['tier']} has no table")
for m in M["missions"]:
    for k in ("task", "lockdown", "decisive", "marginal", "failure", "ship", "land"):
        if not m.get(k):
            errors.append(f"mission {m['id']}: needs '{k}'")

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

# Campaign (chapter 14).
Cp = C["campaign"]


def covers_2d6(rows, where):
    got = []
    for r in rows:
        lo, _, hi = str(r["roll"]).partition("-")
        got += list(range(int(lo), int(hi or lo) + 1))
    if sorted(got) != list(range(2, 13)):
        errors.append(f"{where}: 2d6 rows must cover 2-12 exactly once")


covers_d6(Cp["nodes"], "campaign.nodes")
covers_d6(Cp["advances"]["table"], "campaign.advances.table")
covers_2d6(Cp["injuries"], "campaign.injuries")
covers_2d6(Cp["events"], "campaign.events")
ids(Cp["nodes"] + Cp["special_nodes"], "campaign nodes")
for n in Cp["nodes"] + Cp["special_nodes"]:
    need(n["mission"], mission_ids, f"campaign node {n['id']} mission")
    if n.get("vessel"):
        need(n["vessel"], vessel_ids, f"campaign node {n['id']} vessel")
    if n.get("environment"):
        need(n["environment"], env_ids, f"campaign node {n['id']} environment")
for sk in Cp["advances"]["skills"]:
    need(sk, skills, "campaign.advances.skills")
need(Cp["start"]["force"], {f["id"] for f in C["rosters"]["starting_forces"]}, "campaign.start force")
if [x["level"] for x in Cp["standing"]] != list(range(0, 7)):
    errors.append("campaign.standing: levels must run 0 to 6")
if sum(c["nodes"] for c in Cp["arc"]["columns"]) != 6 or sum(1 for c in Cp["arc"]["columns"] if c.get("target")) != 1:
    errors.append("campaign.arc: six nodes in all, one of them the target")
ids(Cp["support"], "campaign.support")
B = Cp["bases"]
ids(B["kinds"], "campaign.bases.kinds")
ids(B["facilities"], "campaign.bases.facilities")
for fa in B["facilities"]:
    if len(fa["levels"]) != 3:
        errors.append(f"campaign facility {fa['id']}: three levels")
th = B["threat"]
if not 1 <= th["attack_at"] <= th["max"]:
    errors.append("campaign.bases.threat: attack_at must be 1 to max")
for k in ("decisive", "marginal", "failure"):
    if not B["defence"]["grades"].get(k):
        errors.append(f"campaign.bases.defence: needs a {k} grade")
if [o["result"] for o in B["outcomes"]] != ["Decisive", "Marginal", "Failure"]:
    errors.append("campaign.bases.outcomes: Decisive, Marginal and Failure, in order")
ids(Cp.get("latitudes", []), "campaign.latitudes")
ids(Cp.get("reports", []), "campaign.reports")
for ev in Cp["events"]:
    if not ev.get("text"):
        errors.append(f"campaign event {ev['roll']}: needs text")

# Troop values (chapter 15).
Va = C["values"]
for g in C["gear"]["gear"]:
    if g["id"] not in Va["gear"]:
        errors.append(f"values.gear: no cost for gear '{g['id']}'")
for g in C["weapons"]["thrown"]:
    if g["id"] not in Va["grenades"]:
        errors.append(f"values.grenades: no cost for '{g['id']}'")
for sk in C["skills"]["skills"]:
    if sk["id"] not in Va["skills"]:
        errors.append(f"values.skills: no cost for skill '{sk['id']}'")
attr_ids = {a["id"] for a in C["core"]["attributes"]}
for grp in ("gear", "grenades", "skills"):
    for k, c in Va[grp].items():
        if c.get("per") and c["per"] not in attr_ids:
            errors.append(f"values.{grp}.{k}: unknown attribute '{c['per']}'")
if [Va["multiplier"][str(i)] for i in range(5)] != sorted(Va["multiplier"][str(i)] for i in range(5)):
    errors.append("values.multiplier: must rise with the attribute")

if not isinstance(C["core"].get("overwatch_reactions"), int) or C["core"]["overwatch_reactions"] < 1:
    errors.append("core.overwatch_reactions: a whole number, 1 or more")

# Two players (chapter 16).
Vs = C["versus"]
vs_ids = ids(Vs["missions"], "versus.missions")
covers_d6(Vs["mission_roll"], "versus.mission_roll")
for r in Vs["mission_roll"]:
    need(r["mission"], vs_ids, "versus.mission_roll mission")
clock_ids = {z["id"] for z in C["missions"]["clock"]["sizes"]}
for z in Vs["sizes"]:
    need(z["clock"], clock_ids, f"versus size {z['id']} clock")
for m in Vs["missions"]:
    if m["kind"] not in ("even", "raid") or m["hidden"] not in ("none", "blinds", "defender"):
        errors.append(f"versus mission {m['id']}: kind even/raid and hidden none/blinds/defender")
    if m["kind"] == "raid" and m["raid_type"] not in mission_ids | {"recover_or_capture"}:
        errors.append(f"versus mission {m['id']}: unknown raid_type")

# Campaign definitions (decision 7.15; docs/design/campaign-format.md): bespoke arcs and procedural campaigns.
EFFECTS = {"initiative", "alert_start", "force_tv", "remove_contacts", "requisition", "intel", "standing", "evidence", "threat", "flag"}
latitude_ids = {x["id"] for x in Cp.get("latitudes", [])}
opposition_ids = {o["id"] for o in M.get("oppositions", [])}
start_forces = {f["id"] for f in C["rosters"]["starting_forces"]}
base_kinds = {k["id"] for k in Cp["bases"]["kinds"]}
facility_ids = {f["id"] for f in Cp["bases"]["facilities"]}
campaign_files = sorted((ROOT / "docs/scenarios/arcs").glob("*/arc.yaml")) + sorted((ROOT / "docs/scenarios/campaigns").glob("*.yaml"))
for cf in campaign_files:
    cd = yaml.safe_load(cf.read_text())
    if cd.get("kind") != "campaign":
        continue
    w = f"campaign {cd.get('id')}"
    need(cd["faction"], factions, f"{w} faction")
    if cd.get("opposition") is not None:
        need(cd["opposition"], opposition_ids, f"{w} opposition")
    need(cd["command"], latitude_ids, f"{w} command")
    if cd.get("force_selection") not in ("fireteam", "soldier"):
        errors.append(f"{w}: force_selection must be fireteam or soldier")
    if cd.get("format") not in ("short_arc", "arcs", "story") or cd.get("source") not in ("bespoke", "procedural"):
        errors.append(f"{w}: format short_arc/arcs/story and source bespoke/procedural")
    if cd.get("base"):
        need(cd["base"]["kind"], base_kinds, f"{w} base kind")
        for fa, lvl in (cd["base"].get("facilities") or {}).items():
            need(fa, facility_ids, f"{w} base facility")
            if not 0 <= lvl <= 3:
                errors.append(f"{w}: facility {fa} level 0-3")
    if cd["source"] == "procedural":
        need(cd["start_force"], start_forces, f"{w} start force")
        continue
    labels = {m["label"] for m in cd["missions"]}
    if cd.get("start") not in labels:
        errors.append(f"{w}: start must be a mission label")
    for m in cd["missions"]:
        mf = cf.parent / m["file"]
        if not mf.exists():
            errors.append(f"{w}: mission file {m['file']} missing")
            continue
        md = yaml.safe_load(mf.read_text())
        if sorted(n["grade"] for n in md.get("next", [])) != ["Decisive", "Failure", "Marginal"]:
            errors.append(f"{w} mission {m['label']}: next must cover Decisive, Marginal and Failure")
        for n in md.get("next", []):
            if n.get("to") not in labels | {"end"}:
                errors.append(f"{w} mission {m['label']} {n.get('grade')}: to must be a mission label or end")
            for k in (n.get("effects") or {}):
                if k not in EFFECTS:
                    errors.append(f"{w} mission {m['label']} {n.get('grade')}: unknown effect '{k}'")


if errors:
    print("\n".join(errors))
    sys.exit(1)
print(f"Content valid: {len(troops)} troop types, {len(elements)} elements, {len(teams)} teams, {len(gear)} gear items, {len(behaviors)} behavior profiles, {len(vessel_ids)} vessels, {len(Cp['nodes'])} campaign node types.")
