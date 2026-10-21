#!/usr/bin/env python3
"""Fill rulebook tables from content YAML.

Each table in rules/*.md sits between markers:

    <!-- table:<file>.<section> -->
    ...generated...
    <!-- /table -->

<file> is a YAML file in content/ (without extension); <section> is a top-level key.
Run from the repo root: python3 tools/render_tables.py [--check]
--check exits non-zero if any table is out of date (for CI).
"""
import re
import sys
from pathlib import Path

import yaml

ROOT = Path(__file__).resolve().parent.parent
CONTENT = ROOT / "content"
RULES = ROOT / "rules"

KEYWORD_NAMES = {}


def load(name):
    return yaml.safe_load((CONTENT / f"{name}.yaml").read_text())


def kw(ids):
    return ", ".join(KEYWORD_NAMES.get(i, i) for i in ids) or "—"


def table(headers, rows):
    out = ["| " + " | ".join(headers) + " |", "|" + "---|" * len(headers)]
    out += ["| " + " | ".join(str(c) for c in r) + " |" for r in rows]
    return "\n".join(out)


def signed(v):
    return f"+{v}" if v > 0 else str(v)


def names(file, section, field="name"):
    return {x["id"]: x[field] for x in load(file)[section]}


def skill_list(skills):
    names_ = {s["id"]: (s["name"], len(s["levels"])) for s in load("skills")["skills"]}
    out = []
    for sid, lvl in skills.items():
        name, levels = names_[sid]
        out.append(f"{name} {lvl}" if levels > 1 else name)
    return ", ".join(out) or "—"


def numbered(lines):
    return "\n".join(f"{i}. {x}" for i, x in enumerate(lines, 1))


def bulleted(lines):
    return "\n".join(f"- {x}" for x in lines)


def render_behaviors(section):
    B = load("behaviors")
    if section == "awareness":
        return table(["State", "Meaning"], [(f'**{a["name"]}**', a["text"]) for a in B["awareness"]])
    if section == "noise":
        return table(["Noise", "Sources", "Heard by", "Alert track"],
                     [(f'**{n["name"]}**', n["sources"], n["heard"], f'+{n["alert"]}' if n["alert"] else "—") for n in B["noise"]])
    if section == "drills":
        ranks = names("core", "ranks")
        return table(["Lowest rank", "Drill", "Effect"], [(ranks[d["rank"]], f'**{d["name"]}**', d["text"]) for d in B["drills"]])
    if section in ("activation_order", "targets", "tie_breakers"):
        return numbered(B[section])
    if section == "close_quarters":
        return B[section][0] + "\n\n" + numbered(B[section][1:])
    if section == "behaviors":
        users = {}
        for e in load("factions")["elements"]:
            users.setdefault(e["behavior"], []).append(e["name"])
        blocks = []
        for bh in B["behaviors"]:
            used = ", ".join(users.get(bh["id"], [])) or "attached models and as directed by the mission"
            status = " **Proposed.**" if bh["status"] == "proposed" else ""
            cq = "Uses the close-quarters list first." if bh["close_quarters"] else "Uses only its own lists, even in close quarters."
            blocks.append("\n".join([
                f'### {bh["name"]}', "",
                f'*{bh["summary"]}* Used by: {used}.{status}', "",
                "**Target in sight**", "", numbered(bh["engaged"]), "",
                "**No target**", "", numbered(bh["alerted"]), "",
                f'**Investigates:** {bh["investigate"]}', "",
                "**Reactions**", "", bulleted(bh["reactions"]), "",
                f'**Close combat:** {bh["close_combat"]} {cq}' + (" May surrender." if bh["surrenders"] else ""),
            ]))
        return "\n\n".join(blocks)
    return bulleted(B[section])


