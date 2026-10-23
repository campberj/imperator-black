#!/usr/bin/env python3
"""Generate printable unit cards and a quick-reference sheet from content/*.yaml.

Run from the repo root: python3 tools/print_sheets.py
Writes docs/print/ImperatorBlack_unit-cards.pdf (3.5" x 5" cards, four per US Letter page, with cut marks)
and docs/print/ImperatorBlack_quick-reference.pdf (two pages).

The quick reference summarises rules/ chapters 1-8. Its tables come from content; its short
rule summaries are written here, so update them when the rules text changes.
"""
from pathlib import Path

import yaml
from reportlab.lib import colors
from reportlab.lib.enums import TA_CENTER
from reportlab.lib.pagesizes import letter
from reportlab.lib.styles import ParagraphStyle
from reportlab.lib.units import inch
from reportlab.pdfbase import pdfmetrics
from reportlab.pdfbase.ttfonts import TTFont
from reportlab.pdfgen import canvas
from reportlab.platypus import (Frame, KeepInFrame, Paragraph, SimpleDocTemplate, Spacer, Table,
                                TableStyle)

ROOT = Path(__file__).resolve().parent.parent
OUT = ROOT / "docs" / "print"
PREFIX = "ImperatorBlack_"   # every printed PDF is named ImperatorBlack_<name>.pdf (RJ, 2026-10-21)
C = {f.stem: yaml.safe_load(f.read_text()) for f in sorted((ROOT / "content").glob("*.yaml"))}
VERSION = C["core"]["version"]

FONT_DIR = Path("/usr/share/fonts/truetype/dejavu")
for name, file in [("Body", "DejaVuSansCondensed.ttf"), ("Body-Bold", "DejaVuSansCondensed-Bold.ttf"),
                   ("Body-Italic", "DejaVuSansCondensed-Oblique.ttf"), ("Body-BoldItalic", "DejaVuSansCondensed-BoldOblique.ttf")]:
    path = FONT_DIR / file
    if not path.exists():
        raise SystemExit(f"Font not found: {path}")
    pdfmetrics.registerFont(TTFont(name, str(path)))
pdfmetrics.registerFontFamily("Body", normal="Body", bold="Body-Bold", italic="Body-Italic", boldItalic="Body-BoldItalic")

INK = colors.HexColor("#1d2329")
MUTED = colors.HexColor("#5b6670")
RULE = colors.HexColor("#c9cfd4")
FACTION_COLOUR = {
    "tfb": colors.HexColor("#27466b"),
    "freespace": colors.HexColor("#8a5a12"),
    "contractor": colors.HexColor("#4d5560"),
    "directorate": colors.HexColor("#7a1f2b"),
    "republic": colors.HexColor("#a34a1a"),
    "un": colors.HexColor("#2f5d62"),
    "anchorage": colors.HexColor("#3c5a7a"),
    "hankou": colors.HexColor("#6e3b3b"),
    "generic": colors.HexColor("#4a5a3a"),
    "separatist": colors.HexColor("#5a3f6b"),
}

by_id = lambda section: {x["id"]: x for x in section}
RANGED = by_id(C["weapons"]["ranged"])
MELEE = by_id(C["weapons"]["melee"])
THROWN = by_id(C["weapons"]["thrown"])
ARMOUR = by_id(C["armour"]["armour"])
GEAR = by_id(C["gear"]["gear"])
FACTIONS = by_id(C["factions"]["factions"])
BEHAVIORS = by_id(C["behaviors"]["behaviors"])
KEYWORDS = {k["id"]: k for group in ("weapon", "armour") for k in C["keywords"][group]}
SKILLS = by_id(C["skills"]["skills"])
RANKS = {r["id"]: r["name"] for r in C["core"]["ranks"]}
ATTRS = C["core"]["attributes"]


def style(size, font="Body", leading=None, colour=INK, align=None, space_after=0):
    s = ParagraphStyle("s", fontName=font, fontSize=size, leading=leading or size * 1.2,
                       textColor=colour, spaceAfter=space_after)
    if align is not None:
        s.alignment = align
    return s


S_BODY = style(7.4)
S_SMALL = style(6.6, colour=MUTED)
S_HEAD = style(7.6, "Body-Bold", space_after=1)
S_CELL = style(7.2)
S_CELL_B = style(7.2, "Body-Bold")
S_STAT_LABEL = style(5.8, "Body-Bold", colour=MUTED, align=TA_CENTER)
S_STAT = style(11.5, "Body-Bold", leading=13, align=TA_CENTER)
S_STAT_SUB = style(6.2, colour=MUTED, align=TA_CENTER)


# ---------------------------------------------------------------- unit cards

CARD_W, CARD_H = 3.5 * inch, 5 * inch
MARGIN_X = (letter[0] - 2 * CARD_W) / 2
MARGIN_Y = (letter[1] - 2 * CARD_H) / 2


def elements_for(troop_id):
    out = []
    for e in C["factions"]["elements"]:
        ids = [p["troop"] for p in e["composition"]] + e.get("alternatives", [])
        if troop_id in ids:
            out.append(e)
    return out


def stat_text(t, attr):
    v = t["stats"][attr]
    if v is None:
        return "—"
    if attr == "move":
        return f'{v}"' if v else "—"
    return str(v)


def weapon_rows(t):
    rows = [["Weapon", "Dmg", "Short", "AP"]]
    extra_kw = [k for g in t["gear"] for k in GEAR[g].get("weapon_keywords", [])]
    for i, wid in enumerate(([t["weapon"]] if t["weapon"] else []) + t.get("weapon_options", [])):
        w = RANGED[wid]
        label = f"<b>{w['name']}</b>" if i == 0 else f"<i>or {w['name']}</i>"
        rows.append([Paragraph(label, S_CELL), w["damage"], f'{w["short_range"]}"', str(w["ap"])])
        kws = w["keywords"] + (extra_kw if i == 0 else [])
        pen = w.get("range_penalty")
        notes = [f'Auto {w["auto"]}' if k == "auto" and w.get("auto") else KEYWORDS[k]["name"] for k in kws]
        if w.get("explosion"):
            notes.append(f'Explosion ({w["explosion"]})')
        if pen:
            notes.append(f"beyond short {pen[0]}, beyond double {pen[1]}")
        if notes:
            rows.append([Paragraph(", ".join(notes), S_SMALL), "", "", ""])
        for r in w.get("rounds", []):   # launcher rounds: choose one before each Shoot
            rows.append([Paragraph(f"<i>  {w['round']} above, or {r['name'].lower()} round:</i>", S_CELL), r["damage"], f'{w["short_range"]}"', str(r["ap"])])
            if r.get("explosion"):
                rows.append([Paragraph(f'Explosion ({r["explosion"]}). Choose the round before each Shoot.', S_SMALL), "", "", ""])
    if not t["weapon"] and not t.get("weapon_options"):
        rows.append([Paragraph("<i>No ranged weapon: cannot Shoot</i>", S_SMALL), "", "", ""])
    if t["melee"]:
        m = MELEE[t["melee"]]
        rows.append([Paragraph(f"<b>{m['name']}</b> (close combat)", S_CELL), m["damage"], "—", str(m["ap"])])
    return rows


