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

PLAYER_FILL = colors.HexColor("#27466b")
ENEMY_FILL = colors.HexColor("#b4441f")
TILE_FILL = colors.HexColor("#e9edf1")
TILE_EDGE = colors.HexColor("#3c4650")
GRID = colors.HexColor("#cfd6dc")
DOOR_COLOURS = {"open": colors.HexColor("#7fb27f"), "closed": colors.HexColor("#8a5a12"), "locked": colors.HexColor("#b4441f")}
COVER_FILL = {"light": colors.HexColor("#c8d9b8"), "heavy": colors.HexColor("#7d8a6b")}


# ---------------------------------------------------------------- map

class MapFlowable(Flowable):
    """Draws the scenario layout to scale, with a 3" grid, doors, cover, deployment, enemies and objective."""

    def __init__(self, s, width):
        super().__init__()
        self.s = s
        self.bw, self.bh = s["board"]["width"], s["board"]["height"]
        self.scale = width / self.bw
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
        for cv in s.get("cover", []):
            x, y = self.p(cv["x"], cv["y"])
            c.setFillColor(COVER_FILL[cv["type"]])
            c.setStrokeColor(TILE_EDGE)
            c.setLineWidth(0.5)
            c.rect(x, y, cv["w"] * k, cv["h"] * k, fill=1, stroke=1)
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


def legend():
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
    w = ps.RANGED[e.get("weapon", t["weapon"])]["name"]
    return f'{e["n"]}. {t["name"]} ({w})'


def sheet(s, out):
    doc = SimpleDocTemplate(str(out), pagesize=letter, leftMargin=0.55 * inch, rightMargin=0.55 * inch,
                            topMargin=0.45 * inch, bottomMargin=0.45 * inch, title=s["title"], author="Imperator Black")
    W = letter[0] - 1.1 * inch
    story = [Paragraph(s["title"], H1),
             Paragraph(f"Imperator Black gym scenario · rules v{s['rules_version']} · solo · about 60–90 minutes", SMALL),
             Spacer(1, 6), Paragraph(s["situation"].strip(), B), Spacer(1, 8),
             MapFlowable(s, W), Spacer(1, 4), legend(),
             Paragraph('Scale: each grid square is 3". Board 18" × 12". The blue bar on the west wall of P1 is the airlock: the way in and out. Enemy positions are model centres.', SMALL),
             Spacer(1, 6)]

    forces_player = [Paragraph("Task Force Boone (6)", H2)] + bullets([model_name(m) for m in s["player"]])
    forces_enemy = [Paragraph("Freespace Solutions (4)", H2)] + bullets([f'{enemy_name(e)}: {e["state"]}' for e in s["enemies"]])
    two = Table([[forces_player, forces_enemy]], colWidths=[W / 2, W / 2])
    two.setStyle(TableStyle([("VALIGN", (0, 0), (-1, -1), "TOP"), ("LEFTPADDING", (0, 0), (-1, -1), 0)]))
    story += [two, Spacer(1, 4), Paragraph("Setup", H2)] + bullets(s["setup"])

    story.append(PageBreak())
    story += [Paragraph("Special rules", H2)]
    for r in s["special_rules"]:
        story.append(Paragraph(f'<b>{r["title"]}.</b> {" ".join(r["text"].split())}', B))
        story.append(Spacer(1, 2))
    v = s["victory"]
    story += [Spacer(1, 4), Paragraph("Victory", H2), Paragraph(f'<b>Length:</b> {v["length"]} <b>Goal:</b> {v["goal"]}', B), Spacer(1, 2)]
    for o in v["outcomes"]:
        story.append(Paragraph(f'<b>{o["grade"]}:</b> {o["text"]}', B))

    eo = s["enemy_orders"]
    story += [Spacer(1, 6), Paragraph("Enemy orders (stopgap until Phase 3)", H2),
              Paragraph("<b>Who activates next:</b> the first eligible enemy model from this list:", B)]
    story += [Paragraph(f"{i}. {a}", B) for i, a in enumerate(eo["activation"], 1)]
    pr = Table([[Paragraph("<b>Model</b>", B), Paragraph("<b>Each action: take the first that applies</b>", B)]] +
               [[Paragraph(p["who"], B), Paragraph(p["what"], B)] for p in eo["priorities"]],
               colWidths=[1.55 * inch, W - 1.55 * inch])
    pr.setStyle(TableStyle([("GRID", (0, 0), (-1, -1), 0.4, ps.RULE), ("VALIGN", (0, 0), (-1, -1), "TOP"),
                            ("BACKGROUND", (0, 0), (-1, 0), colors.HexColor("#e8ecef"))]))
    story += [Spacer(1, 3), pr, Spacer(1, 3)] + bullets(eo["rules"])
    story += [Spacer(1, 6), Paragraph("Watch for, and log afterwards", H2)] + bullets(s["watch_for"])
    story.append(Paragraph("Log the game with docs/playtests/TEMPLATE.md.", SMALL))
    doc.build(story)


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


