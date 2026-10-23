#!/usr/bin/env python3
"""Reference implementation of the chapter 13 mission procedure.

Rolls a mission type, threat tier, board (a chapter 11 deck aboard ship, a chapter 12 table on land),
complication, guard and contact chips, from content/missions.yaml, with a seed. Writes the mission in the
scenario format (kind: mission) so tools/print_scenario.py can print the sheet, dashboard and cards.

Usage (from the repo root):
    python3 tools/generate_mission.py ship --seed 42 [--vessel freighter] [--mission capture] [--tier 1] [--opposition separatist]
    python3 tools/generate_mission.py land --seed 3 [--environment desert] [--size small|large]
    ... [--out docs/scenarios/missions/x.yaml]

Choices the paper procedure leaves to the player ("which way each piece faces", ties between equally far
compartments) are settled by the seed. The app's generator (Phase 8) must follow the same tables.
"""
import argparse
import math
import random
import sys
from pathlib import Path

import yaml

sys.path.insert(0, str(Path(__file__).resolve().parent))
import generate_layout as gl  # noqa: E402

ROOT = gl.ROOT
C = {f.stem: yaml.safe_load(f.read_text()) for f in sorted((ROOT / "content").glob("*.yaml"))}
M = C["missions"]
T = C["terrain"]
MISSIONS = {m["id"]: m for m in M["missions"]}
TIERS = {t["tier"]: t for t in M["tiers"]}
OPPOSITIONS = {o["id"]: o for o in M.get("oppositions", [])}
ELEMENTS = {e["id"]: e for e in C["factions"]["elements"]}
FACTIONS = {f["id"]: f["name"] for f in C["factions"]["factions"]}
TROOPS = {t["id"]: t for t in C["troops"]["troops"]}
ENVS = {e["id"]: e for e in T["environments"]}
TYPES = {t["id"]: t for t in T["types"]}
SPECIAL = {c["id"] for c in M["contact_results"]}
SIZES = {z["id"]: z for z in M["clock"]["sizes"]}

# Chapter 12 terrain type -> map drawing class (tools/print_scenario.py).
DRAW = {"scrub": "area", "woods": "area", "rubble": "area", "low_wall": "light", "hedge": "light", "wire": "wire",
        "solid": "heavy", "building": "heavy", "bunker": "heavy", "crater": "ditch", "hill": "hill", "water": "water"}
# Where models cannot stand (guards, contact chips, objectives).
NO_STAND = {"heavy", "light", "wire", "water"}


def lookup(table, roll):
    return gl.lookup(table, roll)