def card_flowables(t):
    fac = FACTIONS[t["faction"]]
    flow = []

    cols = [(a["short"], stat_text(t, a["id"])) for a in ATTRS] + [("ARM", str(ARMOUR[t["armour"]]["value"]))]
    stats = Table([[Paragraph(c[0], S_STAT_LABEL) for c in cols], [Paragraph(c[1], S_STAT) for c in cols]],
                  colWidths=[(CARD_W - 0.28 * inch) / len(cols)] * len(cols))
    stats.setStyle(TableStyle([
        ("BOX", (0, 0), (-1, -1), 0.6, RULE), ("INNERGRID", (0, 0), (-1, -1), 0.4, RULE),
        ("BACKGROUND", (-1, 0), (-1, -1), colors.HexColor("#eef1f4")),
        ("TOPPADDING", (0, 0), (-1, -1), 1), ("BOTTOMPADDING", (0, 0), (-1, -1), 2),
        ("LEFTPADDING", (0, 0), (-1, -1), 0), ("RIGHTPADDING", (0, 0), (-1, -1), 0),
    ]))
    arm_text = ARMOUR[t["armour"]]["name"] + "".join(f" or {ARMOUR[a]['name']} ({ARMOUR[a]['value']})" for a in t.get("armour_options", []))
    sup = C["core"]["suppression"]
    nerve = t["stats"]["nerve"]
    if nerve is None:
        pin = ""   # Automated: never gains Suppression
    else:
        at = nerve + sup["pinned_by_nerve"] if sup.get("pinned_by_nerve") is not None else sup["pinned_at"]
        pin = f"  ·  Pinned at {at} Suppression" if at <= sup["max"] else "  ·  Never Pinned"
    flow += [stats, Paragraph(f"Armour: {arm_text}{pin}", S_SMALL), Spacer(1, 3)]

    wt = Table(weapon_rows(t), colWidths=[1.95 * inch, 0.42 * inch, 0.42 * inch, 0.35 * inch])
    ts = [("FONT", (0, 0), (-1, 0), "Body-Bold", 6.4), ("TEXTCOLOR", (0, 0), (-1, 0), MUTED),
          ("FONT", (1, 1), (-1, -1), "Body", 7.2), ("LINEBELOW", (0, 0), (-1, 0), 0.6, RULE),
          ("VALIGN", (0, 0), (-1, -1), "TOP"), ("TOPPADDING", (0, 0), (-1, -1), 1.2), ("BOTTOMPADDING", (0, 0), (-1, -1), 1.2),
          ("LEFTPADDING", (0, 0), (-1, -1), 0), ("ALIGN", (1, 0), (-1, -1), "CENTER")]
    for r, row in enumerate(weapon_rows(t)):
        if row[1] == "" and r > 0:
            ts.append(("SPAN", (0, r), (-1, r)))
    wt.setStyle(TableStyle(ts))
    flow += [wt, Spacer(1, 4)]

    if t["grenades"]:
        flow.append(Paragraph("<b>Grenades:</b> " + ", ".join(THROWN[g]["name"] for g in t["grenades"]), S_BODY))
        flow.append(Spacer(1, 2))

    gear_items = t["gear"]
    if gear_items or t.get("gear_options"):
        flow.append(Paragraph("Gear", S_HEAD))
        for g in gear_items:
            gi = GEAR[g]
            tag = " <font color='#8a5a12'>[proposed]</font>" if gi["status"] == "proposed" else ""
            flow.append(Paragraph(f"<b>{gi['name']}</b>{tag}: {gi['text']}", S_BODY))
        if t.get("gear_options"):
            flow.append(Paragraph("<b>Choose one:</b> " + " / ".join(GEAR[g]["name"] for g in t["gear_options"]) + " □ □ □", S_BODY))
            if "automated" in t["skills"]:   # robot modules: print what each option does (cards have room)
                for g in t["gear_options"]:
                    gi = GEAR[g]
                    granted = " ".join(f"<i>{SKILLS[k]['name']}:</i> " + " ".join(SKILLS[k]["levels"][:v])
                                       for k, v in gi.get("grants_skills", {}).items() if k != "shield")
                    tag = " <font color='#8a5a12'>[proposed]</font>" if gi["status"] == "proposed" else ""
                    flow.append(Paragraph(f"• <b>{gi['name']}</b>{tag}: {gi['text']} {granted}", S_SMALL))
        flow.append(Spacer(1, 2))

    # Skills granted by gear are already explained in the gear text.
    if t["skills"]:
        flow.append(Paragraph("Skills", S_HEAD))
        for sid, lvl in t["skills"].items():
            sk = SKILLS[sid]
            name = sk["name"] + (f" {lvl}" if len(sk["levels"]) > 1 else "")
            tag = " <font color='#8a5a12'>[proposed]</font>" if sk["status"] == "proposed" else ""
            flow.append(Paragraph(f"<b>{name}</b>{tag}: " + " ".join(sk["levels"][:lvl]), S_BODY))
        flow.append(Spacer(1, 2))

    if fac["side"] == "enemy":
        els = elements_for(t["id"])
        if els:
            txt = "; ".join(f"{e['name']} (tier {e['tier']}, {BEHAVIORS[e['behavior']]['name']})" for e in els)
            flow.append(Paragraph(f"<b>Element:</b> {txt}", S_BODY))
    return flow


def draw_card(c, x, y, t):
    fac = FACTIONS[t["faction"]]
    colour = FACTION_COLOUR[t["faction"]]
    c.setStrokeColor(RULE)
    c.setLineWidth(0.6)
    c.rect(x, y, CARD_W, CARD_H)
    band = 0.52 * inch
    c.setFillColor(colour)
    c.rect(x, y + CARD_H - band, CARD_W, band, stroke=0, fill=1)
    c.setFillColor(colors.white)
    c.setFont("Body-Bold", 12)
    c.drawString(x + 0.14 * inch, y + CARD_H - 0.27 * inch, t["name"])
    # Subtitle on the left, the troop's class (chapter 9) on the right; the subtitle shrinks if they would meet.
    sub = f"{RANKS[t['rank']]} {t['role']}  ·  {fac['name']}"
    cls = t.get("class", "").upper()
    room = CARD_W - 0.28 * inch - pdfmetrics.stringWidth(cls, "Body-Bold", 6.5) - 0.12 * inch
    size = 7
    while size > 5 and pdfmetrics.stringWidth(sub, "Body", size) > room:
        size -= 0.25
    c.setFont("Body", size)
    c.drawString(x + 0.14 * inch, y + CARD_H - 0.43 * inch, sub)
    c.setFont("Body-Bold", 6.5)
    c.drawRightString(x + CARD_W - 0.14 * inch, y + CARD_H - 0.43 * inch, cls)

    footer = 0.32 * inch
    c.setFillColor(MUTED)
    c.setFont("Body-Italic", 5.8)
    c.drawString(x + 0.14 * inch, y + 0.17 * inch, t.get("miniatures", ""))
    c.setFont("Body", 5.8)
    c.drawRightString(x + CARD_W - 0.14 * inch, y + 0.08 * inch, f"Imperator Black · content v{VERSION}")

    frame = Frame(x + 0.14 * inch, y + footer, CARD_W - 0.28 * inch, CARD_H - band - footer - 0.04 * inch,
                  leftPadding=0, rightPadding=0, topPadding=4, bottomPadding=0, showBoundary=0)
    frame.addFromList([KeepInFrame(frame._aW, frame._aH, card_flowables(t), mode="shrink")], c)


