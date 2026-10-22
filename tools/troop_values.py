#!/usr/bin/env python3
"""Troop values (rules chapter 15): the reference implementation of the Troop Value (TV) formula.

Every number comes from content/values.yaml and the weapon, armour, gear and skill content. The app's builder
(and C#) must compute the same values; this script is the yardstick, like generate_layout.py.

Usage (from the repo root):
    python3 tools/troop_values.py                 # print every troop's TV and Requisition
    python3 tools/troop_values.py --calibrate     # compare the TV ranking with the simulator (troop-values-v0.5.md)
                                                  # and write docs/analysis/troop-value-calibration.md
"""
import argparse
import itertools
import math
import re
import sys
from functools import lru_cache
from pathlib import Path

import yaml

ROOT = Path(__file__).resolve().parent.parent
C = {f.stem: yaml.safe_load(f.read_text()) for f in sorted((ROOT / "content").glob("*.yaml"))}
V = C["values"]
WOUND = C["core"]["wound_table"]
RANGED = {w["id"]: w for w in C["weapons"]["ranged"]}
MELEE = {w["id"]: w for w in C["weapons"]["melee"]}
ARMOUR = {a["id"]: a for a in C["armour"]["armour"]}
GEAR = {g["id"]: g for g in C["gear"]["gear"]}
TROOPS = C["troops"]["troops"]
FACTIONS = {f["id"]: f["name"] for f in C["factions"]["factions"]}


def mult(x):
    return float(V["multiplier"][str(max(0, min(5, x or 0)))])


def dice_pmf(spec):
    """'2d8' or 'd8+d10' -> {total: p}."""
    sides = []
    for term in spec.split("+"):
        n, _, d = term.partition("d")
        sides += [int(d)] * (int(n) if n else 1)
    out = {}
    for combo in itertools.product(*[range(1, s + 1) for s in sides]):
        out[sum(combo)] = out.get(sum(combo), 0) + 1
    n = sum(out.values())
    return {k: v / n for k, v in out.items()}


def row(total):
    if total < WOUND[0]["min"]:
        return 0
    for w in WOUND:
        if w["min"] <= total <= w["max"]:
            return w["row"]
    return 10


@lru_cache(None)
def lethality(damage, ap):
    """Expected wound row from one hit, averaged over the armour mix."""
    pmf = dice_pmf(damage)
    vals = []
    for a in V["weapon_formula"]["armour_mix"]:
        step = max(0, a - ap)
        vals.append(sum(p * max(0, min(10, row(t)) - step) for t, p in pmf.items()))
    return sum(vals) / len(vals)


def range_factor(short):
    for b in V["weapon_formula"]["range"]:
        if short <= b["up_to"]:
            return b["factor"]
    return V["weapon_formula"]["range"][-1]["factor"]


def shot_value(damage, ap, keywords, auto, explosion, short):
    F = V["weapon_formula"]
    v = lethality(damage, ap) * range_factor(short)
    if auto and "auto" in keywords:
        v *= 1 + (auto - 1) * F["auto_extra_target"]
    for k in keywords:
        v *= F["keyword_factors"].get(k, 1.0)
    if explosion:
        v *= 1 + explosion * F["explosion_per_inch"]
    return v


def weapon_base(wid):
    """Base value of a ranged weapon (before the Shoot multiplier)."""
    w = RANGED[wid]
    F = V["weapon_formula"]
    best = shot_value(w["damage"], w["ap"], w["keywords"], w.get("auto"), w.get("explosion"), w["short_range"])
    if w.get("rounds"):
        for r in w["rounds"]:
            best = max(best, shot_value(r["damage"], r["ap"], w["keywords"] + r.get("keywords", []), w.get("auto"), r.get("explosion"), w["short_range"]))
        best *= F["rounds_flexibility"]
    return round(best * F["scale"], 1)


def melee_base(mid):
    m = MELEE[mid]
    return round(lethality(m["damage"], m["ap"]) * V["melee_scale"], 1)