def render_layouts(section):
    Ly = load("layouts")
    rooms = {r["id"]: r for r in Ly["rooms"]}
    vessels = {v["id"]: v for v in Ly["vessels"]}
    hazards = {h["id"]: h["name"] for h in Ly["hazards"]}
    sec = {s["id"]: s["name"] for s in load("core")["security"]["levels"]}
    if section == "vessel_roll":
        return table(["d6", "Vessel"], [(r["roll"].replace("-", "–"), vessels[r["vessel"]]["name"]) for r in Ly["vessel_roll"]])
    if section == "vessels":
        return table(["Vessel", "Board", "Spine", "Compartments", "Heart", "Locks", "Hazards (5 / 6)"],
                     [(f'**{v["name"]}**', f'{v["board"][0]}" × {v["board"][1]}"', f'{v["spine"]} pairs',
                       f'{v["compartments"][0]}–{v["compartments"][1]}',
                       f'{rooms[v["heart"]]["name"]} ({sec[v["heart_security"]]})', sec[v["security"]],
                       " / ".join(hazards[h] for h in v["hazards"])) for v in Ly["vessels"]])
    if section == "vessel_rooms":
        heads = ["d6"] + [v["name"] for v in Ly["vessels"]]
        rows = []
        for roll in range(1, 7):
            row = [str(roll)]
            for v in Ly["vessels"]:
                for r in v["rooms"]:
                    lo, _, hi = str(r["roll"]).partition("-")
                    if int(lo) <= roll <= int(hi or lo):
                        row.append(rooms[r["room"]]["name"] + (" †" if rooms[r["room"]].get("unique") else ""))
            rows.append(row)
        return table(heads, rows)
    if section in ("spine_turn", "passage_branch", "junction_branch", "loop_door", "door_state", "hazard_roll"):
        return table(["d6", "Result"], [(str(r["roll"]).replace("-", "–"), r["text"]) for r in Ly[section]])
    if section == "small_rooms":
        return table(["d6", "Small room", "Notes"],
                     [(r["roll"], rooms[r["room"]]["name"], rooms[r["room"]].get("text", "—")) for r in Ly["small_rooms"]])
    if section == "rooms":
        out = []
        for r in Ly["rooms"]:
            cover = "; ".join(f'{c["label"]} ({c["type"]}): {c["piece"]}' for c in r["cover"]) or "None"
            extra = []
            if r.get("security"):
                extra.append(f'Locks: {sec[r["security"]]}')
            if r.get("unique"):
                extra.append("Unique")
            out.append((f'**{r["name"]}**', "6\"×6\"" if r["size"] == "compartment" else "3\"×3\"", cover, ", ".join(extra) or "—"))
        return table(["Room", "Tile", "Cover", "Notes"], out)
    if section == "hazards":
        return table(["Hazard", "Effect"], [(f'**{h["name"]}**', h["text"]) for h in Ly["hazards"]])
    return None


def render_forces(key):
    f, section = key.split(".", 1)
    if f == "troops":
        weapons = names("weapons", "ranged")
        melee = names("weapons", "melee")
        thrown = names("weapons", "thrown")
        armour = {a["id"]: f'{a["name"]} ({a["value"]})' for a in load("armour")["armour"]}
        gear = names("gear", "gear")
        ranks = names("core", "ranks")
        rows = []
        for t in load("troops")["troops"]:
            if t["faction"] != section:
                continue
            s = t["stats"]
            dash = lambda v: "—" if v is None else v
            weapon = weapons[t["weapon"]] + "".join(f" / {weapons[o]}" for o in t.get("weapon_options", []))
            arm = armour[t["armour"]] + "".join(f" / {armour[o]}" for o in t.get("armour_options", []))
            kit = [melee[t["melee"]]] if t["melee"] else []
            kit += [thrown[g] for g in t["grenades"]] + [gear[g] for g in t["gear"]]
            opts = t.get("gear_options", [])
            kit_text = ", ".join(kit) or "—"
            if opts:
                kit_text += "; option: " + " or ".join(gear[g] for g in opts)
            mov = f'{s["move"]}"' if s["move"] else "—"
            rows.append((f'**{t["name"]}**', ranks[t["rank"]], mov, dash(s["shoot"]), dash(s["fight"]), dash(s["physique"]),
                         dash(s["nerve"]), dash(s["tech"]), s["wounds"], s["size"], arm, weapon, kit_text, skill_list(t["skills"])))
        return table(["Troop", "Rank", "MOV", "SHT", "FGT", "PHY", "NRV", "TEC", "W", "SZ", "Armour", "Weapon", "Kit", "Skills"], rows)
    if key == "skills.skills":
        rows = []
        for sk in load("skills")["skills"]:
            text = sk["levels"][0] if len(sk["levels"]) == 1 else " ".join(f"**{i}:** {x}" for i, x in enumerate(sk["levels"], 1))
            rows.append((f'*{sk["name"]}*' + (f' 1–{len(sk["levels"])}' if len(sk["levels"]) > 1 else ""), text,
                         "**Proposed**" if sk["status"] == "proposed" else "—"))
        return table(["Skill", "Effect", "Status"], rows)
    if key == "core.ranks":
        return table(["Rank", "Typical attributes", "Who"], [(r["name"], r["typical"], r["text"]) for r in load("core")["ranks"]])
    if f == "behaviors":
        return render_behaviors(section)
    if f == "layouts":
        return render_layouts(section)
    if key == "core.security":
        sec = load("core")["security"]
        return table(["Security level", "Hack test", "Successes needed", "Typical"],
                     [(l["name"], f'2d6 + Tech, {l["target"]}+', l["successes"], l["examples"]) for l in sec["levels"]])
    if key == "core.attributes":
        return table(["Attribute", "Short", "Used for"], [(f'**{a["name"]}**', a["short"], a["text"]) for a in load("core")["attributes"]])
    if key == "gear.gear":
        return table(["Gear", "Effect", "Status"],
                     [(g["name"], g["text"], "**Proposed**" if g["status"] == "proposed" else "—") for g in load("gear")["gear"]])
    if key == "rosters.teams":
        troops = names("troops", "troops")
        return table(["Team", "Models", "Role"],
                     [(tm["name"], ", ".join(f'{s["count"]}× {troops[s["troop"]]}' for s in tm["slots"]), tm["summary"])
                      for tm in load("rosters")["teams"]])
    if key == "factions.elements":
        troops = names("troops", "troops")
        behaviors = names("behaviors", "behaviors")
        fac = names("factions", "factions")
        rows = []
        for e in load("factions")["elements"]:
            comp = ", ".join(f'{p["count"]}× {troops[p["troop"]]}' for p in e["composition"])
            if e.get("alternatives"):
                comp += " (or " + ", ".join(troops[a] for a in e["alternatives"]) + ")"
            rows.append((e["name"], fac[e["faction"]], e["tier"], comp, behaviors[e["behavior"]]
                         + (" — reaction force" if e.get("reaction_force") else "")))
        return table(["Element", "Faction", "Tier", "Models", "Behavior"], rows)
    if key == "factions.threat_tiers":
        return table(["Tier", "Name", "Draws on"], [(t["tier"], t["name"], t["summary"]) for t in load("factions")["threat_tiers"]])
    return None