def cut_marks(c):
    c.setStrokeColor(MUTED)
    c.setLineWidth(0.3)
    xs = [MARGIN_X, MARGIN_X + CARD_W, MARGIN_X + 2 * CARD_W]
    ys = [MARGIN_Y, MARGIN_Y + CARD_H, MARGIN_Y + 2 * CARD_H]
    for x in xs:
        c.line(x, 0.1 * inch, x, MARGIN_Y - 0.08 * inch)
        c.line(x, letter[1] - 0.1 * inch, x, letter[1] - MARGIN_Y + 0.08 * inch)
    for y in ys:
        c.line(0.1 * inch, y, MARGIN_X - 0.08 * inch, y)
        c.line(letter[0] - 0.1 * inch, y, letter[0] - MARGIN_X + 0.08 * inch, y)


def unit_cards(factions=("tfb", "freespace", "contractor", "un", "directorate"), name="unit-cards.pdf"):
    order = list(factions)
    troops = sorted([t for t in C["troops"]["troops"] if t["faction"] in order], key=lambda t: order.index(t["faction"]))
    path = OUT / (PREFIX + name)
    c = canvas.Canvas(str(path), pagesize=letter)
    c.setTitle("Imperator Black — Unit Cards")
    c.setAuthor("Imperator Black")
    slots = [(MARGIN_X, MARGIN_Y + CARD_H), (MARGIN_X + CARD_W, MARGIN_Y + CARD_H), (MARGIN_X, MARGIN_Y), (MARGIN_X + CARD_W, MARGIN_Y)]
    for i, t in enumerate(troops):
        if i and i % 4 == 0:
            c.showPage()
        if i % 4 == 0:
            cut_marks(c)
        x, y = slots[i % 4]
        draw_card(c, x, y, t)
    c.save()
    return path, len(troops)


# ---------------------------------------------------------------- behavior cards (chapter 10)

SOLO = C["behaviors"]
BEHAVIOR_COLOUR = colors.HexColor("#3a3f46")


def text_card(c, x, y, title, subtitle, flow, colour=BEHAVIOR_COLOUR, chapter="chapter 10"):
    c.setStrokeColor(RULE)
    c.setLineWidth(0.6)
    c.rect(x, y, CARD_W, CARD_H)
    band = 0.52 * inch
    c.setFillColor(colour)
    c.rect(x, y + CARD_H - band, CARD_W, band, stroke=0, fill=1)
    c.setFillColor(colors.white)
    c.setFont("Body-Bold", 12)
    c.drawString(x + 0.14 * inch, y + CARD_H - 0.27 * inch, title)
    c.setFont("Body", 7)
    c.drawString(x + 0.14 * inch, y + CARD_H - 0.43 * inch, subtitle)
    footer = 0.22 * inch
    c.setFillColor(MUTED)
    c.setFont("Body", 5.8)
    c.drawRightString(x + CARD_W - 0.14 * inch, y + 0.08 * inch, f"Imperator Black · content v{VERSION} · {chapter}")
    frame = Frame(x + 0.14 * inch, y + footer, CARD_W - 0.28 * inch, CARD_H - band - footer - 0.04 * inch,
                  leftPadding=0, rightPadding=0, topPadding=4, bottomPadding=0)
    frame.addFromList([KeepInFrame(frame._aW, frame._aH, flow, mode="shrink")], c)


def _numbered(lines):
    return [Paragraph(f"{i}. {x}", S_BODY) for i, x in enumerate(lines, 1)]


def _bulleted(lines):
    return [Paragraph(f"• {x}", S_BODY) for x in lines]


def behavior_flowables(bh):
    users = [e["name"] for e in C["factions"]["elements"] if e["behavior"] == bh["id"]]
    flow = [Paragraph(f"<i>{bh['summary']}</i>", S_BODY)]
    if users:
        flow.append(Paragraph("Used by: " + ", ".join(users) + ".", S_SMALL))
    if bh["close_quarters"]:
        flow += [Spacer(1, 2), Paragraph("<b>Player model within 6\":</b> close quarters first (reference card).", S_BODY)]
    flow += [Spacer(1, 3), Paragraph("Target in sight (Engaged)", S_HEAD)] + _numbered(bh["engaged"])
    flow += [Spacer(1, 3), Paragraph("No target (Alerted)", S_HEAD)] + _numbered(bh["alerted"])
    flow += [Paragraph(f"<b>Investigates:</b> {bh['investigate']}", S_BODY)]
    flow += [Spacer(1, 3), Paragraph("Reactions", S_HEAD)] + _bulleted(bh["reactions"])
    flow += [Spacer(1, 3), Paragraph("Close combat", S_HEAD), Paragraph(bh["close_combat"] + (" May surrender." if bh["surrenders"] else ""), S_BODY)]
    flow += [Spacer(1, 4), Paragraph("Read each list top-down; take the first line that applies. Unaware models do not activate or react.", S_SMALL)]
    return flow


def solo_reference_cards():
    """Three reference cards for running the enemy: (title, subtitle, flowables)."""
    aw = [Paragraph(f"<b>{a['name']}:</b> {a['text']}", S_BODY) for a in SOLO["awareness"]]
    noise = [Paragraph(f"<b>{n['name']}</b> ({n['sources']}): {n['heard'].lower() if n['alert'] == 0 else n['heard']}"
                       + (f"; alert +{n['alert']}." if n["alert"] else "."), S_BODY) for n in SOLO["noise"]]
    card1 = [Paragraph("Awareness", S_HEAD)] + aw + [Spacer(1, 3), Paragraph("Noise", S_HEAD)] + noise + \
            [Spacer(1, 3), Paragraph("The marker", S_HEAD)] + _bulleted(SOLO["marker"]) + \
            [Spacer(1, 3), Paragraph("The alarm", S_HEAD)] + _bulleted(SOLO["alarm"])
    card2 = [Paragraph("Who activates next", S_HEAD)] + _numbered(SOLO["activation_order"]) + \
            [Spacer(1, 3), Paragraph("Targets", S_HEAD)] + _numbered(SOLO["targets"]) + \
            [Spacer(1, 3), Paragraph("Moving", S_HEAD)] + _bulleted(SOLO["moving"]) + \
            [Spacer(1, 3), Paragraph("Surrender", S_HEAD)] + _bulleted(SOLO["surrender"]) + \
            [Spacer(1, 3), Paragraph("Tie-breakers", S_HEAD)] + _numbered(SOLO["tie_breakers"]) + \
            [Spacer(1, 3), Paragraph("Terms", S_HEAD)] + [Paragraph(f"<b>{x['term']}:</b> {x['text']}", S_BODY) for x in SOLO.get("terms", [])]
    card3 = [Paragraph("Drill: set by the element's lowest rank; a friendly Leader within 6\" raises it one step.", S_BODY), Spacer(1, 2)]
    card3 += [Paragraph(f"<b>{RANKS[d['rank']]}, {d['name']}:</b> {d['text']}", S_BODY) for d in SOLO["drills"]]
    card3 += [Spacer(1, 4), Paragraph("Close quarters", S_HEAD), Paragraph(SOLO["close_quarters"][0], S_BODY)] + _numbered(SOLO["close_quarters"][1:])
    return [("Enemy awareness", "Solo opponent reference 1 of 3", card1),
            ("Enemy turn", "Solo opponent reference 2 of 3", card2),
            ("Drill and close quarters", "Solo opponent reference 3 of 3", card3)]


