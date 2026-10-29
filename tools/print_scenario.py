#!/usr/bin/env python3
"""Print a scenario: a scenario sheet with a to-scale map, and cards for just the models in it.

Usage (from the repo root):
    python3 tools/print_scenario.py docs/scenarios/gym-01-first-breach.yaml
    python3 tools/print_scenario.py docs/scenarios/missions/<id>.yaml     # a generated mission (chapter 13)
    python3 tools/print_scenario.py --dashboard                           # the blank mission dashboard

Writes docs/print/ImperatorBlack_<id>-sheet.pdf and docs/print/ImperatorBlack_<id>-cards.pdf.
With --with-unit-cards it also writes ImperatorBlack_<id>-and-unit-cards.pdf: the scenario's cards followed
by every other unit card (Task Force Boone and the opposition), with no troop printed twice.
Troop profiles, weapons and gear come from content/ (via print_sheets.py), so the cards
always match the rules.
"""
import copy
import sys
from pathlib import Path

import yaml
from reportlab.lib import colors
from reportlab.lib.pagesizes import letter
from reportlab.lib.units import inch
from reportlab.pdfgen import canvas
from reportlab.platypus import (Flowable, Frame, KeepInFrame, PageBreak, Paragraph, SimpleDocTemplate,
                                Spacer, Table, TableStyle)

sys.path.insert(0, str(Path(__file__).resolve().parent))
import print_sheets as ps  # noqa: E402  (fonts, styles, content, card drawing)

ROOT = ps.ROOT
TROOPS = ps.by_id(ps.C["troops"]["troops"])
ROOM_DEFS = ps.C["layouts"]["rooms"] if "layouts" in ps.C else []

PLAYER_FILL = colors.HexColor("#27466b")
ENEMY_FILL = colors.HexColor("#b4441f")
TILE_FILL = colors.HexColor("#e9edf1")
TILE_EDGE = colors.HexColor("#3c4650")
GRID = colors.HexColor("#cfd6dc")
DOOR_COLOURS = {"open": colors.HexColor("#7fb27f"), "closed": colors.HexColor("#8a5a12"), "locked": colors.HexColor("#b4441f")}
COVER_FILL = {"light": colors.HexColor("#c8d9b8"), "heavy": colors.HexColor("#7d8a6b"),
              "hill": colors.HexColor("#e3cfa4"), "area": colors.HexColor("#d9e3c4"), "ditch": colors.HexColor("#b98c6a"),
              "water": colors.HexColor("#a9c8e0"), "wire": colors.HexColor("#9a9a9a")}
COVER_ORDER = {"hill": 0, "area": 1, "water": 1, "ditch": 2, "light": 3, "wire": 3, "heavy": 4}
COVER_NAMES = {"light": "Low wall or hedge", "heavy": "Solid or building", "hill": "Raised ground", "area": "Area terrain",
               "ditch": "Crater or trench", "water": "Water", "wire": "Wire"}
CONTACT_FILL = colors.HexColor("#f2f2f2")
OBJECTIVE_FILL = colors.HexColor("#d9a300")


# ---------------------------------------------------------------- map

