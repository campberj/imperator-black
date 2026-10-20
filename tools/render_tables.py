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


def render(key):
    f, section = key.split(".", 1)
    data = load(f)[section]
    if key == "core.quality":
        return table(["Q", "Rating"], [(q["value"], q["name"]) for q in data])
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
