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
    if section == "terms":
        return table(["Term", "Meaning"], [(f'**{x["term"]}**', x["text"]) for x in B["terms"]])
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


def render_missions(section):
    M = load("missions")
    els = {e["id"]: e for e in load("factions")["elements"]}
    troops = names("troops", "troops")
    special = {c["id"]: c["name"] for c in M["contact_results"]}
    mnames = {m["id"]: m["name"] for m in M["missions"]}
    d = lambda r: str(r).replace("-", "–")
    if section == "mission_roll":
        return table(["d6", "Mission"], [(d(r["roll"]), mnames[r["mission"]]) for r in M["mission_roll"]])
    if section == "tier_roll":
        return table(["d6", "Threat tier"], [(d(r["roll"]), r["tier"]) for r in M["tier_roll"]])
    if section == "missions":
        blocks = []
        for m in M["missions"]:
            blocks.append("\n".join([f'### {m["name"]}', "", f'*{m["summary"]}*', "",
                                      f'- **Objective aboard ship:** {m["ship"]}', f'- **Objective on land:** {m["land"]}',
                                      f'- **Taking it:** {m["task"]}', f'- **Lockdown (alert 6):** {m["lockdown"]}', "",
                                      table(["Grade", "Result"], [("**Decisive**", m["decisive"]), ("**Marginal**", m["marginal"]), ("**Failure**", m["failure"])])]))
        return "\n\n".join(blocks)
    if section == "tiers":
        heads = ["d6"] + [f'Tier {t["tier"]}' for t in M["tiers"]]
        rows = []
        for n in range(1, 7):
            row = [str(n)]
            for t in M["tiers"]:
                res = next(r["result"] for r in t["table"] if r["roll"] == str(n))
                row.append(special.get(res) or els[res]["name"])
            rows.append(row)
        rows.append(["Contacts"] + [str(t["contacts"]) for t in M["tiers"]])
        rows.append(["Reinforcement"] + [els[t["reinforcement"]]["name"] for t in M["tiers"]])
        rows.append(["HVT"] + [troops[t["hvt"]] for t in M["tiers"]])
        return table(heads, rows)
    if section == "contact_results":
        return table(["Result", "Effect"], [(f'**{c["name"]}**', c["text"]) for c in M["contact_results"]])
    if section == "alert_raises":
        return table(["Raises the alert track", "By"], [(r["source"], f'+{r["amount"]}') for r in M["alert"]["raises"]])
    if section == "alert_thresholds":
        return table(["At", "Effect"], [(f'**{t["at"]}**', t["effect"]) for t in M["alert"]["thresholds"]])
    if section == "complications":
        return table(["d6", "Complication", "Effect"], [(d(c["roll"]), f'**{c["name"]}**', c["text"]) for c in M["complications"]])
    if section in ("placement", "contacts"):
        return "\n".join(f"{i}. {x}" for i, x in enumerate(M[section], 1))
    if section == "briefing":
        return table(["Heading", "Contents"], [(f'**{b["heading"]}**', b["text"]) for b in M["briefing"]])
    if section == "clock_sizes":
        return table(["Mission size", "Player force", "Clock die", "Time limit"],
                     [(f'**{z["name"]}**', z["force"], z["die"], f'{z["limit"]} minutes') for z in M["clock"]["sizes"]])
    if section == "clock":
        return "\n".join(f"- {x}" for x in M["clock"]["rules"])
    return None