def draw_behavior_card(c, x, y, bh):
    text_card(c, x, y, bh["name"], "Enemy behavior profile" + ("  ·  proposed" if bh["status"] == "proposed" else ""),
              behavior_flowables(bh))


def card_sheet(path, title, drawers):
    """drawers: list of callables (c, x, y) -> None, laid out four to a page."""
    c = canvas.Canvas(str(path), pagesize=letter)
    c.setTitle(title)
    c.setAuthor("Imperator Black")
    slots = [(MARGIN_X, MARGIN_Y + CARD_H), (MARGIN_X + CARD_W, MARGIN_Y + CARD_H), (MARGIN_X, MARGIN_Y), (MARGIN_X + CARD_W, MARGIN_Y)]
    for i, draw in enumerate(drawers):
        if i and i % 4 == 0:
            c.showPage()
        if i % 4 == 0:
            cut_marks(c)
        x, y = slots[i % 4]
        draw(c, x, y)
    c.save()
    return len(drawers)


def behavior_cards():
    path = OUT / f"{PREFIX}behavior-cards.pdf"
    drawers = [(lambda c, x, y, r=r: text_card(c, x, y, *r)) for r in solo_reference_cards()]
    drawers += [(lambda c, x, y, b=b: draw_behavior_card(c, x, y, b)) for b in SOLO["behaviors"]]
    return path, card_sheet(path, "Imperator Black — Behavior Cards", drawers)


# ---------------------------------------------------------------- quick reference

Q_TITLE = style(15, "Body-Bold", leading=18)
Q_H = style(10.5, "Body-Bold", leading=13, space_after=2)
Q_B = style(8.5, leading=10.6)
Q_SMALL = style(6.6, leading=8, colour=MUTED)


def qtable(rows, widths, header=True):
    data = [[Paragraph(str(c), style(7.6, "Body-Bold" if header and r == 0 else "Body", leading=9)) for c in row]
            for r, row in enumerate(rows)]
    t = Table(data, colWidths=widths, repeatRows=1 if header else 0)
    st = [("GRID", (0, 0), (-1, -1), 0.4, RULE), ("VALIGN", (0, 0), (-1, -1), "TOP"),
          ("TOPPADDING", (0, 0), (-1, -1), 1.5), ("BOTTOMPADDING", (0, 0), (-1, -1), 1.5),
          ("LEFTPADDING", (0, 0), (-1, -1), 3), ("RIGHTPADDING", (0, 0), (-1, -1), 3)]
    if header:
        st.append(("BACKGROUND", (0, 0), (-1, 0), colors.HexColor("#e8ecef")))
    t.setStyle(TableStyle(st))
    return t


def bullets(items):
    return [Paragraph("• " + i, Q_B) for i in items]