@lru_cache(None)
def hits_to_down(armour, wounds):
    """Expected number of 2d8, AP 0 hits to put a model Down (rows 4-5 add a marker; 6+ is Down)."""
    pmf = dice_pmf("2d8")
    pd = pw = 0.0
    for t, p in pmf.items():
        r = min(10, row(t)) - armour
        if r >= 6:
            pd += p
        elif r >= 4:
            pw += p
    if pd + pw == 0:
        return 50.0
    e = 0.0   # expected hits from W markers (one more Wounded or a Down ends it)
    for m in range(wounds, -1, -1):
        e = (1 + pw * (e if m < wounds else 0)) / (pd + pw) if m == wounds else (1 + pw * e) / (pd + pw)
    return e


def survival(armour, wounds):
    """Rounded to 2 decimals, as printed, so the paper sum and this script agree."""
    return round((hits_to_down(armour, wounds) / hits_to_down(0, 1)) ** V["survival_exponent"], 2)


def tv_parts(t, weapon=None, melee=None, gear=None):
    """(TV, breakdown) for a troop profile with its default loadout (or the given one)."""
    s = t["stats"]
    b = V["baseline"]
    parts = {"baseline": b["tv"]}
    for attr, cost in V["attribute_costs"].items():
        if s.get(attr):
            parts[attr] = s[attr] * cost
    parts["move"] = (s["move"] - b["move"]) * V["move_per_inch"]
    weapon = t["weapon"] if weapon is None else weapon
    if weapon:
        parts["weapon"] = round(weapon_base(weapon) * mult(s["shoot"]), 1)
    gear = t["gear"] if gear is None else gear
    melee_id = melee or t["melee"]
    for g in gear:
        if GEAR[g].get("melee"):
            melee_id = GEAR[g]["melee"]
    if melee_id:
        parts["melee"] = round(melee_base(melee_id) * mult(s["fight"]), 1)
    kit = 0.0
    for g in gear:
        c = V["gear"][g]
        kit += c["cost"] * (mult(s.get(c["per"])) if c.get("per") else 1)
    for g in t["grenades"]:
        c = V["grenades"][g]
        kit += c["cost"] * (mult(s.get(c["per"])) if c.get("per") else 1)
    granted = {k for g in gear for k in GEAR[g].get("grants_skills", {})}
    for sk, lvl in t["skills"].items():
        if sk in granted:
            continue
        c = V["skills"][sk]
        kit += c["cost"] * lvl * (mult(s.get(c["per"])) if c.get("per") else 1)
    parts["kit and skills"] = kit
    subtotal = sum(parts.values())
    surv = survival(ARMOUR[t["armour"]]["value"], s["wounds"])
    auto = V["automated_factor"] if "automated" in t["skills"] else 1.0
    tv = max(1, round(subtotal * surv * auto))
    parts["survival x"] = round(surv, 2)
    if auto != 1.0:
        parts["automated x"] = auto
    return tv, parts


def tv(t):
    return tv_parts(t)[0]


def requisition(value):
    return math.ceil(value / V["requisition_per"])


# ---------------------------------------------------------------- calibration against the simulator

def sim_scores():
    path = ROOT / "docs" / "analysis" / "troop-values-v0.5.md"
    names = {}
    for t in TROOPS:
        names.setdefault((FACTIONS[t["faction"]], t["name"]), t["id"])
    out, fac = {}, None
    for line in path.read_text().splitlines():
        if line.startswith("## "):
            fac = line[3:].strip()
        m = re.match(r"\| (.+?) \| .+? \| (\d+)% \|", line)
        if m and fac:
            name = m.group(1).replace(" †", "").replace(" \\*", "").strip()
            if (fac, name) in names:
                out[names[(fac, name)]] = int(m.group(2))
    return out


def spearman(xs, ys):
    def ranks(v):
        order = sorted(range(len(v)), key=lambda i: v[i])
        r = [0.0] * len(v)
        i = 0
        while i < len(order):
            j = i
            while j + 1 < len(order) and v[order[j + 1]] == v[order[i]]:
                j += 1
            for k in range(i, j + 1):
                r[order[k]] = (i + j) / 2
            i = j + 1
        return r
    rx, ry = ranks(xs), ranks(ys)
    mx, my = sum(rx) / len(rx), sum(ry) / len(ry)
    num = sum((a - mx) * (b - my) for a, b in zip(rx, ry))
    den = math.sqrt(sum((a - mx) ** 2 for a in rx) * sum((b - my) ** 2 for b in ry))
    return num / den


