#!/usr/bin/env python3
"""Print a scenario: a scenario sheet with a to-scale map, and cards for just the models in it.

Usage (from the repo root):
    python3 tools/print_scenario.py docs/scenarios/gym-01-first-breach.yaml

Writes docs/print/<id>-sheet.pdf and docs/print/<id>-cards.pdf.
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
              "hill": colors.HexColor("#e3cfa4"), "area": colors.HexColor("#d9e3c4"), "ditch": colors.HexColor("#b98c6a")}
COVER_ORDER = {"hill": 0, "area": 1, "ditch": 2, "light": 3, "heavy": 4}
COVER_NAMES = {"light": "Light cover", "heavy": "Heavy cover / solid", "hill": "Raised ground", "area": "Area terrain", "ditch": "Ditch or crevice"}


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
        # objective
        o = s.get("objective")
        if o:
            x, y = self.p(o["x"], o["y"])
            r = 0.3 * k
            c.setFillColor(colors.HexColor("#d9a300"))
            c.setStrokeColor(ps.INK)
            c.setLineWidth(0.8)
            path = c.beginPath()
            path.moveTo(x, y + r); path.lineTo(x + r, y); path.lineTo(x, y - r); path.lineTo(x - r, y); path.close()
            c.drawPath(path, fill=1, stroke=1)
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
    if land:
        types = sorted({c["type"] for c in s.get("cover", [])}, key=lambda k: COVER_ORDER.get(k, 9))
        items = [(COVER_NAMES[k], COVER_FILL[k]) for k in types]
        items += [("Enemy model", ENEMY_FILL), ("Deployment", colors.Color(0.15, 0.27, 0.42, alpha=0.35)), ("Objective", colors.HexColor("#d9a300"))]
        items = items[:8] + [("", colors.white)] * (8 - len(items[:8]))
    else:
        items = [("Open door", DOOR_COLOURS["open"]), ("Closed door", DOOR_COLOURS["closed"]), ("Locked door (L)", DOOR_COLOURS["locked"]),
                 ("Light cover", COVER_FILL["light"]), ("Heavy cover", COVER_FILL["heavy"]), ("Enemy model", ENEMY_FILL),
                 ("Deployment", colors.Color(0.15, 0.27, 0.42, alpha=0.35)), ("Objective", colors.HexColor("#d9a300"))]
    cells = []
    for name, col in items:
        sw = Table([[""]], colWidths=[8], rowHeights=[8])
        sw.setStyle(TableStyle([("BACKGROUND", (0, 0), (-1, -1), col)]))
        cells += [sw, Paragraph(name, ps.style(7, colour=ps.MUTED))]
    t = Table([cells[:8], cells[8:]], colWidths=[12, 1.1 * inch] * 4)
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
    story.append(Paragraph("Log the game with docs/playtests/TEMPLATE.md.", SMALL))


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
    return [(ELEMENTS[k], v) for k, v in out.items()]


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


def cards(s, out):
    c = canvas.Canvas(str(out), pagesize=letter)
    c.setTitle(f"{s['title']} — cards")
    slots = [(ps.MARGIN_X, ps.MARGIN_Y + ps.CARD_H), (ps.MARGIN_X + ps.CARD_W, ps.MARGIN_Y + ps.CARD_H),
             (ps.MARGIN_X, ps.MARGIN_Y), (ps.MARGIN_X + ps.CARD_W, ps.MARGIN_Y)]
    items = [("troop", troop_for(m)) for m in s["player"]]
    items += [("troop", troop_for({"troop": e["troop"], "weapon": e.get("weapon")}, e["n"])) for e in s["enemies"]]
    items += [("ref", r) for r in ps.solo_reference_cards()]
    seen = []
    for el, _ in enemy_elements(s):
        if el["behavior"] not in seen:
            seen.append(el["behavior"])
    items += [("behavior", ps.BEHAVIORS[b]) for b in seen]
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
        else:
            ps.draw_behavior_card(c, x, y, t)
    c.save()
    return len(items)


def main():
    if len(sys.argv) != 2:
        raise SystemExit(__doc__)
    path = Path(sys.argv[1])
    s = yaml.safe_load(path.read_text())
    if s.get("kind") == "deck":
        ps.OUT.mkdir(parents=True, exist_ok=True)
        out = ps.OUT / "decks" / f"{s['id']}.pdf"
        out.parent.mkdir(exist_ok=True)
        deck_sheet(s, out)
        print(f"Wrote {out.relative_to(ROOT)}")
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
    sheet_path = ps.OUT / f"{s['id']}-sheet.pdf"
    cards_path = ps.OUT / f"{s['id']}-cards.pdf"
    sheet(s, sheet_path)
    n = cards(s, cards_path)
    print(f"Wrote {sheet_path.relative_to(ROOT)}")
    print(f"Wrote {cards_path.relative_to(ROOT)} ({n} cards)")


if __name__ == "__main__":
    main()