def quick_reference():
    path = OUT / f"{PREFIX}quick-reference.pdf"
    doc = SimpleDocTemplate(str(path), pagesize=letter, leftMargin=0.5 * inch, rightMargin=0.5 * inch,
                            topMargin=0.45 * inch, bottomMargin=0.45 * inch,
                            title="Imperator Black — Quick Reference", author="Imperator Black")
    W = letter[0] - inch
    half = (W - 0.25 * inch) / 2
    core = C["core"]

    def two_col(left, right):
        t = Table([[left, right]], colWidths=[half, half])
        t.setStyle(TableStyle([("VALIGN", (0, 0), (-1, -1), "TOP"), ("LEFTPADDING", (0, 0), (-1, -1), 0),
                               ("RIGHTPADDING", (0, 0), (0, 0), 0.25 * inch), ("RIGHTPADDING", (1, 0), (1, 0), 0)]))
        return t

    # Page 1 -----------------------------------------------------------
    kw_rows = [["Keyword", "Effect"]] + [[f'<b>{k["name"]}</b>', k["text"]] for group in ("weapon", "armour") for k in C["keywords"][group]]
    kw_rows.append(["<b>Suppressive Fire</b> (action)", "Auto only. Hit roll ignoring cover, no damage: +1 Suppression (+2 if beating 8 by 3+); target loses Overwatch, can't react this round."])
    keywords = qtable(kw_rows, [1.0 * inch, half - 1.0 * inch])

    left = [Paragraph("The round", Q_H)] + bullets([
        "<b>Initiative:</b> each side 2d6 + the Nerve of its Leader with the highest Nerve. Winner chooses who activates first. Reroll ties.",
        "<b>Activations:</b> alternate one model at a time; two actions each. When one side runs out, the other finishes.",
        "<b>End of round:</b> (1) Bleeding/Critical clocks; (2) Down models test 8+ to recover; (3) unhit models in cover lose 1 Suppression; (4) hazards; (5) mission clock and alert (p. 3); (6) clear Activated (Overwatch stays).",
    ]) + [Spacer(1, 4), Paragraph("Attributes", Q_H),
          qtable([["Attr.", "Used for"]] + [[a["short"], a["text"]] for a in ATTRS], [0.5 * inch, half - 0.5 * inch]),
          Paragraph("A test is 2d6 + the named attribute, −1 per Suppression, −1 if Wounded; 8+ unless stated. Rank is a label only.", Q_SMALL),
          Spacer(1, 4), Paragraph("Actions", Q_H), qtable([
        ["Action", "Effect"],
        ["Move", "Up to Move. Obstacle +2\". Contact with an enemy starts close combat."],
        ["Shoot", "Hit roll vs a visible target."],
        ["Aim", "+1 to the next Shoot this activation."],
        ["Suppressive Fire", "Auto only. Ignores cover. Hit: +1 Supp. (+2 by 3+), no reactions. No damage."],
        ["Throw", "Grenade; 2d6 + Physique, 8+ to land on the point. Handy."],
        ["Overwatch", "Last action. One reaction before this model next activates."],
        ["Find Cover", "In the open within 1\" of micro-terrain: 2d6 + Nerve, 7+ Light, 10+ Heavy."],
        ["Rally", "Remove 1 Suppression from self or a friend within 6\" (2 with a Leader)."],
        ["Medic", "2d6 + Tech (+2 Medic): a Down friend in contact."],
        ["Interact", "Objective, door, console or charge within 1\"."],
    ], [0.95 * inch, half - 0.95 * inch]), Spacer(1, 4), Paragraph("Reactions", Q_H)] + bullets([
        "<b>Who:</b> a model not yet activated (it becomes Activated), or a model on Overwatch (once).",
        "<b>Triggers (in its line of sight):</b> enemy Moves (interrupt anywhere along the move); Shoots, Throws, Suppressive Fire or Interacts (after); comes into view of an Overwatch model for any reason (at once).",
        "<b>Door opens:</b> (1) resolve the opening; (2) cracked-in grenade lands; (3) far side reacts; (4) opening side's Overwatch reacts; (5) opener carries on. Once per model per door.",
        "<b>One action:</b> Shoot the trigger at −1; Move up to half to cover or out of sight; or Find Cover.",
        "<b>Timing:</b> never simultaneous. The trigger resolves in full first (damage, Suppression, casualty shock); then only models still able may react. Down: no reaction; Pinned: Find Cover only.",
        "No reactions to reactions. Pinned: Find Cover only. 2+ Suppression removes Overwatch. Cumbersome can't react.",
    ])
    right = [Paragraph("Shooting", Q_H)] + bullets([
        f"Throw 2d6 hit dice <b>with</b> the damage dice. Hit: 2d6 + <b>Shoot</b> + modifiers ≥ <b>{core['hit_target']}</b>. Natural 2 misses; double 6 is a critical (+1 row).",
        "Damage total → wound row, +1 if critical, then step down by Armour − AP. Row ≤ 0: no effect.",
        "Range: beyond short −1, beyond double −2 (shotgun −2/−3). No maximum range.",
        "Cover: Light ≥⅓ hidden or a model in the way; Heavy ≥½ hidden behind solid material.",
    ]) + [Spacer(1, 3), qtable([["Hit modifier", "Value"]] + [[m["text"], f'{m["value"]:+d}'] for m in core["hit_modifiers"]],
                                [half - 0.5 * inch, 0.5 * inch]),
          Spacer(1, 5), Paragraph("Wound table", Q_H),
          qtable([["Row", "Total", "Result"]] + [[w["row"], f"{w['min']}–{w['max']}" if w["max"] < 99 else f"{w['min']}+", w["text"]] for w in core["wound_table"]],
                 [0.35 * inch, 0.5 * inch, half - 0.85 * inch]),
          Paragraph("Rows above 10 count as 10. Wounded = −1 to all rolls.", Q_SMALL),
          Spacer(1, 5), Paragraph("Weapon and armour keywords", Q_H), keywords]

    story = [Paragraph(f"Imperator Black — Quick Reference  <font size=8 color='#5b6670'>rules v{'.'.join(VERSION.split('.')[:2])}</font>", Q_TITLE),
             Spacer(1, 6), two_col(left, right)]

    # Page 2 -----------------------------------------------------------
    from reportlab.platypus import PageBreak
    story.append(PageBreak())
    weapons = qtable([["Weapon", "Dmg", "Short", "AP", "Keywords"]] +
                     [row for w in C["weapons"]["ranged"] if not w.get("faction") for row in
                      [[w["name"], w["damage"], f'{w["short_range"]}"', w["ap"], ", ".join(f'Auto {w["auto"]}' if k == "auto" and w.get("auto") else KEYWORDS[k]["name"] for k in w["keywords"]) or "—"]] +
                      [[f'  or {r["name"].lower()}', r["damage"], f'{w["short_range"]}"', r["ap"], f'Explosion ({r["explosion"]})' if r.get("explosion") else "—"] for r in w.get("rounds", [])]],
                     [0.95 * inch, 0.4 * inch, 0.42 * inch, 0.3 * inch, half - 2.07 * inch])
    melee = qtable([["Melee", "Dmg", "AP"]] + [[m["name"], m["damage"], m["ap"]] for m in C["weapons"]["melee"] if not m.get("faction")],
                   [half - 0.7 * inch, 0.4 * inch, 0.3 * inch])
    armour = qtable([["Armour", "Value", "Keywords"]] + [[a["name"], a["value"], ", ".join(KEYWORDS[k]["name"] for k in a["keywords"]) or "—"]
                                                        for a in C["armour"]["armour"] if not a.get("faction")],
                    [1.2 * inch, 0.45 * inch, half - 1.65 * inch])
    left2 = [Paragraph("Weapons", Q_H), weapons, Spacer(1, 4), melee, Spacer(1, 4), armour, Spacer(1, 5),

             Paragraph("Close combat", Q_H)] + bullets([
        "Starts when a model Moves into contact (a Move: it can trigger reactions). Resolves at once, in exchanges.",
        "Each exchange: both roll 2d6 + <b>Fight</b>. Charge +1 (first exchange); +1 per friend also touching the enemy (max +2); −1 per Suppression; −1 if Wounded.",
        "Higher wins and rolls its melee damage on the wound table. Tie: both +1 Suppression. No effect: loser still +1 Suppression.",
        "A model at 5 Suppression that loses is overpowered: Down, or captured if allowed.",
        "After each exchange, loser then winner declares Continue or Break; it ends only when both break (move 1\" apart).",
    ])
    left2 += [Spacer(1, 5), Paragraph("Tokens (cube colours)", Q_H), qtable([
        ["Token", "Cube", "Use"],
        ["Activated", "Blue", "Activated or reacted unactivated this round. Clear at end of round."],
        ["Overwatch", "Green", "One when placed; remove it when the model reacts or next activates."],
        ["Suppression", "Yellow", "One per point, max 5."],
        ["Wounded", "Red", "One per Wounded marker."],
        ["Bleeding / Critical", "Black", "On a Down model (lay it on its side): 3 / 1 rounds left; remove one each later round end. On a door: locked."],
        ["Hack progress", "Red / White", "On the door: red per failed attempt (+1 each, max 3), white per success."],
        ["Last-known position", "White", "The enemy marker. Also marks a surrendered model."],
    ], [0.95 * inch, 0.55 * inch, half - 1.5 * inch]),
        Paragraph("Alerted enemy element: turn its unit cards sideways.", Q_SMALL)]
    grenades = qtable([["Grenade", "Range", "Radius", "Effect"]] +
                      [[g["name"], f'{g["throw_range"]}"', f'{g["blast_radius"]}"', (g.get("damage") and f'{g["damage"]} damage') or g.get("effect", "")]
                       for g in C["weapons"]["thrown"] if not g.get("faction")],
                      [0.85 * inch, 0.52 * inch, 0.58 * inch, half - 1.95 * inch])
    right2 = [Paragraph("Grenades and breaching", Q_H)] + bullets([
        "Throw up to double range: test 2d6 + <b>Physique</b> (−1 beyond range), 8+ lands on target. Miss: lands short by the miss (min 1\"). Natural 2: lands 2\" in front of the thrower. Zero-g: bounces past instead.",
        "Frag: one damage roll for all; step down per model's Armour; Heavy cover −1 row; same compartment ≤ 6\"×6\": +1 row.",
        "Breaching charge: place (Interact), detonate later (action). Door destroyed; within 1\": 2d6; far side within 2\": +2 Suppression, no reactions this round.",
        "<b>Crack and throw:</b> within 1\" of a closed, unlocked door: open it and Throw in one Interact, before anyone reacts. Miss: lands in the doorway.",
        "Locked door: charge; breaching tool (Interact, test 8+); or hack (Interact, 2d6 + Tech, quiet): " + "; ".join(f'{l["name"]} {l["target"]}+' + (f' ×{l["successes"]}' if l["successes"] > 1 else "") for l in core["security"]["levels"]) + ". +1 per earlier failed attempt on it (max +3).",
    ]) + [Spacer(1, 3), grenades, Spacer(1, 5), Paragraph("Suppression, morale, casualties", Q_H)] + bullets([
        "Suppression: −1 to all rolls each (max 5). <b>Pinned at Nerve + 2</b> (Nerve 4: never): only Rally, Find Cover, Move to cover/out of sight, Shoot from cover.",
        "Casualty shock: friend within 6\" goes Down or Dead: +1 Suppression (+2 if the Leader).",
        "Breaking: half a fireteam out → each tests 2d6 + <b>Nerve</b> (+1 Leader within 6\") at <b>every</b> activation; fail = both actions moving away (normal Moves). Enemy elements count as fireteams; Automated never test. Off the board via entry point = fled.",
        "Wounded markers from rows 4–5; any marker −1 to all rolls; more markers than Wounds → Down. Wounded is a flat −1, not per marker. Down: end of round 2d6 + <b>Physique</b> 8+ to rise, no Wounded penalty (gets up with 3 Suppression, markers set to Wounds); no test while a standing enemy is in base contact. Capture (if allowed): Interact in contact with a Down enemy. Medic action, 2d6 + <b>Tech</b>: Down 8+ (auto with Medic), Bleeding 8+ (becomes Down), Critical 10+ (stabilised).",
    ]) + [Spacer(1, 5), Paragraph("Ship hazards", Q_H)] + bullets([
        "<b>Zero-g:</b> push off in a straight line; end on a surface or keep drifting. Unbraced −1 to shoot, throw and fight (not Void-trained).",
        "<b>Decompression:</b> each model tests 2d6 + Physique, 8+ (+2 Sealed), or is dragged d6\" to the breach; doors seal at end of round; vacuum follows.",
        "<b>Vacuum:</b> unsealed models ending an activation there go Critical. Shooting is silent. No fire or smoke.",
        "<b>Fire:</b> 2d6 damage on entering or ending there (Sealed +1 armour); spreads on 10+, out on 2–3; extinguish 2d6 + Tech, 8+.",
        "<b>Darkness:</b> seen only within 12\"; −2 beyond 6\" unless Low-light; a model that shot this round is seen at −1.",
    ])
    story += [Paragraph("Imperator Black — Quick Reference (2)", Q_TITLE), Spacer(1, 6), two_col(left2, right2)]

    # Page 3: missions (chapter 13) -------------------------------------
    M = C["missions"]
    al, ck = M["alert"], M["clock"]
    left3 = [Paragraph("Alert track (0–6)", Q_H),
             qtable([["Raises the alert", "By"]] + [[r["source"], f'+{r["amount"]}'] for r in al["raises"]], [half - 0.4 * inch, 0.4 * inch]),
             Spacer(1, 3),
             qtable([["At", "Threshold (once each, at the end of the round)"]] + [[f'<b>{t["at"]}</b>', t["effect"]] for t in al["thresholds"]],
                    [0.3 * inch, half - 0.3 * inch]),
             Spacer(1, 5), Paragraph("Mission clock", Q_H),
             qtable([["Size", "Player force", "Die", "Limit"]] + [[z["name"], z["force"], z["die"], f'{z["limit"]} min'] for z in ck["sizes"]],
                    [1.0 * inch, half - 2.0 * inch, 0.4 * inch, 0.6 * inch])] + bullets(ck["rules"]) + [
             Spacer(1, 3), Paragraph("<b>End of each round:</b> (1) roll the clock die and move the clock; a check-in reached or passed: alert +1. "
                                     "(2) Alert threshold effects. (3) Time run out: the mission ends.", Q_B),
             Spacer(1, 5), Paragraph("Complications (d6)", Q_H),
             qtable([["d6", "Complication"]] + [[c["roll"].replace("-", "–"), f'<b>{c["name"]}:</b> {c["text"]}'] for c in M["complications"]],
                    [0.35 * inch, half - 0.35 * inch]),
             Spacer(1, 5), Paragraph("Extraction and the end", Q_H)] + bullets([
        "A player model that leaves by its airlock or its own edge is extracted, with whatever it carries (data, cargo, a hostage or prisoner in base contact).",
        "The mission ends when every player model is extracted, Down, Dead or captured, or when time runs out.",
        "Models still on the board are left behind; left behind Down: captured (campaign).",
    ])
    tasks = []
    for m in M["missions"]:
        tasks += [Paragraph(f'<b>{m["name"]}.</b> {m["task"]}', Q_B),
                  Paragraph(f'<i>Lockdown (6):</i> {m["lockdown"]}', Q_SMALL), Spacer(1, 2)]
    right3 = [Paragraph("Contacts", Q_H)] + bullets(M["contacts"]) + \
             [Spacer(1, 2)] + bullets([f'<b>{c["name"]}:</b> {c["text"]}' for c in M["contact_results"]]) + \
             [Spacer(1, 5), Paragraph("Objectives: taking them", Q_H)] + tasks + \
             [Spacer(1, 3), Paragraph("Grades", Q_H), Paragraph(
                 "Each mission type grades <b>Decisive</b> (objective done and no player model Dead), <b>Marginal</b> (objective done) "
                 "or <b>Failure</b>; the mission sheet gives the exact wording.", Q_B)]
    story += [PageBreak(), Paragraph("Imperator Black — Quick Reference (3): Missions", Q_TITLE), Spacer(1, 6), two_col(left3, right3)]

    # Page 4: after-action (chapter 14 draft) -----------------------------
    Cp = C["campaign"]
    inj = qtable([["2d6", "Injury"]] + [[r["roll"].replace("-", "–"), f'<b>{r["name"]}:</b> {r["text"]}'] for r in Cp["injuries"]],
                 [0.4 * inch, half - 0.4 * inch])
    mods = "; ".join(f'{m["value"]:+d} {m["text"][0].lower() + m["text"][1:]}' for m in Cp["injury_modifiers"])
    left4 = [Paragraph("After-action, in order", Q_H), Paragraph(
                 "(1) Casualties and injuries; (2) prisoners; (3) the grade's rewards and the node's outcome; (4) XP and the citation; (5) advances.", Q_B),
             Spacer(1, 4), Paragraph("1. Casualties", Q_H)] + bullets(Cp["casualties"]) + [
             Spacer(1, 3), inj, Paragraph(f"Modifiers: {mods}. 2 or less reads as 2; 12 or more as 12. Out: campaign turns missed, ticking down at Upkeep.", Q_SMALL),
             Spacer(1, 5), Paragraph("2–3. Prisoners and rewards", Q_H),
             qtable([["Result", "Reward"]] + [[f'<b>{r["result"]}</b>', r["text"]] for r in Cp["rewards"]], [1.35 * inch, half - 1.35 * inch]),
             Paragraph("A taken node also gives its own reward (node table).", Q_SMALL)]
    right4 = [Paragraph("4. XP (each soldier who deployed)", Q_H),
              qtable([["XP", "For"]] + [[f'+{x["amount"]}', x["text"]] for x in Cp["xp"]], [0.35 * inch, half - 0.35 * inch]),
              Spacer(1, 5), Paragraph(f'5. Advances (every {Cp["advances"]["every"]} XP)', Q_H),
              qtable([["d6", "Advance"]] + [[r["roll"], r["text"]] for r in Cp["advances"]["table"]], [0.35 * inch, half - 0.35 * inch])] + \
             bullets(Cp["advances"]["text"]) + [
              Spacer(1, 5), Paragraph("Citations and honours", Q_H)] + bullets([f'<b>{h["name"]}:</b> {h["text"]}' for h in Cp["honours"]]) + \
             [Paragraph(" · ".join(f'<b>{h["name"]}:</b> {h["text"]}' for h in Cp["honour_list"]), Q_SMALL),
              Spacer(1, 5), Paragraph("Then: Upkeep and Events", Q_H), Paragraph(
                  "Upkeep: recovering soldiers tick down one turn; ship upgrades finish; move the turn marker. Events: roll 2d6 (campaign sheets).", Q_B)]
    story += [PageBreak(), Paragraph("Imperator Black — Quick Reference (4): After-action  <font size=8 color='#5b6670'>campaign draft</font>", Q_TITLE),
              Spacer(1, 6), two_col(left4, right4)]

    # Page 5: two players (chapter 16 draft) ------------------------------
    Vs = C["versus"]
    import re as _re
    _b = lambda items: bullets([_re.sub(r"\*\*(.+?)\*\*", r"<b>\1</b>", x) for x in items])
    clocks = {z["id"]: z for z in C["missions"]["clock"]["sizes"]}
    vnames = {m["id"]: m for m in Vs["missions"]}
    left5 = [Paragraph("Forces", Q_H),
             qtable([["Size", "TV", "Board", "Clock"]] + [[z["name"], z["tv"], z["board"], f'{clocks[z["clock"]]["limit"]} min, {clocks[z["clock"]]["die"]}'] for z in Vs["sizes"]],
                    [0.8 * inch, 0.35 * inch, half - 1.95 * inch, 0.8 * inch])] + _b(Vs["forces"][:3]) + [
             Spacer(1, 4), Paragraph("Missions (d6)", Q_H),
             qtable([["d6", "Mission", "Hidden information"]] + [[r["roll"], vnames[r["mission"]]["name"], vnames[r["mission"]]["hidden_text"]] for r in Vs["mission_roll"]],
                    [0.3 * inch, 1.3 * inch, half - 1.6 * inch])]
    for m in Vs["missions"]:
        if m["kind"] == "even":
            left5 += [Paragraph(f'<b>{m["name"]}:</b> {m["scoring"]}', Q_SMALL)]
    left5 += [Spacer(1, 4), Paragraph("Ending and winning", Q_H)] + _b(Vs["end"][:2] + Vs["casualty_points"] + Vs["even_grades"])
    right5 = [Paragraph("Blinds", Q_H)] + _b(Vs["blinds"]["rules"]) + [
              Spacer(1, 4), Paragraph("Raids", Q_H)] + _b(Vs["raid"]["setup"][1:3]) + [
              Spacer(1, 2), Paragraph("<b>The alarm</b>", Q_B)] + _b(Vs["raid"]["alarm"][:2]) + [
              Spacer(1, 2), Paragraph("<b>The alert track</b>", Q_B)] + _b(Vs["raid"]["alert"][1:]) + [
              Paragraph(Vs["raid"]["grades"][0], Q_SMALL)]
    story += [PageBreak(), Paragraph("Imperator Black — Quick Reference (5): Two players  <font size=8 color='#5b6670'>draft</font>", Q_TITLE),
              Spacer(1, 6), two_col(left5, right5)]
    doc.build(story)
    return path


