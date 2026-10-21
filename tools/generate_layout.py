#!/usr/bin/env python3
"""Reference implementation of the chapter 11 deck layout procedure.

Rolls the same d6 tables the paper procedure uses (content/layouts.yaml), with a seed, and writes
the deck in the scenario format so tools/print_scenario.py can print it. The player's choices in
the paper procedure ("toward the side with more open board") are made the same way here; true
ties are settled by a die roll.

Usage (from the repo root):
    python3 tools/generate_layout.py freighter --seed 7 [--out docs/scenarios/decks/x.yaml]
    python3 tools/generate_layout.py --check [--n 2000]     # statistics over many seeds

The app's generator (Phase 8) must implement the same tables; this script is the yardstick.
"""
import argparse
import random
import sys
from collections import Counter
from pathlib import Path

import yaml

ROOT = Path(__file__).resolve().parent.parent
L = yaml.safe_load((ROOT / "content" / "layouts.yaml").read_text())
ROOMS = {r["id"]: r for r in L["rooms"]}
VESSELS = {v["id"]: v for v in L["vessels"]}
HAZARDS = {h["id"]: h for h in L["hazards"]}
OWNED = {t["id"]: t["owned"] for t in L["tiles"]}

DIRS = {"E": (1, 0), "N": (0, 1), "W": (-1, 0), "S": (0, -1)}
LEFT = {"E": "N", "N": "W", "W": "S", "S": "E"}
RIGHT = {v: k for k, v in LEFT.items()}


def lookup(table, roll):
    for row in table:
        lo, _, hi = str(row["roll"]).partition("-")
        if int(lo) <= roll <= int(hi or lo):
            return row
    raise ValueError(f"no row for {roll}")