def orders_card(c, x, y, s):
    c.setStrokeColor(ps.RULE)
    c.setLineWidth(0.6)
    c.rect(x, y, ps.CARD_W, ps.CARD_H)
    band = 0.52 * inch
    c.setFillColor(ps.FACTION_COLOUR["freespace"])
    c.rect(x, y + ps.CARD_H - band, ps.CARD_W, band, stroke=0, fill=1)
    c.setFillColor(colors.white)
    c.setFont("Body-Bold", 12)
    c.drawString(x + 0.14 * inch, y + ps.CARD_H - 0.27 * inch, "Enemy orders")
    c.setFont("Body", 7)
    c.drawString(x + 0.14 * inch, y + ps.CARD_H - 0.43 * inch, f"{s['title']}  ·  stopgap until Phase 3")
    eo = s["enemy_orders"]
    flow = [Paragraph("Who activates next", ps.S_HEAD)]
    flow += [Paragraph(f"{i}. {a}", ps.S_BODY) for i, a in enumerate(eo["activation"], 1)]
    flow += [Spacer(1, 3), Paragraph("Each action: first that applies", ps.S_HEAD)]
    flow += [Paragraph(f"<b>{p['who']}:</b> {p['what']}", ps.S_BODY) for p in eo["priorities"]]
    flow += [Spacer(1, 3), Paragraph("Rules", ps.S_HEAD)] + [Paragraph("• " + r, ps.S_BODY) for r in eo["rules"]]
    frame = Frame(x + 0.14 * inch, y + 0.12 * inch, ps.CARD_W - 0.28 * inch, ps.CARD_H - band - 0.16 * inch,
                  leftPadding=0, rightPadding=0, topPadding=4, bottomPadding=0)
    frame.addFromList([KeepInFrame(frame._aW, frame._aH, flow, mode="shrink")], c)


def cards(s, out):
    c = canvas.Canvas(str(out), pagesize=letter)
    c.setTitle(f"{s['title']} — cards")
    slots = [(ps.MARGIN_X, ps.MARGIN_Y + ps.CARD_H), (ps.MARGIN_X + ps.CARD_W, ps.MARGIN_Y + ps.CARD_H),
             (ps.MARGIN_X, ps.MARGIN_Y), (ps.MARGIN_X + ps.CARD_W, ps.MARGIN_Y)]
    items = [("troop", troop_for(m)) for m in s["player"]]
    items += [("troop", troop_for({"troop": e["troop"], "weapon": e.get("weapon")}, e["n"])) for e in s["enemies"]]
    items.append(("orders", None))
    for i, (kind, t) in enumerate(items):
        if i and i % 4 == 0:
            c.showPage()
        if i % 4 == 0:
            ps.cut_marks(c)
        x, y = slots[i % 4]
        if kind == "troop":
            ps.draw_card(c, x, y, t)
        else:
            orders_card(c, x, y, s)
    c.save()
    return len(items)


def main():
    if len(sys.argv) != 2:
        raise SystemExit(__doc__)
    path = Path(sys.argv[1])
    s = yaml.safe_load(path.read_text())
    for m in s["player"] + s["enemies"]:
        if m["troop"] not in TROOPS:
            raise SystemExit(f"Unknown troop '{m['troop']}'")
        for g in m.get("gear", []):
            if g not in ps.GEAR:
                raise SystemExit(f"Unknown gear '{g}'")
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