def render_campaign(section):
    Cp = load("campaign")
    d = lambda r: str(r).replace("-", "–")
    names = {m["id"]: m["name"] for m in load("missions")["missions"]}
    vessels = {v["id"]: v["name"] for v in load("layouts")["vessels"]}
    envs = {e["id"]: e["name"] for e in load("terrain")["environments"]}
    if section in ("start_text",):
        return "\n".join(f"- {x}" for x in Cp["start"]["text"])
    if section == "turn":
        return table(["Step", "What happens"], [(f'**{i}. {t["step"]}**', t["text"]) for i, t in enumerate(Cp["turn"], 1)])
    if section == "resources":
        return table(["Resource", "Track", "What it does"],
                     [(f'**{r["name"]}**', f'0–{r["max"]}' if r["max"] else "No limit", r["text"]) for r in Cp["resources"]])
    if section == "standing":
        return table(["Standing", "", "Income", "Support assets", "Taskings"],
                     [(f'**{x["level"]}**', x["name"], x["income"], x["support"] or "—", x["text"]) for x in Cp["standing"]])
    if section == "support":
        return table(["Support asset", "Effect"], [(f'**{x["name"]}**', x["text"]) for x in Cp["support"]])
    if section == "intel_uses":
        return table(["Intel", "Use", "Effect"], [(x["cost"], f'**{x["name"]}**', x["text"]) for x in Cp["intel_uses"]])
    if section in ("arc_procedure", "arc_taking", "arc_end"):
        key = section.split("_", 1)[1]
        items = Cp["arc"][key]
        return "\n".join(f"{i}. {x}" for i, x in enumerate(items, 1)) if key == "procedure" else "\n".join(f"- {x}" for x in items)
    if section == "nodes":
        rows = []
        for n in Cp["nodes"]:
            where = vessels[n["vessel"]] if n["setting"] == "ship" else envs[n["environment"]] + " (land)"
            rows.append((d(n["roll"]), f'**{n["name"]}**', where, names[n["mission"]], n["reward"]))
        for n in Cp["special_nodes"]:
            rows.append(("—", f'**{n["name"]}**', n["setting"], names[n["mission"]], n["reward"]))
        return table(["d6", "Node", "Board", "Mission", "Reward when taken"], rows)
    if section == "rewards":
        return table(["Result", "Reward"], [(f'**{r["result"]}**', r["text"]) for r in Cp["rewards"]])
    if section == "casualties":
        return "\n".join(f"- {x}" for x in Cp["casualties"])
    if section == "injury_modifiers":
        return table(["Modifier", "When"], [(f'{m["value"]:+d}', m["text"]) for m in Cp["injury_modifiers"]])
    if section == "injuries":
        return table(["2d6", "Injury", "Out", "Effect"],
                     [(d(r["roll"]), f'**{r["name"]}**', "—" if r["out"] is None else (f'{r["out"]} turn' + ("s" if r["out"] != 1 else "") if r["out"] else "0"), r["text"])
                      for r in Cp["injuries"]])
    if section == "xp":
        return table(["XP", "For"], [(f'+{x["amount"]}', x["text"]) for x in Cp["xp"]])
    if section == "advances":
        return "\n".join(f"- {x}" for x in Cp["advances"]["text"]) + "\n\n" + \
            table(["d6", "Advance"], [(r["roll"], r["text"]) for r in Cp["advances"]["table"]])
    if section == "honours":
        return table(["Honour", "How"], [(f'**{h["name"]}**', h["text"]) for h in Cp["honours"]]) + "\n\n" + \
            table(["Unit honour", "Effect"], [(f'**{h["name"]}**', h["text"]) for h in Cp["honour_list"]])
    if section == "ship":
        rows = []
        for sh in Cp["ship"]:
            for i, lv in enumerate(sh["levels"], 1):
                rows.append((f'**{sh["name"]}**' if i == 1 else "", i, lv["cost"], lv["text"]))
        return table(["Upgrade", "Level", "Requisition", "Effect"], rows)
    if section == "costs":
        return table(["Item", "Requisition"], [(c["item"], c["cost"]) for c in Cp["costs"]])
    if section == "events":
        return table(["2d6", "Event", "Effect"], [(r["roll"], f'**{r["name"]}**', r["text"]) for r in Cp["events"]])
    if section == "evidence":
        return "\n".join(f"- {x}" for x in Cp["evidence"])
    return None