def render(key):
    forces = render_forces(key)
    if forces is not None:
        return forces
    f, section = key.split(".", 1)
    data = load(f)[section]
    if key == "core.hit_modifiers":
        return table(["Hit modifier", "Value"], [(m["text"], signed(m["value"])) for m in data])
    if key == "core.wound_table":
        return table(["Row", "Damage total", "Result"],
                     [(w["row"], f"{w['min']}–{w['max']}" if w["max"] < 99 else f"{w['min']}+", w["text"]) for w in data])
    if key == "weapons.ranged":
        rows = []
        for w in data:
            pen = w.get("range_penalty", [-1, -2])
            kws = ", ".join(f'Auto {w["auto"]}' if k == "auto" and w.get("auto") else KEYWORD_NAMES.get(k, k) for k in w["keywords"]) or "—"
            rows.append((w["name"], w["damage"], f'{w["short_range"]}"', f"{pen[0]} / {pen[1]}", w["ap"], kws))
        return table(["Weapon", "Damage", "Short range", "Beyond / double", "AP", "Keywords"], rows)
    if key == "weapons.melee":
        return table(["Melee weapon", "Damage", "AP"], [(w["name"], w["damage"], w["ap"]) for w in data])
    if key == "weapons.thrown":
        return table(["Grenade", "Throw range", "Radius", "Damage / effect"],
                     [(g["name"], f'{g["throw_range"]}"', f'{g["blast_radius"]}"',
                       g.get("damage", "") + (f' (AP {g["ap"]})' if "damage" in g else "") or g.get("effect", ""))
                      for g in data])
    if key == "weapons.demolitions":
        return table(["Charge", "Effect"], [(d["name"], d["effect"]) for d in data])
    if key == "armour.armour":
        return table(["Armour", "Value", "Keywords"], [(a["name"], a["value"], kw(a["keywords"])) for a in data])
    if key.startswith("keywords."):
        return table(["Keyword", "Effect"], [(f'*{k["name"]}*', k["text"]) for k in data])
    raise KeyError(f"No renderer for table '{key}'")


def main():
    check = "--check" in sys.argv
    for group in load("keywords").values():
        if isinstance(group, list):
            KEYWORD_NAMES.update({k["id"]: k["name"] for k in group})
    pattern = re.compile(r"(<!-- table:([\w.]+) -->\n)(.*?)(<!-- /table -->)", re.S)
    stale = []
    for md in sorted(RULES.glob("*.md")):
        text = md.read_text()
        new = pattern.sub(lambda m: m.group(1) + render(m.group(2)) + "\n" + m.group(4), text)
        if new != text:
            stale.append(md.name)
            if not check:
                md.write_text(new)
    if check and stale:
        print("Out-of-date tables in:", ", ".join(stale))
        sys.exit(1)
    print("Updated:" if stale else "All tables current.", ", ".join(stale))


if __name__ == "__main__":
    main()