class MapFlowable(Flowable):
    """Draws the scenario layout to scale, with a 3" grid, doors, cover, deployment, enemies and objective."""

    def __init__(self, s, width):
        super().__init__()
        self.s = s
        self.bw, self.bh = s["board"]["width"], s["board"]["height"]
        self.scale = min(width / self.bw, (4.7 * inch) / self.bh)
        self.width, self.height = width, self.bh * self.scale + 0.15 * inch

    def wrap(self, *_):
        return self.width, self.height

    def p(self, x, y):
        return x * self.scale, y * self.scale + 0.15 * inch

    def draw(self):
        c, s, k = self.canv, self.s, self.scale
        # board edge
        c.setStrokeColor(ps.MUTED)
        c.setDash(2, 3)
        c.setLineWidth(0.5)
        c.rect(*self.p(0, 0), self.bw * k, self.bh * k)
        c.setDash()
        if not s.get("tiles"):   # land: a 6" reference grid over the whole board
            c.setStrokeColor(GRID)
            c.setLineWidth(0.3)
            for gx in range(6, int(self.bw) + 1, 6):
                c.line(*self.p(gx, 0), *self.p(gx, self.bh))
            for gy in range(6, int(self.bh) + 1, 6):
                c.line(*self.p(0, gy), *self.p(self.bw, gy))
            if s.get("sectors"):   # generated land table: 12" sectors, named A1 (bottom left) onward
                c.setStrokeColor(ps.MUTED)
                c.setLineWidth(0.7)
                for gx in range(12, int(self.bw), 12):
                    c.line(*self.p(gx, 0), *self.p(gx, self.bh))
                for gy in range(12, int(self.bh), 12):
                    c.line(*self.p(0, gy), *self.p(self.bw, gy))
                c.setFillColor(ps.MUTED)
                c.setFont("Body-Bold", 7)
                for sec in s.get("sectors", []):
                    col, row = "ABCDE".index(sec["name"][0]), int(sec["name"][1:]) - 1
                    c.drawString(*self.p(col * 12 + 0.3, row * 12 + 11.0), sec["name"])
        # tiles with grid
        for t in s["tiles"]:
            x, y = self.p(t["x"], t["y"])
            c.setFillColor(TILE_FILL)
            c.setStrokeColor(TILE_EDGE)
            c.setLineWidth(2)
            c.rect(x, y, t["w"] * k, t["h"] * k, fill=1, stroke=1)
            c.setStrokeColor(GRID)
            c.setLineWidth(0.4)
            for gx in range(int(t["x"]) + 1, int(t["x"] + t["w"])):
                if gx % 3 == 0:
                    c.line(*self.p(gx, t["y"]), *self.p(gx, t["y"] + t["h"]))
            for gy in range(int(t["y"]) + 1, int(t["y"] + t["h"])):
                if gy % 3 == 0:
                    c.line(*self.p(t["x"], gy), *self.p(t["x"] + t["w"], gy))
        # deployment
        for d in s.get("deploy", []):
            x, y = self.p(d["x"], d["y"])
            c.setFillColor(colors.Color(0.15, 0.27, 0.42, alpha=0.18))
            c.setStrokeColor(PLAYER_FILL)
            c.setLineWidth(0.8)
            c.rect(x, y, d["w"] * k, d["h"] * k, fill=1, stroke=1)
        # cover
        for cv in sorted(s.get("cover", []), key=lambda c: COVER_ORDER.get(c["type"], 9)):
            x, y = self.p(cv["x"], cv["y"])
            c.setFillColor(COVER_FILL[cv["type"]])
            c.setStrokeColor(TILE_EDGE)
            c.setLineWidth(0.5)
            c.rect(x, y, cv["w"] * k, cv["h"] * k, fill=1, stroke=1)
            if cv.get("id"):
                c.setFillColor(colors.white if cv["type"] == "heavy" else ps.INK)
                c.setFont("Body-Bold", 6.5)
                c.drawCentredString(x + cv["w"] * k / 2, y + cv["h"] * k / 2 - 2.3, cv["id"])
        # tile labels
        for t in s["tiles"]:
            x, y = self.p(t["x"] + 0.2, t["y"] + t["h"] - 0.5)
            c.setFillColor(ps.INK)
            c.setFont("Body-Bold", 9)
            c.drawString(x, y, t["id"])
            c.setFont("Body", 7)
            c.drawString(x, y - 9, t["name"])
            c.setFont("Body", 6)
            c.setFillColor(ps.MUTED)
            c.drawString(x, y - 17, f'{t["size"]} tile')
        for d in s.get("deploy", []):
            x, y = self.p(d["x"] + d["w"] / 2, d["y"] + 0.35)
            c.setFillColor(PLAYER_FILL)
            c.setFont("Body-Bold", 7)
            c.drawCentredString(x, y, f'Deploy: {d["label"]}')
        # doors
        for d in s["doors"]:
            self._door(d["x"], d["y"], d["orientation"], DOOR_COLOURS[d["state"]], f'{d["id"]}' + (" (L)" if d["state"] == "locked" else ""))
        e = s.get("entry")
        if e:
            self._door(e["x"], e["y"], e["orientation"], PLAYER_FILL, "")
        # objective(s)
        objs = s.get("objectives") or ([s["objective"]] if s.get("objective") else [])
        for o in objs:
            x, y = self.p(o["x"], o["y"])
            r = 0.3 * k if not s.get("objectives") else 0.45 * k
            c.setFillColor(OBJECTIVE_FILL)
            c.setStrokeColor(ps.INK)
            c.setLineWidth(0.8)
            path = c.beginPath()
            path.moveTo(x, y + r); path.lineTo(x + r, y); path.lineTo(x, y - r); path.lineTo(x - r, y); path.close()
            c.drawPath(path, fill=1, stroke=1)
            if s.get("objectives") and len(objs) > 1:
                c.setFillColor(ps.INK)
                c.setFont("Body-Bold", 6.5)
                c.drawCentredString(x, y - 2.3, str(o["n"]))
        # enemy entry
        ee = s.get("enemy_entry")
        if ee:
            x, y = self.p(ee["x"], ee["y"])
            c.setStrokeColor(ENEMY_FILL)
            c.setLineWidth(1.2)
            c.setDash(2, 1.5)
            c.rect(x - 0.6 * k, y - 0.6 * k, 1.2 * k, 1.2 * k, fill=0, stroke=1)
            c.setDash()
            c.setFillColor(ENEMY_FILL)
            c.setFont("Body-Bold", 6)
            c.drawCentredString(x, y - 2, "EE")
        # contact chips
        for ct in s.get("contacts", []):
            x, y = self.p(ct["x"], ct["y"])
            c.setFillColor(CONTACT_FILL)
            c.setStrokeColor(ps.INK)
            c.setLineWidth(0.9)
            c.setDash(1.5, 1)
            c.circle(x, y, 0.55 * k, fill=1, stroke=1)
            c.setDash()
            c.setFillColor(ps.INK)
            c.setFont("Body-Bold", 7.5)
            c.drawCentredString(x, y - 2.7, f'?{ct["n"]}')
        # enemies
        for en in s.get("enemies", []):
            x, y = self.p(en["x"], en["y"])
            c.setFillColor(ENEMY_FILL)
            c.setStrokeColor(colors.white)
            c.setLineWidth(1)
            c.circle(x, y, 0.45 * k, fill=1, stroke=1)
            c.setFillColor(colors.white)
            c.setFont("Body-Bold", 8)
            c.drawCentredString(x, y - 3, str(en["n"]))

    def _door(self, x, y, orient, colour, label):
        c, k = self.canv, self.scale
        half = 0.75 * k
        px, py = self.p(x, y)
        c.setStrokeColor(colour)
        c.setLineWidth(5)
        if orient == "v":
            c.line(px, py - half, px, py + half)
            lx, ly = px + 4, py - half - 7
        else:
            c.line(px - half, py, px + half, py)
            lx, ly = px + half + 2, py + 3
        c.setFillColor(colour)
        c.setFont("Body-Bold", 6.5)
        c.drawString(lx, ly, label)


def legend(s=None):
    land = bool(s) and not s.get("tiles")
    mission = bool(s) and s.get("kind") == "mission"
    if land:
        types = sorted({c["type"] for c in s.get("cover", [])}, key=lambda k: COVER_ORDER.get(k, 9))
        items = [(COVER_NAMES[k], COVER_FILL[k]) for k in types]
        items += [("Enemy model", ENEMY_FILL), ("Deployment", colors.Color(0.15, 0.27, 0.42, alpha=0.35)), ("Objective", OBJECTIVE_FILL)]
    else:
        items = [("Open door", DOOR_COLOURS["open"]), ("Closed door", DOOR_COLOURS["closed"]), ("Locked door (L)", DOOR_COLOURS["locked"]),
                 ("Light cover", COVER_FILL["light"]), ("Heavy cover", COVER_FILL["heavy"]), ("Enemy model", ENEMY_FILL)]
        if not mission:
            items.append(("Deployment", colors.Color(0.15, 0.27, 0.42, alpha=0.35)))
        items.append(("Objective", OBJECTIVE_FILL))
    if mission:
        items += [("Contact chip (?n)", CONTACT_FILL), ("Enemy entry (EE)", colors.white)]
    while len(items) % 4:
        items.append(("", colors.white))
    rows = []
    for i in range(0, len(items), 4):
        row = []
        for name, col in items[i:i + 4]:
            sw = Table([[""]], colWidths=[8], rowHeights=[8])
            sw.setStyle(TableStyle([("BACKGROUND", (0, 0), (-1, -1), col)] +
                                   ([("BOX", (0, 0), (-1, -1), 0.5, ps.INK)] if name and col in (colors.white, CONTACT_FILL) else [])))
            row += [sw if name else "", Paragraph(name, ps.style(7, colour=ps.MUTED))]
        rows.append(row)
    t = Table(rows, colWidths=[12, 1.1 * inch] * 4)
    t.setStyle(TableStyle([("VALIGN", (0, 0), (-1, -1), "MIDDLE"), ("LEFTPADDING", (0, 0), (-1, -1), 1),
                           ("TOPPADDING", (0, 0), (-1, -1), 1), ("BOTTOMPADDING", (0, 0), (-1, -1), 1)]))
    return t