class Deck:
    def __init__(self, vessel_id, seed):
        self.v = VESSELS[vessel_id]
        self.rng = random.Random(seed)
        self.seed = seed
        self.W, self.H = self.v["board"][0] // 3, self.v["board"][1] // 3
        self.cells = {}            # (x, y) -> tile index
        self.tiles = []            # dicts: kind, cells, id, room
        self.doors = []            # (tile a, tile b, cell a, cell b, kind)
        self.rolls = 0
        self.log = []
        self.notes = []

    # ------------------------------------------------------------ dice and geometry
    def d6(self, what=""):
        self.rolls += 1
        r = self.rng.randint(1, 6)
        self.log.append(f"{what}: {r}")
        return r

    def free(self, cells):
        return all(0 <= x < self.W and 0 <= y < self.H and (x, y) not in self.cells for x, y in cells)

    def open_space(self, cell, d, depth=3):
        """How much open board lies beyond cell in direction d (for 'the side with more room')."""
        dx, dy = DIRS[d]
        n = 0
        x, y = cell
        for _ in range(depth):
            x, y = x + dx, y + dy
            if not self.free([(x, y)]):
                break
            n += 1
        return n

    def place(self, kind, cells, room=None):
        idx = len(self.tiles)
        for c in cells:
            self.cells[c] = idx
        self.tiles.append({"kind": kind, "cells": cells, "room": room})
        return idx

    def connect(self, a, b, ca, cb, kind="door"):
        self.doors.append({"a": a, "b": b, "ca": ca, "cb": cb, "kind": kind})

    def side_order(self, cell, sides, depth=2):
        """Sides sorted by open board (looking one room deep for rooms, further for the spine), most first;
        ties are the player's choice, made here by die roll."""
        keyed = [(self.open_space(cell, s, depth), self.rng.random(), s) for s in sides]
        return [s for *_, s in sorted(keyed, reverse=True)]

    # ------------------------------------------------------------ procedure
    def build(self):
        v = self.v
        # 1. Entry and docking passage.
        x0, y0 = 0, (self.H - 1) // 2      # middle of the west (short, or any) edge
        d = "E"
        dock = self.place("passage", [(x0, y0), (x0 + 1, y0)], room="docking")
        self.entry_cell = (x0, y0)
        self.spine = [dock]
        head, last = (x0 + 1, y0), dock

        # 2. Spine: junction and passage pairs.
        for k in range(v["spine"]):
            dx, dy = DIRS[d]
            j = (head[0] + dx, head[1] + dy)
            if not self.free([j]):
                break
            ji = self.place("junction", [j])
            self.connect(last, ji, head, j, "open")
            self.spine.append(ji)
            if k > 0 and lookup(L["spine_turn"], self.d6(f"turn at junction {k + 1}"))["result"] == "turn":
                d = self.side_order(j, [LEFT[d], RIGHT[d]], 4)[0]
            placed = False
            for nd in [d] + self.side_order(j, [LEFT[d], RIGHT[d]], 4):
                ndx, ndy = DIRS[nd]
                cells = [(j[0] + ndx, j[1] + ndy), (j[0] + 2 * ndx, j[1] + 2 * ndy)]
                if self.free(cells):
                    pi = self.place("passage", cells)
                    self.connect(ji, pi, j, cells[0], "open")
                    self.spine.append(pi)
                    d, head, last, placed = nd, cells[1], pi, True
                    break
            if not placed:
                head, last = j, ji
                break
        self.spine_dir_end = d

        # 3. The heart at the far end.
        heart = self.attach_compartment(last, head, d, room=v["heart"], heart=True)
        if heart is None:
            for t in reversed(self.spine):
                heart = self.compartment_beside(t, room=v["heart"], heart=True)
                if heart is not None:
                    break
        self.heart = heart

        # 4. Branches along the spine.
        cmin, cmax = v["compartments"]
        for t in self.spine:
            tile = self.tiles[t]
            if tile["room"] == "docking":
                continue
            if tile["kind"] == "passage":
                r = lookup(L["passage_branch"], self.d6(f"passage branch (tile {t})"))["result"]
                sides = self.passage_sides(t)
                if r == "compartment":
                    self.try_compartments(t, sides[:1] + sides[1:], 1, cmax)
                elif r == "compartment_both":
                    self.try_compartments(t, sides, 2, cmax)
                elif r == "compartment_and_small":
                    got = self.try_compartments(t, sides, 1, cmax)
                    rest = [s for s in sides if s not in got]
                    if rest:
                        self.small_beside(t, rest)
            else:
                roll = min(6, self.d6(f"junction branch (tile {t})") + v.get("junction_bonus", 0))
                r = lookup(L["junction_branch"], roll)["result"]
                sides = self.junction_sides(t)
                if r == "small" and sides:
                    self.small_beside(t, sides)
                elif r == "side_passage" and sides:
                    self.side_passage(t, sides, cmax)

        # 5. Fill to the minimum, from the far end back.
        for t in reversed(self.spine):
            if self.count_compartments() >= cmin:
                break
            if self.tiles[t]["room"] == "docking":
                continue
            self.compartment_beside(t)
        if self.count_compartments() < cmin:
            self.notes.append("Fewer compartments than the minimum fit: draw the rest on the dry-erase map.")

        # 6. Rooms, second doors, door states, cover, hazard.
        self.roll_rooms()
        self.loops()
        self.roll_doors()
        h = lookup(L["hazard_roll"], self.d6("hazard"))["result"]
        self.hazard = None if h == "none" else v["hazards"][0 if h == "first" else 1]
        return self

    def count_compartments(self):
        return sum(1 for t in self.tiles if t["kind"] == "compartment")

    def passage_sides(self, t):
        a, b = self.tiles[t]["cells"]
        axis = "E" if a[1] == b[1] else "N"
        sides = [LEFT[axis], RIGHT[axis]]
        return self.side_order(a, sides)

    def junction_sides(self, t):
        c = self.tiles[t]["cells"][0]
        out = []
        for s, (dx, dy) in DIRS.items():
            n = (c[0] + dx, c[1] + dy)
            if self.free([n]):
                out.append(s)
        return self.side_order(c, out)

    def try_compartments(self, t, sides, want, cmax):
        got = []
        for s in sides:
            if len(got) >= want or self.count_compartments() >= cmax:
                break
            if self.compartment_on_passage(t, s) is not None:
                got.append(s)
        return got

    def compartment_on_passage(self, t, s, room=None):
        a, b = self.tiles[t]["cells"]
        dx, dy = DIRS[s]
        cells = [(a[0] + dx, a[1] + dy), (b[0] + dx, b[1] + dy), (a[0] + 2 * dx, a[1] + 2 * dy), (b[0] + 2 * dx, b[1] + 2 * dy)]
        if not self.free(cells):
            return None
        ci = self.place("compartment", cells, room=room)
        self.connect(t, ci, (a, b), (cells[0], cells[1]), "door")
        return ci

    def attach_compartment(self, t, cell, d, room=None, heart=False):
        """A compartment against one cell face (a junction side or a passage end), offset either way."""
        dx, dy = DIRS[d]
        tx, ty = DIRS[LEFT[d]]
        n1 = (cell[0] + dx, cell[1] + dy)
        options = []
        for off in (0, -1):
            base = (n1[0] + off * tx, n1[1] + off * ty)
            cells = [base, (base[0] + tx, base[1] + ty), (base[0] + dx, base[1] + dy), (base[0] + tx + dx, base[1] + ty + dy)]
            if self.free(cells):
                options.append(cells)
        if not options:
            return None
        cells = options[0] if len(options) == 1 or self.rng.random() < 0.5 else options[1]
        ci = self.place("compartment", cells, room=room)
        if heart:
            self.tiles[ci]["heart"] = True
        self.connect(t, ci, cell, n1, "door")
        return ci

    def compartment_beside(self, t, room=None, heart=False):
        tile = self.tiles[t]
        if tile["kind"] == "passage":
            for s in self.passage_sides(t):
                ci = self.compartment_on_passage(t, s, room)
                if ci is not None:
                    if heart:
                        self.tiles[ci]["heart"] = True
                    return ci
        else:
            for s in self.junction_sides(t):
                ci = self.attach_compartment(t, tile["cells"][0], s, room, heart)
                if ci is not None:
                    return ci
        return None

    def small_beside(self, t, sides):
        tile = self.tiles[t]
        cells = tile["cells"]
        for s in sides:
            dx, dy = DIRS[s]
            for c in cells:
                n = (c[0] + dx, c[1] + dy)
                if self.free([n]):
                    si = self.place("small", [n])
                    self.connect(t, si, c, n, "door")
                    return si
        return None

    def side_passage(self, t, sides, cmax):
        c = self.tiles[t]["cells"][0]
        for s in sides:
            dx, dy = DIRS[s]
            cells = [(c[0] + dx, c[1] + dy), (c[0] + 2 * dx, c[1] + 2 * dy)]
            if not self.free(cells):
                continue
            pi = self.place("passage", cells, room="side")
            self.connect(t, pi, c, cells[0], "open")
            if self.count_compartments() < cmax:
                if self.attach_compartment(pi, cells[1], s) is None:
                    self.small_beside(pi, [s])
            else:
                self.small_beside(pi, [s])
            return pi
        return None

    def roll_rooms(self):
        used = set()
        for t in self.tiles:
            if t["kind"] == "compartment":
                if t.get("heart"):
                    t["room"] = self.v["heart"]
                    used.add(t["room"])
                    continue
                roll = self.d6("compartment room")
                for step in range(6):
                    room = lookup(self.v["rooms"], (roll - 1 + step) % 6 + 1)["room"]
                    if not (ROOMS[room].get("unique") and room in used):
                        break
                t["room"] = room
                used.add(room)
            elif t["kind"] == "small":
                t["room"] = lookup(L["small_rooms"], self.d6("small room"))["room"]

    def loops(self):
        for i, t in enumerate(self.tiles):
            if t["kind"] != "compartment":
                continue
            linked = {d["a"] if d["b"] == i else d["b"] for d in self.doors if i in (d["a"], d["b"])}
            for c in t["cells"]:
                for dx, dy in DIRS.values():
                    n = (c[0] + dx, c[1] + dy)
                    j = self.cells.get(n)
                    if j is None or j == i or j in linked or self.tiles[j]["kind"] == "small":
                        continue
                    linked.add(j)
                    if lookup(L["loop_door"], self.d6(f"second door {i}-{j}"))["result"] == "door":
                        self.connect(i, j, c, n, "door")
                    break

    def roll_doors(self):
        for dr in self.doors:
            if dr["kind"] == "open":
                dr["state"] = "open"
                continue
            heart_door = any(self.tiles[x].get("heart") for x in (dr["a"], dr["b"]))
            if heart_door:
                dr["state"] = "locked"
                dr["security"] = self.v["heart_security"]
                continue
            dr["state"] = lookup(L["door_state"], self.d6("door"))["result"]
            if dr["state"] == "locked":
                rooms = [ROOMS.get(self.tiles[x]["room"] or "", {}) for x in (dr["a"], dr["b"])]
                dr["security"] = next((r["security"] for r in rooms if r.get("security")), self.v["security"])

    # ------------------------------------------------------------ output
    def to_scenario(self):
        names = {"passage": "P", "junction": "J", "compartment": "C", "small": "S"}
        counters = Counter()
        tiles = []
        for t in self.tiles:
            k = t["kind"]
            counters[k] += 1
            t["id"] = f'{names[k]}{counters[k]}'
            xs = [c[0] for c in t["cells"]]
            ys = [c[1] for c in t["cells"]]
            x, y = min(xs) * 3, min(ys) * 3
            w, h = (max(xs) - min(xs) + 1) * 3, (max(ys) - min(ys) + 1) * 3
            if t["room"] == "docking":
                name = "Docking passage"
            elif k in ("passage", "junction"):
                name = "Passage" if k == "passage" else "Junction"
            else:
                name = ROOMS[t["room"]]["name"]
            size = {"compartment": "6x6", "passage": "6x3", "junction": "3x3", "small": "3x3"}[k]
            tiles.append({"id": t["id"], "name": name, "size": size, "x": x, "y": y, "w": w, "h": h})
            t["box"] = (x, y, w, h)
        doors = []
        for n, dr in enumerate(self.doors, 1):
            if dr["kind"] == "open" and dr["a"] in self.spine and dr["b"] in self.spine:
                continue   # spine joints are plain openings
            ca, cb = dr["ca"], dr["cb"]
            if isinstance(ca, tuple) and isinstance(ca[0], tuple):   # passage long side: middle of the 6" wall
                (a1, a2), (b1, _) = ca, cb
                mx, my = (a1[0] + a2[0] + 1) * 1.5, (a1[1] + a2[1] + 1) * 1.5
                if b1[1] != a1[1]:
                    y = (max(a1[1], b1[1])) * 3
                    doors.append({"x": mx, "y": y, "orientation": "h"})
                else:
                    x = (max(a1[0], b1[0])) * 3
                    doors.append({"x": x, "y": my, "orientation": "v"})
            else:
                if ca[1] != cb[1]:
                    doors.append({"x": ca[0] * 3 + 1.5, "y": max(ca[1], cb[1]) * 3, "orientation": "h"})
                else:
                    doors.append({"x": max(ca[0], cb[0]) * 3, "y": ca[1] * 3 + 1.5, "orientation": "v"})
            doors[-1].update({"state": dr["state"]})
            if dr.get("security"):
                doors[-1]["security"] = dr["security"]
            doors[-1]["note"] = f'{self.tiles[dr["a"]]["id"]} to {self.tiles[dr["b"]]["id"]}'
        for i, dd in enumerate(doors, 1):
            dd["id"] = f"D{i}"
            for k in ("x", "y"):
                dd[k] = round(dd[k], 2)
        doors = [{"id": dd.pop("id"), **dd} for dd in doors]

        cover, n = [], 0
        door_pts = [(dd["x"], dd["y"]) for dd in doors]
        for t in self.tiles:
            if t["kind"] not in ("compartment", "small") or not t["room"]:
                continue
            x, y, w, h = t["box"]
            pieces = ROOMS[t["room"]].get("cover", [])
            heavy = [p for p in pieces if p["type"] == "heavy"]
            light = [p for p in pieces if p["type"] == "light"]
            if t["kind"] == "small":
                slots = [(x + 0.9, y + 0.9)]
            else:
                corners = [(x + 1.5, y + 1.5), (x + 4.5, y + 4.5), (x + 1.5, y + 4.5), (x + 4.5, y + 1.5)]
                # corners farthest from the doors first
                corners.sort(key=lambda p: -min(((p[0] - dx) ** 2 + (p[1] - dy) ** 2 for dx, dy in door_pts), default=0))
                slots = corners
            centre = (x + w / 2, y + h / 2)
            plan = [(p, centre) for p in heavy[:1]] + list(zip(light, slots))
            for p, (cx, cy) in plan:
                n += 1
                cover.append({"id": f"T{n}", "x": round(cx - p["w"] / 2, 2), "y": round(cy - p["h"] / 2, 2), "w": p["w"], "h": p["h"],
                              "type": p["type"], "label": p["label"], "piece": p["piece"], "room": t["id"]})

        heart = self.tiles[self.heart] if self.heart is not None else None
        ex, ey = self.entry_cell
        out = {
            "id": f"deck-{self.v['id']}-{self.seed}",
            "title": f"{self.v['name']}, deck {self.seed}",
            "rules_version": L["version"].rsplit(".", 1)[0],
            "kind": "deck",
            "vessel": self.v["id"],
            "seed": self.seed,
            "situation": self.v["text"],
            "board": {"width": self.v["board"][0], "height": self.v["board"][1]},
            "tiles": tiles,
            "doors": doors,
            "entry": {"x": ex * 3, "y": ey * 3 + 1.5, "orientation": "v", "label": "Airlock (entry and exit)"},
            "cover": cover,
            "hazard": HAZARDS[self.hazard]["text"] if self.hazard else "None",
            "rolls": self.rolls,
            "notes": self.notes,
        }
        if heart:
            hx, hy, hw, hh = heart["box"]
            out["objective"] = {"x": hx + hw / 2, "y": hy + hh * 0.75 if hy > 0 else hy + hh * 0.25, "label": ROOMS[heart["room"]]["name"]}
        out["tile_counts"] = dict(Counter(("junction" if t["kind"] == "small" else t["kind"]) for t in self.tiles))
        return out


