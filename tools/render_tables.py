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
            rows.append((w["name"], w["damage"], f'{w["short_range"]}"', f"{pen[0]} / {pen[1]}", w["ap"], kw(w["keywords"])))
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