# ---------------------------------------------------------------- deck builder (chapter 11)

def deck_builder():
    """Two-page table sheet for building a deck at the table."""
    from reportlab.platypus import PageBreak
    Ly = C["layouts"]
    rooms = {r["id"]: r for r in Ly["rooms"]}
    sec = {s["id"]: s["name"] for s in C["core"]["security"]["levels"]}
    haz = {h["id"]: h for h in Ly["hazards"]}
    path = OUT / f"{PREFIX}deck-builder.pdf"
    doc = SimpleDocTemplate(str(path), pagesize=letter, leftMargin=0.45 * inch, rightMargin=0.45 * inch,
                            topMargin=0.4 * inch, bottomMargin=0.4 * inch, title="Imperator Black — Deck Builder")
    W = letter[0] - 0.9 * inch
    half = (W - 0.25 * inch) / 2

    def d6(rows, label="Result"):
        return qtable([["d6", label]] + [[str(r["roll"]).replace("-", "–"), r["text"]] for r in rows], [0.35 * inch, half - 0.35 * inch])

    def two_col(left, right):
        tb = Table([[left, right]], colWidths=[half + 0.125 * inch, half + 0.125 * inch])
        tb.setStyle(TableStyle([("VALIGN", (0, 0), (-1, -1), "TOP"), ("LEFTPADDING", (0, 0), (-1, -1), 0), ("RIGHTPADDING", (0, 0), (-1, -1), 6)]))
        return tb

    vessels = qtable([["Vessel", "Board", "Spine", "Rooms", "Heart (locks)", "Locks", "Hazard 5 / 6"]] +
                     [[v["name"], f'{v["board"][0]}"×{v["board"][1]}"', v["spine"], f'{v["compartments"][0]}–{v["compartments"][1]}',
                       f'{rooms[v["heart"]]["name"]} ({sec[v["heart_security"]]})', sec[v["security"]],
                       " / ".join(haz[h]["name"] for h in v["hazards"])] for v in Ly["vessels"]],
                     [1.45 * inch, 0.6 * inch, 0.45 * inch, 0.5 * inch, 1.75 * inch, 0.65 * inch, W - 5.4 * inch])
    vroll = " · ".join(f'{r["roll"].replace("-", "–")} {next(v["name"] for v in Ly["vessels"] if v["id"] == r["vessel"])}' for r in Ly["vessel_roll"])
    rooms_tbl = [["d6"] + [v["name"].split(" /")[0] for v in Ly["vessels"]]]
    for roll in range(1, 7):
        row = [str(roll)]
        for v in Ly["vessels"]:
            for r in v["rooms"]:
                lo, _, hi = str(r["roll"]).partition("-")
                if int(lo) <= roll <= int(hi or lo):
                    row.append(rooms[r["room"]]["name"] + (" †" if rooms[r["room"]].get("unique") else ""))
        rooms_tbl.append(row)
    steps = bullets([
        "<b>1. Vessel:</b> from the mission, or roll: " + vroll + ".",
        "<b>2. Spine:</b> docking passage against the middle of the west edge, running inward (the airlock). Then junction, passage, repeated for the spine count. At each junction after the first, roll the turn. A tile that won't fit turns toward more open board; if it fits nowhere, the spine ends.",
        "<b>3. Heart</b> against the far end of the spine, with a door (or beside the last spine tile with room).",
        "<b>4. Branches:</b> walk the spine from the airlock (skip the docking passage); one roll per passage and per junction. A room that won't fit isn't placed. At the vessel's most rooms, ignore further compartments; a side passage then ends in a small room.",
        "<b>5. Fill:</b> fewer rooms than the minimum? Add one beside each spine tile from the far end back. Still short: dry-erase map.",
        "<b>6. Rooms:</b> roll each compartment (not the heart) and each small room. † unique: on a repeat, take the next line down.",
        "<b>7. Second doors:</b> each compartment, for each other tile it touches with no door (not small rooms): roll.",
        "<b>8. Doors:</b> spine and side-passage joints are open archways; roll every other door. Heart doors: always locked at the heart's level. Other locks: the room's level, else the vessel's.",
        "<b>9. Cover:</b> heavy pieces in the middle, light pieces in the corners farthest from the doors, a clear lane in front of every door.",
        "<b>10. Hazard:</b> roll once.",
    ])
    left = steps + [Spacer(1, 4), Paragraph("Spine turn", Q_H), d6(Ly["spine_turn"]), Spacer(1, 3), Paragraph("Each passage", Q_H), d6(Ly["passage_branch"])]
    right = [Paragraph("Each junction (open sides)", Q_H), d6(Ly["junction_branch"]),
             Paragraph("Station modules add 1.", Q_SMALL), Spacer(1, 3),
             Paragraph("Second door", Q_H), d6(Ly["loop_door"]), Spacer(1, 3),
             Paragraph("Door state", Q_H), d6(Ly["door_state"]), Spacer(1, 3),
             Paragraph("Small room", Q_H),
             qtable([["d6", "Room"]] + [[r["roll"], rooms[r["room"]]["name"] + (f': {rooms[r["room"]]["text"]}' if rooms[r["room"]].get("text") else "")] for r in Ly["small_rooms"]],
                    [0.35 * inch, half - 0.35 * inch]), Spacer(1, 3),
             Paragraph("Hazard", Q_H), d6(Ly["hazard_roll"]),
             qtable([[h["name"], h["text"]] for h in Ly["hazards"]], [0.9 * inch, half - 0.9 * inch], header=False)]
    story = [Paragraph("Imperator Black — Deck Builder", Q_TITLE),
             Paragraph(f"Rules chapter 11 · content v{VERSION} · 3\" grid · compartments 6\"×6\", passages 6\"×3\", junctions and small rooms 3\"×3\"", Q_SMALL),
             Spacer(1, 5), vessels, Spacer(1, 5), two_col(left, right), PageBreak(),
             Paragraph("Rooms (d6, per compartment)", Q_H),
             qtable(rooms_tbl, [0.35 * inch] + [(W - 0.35 * inch) / 4] * 4), Spacer(1, 6),
             Paragraph("Room cover", Q_H),
             qtable([["Room", "Cover"]] + [[r["name"] + (f' ({sec[r["security"]]} locks)' if r.get("security") else ""),
                                            "; ".join(f'{c["label"]} ({c["type"]}): {c["piece"].rstrip(".")}' for c in r["cover"]) or "None"]
                                           for r in Ly["rooms"]], [1.6 * inch, W - 1.6 * inch])]
    doc.build(story)
    return path