# ---------------------------------------------------------------- checks

def connected(deck):
    adj = {i: set() for i in range(len(deck.tiles))}
    for d in deck.doors:
        adj[d["a"]].add(d["b"]); adj[d["b"]].add(d["a"])
    seen, stack = {0}, [0]
    while stack:
        for j in adj[stack.pop()]:
            if j not in seen:
                seen.add(j); stack.append(j)
    return len(seen) == len(deck.tiles)


def check(n):
    report = []
    for vid in VESSELS:
        comps, rolls, loops, under, fail, over = Counter(), [], 0, 0, 0, 0
        maxuse = Counter()
        for seed in range(1, n + 1):
            d = Deck(vid, seed).build()
            s = d.to_scenario()
            if not connected(d):
                fail += 1
            c = d.count_compartments()
            comps[c] += 1
            rolls.append(d.rolls)
            under += c < d.v["compartments"][0]
            loops += len(d.doors) - len(d.tiles) + 1 > 0
            for k, v in s["tile_counts"].items():
                maxuse[k] = max(maxuse[k], v)
                over += v > OWNED[k]
        report.append(f"| {VESSELS[vid]['name']} | {sum(k * v for k, v in comps.items()) / n:.1f} "
                      f"({min(comps)}–{max(comps)}) | {under / n * 100:.0f}% | {loops / n * 100:.0f}% | "
                      f"{sum(rolls) / n:.0f} | {maxuse['compartment']} / {maxuse['passage']} / {maxuse['junction']} | {fail} |")
        if fail or over:
            print(f"FAIL: {vid}: {fail} disconnected, {over} over the tile inventory", file=sys.stderr)
            sys.exit(1)
    print(f"| Vessel | Compartments, mean (range) | Below minimum | With a loop | d6 rolls | Most tiles used (6x6 / 6x3 / 3x3) | Disconnected |")
    print("|---|---|---|---|---|---|---|")
    print("\n".join(report))


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("vessel", nargs="?", choices=list(VESSELS))
    ap.add_argument("--seed", type=int, default=1)
    ap.add_argument("--out")
    ap.add_argument("--check", action="store_true")
    ap.add_argument("--n", type=int, default=2000)
    ap.add_argument("--trace", action="store_true", help="print every roll")
    a = ap.parse_args()
    if a.check:
        check(a.n)
        return
    if not a.vessel:
        ap.error("vessel required")
    deck = Deck(a.vessel, a.seed).build()
    s = deck.to_scenario()
    if a.trace:
        print("\n".join(deck.log), file=sys.stderr)
    text = yaml.safe_dump(s, sort_keys=False, allow_unicode=True, width=120)
    if a.out:
        Path(a.out).parent.mkdir(parents=True, exist_ok=True)
        Path(a.out).write_text(text)
        print(f"Wrote {a.out} ({s['rolls']} rolls)")
    else:
        print(text)


if __name__ == "__main__":
    main()