# ---------------------------------------------------------------- sheet

H1 = ps.style(17, "Body-Bold", leading=20)
H2 = ps.style(10.5, "Body-Bold", leading=13, space_after=2)
B = ps.style(8.8, leading=11.2)
SMALL = ps.style(7.2, leading=9, colour=ps.MUTED)


def bullets(items):
    return [Paragraph("• " + i, B) for i in items]


def model_name(m):
    t = TROOPS[m["troop"]]
    return f'{t["name"]} ({m["label"]})' if m.get("label") else t["name"]


def enemy_name(e):
    t = TROOPS[e["troop"]]
    wid = e.get("weapon", t["weapon"])
    w = ps.RANGED[wid]["name"] if wid else "melee only"
    return f'{e["n"]}. {t["name"]} ({w})'


def sheet(s, out):
    doc = SimpleDocTemplate(str(out), pagesize=letter, leftMargin=0.55 * inch, rightMargin=0.55 * inch,
                            topMargin=0.45 * inch, bottomMargin=0.45 * inch, title=s["title"], author="Imperator Black")
    W = letter[0] - 1.1 * inch
    story = [Paragraph(s["title"], H1),
             Paragraph(f"Imperator Black {s.get('kind_label', 'gym scenario')} · rules v{s['rules_version']} · solo · about 60–90 minutes", SMALL),
             Spacer(1, 6), Paragraph(s["situation"].strip(), B), Spacer(1, 8),
             MapFlowable(s, W), Spacer(1, 4), legend(s),
             Paragraph(s.get("map_note") or f'Scale: each grid square is 3". Board {s["board"]["width"]}" × {s["board"]["height"]}". The blue bar on the west wall of P1 is the airlock: the way in and out. Enemy positions are model centres.', SMALL),
             Spacer(1, 6)]

    pf = ps.FACTIONS[TROOPS[s["player"][0]["troop"]]["faction"]]["name"]
    efs = []
    for e in s["enemies"]:
        n = ps.FACTIONS[TROOPS[e["troop"]]["faction"]]["name"]
        if n not in efs:
            efs.append(n)
    ef = " and ".join(efs)
    forces_player = [Paragraph(f"{pf} ({len(s['player'])})", H2)] + bullets([model_name(m) for m in s["player"]])
    forces_enemy = [Paragraph(f"{ef} ({len(s['enemies'])})", H2)] + bullets([f'{enemy_name(e)}: {e["state"]}' for e in s["enemies"]])
    two = Table([[forces_player, forces_enemy]], colWidths=[W / 2, W / 2])
    two.setStyle(TableStyle([("VALIGN", (0, 0), (-1, -1), "TOP"), ("LEFTPADDING", (0, 0), (-1, -1), 0)]))
    story += [two, Spacer(1, 4), Paragraph("Setup", H2)] + bullets(s["setup"])

    story.append(PageBreak())
    story += terrain_flow(s, W)
    story += [Spacer(1, 6), Paragraph("Special rules", H2)]
    for r in s["special_rules"]:
        story.append(Paragraph(f'<b>{r["title"]}.</b> {" ".join(r["text"].split())}', B))
        story.append(Spacer(1, 2))
    _rest(s, story, W)
    doc.build(story)


def grid_table(rows, widths):
    t = Table([[Paragraph(f"<b>{h}</b>", B) for h in rows[0]]] + [[Paragraph(str(c), B) for c in r] for r in rows[1:]], colWidths=widths)
    t.setStyle(TableStyle([("GRID", (0, 0), (-1, -1), 0.4, ps.RULE), ("VALIGN", (0, 0), (-1, -1), "TOP"),
                           ("BACKGROUND", (0, 0), (-1, 0), colors.HexColor("#e8ecef"))]))
    return t


def terrain_flow(s, W):
    story = [Paragraph("Terrain", H2)]
    if s.get("walls_and_doors"):
        story += [Paragraph(" ".join(s["walls_and_doors"].split()), B), Spacer(1, 3)]
    if s.get("terrain_table") is False:   # scenario opts out of the per-piece table (the map still shows the pieces)
        return story
    rows = [[Paragraph(f"<b>{h}</b>", B) for h in ("Map", "Piece", "Cover", "Footprint", "Suggested terrain")]]
    for cv in s.get("cover", []):
        rows.append([Paragraph(cv.get("id", ""), B), Paragraph(cv["label"], B), Paragraph(cv["type"].capitalize(), B),
                     Paragraph(f'{cv["w"]:g}" × {cv["h"]:g}"', B), Paragraph(cv.get("piece", "Any suitable scenery."), B)])
    tt = Table(rows, colWidths=[0.45 * inch, 0.95 * inch, 0.55 * inch, 0.8 * inch, W - 2.75 * inch])
    tt.setStyle(TableStyle([("GRID", (0, 0), (-1, -1), 0.4, ps.RULE), ("VALIGN", (0, 0), (-1, -1), "TOP"),
                            ("BACKGROUND", (0, 0), (-1, 0), colors.HexColor("#e8ecef"))]))
    story += [tt, Paragraph("Light cover: at least a third of the model hidden. Heavy cover: at least half hidden behind solid material. "
                            "Exact pieces don't matter; keep the footprint and height close.", SMALL)]
    return story


def _rest(s, story, W):
    v = s["victory"]
    story += [Spacer(1, 4), Paragraph("Victory", H2), Paragraph(f'<b>Length:</b> {v["length"]} <b>Goal:</b> {v["goal"]}', B), Spacer(1, 2)]
    for o in v["outcomes"]:
        story.append(Paragraph(f'<b>{o["grade"]}:</b> {o["text"]}', B))

    story += [Spacer(1, 6), Paragraph("The enemy", H2),
              Paragraph("Run the enemy with chapter 10 and the behavior cards: read each list top-down and take the first line that applies.", B),
              Spacer(1, 3)]
    rows = [[Paragraph(f"<b>{h}</b>", B) for h in ("Element", "Models", "Behavior", "Drill")]]
    for el, models in enemy_elements(s):
        rows.append([Paragraph(el["name"], B), Paragraph(", ".join(str(m["n"]) for m in models), B),
                     Paragraph(ps.BEHAVIORS[el["behavior"]]["name"], B), Paragraph(drill_text(el, models, s), B)])
    et = Table(rows, colWidths=[1.6 * inch, 0.8 * inch, 1.1 * inch, W - 3.5 * inch])
    et.setStyle(TableStyle([("GRID", (0, 0), (-1, -1), 0.4, ps.RULE), ("VALIGN", (0, 0), (-1, -1), "TOP"),
                            ("BACKGROUND", (0, 0), (-1, 0), colors.HexColor("#e8ecef"))]))
    story += [et]
    story += [Spacer(1, 6), Paragraph("Watch for, and log afterwards", H2)] + bullets(s["watch_for"])
    story.append(Paragraph("Log the game on the playtest feedback form.", SMALL))


