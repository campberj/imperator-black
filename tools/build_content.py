#!/usr/bin/env python3
"""Compile content/*.yaml into content/dist/content.json.

The app and simulator read the JSON (no YAML dependency in .NET); humans edit the YAML.
Run from the repo root: python3 tools/build_content.py [--check]
--check exits non-zero if content.json is out of date (for CI).
"""
import json
import sys
from pathlib import Path

import yaml

ROOT = Path(__file__).resolve().parent.parent
CONTENT = ROOT / "content"
OUT = CONTENT / "dist" / "content.json"


def build():
    data = {}
    for f in sorted(CONTENT.glob("*.yaml")):
        data[f.stem] = yaml.safe_load(f.read_text())
    return json.dumps(data, indent=2, ensure_ascii=False) + "\n"


def main():
    text = build()
    if "--check" in sys.argv:
        if not OUT.exists() or OUT.read_text() != text:
            print("content/dist/content.json is out of date; run tools/build_content.py")
            sys.exit(1)
        print("content.json current.")
        return
    OUT.parent.mkdir(exist_ok=True)
    OUT.write_text(text)
    print(f"Wrote {OUT.relative_to(ROOT)}")


if __name__ == "__main__":
    main()