# ---------------------------------------------------------------- land battles (chapter 12)

def land_reference():
    from reportlab.platypus import PageBreak
    T = C["terrain"]
    path = OUT / f"{PREFIX}land-battles.pdf"
    doc = SimpleDocTemplate(str(path), pagesize=letter, leftMargin=0.45 * inch, rightMargin=0.45 * inch,
                            topMargin=0.4 * inch, bottomMargin=0.4 * inch, title="Imperator Black — Land Battles")
    W = letter[0] - 0.9 * inch
    moves = {"normal": "Normal", "difficult": "Difficult (each inch costs 2\")", "obstacle": "Obstacle (+2\")", "blocked": "Blocked"}
    types = qtable([["Terrain", "Cover", "Movement", "Line of sight"]] +
                   [[x["name"], x["cover"], moves.get(x["move"], x["move"]), x["sight"]] for x in T["types"]],
                   [1.35 * inch, 2.2 * inch, 1.45 * inch, W - 5.0 * inch])
    envs = T["environments"]
    env_tbl = qtable([["d6"] + [e["name"] for e in envs[:4]]] +
                     [[str(n)] + [next(r["text"] for r in e["table"] if r["roll"] == str(n)) for e in envs[:4]] for n in range(1, 7)] +
                     [["Fill"] + [e["fill"] for e in envs[:4]]], [0.35 * inch] + [(W - 0.35 * inch) / 4] * 4)
    env_tbl2 = qtable([["d6"] + [e["name"] for e in envs[4:]]] +
                      [[str(n)] + [next(r["text"] for r in e["table"] if r["roll"] == str(n)) for e in envs[4:]] for n in range(1, 7)] +
                      [["Fill"] + [e["fill"] for e in envs[4:]]], [0.35 * inch] + [(W - 0.35 * inch) / 3] * 3)
    eroll = " · ".join(f'{r["roll"]} {next(e["name"] for e in envs if e["id"] == r["environment"])}' for r in T["environment_roll"])
    story = [Paragraph("Imperator Black — Land Battles", Q_TITLE),
             Paragraph(f"Rules chapter 12 · content v{VERSION}", Q_SMALL), Spacer(1, 5),
             Paragraph("Terrain", Q_H), types, Spacer(1, 5),
             Paragraph("Height", Q_H)] + bullets(T["height"]["rules"]) + [Spacer(1, 4),
             Paragraph("Conditions (d6)", Q_H),
             qtable([["d6", "Conditions", "Effect"]] + [[c["roll"].replace("-", "–"), c["name"], c["text"]] for c in T["conditions"]],
                    [0.4 * inch, 1.3 * inch, W - 1.7 * inch]), Spacer(1, 4),
             Paragraph("Noise in the open", Q_H), Paragraph(T["noise"]["text"] + " Inside a closed building, 12\".", Q_B),
             PageBreak(), Paragraph("Setting up a land table", Q_H)] + bullets([
        "<b>1. Environment:</b> from the mission, or roll: " + eroll + ". Fortified positions by mission only.",
        "<b>2. Board:</b> " + "; ".join(f'{b["models"]}: {b["size"]} ({b["sectors"]} sectors)' for b in T["boards"]) + ".",
        "<b>3. Sectors:</b> divide the board into 12\" squares; roll once per sector and place the result near its middle, facing your choice.",
        "<b>4. Density check:</b> three sectors in a line, edge to edge, with no cover? Change the middle one to the environment's Fill.",
        "<b>5. Conditions:</b> roll once. <b>6. Deployment:</b> player within 6\" of a rolled edge; enemy in the farthest sectors; its exit is its own edge.",
    ]) + [Spacer(1, 5), env_tbl, Spacer(1, 5), env_tbl2, Spacer(1, 5),
          Paragraph("Solo opponent terms", Q_H)] + bullets([f"<b>{x['term']}:</b> {x['text']}" for x in SOLO.get("terms", [])])
    doc.build(story)
    return path


if __name__ == "__main__":
    OUT.mkdir(parents=True, exist_ok=True)
    p, n = unit_cards()
    print(f"Wrote {p.relative_to(ROOT)} ({n} cards)")
    p, n = unit_cards(("republic",), "republic-cards.pdf")
    print(f"Wrote {p.relative_to(ROOT)} ({n} cards)")
    p, n = unit_cards(("separatist",), "separatist-cards.pdf")
    print(f"Wrote {p.relative_to(ROOT)} ({n} cards)")
    p, n = unit_cards(("anchorage", "hankou"), "near-present-cards.pdf")
    print(f"Wrote {p.relative_to(ROOT)} ({n} cards)")
    p, n = unit_cards(("generic",), "generic-cards.pdf")
    print(f"Wrote {p.relative_to(ROOT)} ({n} cards)")
    print(f"Wrote {quick_reference().relative_to(ROOT)}")
    p, n = behavior_cards()
    print(f"Wrote {p.relative_to(ROOT)} ({n} cards)")
    print(f"Wrote {deck_builder().relative_to(ROOT)}")
    print(f"Wrote {land_reference().relative_to(ROOT)}")
