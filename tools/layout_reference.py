#!/usr/bin/env python3
"""Reference deck statistics from tools/generate_layout.py, for the C# deck generator's tests.

The C# generator (app/src/Imperator.Core/Layouts) uses .NET's random stream, so its decks match the
reference statistically rather than seed for seed. This script measures the reference over seeds 1..N
per vessel and writes the numbers the C# tests compare against.

Run from the repo root: python3 tools/layout_reference.py [--check]
--check exits non-zero if the fixture is out of date (for CI); regenerate it after editing layouts.yaml.
"""
import json
import sys
from pathlib import Path

sys.path.insert(0, str(Path(__file__).resolve().parent))
import generate_layout as gl  # noqa: E402

ROOT = Path(__file__).resolve().parent.parent
OUT = ROOT / "app" / "tests" / "Imperator.Core.Tests" / "Fixtures" / "layout-reference.json"
N = 2000
KINDS = ("compartment", "passage", "junction")


def stats(vid, n):
    comps, rolls, loops, under, disconnected, over = [], [], 0, 0, 0, 0
    used = {k: [] for k in KINDS}
    for seed in range(1, n + 1):
        d = gl.Deck(vid, seed).build()
        counts = d.to_scenario()["tile_counts"]
        c = d.count_compartments()
        comps.append(c)
        rolls.append(d.rolls)
        under += c < d.v["compartments"][0]
        loops += len(d.doors) - len(d.tiles) + 1 > 0
        disconnected += not gl.connected(d)
        over += any(counts.get(k, 0) > gl.OWNED[k] for k in gl.OWNED)
        for k in KINDS:
            used[k].append(counts.get(k, 0))
    r4 = lambda x: round(x, 4)
    return {
        "compartments_mean": r4(sum(comps) / n),
        "compartments_min": min(comps),
        "compartments_max": max(comps),
        "below_minimum": r4(under / n),
        "with_loop": r4(loops / n),
        "rolls_mean": r4(sum(rolls) / n),
        "tiles_mean": {k: r4(sum(v) / n) for k, v in used.items()},
        "tiles_max": {k: max(v) for k, v in used.items()},
        "disconnected": disconnected,
        "over_inventory": over,
    }


def build():
    data = {
        "source": "tools/generate_layout.py",
        "content_version": gl.L["version"],
        "seeds": N,
        "vessels": {vid: stats(vid, N) for vid in gl.VESSELS},
    }
    return json.dumps(data, indent=2) + "\n"


def main():
    text = build()
    if "--check" in sys.argv:
        if not OUT.exists() or OUT.read_text() != text:
            print(f"{OUT.relative_to(ROOT)} is out of date; run tools/layout_reference.py")
            sys.exit(1)
        print("Layout reference statistics current.")
        return
    OUT.parent.mkdir(parents=True, exist_ok=True)
    OUT.write_text(text)
    print(f"Wrote {OUT.relative_to(ROOT)}")


if __name__ == "__main__":
    main()