def deck_sheet(s, out):
    """A generated deck (tools/generate_layout.py): map, rooms, doors and terrain. No forces."""
    doc = SimpleDocTemplate(str(out), pagesize=letter, leftMargin=0.55 * inch, rightMargin=0.55 * inch,
                            topMargin=0.45 * inch, bottomMargin=0.45 * inch, title=s["title"], author="Imperator Black")
    W = letter[0] - 1.1 * inch
    story = [Paragraph(s["title"], H1),
             Paragraph(f"Imperator Black deck layout · rules v{s['rules_version']} · chapter 11 · seed {s['seed']} · {s['rolls']} d6 rolls", SMALL),
             Spacer(1, 6), Paragraph(s["situation"].strip(), B), Spacer(1, 8),
             MapFlowable(s, W), Spacer(1, 4), legend(),
             Paragraph(f'Scale: each grid square is 3". Board {s["board"]["width"]}" × {s["board"]["height"]}". '
                       'The blue bar on the west edge is the airlock: the way in and out. Spine joints are open; other links are doors.', SMALL),
             Spacer(1, 6)]
    counts = s.get("tile_counts", {})
    story += [Paragraph("Tiles", H2), Paragraph(
        f'{counts.get("compartment", 0)} × 6"×6", {counts.get("passage", 0)} × 6"×3", {counts.get("junction", 0)} × 3"×3" '
        '(junctions and small rooms).', B)]
    story += [Paragraph("Hazard", H2), Paragraph(s.get("hazard") or "None", B)]
    for n in s.get("notes", []):
        story.append(Paragraph(n, B))
    story.append(PageBreak())
    rooms = [["Tile", "Room", "Notes"]]
    texts = {r["name"]: r.get("text", "") for r in ROOM_DEFS}
    for t in s["tiles"]:
        if t["id"].startswith(("C", "S")):
            rooms.append([t["id"], t["name"], texts.get(t["name"], "")])
    story += [Paragraph("Rooms", H2), grid_table(rooms, [0.5 * inch, 1.5 * inch, W - 2.0 * inch]), Spacer(1, 6)]
    drs = [["Door", "Between", "State", "Security"]]
    for d in s["doors"]:
        drs.append([d["id"], d.get("note", ""), d["state"].capitalize(), (d.get("security") or "—").capitalize()])
    story += [Paragraph("Doors", H2), grid_table(drs, [0.5 * inch, 1.4 * inch, 0.9 * inch, W - 2.8 * inch]), Spacer(1, 6)]
    story += terrain_flow(s, W)
    doc.build(story)


# ---------------------------------------------------------------- enemy elements and drill

ELEMENTS = ps.by_id(ps.C["factions"]["elements"])
DRILL_BY_RANK = {d["rank"]: d for d in ps.SOLO["drills"]}
RANK_ORDER = [r["id"] for r in ps.C["core"]["ranks"]]


def enemy_elements(s):
    """[(element, [enemy models])] in order of first appearance."""
    out = {}
    for e in s["enemies"]:
        out.setdefault(e["element"], []).append(e)
    over = s.get("behavior_overrides", {})   # e.g. a base defence: Sentry attackers use Aggressive
    return [({**ELEMENTS[k], "behavior": over.get(k, ELEMENTS[k]["behavior"])}, v) for k, v in out.items()]


def drill_text(el, models, s):
    low = min((TROOPS[m["troop"]]["rank"] for m in models), key=RANK_ORDER.index)
    text = DRILL_BY_RANK[low]["name"]
    leaders = [m for m in s["enemies"] if "leader" in TROOPS[m["troop"]]["skills"] and m not in models]
    if leaders and low != "elite":
        up = DRILL_BY_RANK[RANK_ORDER[RANK_ORDER.index(low) + 1]]["name"]
        text += f"; {up} with model {', '.join(str(m['n']) for m in leaders)} within 6\""
    return text


# ---------------------------------------------------------------- cards

def troop_for(model, number=None):
    """A copy of the troop profile with the scenario's loadout applied, for the card."""
    t = copy.deepcopy(TROOPS[model["troop"]])
    if model.get("weapon"):
        t["weapon"] = model["weapon"]
    t["weapon_options"] = []
    for g in model.get("gear", []):
        t["gear"].append(g)
        if ps.GEAR[g].get("melee"):
            t["melee"] = ps.GEAR[g]["melee"]
    t["gear_options"] = []
    if model.get("label"):
        t["role"] = model["label"]
    if number is not None:
        t["name"] = f"{number}. {t['name']}"
    return t


def scenario_items(s):
    """The scenario's cards: player models, enemy models (identical enemies share one card), references, behaviors."""
    items = [("troop", troop_for(m)) for m in s["player"]]
    groups = {}
    for e in s["enemies"]:
        groups.setdefault((e["troop"], e.get("weapon")), []).append(e["n"])
    for (troop, weapon), ns in groups.items():
        label = ", ".join(str(n) for n in ns)
        items.append(("troop", troop_for({"troop": troop, "weapon": weapon}, label)))
    extra = s.get("extra_cards", {})   # e.g. enemy waves that may arrive during the game
    items += [("troop", troop_for({"troop": t})) for t in extra.get("troops", [])]
    items += [("ref", r) for r in ps.solo_reference_cards()]
    seen = []
    for b in [el["behavior"] for el, _ in enemy_elements(s)] + extra.get("behaviors", []):
        if b not in seen:
            seen.append(b)
    items += [("behavior", ps.BEHAVIORS[b]) for b in seen]
    return items


def unit_items(exclude):
    """Generic unit cards for every non-Republic troop not already covered by a scenario card."""
    order = ["tfb", "freespace", "contractor", "un", "directorate"]
    troops = [t for t in ps.C["troops"]["troops"] if t["faction"] in order and t["id"] not in exclude]
    return [("troop", t) for t in sorted(troops, key=lambda t: order.index(t["faction"]))]