def calibrate(write=True):
    sim = sim_scores()
    rows = []
    for t in TROOPS:
        if t["id"] in sim:
            rows.append((t, tv(t), sim[t["id"]]))
    rho = spearman([r[1] for r in rows], [r[2] for r in rows])
    ranked_tv = sorted(rows, key=lambda r: -r[1])
    ranked_sim = sorted(rows, key=lambda r: -r[2])
    pos_tv = {r[0]["id"]: i for i, r in enumerate(ranked_tv)}
    pos_sim = {r[0]["id"]: i for i, r in enumerate(ranked_sim)}
    lines = ["# Troop value calibration", "",
             "Generated by `python3 tools/troop_values.py --calibrate` from `content/values.yaml` and the simulator's duel scores "
             "(`troop-values-v0.5.md`). The duel score is one measure of worth, not the target: it ignores multiple targets "
             "(Auto, Explosion), grenades, reactions, Deflect and command, so the two rankings should agree broadly, not exactly.", "",
             f"**Rank correlation (Spearman) between TV and the duel score: {rho:.2f}** over {len(rows)} profiles.", "",
             "| Troop | Faction | TV | Requisition | Duel score | TV rank | Duel rank | Gap |", "|---|---|---|---|---|---|---|---|"]
    for t, v, s in ranked_tv:
        gap = pos_tv[t["id"]] - pos_sim[t["id"]]
        lines.append(f'| {t["name"]} | {FACTIONS[t["faction"]]} | {v} | {requisition(v)} | {s}% | {pos_tv[t["id"]] + 1} | {pos_sim[t["id"]] + 1} | {gap:+d} |')
    big = [r for r in ranked_tv if abs(pos_tv[r[0]["id"]] - pos_sim[r[0]["id"]]) >= 20]
    lines += ["", "**Largest gaps (20 places or more), and why:**", ""]
    for t, v, s in big:
        lines.append(f'- {t["name"]} ({FACTIONS[t["faction"]]}): TV rank {pos_tv[t["id"]] + 1}, duel rank {pos_sim[t["id"]] + 1}.')
    if write:
        (ROOT / "docs" / "analysis" / "troop-value-calibration.md").write_text("\n".join(lines) + "\n")
    return rho, rows


FIXTURE = ROOT / "app" / "tests" / "Imperator.Core.Tests" / "Fixtures" / "troop-values-reference.json"


def reference():
    """Every troop's TV and Requisition, for the C# implementation's tests."""
    import json
    data = {"content_version": V["version"],
            "troops": {t["id"]: {"tv": tv(t), "requisition": requisition(tv(t))} for t in TROOPS},
            "weapon_base": {w: weapon_base(w) for w in RANGED},
            "melee_base": {m: melee_base(m) for m in MELEE},
            "survival": {f"{a},{w}": survival(a, w) for a in range(4) for w in (1, 2, 3)}}
    return json.dumps(data, indent=2) + "\n"


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--calibrate", action="store_true")
    ap.add_argument("--reference", action="store_true", help="write the C# test fixture")
    ap.add_argument("--check", action="store_true", help="exit non-zero if the C# test fixture is stale (CI)")
    a = ap.parse_args()
    if a.reference or a.check:
        text = reference()
        if a.check:
            if not FIXTURE.exists() or FIXTURE.read_text() != text:
                print(f"{FIXTURE.relative_to(ROOT)} is stale: run python3 tools/troop_values.py --reference", file=sys.stderr)
                sys.exit(1)
            print("Troop value fixture current.")
            return
        FIXTURE.write_text(text)
        print(f"Wrote {FIXTURE.relative_to(ROOT)}")
        return
    if a.calibrate:
        rho, rows = calibrate()
        print(f"Spearman {rho:.2f} over {len(rows)} profiles; wrote docs/analysis/troop-value-calibration.md")
        return
    for t in TROOPS:
        v, parts = tv_parts(t)
        print(f'{t["id"]:28} TV {v:4}  Req {requisition(v):3}   ' + ", ".join(f"{k} {round(x, 1) if isinstance(x, float) else x}" for k, x in parts.items()))


if __name__ == "__main__":
    main()