def render_values(section):
    sys.path.insert(0, str(Path(__file__).resolve().parent))
    import troop_values as tvm
    Va = load("values")
    shoots = range(5)
    m = tvm.mult
    names = {a["id"]: a["name"] for a in load("core")["attributes"]}
    fmt = lambda x: f"{x:g}" if x == int(x) else f"{x:.1f}"
    if section == "multiplier":
        return table(["Attribute value"] + [str(i) for i in shoots], [["Multiplier"] + [fmt(m(i)) for i in shoots]])
    if section == "attribute_costs":
        rows = [(names[k], f"{v} per point") for k, v in Va["attribute_costs"].items()]
        rows.append(("Move", f'{Va["move_per_inch"]} per inch above {Va["baseline"]["move"]}" (minus below it)'))
        return table(["Attribute", "Cost"], rows)
    if section in ("weapons_core", "weapons_faction"):
        W = load("weapons")["ranged"]
        sel = [w for w in W if bool(w.get("faction")) == (section == "weapons_faction")]
        rows = []
        for w in sel:
            b = tvm.weapon_base(w["id"])
            facs = {f["id"]: f["name"] for f in load("factions")["factions"]}
            label = w["name"] + (f' ({facs[w["faction"]]})' if w.get("faction") else "")
            rows.append([label] + [fmt(round(b * m(i), 1)) for i in shoots])
        return table(["Weapon", "Shoot 0", "1", "2", "3", "4"], rows)
    if section == "melee":
        rows = [[w["name"]] + [fmt(round(tvm.melee_base(w["id"]) * m(i), 1)) for i in shoots] for w in load("weapons")["melee"]]
        return table(["Melee weapon", "Fight 0", "1", "2", "3", "4"], rows)
    if section in ("gear", "grenades", "skills"):
        src = {"gear": {g["id"]: g["name"] for g in load("gear")["gear"]},
               "grenades": {g["id"]: g["name"] for g in load("weapons")["thrown"]},
               "skills": {k["id"]: k["name"] for k in load("skills")["skills"]}}[section]
        rows = []
        for k, c in Va[section].items():
            if k == "automated":
                continue
            per = f' × {names[c["per"]]} multiplier' if c.get("per") else ""
            rows.append((src[k], fmt(c["cost"]) + (" per level" if section == "skills" else "") + per))
        return table(["Item" if section != "skills" else "Skill", "Cost"], rows)
    if section == "survival":
        rows = [[f"Armour {a}"] + [f"{tvm.survival(a, w):.2f}" for w in (1, 2, 3)] for a in range(4)]
        return table(["", "Wounds 1", "Wounds 2", "Wounds 3"], rows)
    if section.startswith("tv_"):
        fac = section[3:]
        rows = [(f'**{t["name"]}**', tvm.tv(t), tvm.requisition(tvm.tv(t))) for t in load("troops")["troops"] if t["faction"] == fac]
        return table(["Troop", "TV", "Requisition"], rows)
    return None


def render_versus(section):
    Vs = load("versus")
    names = {m["id"]: m["name"] for m in Vs["missions"]}
    clocks = {z["id"]: z for z in load("missions")["clock"]["sizes"]}
    bullets = lambda items: "\n".join(f"- {x}" for x in items)
    if section == "sizes":
        return table(["Size", "Troop Value", "Models", "Board", "Mission clock"],
                     [(f'**{z["name"]}**', z["tv"], z["models"], z["board"],
                       f'{clocks[z["clock"]]["limit"]} minutes, {clocks[z["clock"]]["die"]}') for z in Vs["sizes"]])
    if section == "forces":
        return bullets(Vs["forces"])
    if section == "mission_roll":
        kinds = {m["id"]: "Even fight" if m["kind"] == "even" else "Raid" for m in Vs["missions"]}
        hidden = {m["id"]: m["hidden_text"] for m in Vs["missions"]}
        return table(["d6", "Mission", "Kind", "Hidden information"],
                     [(r["roll"], f'**{names[r["mission"]]}**', kinds[r["mission"]], hidden[r["mission"]]) for r in Vs["mission_roll"]])
    if section == "even":
        out = []
        for m in Vs["missions"]:
            if m["kind"] == "even":
                out.append(f'### {m["name"]}\n\n*{m["summary"]}*\n\n- **Hidden information:** {m["hidden_text"]}\n'
                           f'- **Deployment:** {m["deploy"]}\n- **Scoring:** {m["scoring"]}')
        return "\n\n".join(out)
    if section in ("raid_setup", "raid_alarm", "raid_alert", "raid_grades"):
        return bullets(Vs["raid"][section.split("_", 1)[1]])
    if section == "raid_secrets":
        return bullets([x["text"] for x in Vs["raid"]["secrets"]])
    if section == "blinds":
        return "\n".join(f"{i}. {x}" for i, x in enumerate(Vs["blinds"]["rules"], 1))
    if section in ("casualty_points", "end", "even_grades", "changes"):
        return bullets(Vs[section])
    return None


def render_terrain(section):
    T = load("terrain")
    moves = {"normal": "Normal", "difficult": "Difficult (each inch costs 2\")", "obstacle": "Obstacle (+2\" to cross)", "blocked": "Blocked"}
    if section == "types":
        return table(["Terrain", "Cover", "Movement", "Line of sight"],
                     [(f'**{x["name"]}**', x["cover"], moves.get(x["move"], x["move"]), x["sight"]) for x in T["types"]])
    if section == "height":
        return "\n".join(f"- {r}" for r in T["height"]["rules"])
    if section == "conditions":
        return table(["d6", "Conditions", "Effect"], [(c["roll"].replace("-", "–"), f'**{c["name"]}**', c["text"]) for c in T["conditions"]])
    if section == "environment_roll":
        envs = {e["id"]: e["name"] for e in T["environments"]}
        return table(["d6", "Environment"], [(r["roll"], envs[r["environment"]]) for r in T["environment_roll"]])
    if section == "boards":
        return table(["Force size", "Board", "12\" sectors"], [(b["models"], b["size"], b["sectors"]) for b in T["boards"]])
    if section == "environments":
        heads = ["d6"] + [e["name"] for e in T["environments"]]
        rows = [[str(n)] + [next(r["text"] for r in e["table"] if r["roll"] == str(n)) for e in T["environments"]] for n in range(1, 7)]
        rows.append(["Fill"] + [e["fill"] for e in T["environments"]])
        return table(heads, rows)
    return None


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
            cover = "; ".join(f'{c["label"]} ({c["type"]}): {c["piece"].rstrip(".")}' for c in r["cover"]) or "None"
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