def write_cards(items, out, title):
    c = canvas.Canvas(str(out), pagesize=letter)
    c.setTitle(title)
    slots = [(ps.MARGIN_X, ps.MARGIN_Y + ps.CARD_H), (ps.MARGIN_X + ps.CARD_W, ps.MARGIN_Y + ps.CARD_H),
             (ps.MARGIN_X, ps.MARGIN_Y), (ps.MARGIN_X + ps.CARD_W, ps.MARGIN_Y)]
    for i, (kind, t) in enumerate(items):
        if i and i % 4 == 0:
            c.showPage()
        if i % 4 == 0:
            ps.cut_marks(c)
        x, y = slots[i % 4]
        if kind == "troop":
            ps.draw_card(c, x, y, t)
        elif kind == "ref":
            ps.text_card(c, x, y, *t)
        elif kind == "mission":
            title, sub, flow, colour, chapter = t
            ps.text_card(c, x, y, title, sub, flow, colour, chapter)
        else:
            ps.draw_behavior_card(c, x, y, t)
    c.save()
    return len(items)


def cards(s, out):
    return write_cards(scenario_items(s), out, f"{s['title']} — cards")


def print_set(s, out):
    """One print run: the scenario's cards, then every other unit card, packed four to a page with no repeats."""
    used = {m["troop"] for m in s["player"] + s["enemies"]}
    items = scenario_items(s) + unit_items(used)
    return write_cards(items, out, f"{s['title']} — cards and unit cards")


# ---------------------------------------------------------------- missions (chapter 13)

MISSIONS = ps.by_id(ps.C["missions"]["missions"])
MC = ps.C["missions"]
TIER_DEFS = {t["tier"]: t for t in MC["tiers"]}
OPPOSITIONS = {o["id"]: {t["tier"]: t for t in o["tiers"]} for o in MC.get("oppositions", [])}


def tier_def(s):
    """The mission's threat tier: from its named opposition (chapter 13), else the default tiers."""
    return (OPPOSITIONS[s["opposition"]] if s.get("opposition") else TIER_DEFS)[s["tier"]]


def reinforcement(s):
    """The element that answers alert 4: the mission's own (an authored mission may name one), else the tier's."""
    return s.get("reinforcement") or tier_def(s)["reinforcement"]


def mission_text(s):
    """Task, lockdown and grades: the mission type's, with an authored mission's own text where it gives one."""
    m = MISSIONS[s["mission"]]
    g = {x["grade"].lower(): x["text"] for x in s.get("grades", [])}
    return {"task": s.get("task") or m["task"], "lockdown": s.get("lockdown") or m["lockdown"],
            "decisive": g.get("decisive", m["decisive"]), "marginal": g.get("marginal", m["marginal"]),
            "failure": g.get("failure", m["failure"])}
SPECIALS = {c["id"]: c for c in MC["contact_results"]}
MISSION_COLOUR = colors.HexColor("#6b4a12")


def composition(el):
    return ", ".join(f'{c["count"]} × {TROOPS[c["troop"]]["name"]}' if c["count"] > 1 else TROOPS[c["troop"]]["name"]
                     for c in el["composition"])


def mission_elements(s):
    """Every element the mission can put on the table: guard, contact table, reinforcement. In that order, no repeats."""
    tier = tier_def(s)
    ids = [s["enemies"][0]["element"]] + [r["result"] for r in tier["table"] if r["result"] in ELEMENTS] + [reinforcement(s)]
    out = []
    for i in ids:
        if i not in out:
            out.append(i)
    return [ELEMENTS[i] for i in out]


def element_drill(el, extra_leader=False):
    low = min((TROOPS[c["troop"]]["rank"] for c in el["composition"]), key=RANK_ORDER.index)
    text = DRILL_BY_RANK[low]["name"]
    has_leader = any("leader" in TROOPS[c["troop"]]["skills"] for c in el["composition"])
    if (has_leader or extra_leader) and low != "elite":
        text += f'; {DRILL_BY_RANK[RANK_ORDER[RANK_ORDER.index(low) + 1]]["name"]} with its Leader within 6"'
    return text


def contact_table_rows(s):
    tier = tier_def(s)
    rows = [["d6", "Contact", "Models", "Behavior"]]
    for r in tier["table"]:
        res = r["result"]
        if res in SPECIALS:
            rows.append([r["roll"], SPECIALS[res]["name"], SPECIALS[res]["text"], "—"])
        else:
            el = ELEMENTS[res]
            rows.append([r["roll"], el["name"], composition(el), ps.BEHAVIORS[el["behavior"]]["name"]])
    return rows


