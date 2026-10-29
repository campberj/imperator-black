#!/usr/bin/env python3
"""Compile content/*.yaml into content/dist/content.json, and campaign definitions into content/dist/campaigns.json.

The app and simulator read the JSON (no YAML dependency in .NET); humans edit the YAML.
Campaign definitions (docs/design/campaign-format.md) are the files with `kind: campaign` in docs/scenarios/arcs/*/arc.yaml
and docs/scenarios/campaigns/*.yaml; a bespoke campaign's mission files are embedded under each mission.
Run from the repo root: python3 tools/build_content.py [--check]
--check exits non-zero if either JSON file is out of date (for CI).
"""
import json
import sys
from pathlib import Path

import yaml

ROOT = Path(__file__).resolve().parent.parent
CONTENT = ROOT / "content"
OUT = CONTENT / "dist" / "content.json"
CAMPAIGNS_OUT = CONTENT / "dist" / "campaigns.json"


def build():
    data = {}
    for f in sorted(CONTENT.glob("*.yaml")):
        data[f.stem] = yaml.safe_load(f.read_text())
    return json.dumps(data, indent=2, ensure_ascii=False) + "\n"


def build_campaigns():
    files = sorted((ROOT / "docs/scenarios/arcs").glob("*/arc.yaml")) + sorted((ROOT / "docs/scenarios/campaigns").glob("*.yaml"))
    campaigns = []
    for f in files:
        c = yaml.safe_load(f.read_text())
        if c.get("kind") != "campaign":
            continue
        c["path"] = str(f.relative_to(ROOT))
        for m in c.get("missions", []):
            m["mission"] = yaml.safe_load((f.parent / m["file"]).read_text())
        campaigns.append(c)
    return json.dumps({"campaigns": campaigns}, indent=2, ensure_ascii=False) + "\n"


def main():
    outputs = [(OUT, build()), (CAMPAIGNS_OUT, build_campaigns())]
    if "--check" in sys.argv:
        stale = [o for o, text in outputs if not o.exists() or o.read_text() != text]
        for o in stale:
            print(f"{o.relative_to(ROOT)} is out of date; run tools/build_content.py")
        if stale:
            sys.exit(1)
        print("content.json and campaigns.json current.")
        return
    OUT.parent.mkdir(exist_ok=True)
    for o, text in outputs:
        o.write_text(text)
        print(f"Wrote {o.relative_to(ROOT)}")


if __name__ == "__main__":
    main()