def ranged_rows(w):
    """Table rows for a ranged weapon: its own line, then one line per extra round (launchers)."""
    pen = w.get("range_penalty", [-1, -2])
    def kws(keys, auto=None, explosion=None):
        out = [f"Auto {auto}" if k == "auto" and auto else KEYWORD_NAMES.get(k, k) for k in keys]
        if explosion:
            out.append(f"Explosion ({explosion})")
        return ", ".join(out) or "—"
    name = w["name"] + (f' ({w["round"].lower()})' if w.get("round") else "")
    rows = [(name, w["damage"], f'{w["short_range"]}"', f"{pen[0]} / {pen[1]}", w["ap"], kws(w["keywords"], w.get("auto"), w.get("explosion")))]
    for r in w.get("rounds", []):
        rows.append((f'↳ or {r["name"].lower()} round', r["damage"], f'{w["short_range"]}"', f"{pen[0]} / {pen[1]}", r["ap"],
                     kws(w["keywords"] + r.get("keywords", []), w.get("auto"), r.get("explosion"))))
    return rows


def faction_kit(faction):
    """Weapons, armour and grenades that belong to one faction (marked faction: in content)."""
    W, A = load("weapons"), load("armour")["armour"]
    out = []
    ranged = [w for w in W["ranged"] if w.get("faction") == faction]
    if ranged:
        rows = []
        for w in ranged:
            rows += ranged_rows(w)
        out.append(table(["Weapon", "Damage", "Short range", "Beyond / double", "AP", "Keywords"], rows))
    melee = [m for m in W["melee"] if m.get("faction") == faction]
    if melee:
        out.append(table(["Melee weapon", "Damage", "AP"], [(m["name"], m["damage"], m["ap"]) for m in melee]))
    thrown = [g for g in W["thrown"] if g.get("faction") == faction]
    if thrown:
        out.append(table(["Grenade", "Throw range", "Radius", "Damage / effect"],
                         [(g["name"], f'{g["throw_range"]}"', f'{g["blast_radius"]}"',
                           g.get("damage", "") + (f' (AP {g["ap"]})' if "damage" in g else "") or g.get("effect", "")) for g in thrown]))
    armour = [a for a in A if a.get("faction") == faction]
    if armour:
        out.append(table(["Armour", "Value", "Keywords"], [(a["name"], a["value"], kw(a["keywords"])) for a in armour]))
    return "\n\n".join(out)


def render_forces(key):
    f, section = key.split(".", 1)
    if f == "kit":
        return faction_kit(section)
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
            weapon = (weapons[t["weapon"]] if t["weapon"] else "—") + "".join(f" / {weapons[o]}" for o in t.get("weapon_options", []))
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
    if f == "terrain":
        return render_terrain(section)
    if f == "missions":
        return render_missions(section)
    if f == "campaign":
        return render_campaign(section)
    if f == "values":
        return render_values(section)
    if f == "versus":
        return render_versus(section)
    if key == "core.security":
        sec = load("core")["security"]
        return table(["Security level", "Hack test", "Successes needed", "Typical"],
                     [(l["name"], f'2d6 + Tech, {l["target"]}+', l["successes"], l["examples"]) for l in sec["levels"]])
    if key == "core.shoot_scale":
        return table(["Shoot", "Who"], [(f'**+{s["value"]}**', s["text"]) for s in load("core")["shoot_scale"]])
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
    if isinstance(data, list):
        data = [x for x in data if not (isinstance(x, dict) and x.get("faction"))]   # faction kit is listed with its faction
    if key == "core.hit_modifiers":
        return table(["Hit modifier", "Value"], [(m["text"], signed(m["value"])) for m in data])
    if key == "core.wound_table":
        return table(["Row", "Damage total", "Result"],
                     [(w["row"], f"{w['min']}–{w['max']}" if w["max"] < 99 else f"{w['min']}+", w["text"]) for w in data])
    if key == "weapons.ranged":
        rows = []
        for w in data:
            rows += ranged_rows(w)
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