def mission_sheet(s, out):
    m = MISSIONS[s["mission"]]
    doc = SimpleDocTemplate(str(out), pagesize=letter, leftMargin=0.55 * inch, rightMargin=0.55 * inch,
                            topMargin=0.45 * inch, bottomMargin=0.45 * inch, title=s["title"], author="Imperator Black")
    W = letter[0] - 1.1 * inch
    brief = {b["heading"]: b["text"] for b in s["briefing"]}
    where = "aboard ship" if s["setting"] == "ship" else "land"
    opp = f'{next(o["name"] for o in MC["oppositions"] if o["id"] == s["opposition"])}, ' if s.get("opposition") else ""
    if s.get("arc"):
        head = (f'Imperator Black short arc · {s["arc"]["name"]} · {s["arc"]["label"]} · rules v{s["rules_version"]} · '
                f'{where} · {opp}threat tier {s["tier"]} · solo')
    else:
        head = f'Imperator Black mission · rules v{s["rules_version"]} · chapter 13 · {where} · {opp}threat tier {s["tier"]} · seed {s["seed"]} · solo'
    story = [Paragraph(s["title"], H1), Paragraph(head, SMALL),
             Spacer(1, 5),
             Paragraph(f'<b>Tasking.</b> {brief["Tasking"]}', B), Spacer(1, 2),
             Paragraph(f'<b>Situation.</b> {brief["Situation"]}', B), Spacer(1, 6),
             MapFlowable(s, W), Spacer(1, 3), legend(s)]
    if s["setting"] == "ship":
        note = (f'Scale: each grid square is 3". Board {s["board"]["width"]}" × {s["board"]["height"]}". The blue bar is the airlock: '
                'the way in and out. Enemy models (numbered) are the guard, placed openly. Contact chips (?n) are hidden contacts: '
                'roll for them when revealed.')
    else:
        note = (f'Each grid square is 6"; heavier lines mark the 12" sectors (A1 bottom left). Board {s["board"]["width"]}" × '
                f'{s["board"]["height"]}". The player edge is the bottom. Pieces are footprints from the sector rolls: '
                'place your own terrain to match. Contact chips (?n) are hidden contacts.')
    story += [Paragraph(s.get("map_note") or note, SMALL), Spacer(1, 5)]
    standard = ("Tasking", "Situation", "Objective", "Opposition", "Ground", "Complication", "Extraction and time", "Grades")
    for h in standard[2:7] + tuple(b["heading"] for b in s["briefing"] if b["heading"] not in standard):
        story += [Paragraph(f'<b>{h}.</b> {brief[h]}', B), Spacer(1, 2)]

    # page 2: how it plays
    story.append(PageBreak())
    mt = mission_text(s)
    story += [Paragraph(f'{m["name"]}', H2),
              Paragraph(f'<b>Taking it.</b> {mt["task"]}', B), Spacer(1, 2),
              Paragraph(f'<b>Lockdown (alert 6).</b> {mt["lockdown"]}', B), Spacer(1, 3),
              grid_table([["Grade", "Result"], ["Decisive", mt["decisive"]], ["Marginal", mt["marginal"]], ["Failure", mt["failure"]]],
                         [0.9 * inch, W - 0.9 * inch]), Spacer(1, 6)]
    if s.get("next"):
        story += [Paragraph("After the mission", H2),
                  grid_table([["Grade", "Next"]] + [[x["grade"], x["text"]] for x in s["next"]], [0.9 * inch, W - 0.9 * inch]),
                  Spacer(1, 6)]
    if s.get("special_rules"):
        story += [Paragraph("Special rules", H2)]
        story += [Paragraph(f'<b>{r["title"]}.</b> {r["text"]}', B) for r in s["special_rules"]] + [Spacer(1, 6)]
    al = MC["alert"]
    raises = "; ".join(f'{r["source"]} +{r["amount"]}' for r in al["raises"])
    story += [Paragraph("Alert track and clock", H2),
              Paragraph(f'<b>Raises:</b> {raises}. Starts at {s["alert_start"]}.', B), Spacer(1, 2)]
    story += [Paragraph(f'<b>At {t["at"]}:</b> {t["effect"]}', B) for t in al["thresholds"]]
    ck = s["clock"]
    story += [Spacer(1, 2), Paragraph(
        f'<b>Clock:</b> {ck["limit"]} minutes, clock die {ck["die"]}. At the end of each round roll the clock die: that many minutes pass; '
        f'then resolve the alert track. Check-ins at minute {ck["first_check_in"]} and every {ck["check_in_every"]} after: each one '
        'reached or passed raises the alert by 1. Time runs out at the limit: the mission ends.', B), Spacer(1, 6)]
    story += [Paragraph(f'Contacts (threat tier {s["tier"]})', H2)] + \
             [Paragraph(f'{i}. {x}', B) for i, x in enumerate(MC["contacts"], 1)] + [Spacer(1, 3)]
    story += [grid_table(contact_table_rows(s), [0.35 * inch, 1.5 * inch, W - 2.85 * inch, 1.0 * inch]), Spacer(1, 6)]
    tier = tier_def(s)
    rows = [["Element", "When", "Behavior", "Drill"]]
    hvt_extra = bool(s.get("hvt")) and "leader" in TROOPS[s["hvt"]]["skills"]
    for el in mission_elements(s):
        when = []
        if el["id"] == s["enemies"][0]["element"]:
            when.append(f'Guard: models {", ".join(str(e["n"]) for e in s["enemies"])}')
        if any(r["result"] == el["id"] for r in tier["table"]):
            when.append("Contact")
        if el["id"] == reinforcement(s):
            when.append("Reinforcements (alert 4), at the enemy entry")
        guard = el["id"] == s["enemies"][0]["element"]
        rows.append([el["name"], "; ".join(when), ps.BEHAVIORS[el["behavior"]]["name"], element_drill(el, guard and hvt_extra)])
    story += [Paragraph("The enemy", H2),
              Paragraph("Run the enemy with chapter 10 and the behavior cards. The enemy's objective, for tie-breakers, is the mission objective.", B),
              Spacer(1, 3), grid_table(rows, [1.6 * inch, 2.2 * inch, 1.0 * inch, W - 4.8 * inch])]

    # page 3: the ground in detail
    story.append(PageBreak())
    if s["setting"] == "ship":
        texts = {r["name"]: r.get("text", "") for r in ROOM_DEFS}
        rooms = [["Tile", "Room", "Notes"]] + [[t["id"], t["name"], texts.get(t["name"], "")] for t in s["tiles"] if t["id"].startswith(("C", "S"))]
        story += [Paragraph("Rooms", H2), grid_table(rooms, [0.5 * inch, 1.5 * inch, W - 2.0 * inch]), Spacer(1, 6)]
        drs = [["Door", "Between", "State", "Security"]] + \
              [[d["id"], d.get("note", ""), d["state"].capitalize(), (d.get("security") or "—").capitalize()] for d in s["doors"]]
        story += [Paragraph("Doors", H2), grid_table(drs, [0.5 * inch, 1.4 * inch, 0.9 * inch, W - 2.8 * inch]), Spacer(1, 6)]
        story += [Paragraph("Hazard", H2), Paragraph(s.get("hazard") or "None", B)]
        story += [Paragraph(n, B) for n in s.get("deck_notes", [])]
        story += [Spacer(1, 6)] + terrain_flow(s, W)
    else:
        if s.get("sectors"):
            secs = [["Sector", "Roll result"]] + [[x["name"], x["text"]] for x in s["sectors"]]
            story += [Paragraph("Sectors", H2), grid_table(secs, [0.6 * inch, W - 0.6 * inch]), Spacer(1, 6)]
        story += terrain_flow(s, W)
    if s.get("rolls"):
        story += [Spacer(1, 6), Paragraph("Rolls", H2), Paragraph("; ".join(s["rolls"]) + ".", SMALL)]
    story.append(PageBreak())
    story += dashboard_flow(s, W)
    doc.build(story)


# ---------------------------------------------------------------- dashboard

DB_H = ps.style(9.5, "Body-Bold", leading=11, space_after=1)
DB_B = ps.style(7.6, leading=9.2)
DB_S = ps.style(6.4, leading=7.6, colour=ps.MUTED)
SHADE = colors.HexColor("#e8ecef")


def _boxes(labels, widths, marks=None, height=0.3 * inch, sub=None):
    """A row of boxes with labels; marks: {index: colour}; sub: a second row of small notes."""
    rows = [[Paragraph(f"<b>{x}</b>", ps.style(7.5, align=1)) for x in labels]]
    if sub:
        rows.append([Paragraph(x, ps.style(5.6, leading=6.6, colour=ps.MUTED, align=1)) for x in sub])
    t = Table(rows, colWidths=widths, rowHeights=[height] + ([None] if sub else []))
    st = [("GRID", (0, 0), (-1, 0), 0.6, ps.INK), ("VALIGN", (0, 0), (-1, -1), "MIDDLE"),
          ("LEFTPADDING", (0, 0), (-1, -1), 1), ("RIGHTPADDING", (0, 0), (-1, -1), 1)]
    for i, col in (marks or {}).items():
        st.append(("BACKGROUND", (i, 0), (i, 0), col))
    t.setStyle(TableStyle(st))
    return t