class Mission:
    def __init__(self, setting, seed, mission=None, tier=None, vessel=None, environment=None, size="small", opposition=None):
        self.opposition = opposition
        self.tiers = {t["tier"]: t for t in OPPOSITIONS[opposition]["tiers"]} if opposition else TIERS
        self.setting, self.seed = setting, seed
        self.rng = random.Random(f"mission-{setting}-{seed}")
        self.want = dict(mission=mission, tier=tier, vessel=vessel, environment=environment)
        self.size = SIZES[size]
        self.log = []

    def d6(self, what):
        r = self.rng.randint(1, 6)
        self.log.append(f"{what}: {r}")
        return r

    # ------------------------------------------------------------ geometry
    def free_point(self, box, target, taken, blocked, pref=0.0, rmax=None, spacing=1.3):
        """The free point in box nearest to pref inches from target (on a 0.5" grid), clear of blocked and taken."""
        x0, y0, w, h = box
        best, best_key = None, None
        steps_x, steps_y = int((w - 1.2) / 0.5) + 1, int((h - 1.2) / 0.5) + 1
        for i in range(max(steps_x, 1)):
            for j in range(max(steps_y, 1)):
                px, py = x0 + 0.6 + i * 0.5, y0 + 0.6 + j * 0.5
                if any(bx - 0.5 <= px <= bx + bw + 0.5 and by - 0.5 <= py <= by + bh + 0.5 for bx, by, bw, bh in blocked):
                    continue
                if any(math.dist((px, py), q) < spacing for q in taken):
                    continue
                dist = math.dist((px, py), target)
                if rmax is not None and dist > rmax:
                    continue
                key = (abs(dist - pref), py, px)
                if best_key is None or key < best_key:
                    best, best_key = (round(px, 2), round(py, 2)), key
        return best

    # ------------------------------------------------------------ steps
    def roll_type_and_tier(self):
        if self.want["mission"]:
            self.mission = MISSIONS[self.want["mission"]]
            self.log.append(f"Mission type: named, {self.mission['name']}")
        else:
            self.mission = MISSIONS[lookup(M["mission_roll"], self.d6("Mission type"))["mission"]]
        if self.want["tier"]:
            self.tier = self.tiers[self.want["tier"]]
            self.log.append(f"Threat tier: named, {self.tier['tier']}")
        else:
            self.tier = self.tiers[lookup(M["tier_roll"], self.d6("Threat tier"))["tier"]]

    def roll_complication(self):
        self.complication = lookup(M["complications"], self.d6("Complication"))

    def roll_guard(self):
        while True:
            r = lookup(self.tier["table"], self.d6("Guard"))["result"]
            if r not in SPECIAL:
                return ELEMENTS[r]

    def build(self):
        self.roll_type_and_tier()
        if self.setting == "ship":
            self.build_ship()
        else:
            self.build_land()
        self.roll_complication()
        if self.setting == "ship":
            hz = gl.HAZARDS
            if self.complication_is("hazard"):
                first = self.deck.v["hazards"][0]
                if self.deck.hazard != first:
                    self.deck.hazard = first
                    self.board["hazard"] = hz[first]["text"]
                    self.log.append("Complication: the vessel's first hazard replaces the deck's hazard roll")
            self.ground = f'{self.deck.v["name"]}, deck {self.seed}. Hazard: {self.board["hazard"]}'
        else:
            if self.complication_is("hazard"):
                self.conditions = next(x for x in T["conditions"] if x["id"] == "night")
                self.log.append("Complication: Night replaces the conditions roll")
            cond = self.conditions
            self.ground = (f'{self.env["name"]}, {self.W}" × {self.H}" table ({self.cols} × {self.rows} sectors). '
                           f'Conditions: {cond["name"]}: {cond["text"]}')
        self.place_forces()
        return self

    # ------------------------------------------------------------ ship
    def build_ship(self):
        vid = self.want["vessel"]
        if not vid:
            vid = lookup(gl.L["vessel_roll"], self.d6("Vessel"))["vessel"]
        self.deck = gl.Deck(vid, self.seed).build()
        self.board = self.deck.to_scenario()
        self.log.append(f"Deck: {self.deck.v['name']}, seed {self.seed} ({self.deck.rolls} rolls, chapter 11)")
        d = self.deck
        adj = {i: set() for i in range(len(d.tiles))}
        for dr in d.doors:
            adj[dr["a"]].add(dr["b"]); adj[dr["b"]].add(dr["a"])
        self.adj = adj
        self.hops = self.route_from(0)
        tiles = self.board["tiles"]
        self.boxes = [(t["x"], t["y"], t["w"], t["h"]) for t in tiles]
        ex, ey = self.board["entry"]["x"], self.board["entry"]["y"]
        self.far = lambda i: (self.hops[i], math.dist(self.centre(i), (ex, ey)), i)

        comps = [i for i, t in enumerate(d.tiles) if t["kind"] == "compartment"]
        heart = d.heart if d.heart is not None else max(comps, key=self.far)
        mid = self.mission["id"]
        if mid in ("seize", "capture"):
            objs = [heart]
        elif mid == "recover":
            others = [i for i in comps if i != heart]
            objs = [max(others, key=self.far) if others else heart]
        else:   # sabotage: the heart and the compartment farthest from it by route
            from_heart = self.route_from(heart)
            others = [i for i in comps if i != heart] or [i for i, t in enumerate(d.tiles) if t["kind"] in ("junction", "small") and i != heart]
            objs = [heart, max(others, key=lambda i: (from_heart[i], i))]
        self.obj_tiles = objs
        self.blocked = [(c["x"], c["y"], c["w"], c["h"]) for c in self.board["cover"]]
        self.objectives = []
        for n, i in enumerate(objs, 1):
            if i == d.heart and self.board.get("objective"):
                o = self.board["objective"]
                pt = (o["x"], o["y"])
            else:
                pt = self.free_point(self.boxes[i], self.centre(i), [], self.blocked)
            self.objectives.append({"n": n, "x": pt[0], "y": pt[1], "tile": tiles[i]["id"],
                                    "label": f'{tiles[i]["name"]} ({tiles[i]["id"]})',
                                    "prose": f'the {tiles[i]["name"].lower()} ({tiles[i]["id"]})'})
        spine_far = max(d.spine, key=self.far)
        self.enemy_entry = {"x": self.centre(spine_far)[0], "y": self.centre(spine_far)[1], "tile": tiles[spine_far]["id"],
                            "label": f'Enemy entry and exit ({tiles[spine_far]["id"]})'}
        self.second_exits = [f'{tiles[i]["name"]} ({tiles[i]["id"]})' for i, t in enumerate(d.tiles)
                             if t["room"] in ("secondary_airlock", "escape_pod")]
        # contact places: compartments (not objectives), farthest first; then junctions and small rooms
        first = sorted([i for i in comps if i not in objs], key=self.far, reverse=True)
        second = sorted([i for i, t in enumerate(d.tiles) if t["kind"] in ("junction", "small") and i not in objs and i not in d.spine[:2]],
                        key=self.far, reverse=True)
        self.places = [(self.boxes[i], tiles[i]["id"]) for i in first + second]

    def route_from(self, start):
        dist, frontier = {start: 0}, [start]
        while frontier:
            nxt = []
            for a in frontier:
                for b in self.adj[a]:
                    if b not in dist:
                        dist[b] = dist[a] + 1
                        nxt.append(b)
            frontier = nxt
        return {i: dist.get(i, 99) for i in self.adj}

    def centre(self, i):
        x, y, w, h = self.boxes[i]
        return (x + w / 2, y + h / 2)

    # ------------------------------------------------------------ land
    def build_land(self):
        eid = self.want["environment"]
        if not eid:
            eid = lookup(T["environment_roll"], self.d6("Environment"))["environment"]
        env = ENVS[eid]
        W = H = 36 if self.size["id"] == "small" else 48
        cols, rows = W // 12, H // 12
        sectors = {}
        for r in range(rows):
            for c in range(cols):
                name = f"{'ABCDE'[c]}{r + 1}"
                row = lookup(env["table"], self.d6(f"Sector {name}"))
                sectors[(c, r)] = {"name": name, "text": row["text"], "pieces": row["pieces"]}
        # density check: three sectors in a line with no cover -> the middle one becomes the fill
        changed = True
        while changed:
            changed = False
            lines = [[(c, r) for c in range(cols)] for r in range(rows)] + [[(c, r) for r in range(rows)] for c in range(cols)]
            for line in lines:
                for k in range(len(line) - 2):
                    run = line[k:k + 3]
                    if all(not sectors[s]["pieces"] for s in run):
                        mid = sectors[run[1]]
                        mid.update(text=f'{env["fill"]} (density check)', pieces=env["fill_pieces"])
                        self.log.append(f'Density check: sector {mid["name"]} becomes the fill')
                        changed = True
        cover, n = [], 0
        for (c, r), s in sorted(sectors.items(), key=lambda kv: (kv[0][1], kv[0][0])):
            cx, cy = c * 12 + 6, r * 12 + 6
            placed = []
            for k, pc in enumerate(s["pieces"]):
                w, h = pc["w"], pc["h"]
                if w != h and self.rng.random() < 0.5:   # which way it faces: the player's choice in the rules
                    w, h = h, w
                if k == 0:
                    x, y = cx - w / 2, cy - h / 2
                elif pc["type"] == "solid" and s["pieces"][0]["type"] == "hill":   # rocks on the hill
                    off = [(-2, 0), (2, 0), (0, 1.5)][k - 1]
                    x, y = cx + off[0] - w / 2, cy + off[1] - h / 2
                else:
                    x = y = None
                    for ox, oy in [(0, 4.5), (0, -4.5), (4.5, 0), (-4.5, 0), (4, 4), (-4, -4), (4, -4), (-4, 4)]:
                        tx = min(max(cx + ox - w / 2, c * 12 + 0.3), c * 12 + 11.7 - w)
                        ty = min(max(cy + oy - h / 2, r * 12 + 0.3), r * 12 + 11.7 - h)
                        if not any(tx < px + pw and px < tx + w and ty < py + ph and py < ty + h for px, py, pw, ph in placed):
                            x, y = tx, ty
                            break
                    if x is None:
                        x, y = cx - w / 2, cy - h / 2
                placed.append((x, y, w, h))
                n += 1
                cover.append({"id": f"T{n}", "x": round(x, 2), "y": round(y, 2), "w": w, "h": h, "type": DRAW[pc["type"]],
                              "label": pc["label"], "terrain": pc["type"],
                              "piece": f'{TYPES[pc["type"]]["name"]}. Sector {s["name"]}: {s["text"]}', "sector": s["name"]})
        self.conditions = lookup(T["conditions"], self.d6("Conditions"))
        self.env, self.sectors, self.W, self.H, self.cols, self.rows = env, sectors, W, H, cols, rows
        self.blocked = [(cv["x"], cv["y"], cv["w"], cv["h"]) for cv in cover if cv["type"] in NO_STAND]
        self.board = {"board": {"width": W, "height": H}, "tiles": [], "doors": [], "cover": cover}

        far = rows - 1
        mid = self.mission["id"]
        if mid in ("seize", "capture"):
            middle = cols // 2 if cols % 2 else cols // 2 - (self.d6("Middle sector (even row): 1–3 left, 4–6 right") <= 3)
            obj_cols = [middle]
        elif mid == "recover":
            r = self.d6("Hostage sector")
            obj_cols = [0 if r <= 2 else cols // 2 if r <= 4 else cols - 1]
        else:
            obj_cols = [0, cols - 1]
        self.objectives = []
        for n, c in enumerate(obj_cols, 1):
            box = (c * 12, far * 12, 12, 12)
            pt = self.free_point(box, (c * 12 + 6, far * 12 + 6), [], self.blocked)
            self.objectives.append({"n": n, "x": pt[0], "y": pt[1], "tile": sectors[(c, far)]["name"],
                                    "label": f'Sector {sectors[(c, far)]["name"]}', "prose": f'sector {sectors[(c, far)]["name"]}'})
        self.obj_sectors = {(c, far) for c in obj_cols}
        self.enemy_entry = {"x": W / 2, "y": H - 0.4, "tile": "edge", "label": "Enemy entry and exit (middle of the far edge)"}
        self.second_exits = []
        cand = [(c, r) for r in (rows - 1, rows - 2) for c in range(cols) if (c, r) not in self.obj_sectors and r * 12 >= 12]
        cand.sort(key=lambda s: (-math.dist((s[0] * 12 + 6, s[1] * 12 + 6), (W / 2, 0)), s[0]))
        self.places = [((c * 12, r * 12, 12, 12), sectors[(c, r)]["name"]) for c, r in cand]

    def complication_is(self, cid):
        return getattr(self, "complication", None) is not None and self.complication["id"] == cid

    # ------------------------------------------------------------ forces
    def place_forces(self):
        guard = self.roll_guard()
        self.guard = guard
        o = self.objectives[0]
        box = self.boxes[self.obj_tiles[0]] if self.setting == "ship" else (o["x"] - 3.5, o["y"] - 3.5, 7, 7)
        taken = [(x["x"], x["y"]) for x in self.objectives]
        models = [c["troop"] for c in guard["composition"] for _ in range(c["count"])]
        hvt = None
        if self.mission["id"] == "capture":
            hvt = self.tier["hvt"]
            if hvt not in models:
                models.append(hvt)
        self.enemies = []
        for n, troop in enumerate(models, 1):
            pt = self.free_point(box, (o["x"], o["y"]), taken, self.blocked, pref=2.0, rmax=3.0) or \
                 self.free_point(box, (o["x"], o["y"]), taken, self.blocked, pref=2.0, spacing=1.0)
            taken.append(pt)
            state = ("Alerted" if guard["behavior"] == "automated" else "Unaware") + (" (the HVT)" if troop == hvt else "")
            self.enemies.append({"n": n, "troop": troop, "element": guard["id"], "x": pt[0], "y": pt[1], "state": state})
        k = self.tier["contacts"] + (1 if self.complication_is("extra_contact") else 0)
        self.contacts = []
        chip_taken = list(taken)
        for i in range(k):
            box, where = self.places[i % len(self.places)]
            bx, by, bw, bh = box
            # A second chip in a small room may not fit 2" from the first: then 1", as for the guard.
            pt = self.free_point(box, (bx + bw / 2, by + bh / 2), chip_taken, self.blocked, spacing=2.0) or \
                 self.free_point(box, (bx + bw / 2, by + bh / 2), chip_taken, self.blocked, spacing=1.0)
            chip_taken.append(pt)
            self.contacts.append({"n": i + 1, "x": pt[0], "y": pt[1], "where": where})
        self.hvt = hvt

    # ------------------------------------------------------------ output
    def factions(self):
        names = []
        for r in self.tier["table"]:
            if r["result"] in ELEMENTS:
                f = FACTIONS[ELEMENTS[r["result"]]["faction"]]
                if f not in names:
                    names.append(f)
        return " and ".join(names)

    def briefing(self):
        m, objs = self.mission, self.objectives
        where = " and ".join(o["prose"] for o in objs)
        if self.setting == "ship":
            place = f'a {self.deck.v["name"].split(" / ")[0].lower()}'
        else:
            env = self.env["name"].split(",")[0].lower()
            place = f'{"an" if env[0] in "aeiou" else "a"} {env} position'
        holder = self.factions()
        why = {
            "seize": f"Data or cargo the force needs is in {where}. The holders will move it soon.",
            "recover": f"One of ours is held in {where}, bound and under guard. They will be moved soon.",
            "sabotage": f"{where[0].upper() + where[1:]} keep the place running. Both have to go before it can be used against us.",
            "capture": f"The {TROOPS[self.hvt]['name'].lower() if self.hvt else 'target'} is in {where} with a guard. Taken alive, they will talk.",
        }[m["id"]]
        alert0 = 1 if self.complication_is("early_warning") else 0
        limit = self.size["limit"] - (5 if self.complication_is("tight_window") else 0)
        if self.setting == "ship":
            out = f'Out by the airlock (P1).'
        else:
            out = "Out by the player's own edge."
        exits = f' Enemy entry and exit: {self.enemy_entry["label"].split("(")[-1].rstrip(")")}.'
        if self.second_exits:
            exits += " Second enemy exit: " + ", ".join(self.second_exits) + "."
        guard_n = len(self.enemies)
        guard_txt = f'{self.guard["name"]} ({guard_n - (1 if self.hvt and self.hvt not in [c["troop"] for c in self.guard["composition"]] else 0)} models)'
        if self.hvt and self.hvt not in [c["troop"] for c in self.guard["composition"]]:
            guard_txt += f' with the {TROOPS[self.hvt]["name"]}'
        comp = self.complication
        return [
            ("Tasking", f'{m["name"]}. {m["summary"]}'),
            ("Situation", f'{holder} hold {place}. {why}'),
            ("Objective", f'{where[0].upper() + where[1:]}. {m["task"]}'),
            ("Opposition", f'{holder}, threat tier {self.tier["tier"]}. Guard (known): {guard_txt}, at the objective, '
                           + ('Alerted. ' if self.guard['behavior'] == 'automated' else 'Unaware. ')
                           + f'Contacts (unknown): {len(self.contacts)} chips.'),
            ("Ground", self.ground),
            ("Complication", "None." if comp["id"] == "none" else
             f'{comp["name"]}: ' + ("see Ground." if comp["id"] == "hazard" else comp["text"])),
            ("Extraction and time", f'{out}{exits} {self.size["name"]}: {limit} minutes, clock die {self.size["die"]}. '
                                    f'Check-ins every {M["clock"]["check_in_every"]} minutes from minute {M["clock"]["first_check_in"]}. '
                                    f'Alert track starts at {alert0}.'),
            ("Grades", f'Decisive: {m["decisive"]} Marginal: {m["marginal"]} Failure: {m["failure"]}'),
        ]

    def to_scenario(self):
        m = self.mission
        title_place = self.deck.v["name"] if self.setting == "ship" else self.env["name"]
        sid = f'mission-{self.setting}-{self.seed}'
        limit = self.size["limit"] - (5 if self.complication_is("tight_window") else 0)
        out = {
            "id": sid,
            "title": f'{m["name"]}: {title_place.split(" / ")[0]}, mission {self.seed}',
            "kind": "mission",
            "kind_label": "mission",
            "rules_version": M["version"].rsplit(".", 1)[0],
            "setting": self.setting,
            "seed": self.seed,
            "mission": m["id"],
            "tier": self.tier["tier"],
            **({"opposition": self.opposition} if self.opposition else {}),
            "size": self.size["id"],
            "clock": {"die": self.size["die"], "limit": limit, "check_in_every": M["clock"]["check_in_every"],
                      "first_check_in": M["clock"]["first_check_in"]},
            "alert_start": 1 if self.complication_is("early_warning") else 0,
            "complication": self.complication["id"],
            "situation": dict(self.briefing())["Situation"],
            "briefing": [{"heading": h, "text": t} for h, t in self.briefing()],
            "board": self.board["board"],
            "tiles": self.board["tiles"],
            "doors": self.board["doors"],
            "cover": self.board["cover"],
            "objectives": self.objectives,
            "enemy_entry": self.enemy_entry,
            "enemies": self.enemies,
            "contacts": self.contacts,
            "reinforcement": self.tier["reinforcement"],
            "hvt": self.hvt,
            "player": [],
            "rolls": self.log,
        }
        if self.setting == "ship":
            out["entry"] = self.board["entry"]
            out["vessel"] = self.deck.v["id"]
            out["hazard"] = self.board["hazard"]
            out["deck_notes"] = self.board.get("notes", [])
        else:
            out["environment"] = self.env["id"]
            out["conditions"] = self.conditions["id"]
            out["deploy"] = [{"x": 0.3, "y": 0.3, "w": self.W - 0.6, "h": 5.7, "side": "player", "label": "within 6\" of this edge"}]
            out["sectors"] = [{"name": s["name"], "text": s["text"]} for _, s in sorted(self.sectors.items(), key=lambda kv: (kv[0][1], kv[0][0]))]
        return out


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("setting", choices=["ship", "land"])
    ap.add_argument("--seed", type=int, default=1)
    ap.add_argument("--mission", choices=list(MISSIONS))
    ap.add_argument("--tier", type=int, choices=list(TIERS))
    ap.add_argument("--vessel", choices=list(gl.VESSELS))
    ap.add_argument("--environment", choices=list(ENVS))
    ap.add_argument("--size", choices=list(SIZES), default="small")
    ap.add_argument("--opposition", choices=list(OPPOSITIONS), help="another enemy faction's threat tiers (chapter 13)")
    ap.add_argument("--out")
    a = ap.parse_args()
    if a.setting == "ship" and a.size == "large":
        ap.error("decks are small missions (chapter 11 boards hold up to 8 a side)")
    ms = Mission(a.setting, a.seed, a.mission, a.tier, a.vessel, a.environment, a.size, a.opposition).build()
    s = ms.to_scenario()
    text = yaml.safe_dump(s, sort_keys=False, allow_unicode=True, width=120)
    if a.out:
        Path(a.out).parent.mkdir(parents=True, exist_ok=True)
        Path(a.out).write_text(text)
        print(f"Wrote {a.out}")
    else:
        print(text)


if __name__ == "__main__":
    main()