def _form(head, rows, widths, n_blank=0):
    data = [[Paragraph(f"<b>{h}</b>", DB_S) for h in head]] + [[Paragraph(str(c), DB_B) for c in r] for r in rows]
    data += [[""] * len(head) for _ in range(n_blank)]
    t = Table(data, colWidths=widths, rowHeights=[None] + [0.25 * inch] * (len(data) - 1))
    t.setStyle(TableStyle([("GRID", (0, 0), (-1, -1), 0.4, ps.RULE), ("BACKGROUND", (0, 0), (-1, 0), SHADE),
                           ("VALIGN", (0, 0), (-1, -1), "MIDDLE"), ("TOPPADDING", (0, 0), (-1, -1), 1), ("BOTTOMPADDING", (0, 0), (-1, -1), 1)]))
    return t


def dashboard_flow(s, W):
    """The mission dashboard: one page for a dry-erase sleeve. s=None prints the blank one."""
    m = MISSIONS[s["mission"]] if s else None
    title = s["title"] if s else "Mission: ________________________________"
    story = [Paragraph("Mission dashboard", H1), Paragraph(title, B),
             Paragraph("Imperator Black · chapter 13 · in a dry-erase sleeve, mark the tracks and fill the boxes as you play.", DB_S), Spacer(1, 6)]
    # alert track
    th = {t["at"]: t["effect"].split(":")[0] for t in MC["alert"]["thresholds"]}
    start = s["alert_start"] if s else None
    marks = {start: colors.HexColor("#f6e3a1")} if start is not None else {}
    for at in th:
        marks.setdefault(at, colors.HexColor("#f3d2c6"))
    story += [Paragraph("Alert track", DB_H),
              _boxes([str(i) for i in range(MC["alert"]["max"] + 1)], [W / 7] * 7, marks, 0.32 * inch,
                     ["start" if start == 0 else ""] + [("start · " if start == i else "") + th.get(i, "") for i in range(1, 7)]),
              Paragraph("Raises: Loud +1 · Very loud +2 · alarm raised +1 · each check-in +1. Thresholds take effect once, at the end of the round.", DB_S),
              Spacer(1, 2)]
    story += [_boxes(["□ 2: contacts move 6\" a round", "□ 4: reinforcements arrive", "□ 6: lockdown"], [W / 3] * 3, height=0.22 * inch), Spacer(1, 6)]
    # clock
    ck = s["clock"] if s else {"limit": 30, "die": "d4 / d6", "check_in_every": 5, "first_check_in": 10}
    n = ck["limit"]
    cm = {i - 1: colors.HexColor("#f3d2c6") for i in range(ck["first_check_in"], n, ck["check_in_every"])}
    cm[n - 1] = colors.HexColor("#c9cfd4")
    story += [Paragraph(f'Mission clock: {n} minutes, clock die {ck["die"]}' + ("" if s else " (small missions 20 minutes on a d4; large 30 on a d6)"), DB_H),
              _boxes([str(i) for i in range(1, n + 1)], [W / n] * n, cm, 0.26 * inch),
              Paragraph(f'Shaded red: check-ins (alert +1 when reached or passed). Grey: time runs out. '
                        f'End of each round: roll {ck["die"]}, move the clock, then resolve the alert track.', DB_S), Spacer(1, 3),
              Paragraph("Rounds played", DB_S), _boxes([str(i) for i in range(1, 16)], [W / 15] * 15, height=0.2 * inch), Spacer(1, 6)]
    # objectives
    if s:
        objs = [[o["n"], o["label"], "□ taken  □ destroyed  □ extracted"] for o in s["objectives"]]
        story += [Paragraph(f'Objectives: {m["name"]}', DB_H), _form(["#", "Where", "Status"], objs, [0.3 * inch, 2.4 * inch, W - 2.7 * inch])]
    else:
        story += [Paragraph("Objectives", DB_H), _form(["#", "Where", "Status"], [], [0.3 * inch, 2.4 * inch, W - 2.7 * inch], 2)]
    story.append(Spacer(1, 6))
    # contacts and enemy
    cts = [[c["n"], c["where"], "", ""] for c in s["contacts"]] if s else []
    ctab = _form(["Chip", "Where", "Revealed as", "State"], cts, [0.4 * inch, 0.55 * inch, 1.25 * inch, 1.3 * inch], max(0, 6 - len(cts)))
    if s:
        els = [[ELEMENTS[s["enemies"][0]["element"]]["name"] + " (guard)", "U  A  E", ""]]
        els += [[ELEMENTS[reinforcement(s)]["name"] + " (reinf.)", "U  A  E", ""]]
    else:
        els = []
    etab = _form(["Element", "Awareness", "Down / Dead / surrendered"], els, [1.45 * inch, 0.7 * inch, W / 2 - 2.3 * inch], max(0, 6 - len(els)))
    two = Table([[[Paragraph("Contacts", DB_H), ctab], [Paragraph("Enemy elements", DB_H), etab]]], colWidths=[W / 2, W / 2])
    two.setStyle(TableStyle([("VALIGN", (0, 0), (-1, -1), "TOP"), ("LEFTPADDING", (0, 0), (-1, -1), 0)]))
    story += [two, Spacer(1, 6)]
    # player models
    k = 8 if not s or s.get("size") == "small" else 12
    story += [Paragraph("Player force", DB_H),
              _form(["Model", "Wounds", "Pinned", "Wounded", "Down", "Carrying", "Extracted"], [],
                    [2.0 * inch, 0.6 * inch, 0.6 * inch, 0.7 * inch, 0.55 * inch, W - 5.3 * inch, 0.85 * inch], k),
              Spacer(1, 4), Paragraph("Notes: _______________________________________________________________________________________", DB_B)]
    return story


def dashboard_pdf(out):
    doc = SimpleDocTemplate(str(out), pagesize=letter, leftMargin=0.55 * inch, rightMargin=0.55 * inch,
                            topMargin=0.45 * inch, bottomMargin=0.4 * inch, title="Mission dashboard", author="Imperator Black")
    doc.build(dashboard_flow(None, letter[0] - 1.1 * inch))


# ---------------------------------------------------------------- mission cards

def mission_cards(s):
    m, tier, mt = MISSIONS[s["mission"]], tier_def(s), mission_text(s)
    S, H = ps.S_BODY, ps.S_HEAD
    obj = [Paragraph(f'<i>{m["summary"]}</i>', S), Spacer(1, 2), Paragraph("Taking it", H), Paragraph(mt["task"], S),
           Spacer(1, 2), Paragraph("Lockdown (alert 6)", H), Paragraph(mt["lockdown"], S), Spacer(1, 2), Paragraph("Grades", H),
           Paragraph(f'<b>Decisive:</b> {mt["decisive"]}', S), Paragraph(f'<b>Marginal:</b> {mt["marginal"]}', S),
           Paragraph(f'<b>Failure:</b> {mt["failure"]}', S)]
    al, ck = MC["alert"], s["clock"]
    alert = [Paragraph("Raises", H)] + [Paragraph(f'• {r["source"]}: +{r["amount"]}', S) for r in al["raises"]] + \
            [Spacer(1, 2), Paragraph("Thresholds (once each)", H)] + [Paragraph(f'<b>{t["at"]}:</b> {t["effect"]}', S) for t in al["thresholds"]] + \
            [Spacer(1, 2), Paragraph("Mission clock", H),
             Paragraph(f'{ck["limit"]} minutes, clock die {ck["die"]}. End of each round: roll it, move the clock, then resolve the alert '
                       f'track. Check-ins at {ck["first_check_in"]} and every {ck["check_in_every"]} minutes after.', S)]
    rows = contact_table_rows(s)[1:]
    cts = [Paragraph(f'{i}. {x}', S) for i, x in enumerate(MC["contacts"], 1)] + [Spacer(1, 2), Paragraph(f"d6 (threat tier {s['tier']})", H)]
    cts += [Paragraph(f'<b>{r[0]}:</b> {r[1]}' + (f' ({r[2]})' if r[3] != "—" else ""), S) for r in rows]
    cts += [Spacer(1, 2), Paragraph(f'<b>Reinforcements:</b> {ELEMENTS[reinforcement(s)]["name"]} ({composition(ELEMENTS[reinforcement(s)])}).', S)]
    col = MISSION_COLOUR
    out = [("mission", (m["name"], "Mission card · objective and grades", obj, col, "chapter 13")),
           ("mission", ("Alert and clock", "Mission card · the pressure", alert, col, "chapter 13")),
           ("mission", ("Contacts", f"Mission card · threat tier {s['tier']}", cts, col, "chapter 13"))]
    if s.get("special_rules"):
        sr = [Paragraph(f'<b>{r["title"]}.</b> {r["text"]}', S) for r in s["special_rules"]]
        out.append(("mission", ("Special rules", "Mission card · this mission only", sr, col, "chapter 13")))
    return out


def mission_items(s):
    """Cards for a mission: mission cards, the guard (numbered), every other troop it can meet, the hostage, references, behaviors."""
    items = mission_cards(s)
    if s["mission"] == "recover":
        items.append(("troop", troop_for({"troop": "hostage"})))
    groups = {}
    for e in s["enemies"]:
        groups.setdefault(e["troop"], []).append(e["n"])
    for troop, ns in groups.items():
        items.append(("troop", troop_for({"troop": troop}, ", ".join(str(n) for n in ns))))
    seen = set(groups)
    for el in mission_elements(s):
        for c in el["composition"]:
            if c["troop"] not in seen:
                seen.add(c["troop"])
                items.append(("troop", troop_for({"troop": c["troop"]})))
    items += [("ref", r) for r in ps.solo_reference_cards()]
    bh = []
    for el in mission_elements(s):
        if el["behavior"] not in bh:
            bh.append(el["behavior"])
    items += [("behavior", ps.BEHAVIORS[b]) for b in bh]
    return items


def main():
    if "--dashboard" in sys.argv:
        ps.OUT.mkdir(parents=True, exist_ok=True)
        out = ps.OUT / f"{ps.PREFIX}mission-dashboard.pdf"
        dashboard_pdf(out)
        print(f"Wrote {out.relative_to(ROOT)}")
        return
    args = [a for a in sys.argv[1:] if not a.startswith("--")]
    if len(args) != 1:
        raise SystemExit(__doc__)
    path = Path(args[0])
    s = yaml.safe_load(path.read_text())
    if s.get("kind") == "deck":
        ps.OUT.mkdir(parents=True, exist_ok=True)
        out = ps.OUT / "decks" / f"{ps.PREFIX}{s['id']}.pdf"
        out.parent.mkdir(exist_ok=True)
        deck_sheet(s, out)
        print(f"Wrote {out.relative_to(ROOT)}")
        return
    if s.get("kind") == "mission":
        out_dir = ps.OUT / ("arcs" if s.get("arc") else "missions")
        out_dir.mkdir(parents=True, exist_ok=True)
        sheet_path = out_dir / f"{ps.PREFIX}{s['id']}-sheet.pdf"
        cards_path = out_dir / f"{ps.PREFIX}{s['id']}-cards.pdf"
        mission_sheet(s, sheet_path)
        n = write_cards(mission_items(s), cards_path, f"{s['title']} — cards")
        print(f"Wrote {sheet_path.relative_to(ROOT)}")
        print(f"Wrote {cards_path.relative_to(ROOT)} ({n} cards, {-(-n // 4)} pages)")
        return
    for m in s["player"] + s["enemies"]:
        if m["troop"] not in TROOPS:
            raise SystemExit(f"Unknown troop '{m['troop']}'")
        for g in m.get("gear", []):
            if g not in ps.GEAR:
                raise SystemExit(f"Unknown gear '{g}'")
        if m in s["enemies"] and m.get("element") not in ELEMENTS:
            raise SystemExit(f"Enemy {m['n']}: unknown element '{m.get('element')}'")
        if m.get("weapon") and m["weapon"] not in ps.RANGED:
            raise SystemExit(f"Unknown weapon '{m['weapon']}'")
    ps.OUT.mkdir(parents=True, exist_ok=True)
    sheet_path = ps.OUT / f"{ps.PREFIX}{s['id']}-sheet.pdf"
    cards_path = ps.OUT / f"{ps.PREFIX}{s['id']}-cards.pdf"
    sheet(s, sheet_path)
    n = cards(s, cards_path)
    print(f"Wrote {sheet_path.relative_to(ROOT)}")
    print(f"Wrote {cards_path.relative_to(ROOT)} ({n} cards)")
    if "--with-unit-cards" in sys.argv:
        set_path = ps.OUT / f"{ps.PREFIX}{s['id']}-and-unit-cards.pdf"
        n = print_set(s, set_path)
        print(f"Wrote {set_path.relative_to(ROOT)} ({n} cards, {-(-n // 4)} pages)")


if __name__ == "__main__":
    main()
