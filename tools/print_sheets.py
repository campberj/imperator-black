#!/usr/bin/env python3
"""Generate printable unit cards and a quick-reference sheet from content/*.yaml.

Run from the repo root: python3 tools/print_sheets.py
Writes docs/print/unit-cards.pdf (3.5" x 5" cards, four per US Letter page, with cut marks)
and docs/print/quick-reference.pdf (two pages).

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
    for i, wid in enumerate([t["weapon"]] + t.get("weapon_options", [])):
        w = RANGED[wid]
        label = f"<b>{w['name']}</b>" if i == 0 else f"<i>or {w['name']}</i>"
        rows.append([Paragraph(label, S_CELL), w["damage"], f'{w["short_range"]}"', str(w["ap"])])
        kws = w["keywords"] + (extra_kw if i == 0 else [])
        pen = w.get("range_penalty")
        notes = [f'Auto {w["auto"]}' if k == "auto" and w.get("auto") else KEYWORDS[k]["name"] for k in kws]
        if pen:
            notes.append(f"beyond short {pen[0]}, beyond double {pen[1]}")
        if notes:
            rows.append([Paragraph(", ".join(notes), S_SMALL), "", "", ""])
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
    flow += [stats, Paragraph(f"Armour: {arm_text}", S_SMALL), Spacer(1, 3)]

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
    c.setFont("Body", 7)
    c.drawString(x + 0.14 * inch, y + CARD_H - 0.43 * inch, f"{RANKS[t['rank']]} {t['role']}  ·  {fac['name']}")

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


def unit_cards():
    order = ["tfb", "freespace", "contractor", "directorate"]
    troops = sorted(C["troops"]["troops"], key=lambda t: order.index(t["faction"]))
    path = OUT / "unit-cards.pdf"
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
    path = OUT / "quick-reference.pdf"
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
    kw_rows.append(["<b>Suppressive Fire</b> (action)", "Auto weapons only. Hit roll, no damage: +1 Suppression, or +2 if the roll beats 8 by 3+."])
    keywords = qtable(kw_rows, [1.0 * inch, half - 1.0 * inch])

    left = [Paragraph("The round", Q_H)] + bullets([
        "<b>Initiative:</b> each side 2d6 + the Nerve of its Leader with the highest Nerve. Winner chooses who activates first. Reroll ties.",
        "<b>Activations:</b> alternate one model at a time; two actions each. When one side runs out, the other finishes.",
        "<b>End of round:</b> (1) Bleeding/Critical clocks; (2) Down models test 8+ to recover; (3) unhit models in cover lose 1 Suppression; (4) hazards; (5) mission effects; (6) clear Activated (Overwatch stays).",
    ]) + [Spacer(1, 4), Paragraph("Attributes", Q_H),
          qtable([["Attr.", "Used for"]] + [[a["short"], a["text"]] for a in ATTRS], [0.5 * inch, half - 0.5 * inch]),
          Paragraph("A test is 2d6 + the named attribute, −1 per Suppression, −1 if Wounded; 8+ unless stated. Rank is a label only.", Q_SMALL),
          Spacer(1, 4), Paragraph("Actions", Q_H), qtable([
        ["Action", "Effect"],
        ["Move", "Up to Move. Obstacle +2\". Contact with an enemy starts close combat."],
        ["Shoot", "Hit roll vs a visible target."],
        ["Aim", "+1 to the next Shoot this activation."],
        ["Suppressive Fire", "Auto only. Hit: +1 Suppression (+2 if beating 8 by 3+). No damage."],
        ["Throw", "Grenade; 2d6 + Physique, 8+ to land on the point. Handy."],
        ["Overwatch", "Last action. Up to two reactions before this model next activates."],
        ["Find Cover", "In the open within 1\" of micro-terrain: 2d6 + Nerve, 7+ Light, 10+ Heavy."],
        ["Rally", "Remove 1 Suppression from self or a friend within 6\" (2 with a Leader)."],
        ["Medic", "Treat a Down/Bleeding/Critical friend in contact."],
        ["Interact", "Objective, door, console or charge within 1\"."],
    ], [0.95 * inch, half - 0.95 * inch]), Spacer(1, 4), Paragraph("Reactions", Q_H)] + bullets([
        "<b>Who:</b> a model not yet activated (it becomes Activated), or a model on Overwatch (up to two).",
        "<b>Triggers (in its line of sight):</b> enemy Moves (interrupt anywhere along the move); Shoots, Throws or Suppressive Fire (after); Interacts, incl. doors (after).",
        "<b>One action:</b> Shoot the trigger at −1; Move up to half to cover or out of sight; or Find Cover.",
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
                     [[w["name"], w["damage"], f'{w["short_range"]}"', w["ap"], ", ".join(f'Auto {w["auto"]}' if k == "auto" and w.get("auto") else KEYWORDS[k]["name"] for k in w["keywords"]) or "—"]
                      for w in C["weapons"]["ranged"]],
                     [0.95 * inch, 0.4 * inch, 0.42 * inch, 0.3 * inch, half - 2.07 * inch])
    melee = qtable([["Melee", "Dmg", "AP"]] + [[m["name"], m["damage"], m["ap"]] for m in C["weapons"]["melee"]],
                   [half - 0.7 * inch, 0.4 * inch, 0.3 * inch])
    armour = qtable([["Armour", "Value", "Keywords"]] + [[a["name"], a["value"], ", ".join(KEYWORDS[k]["name"] for k in a["keywords"]) or "—"]
                                                        for a in C["armour"]["armour"]],
                    [1.2 * inch, 0.45 * inch, half - 1.65 * inch])
    left2 = [Paragraph("Weapons", Q_H), weapons, Spacer(1, 4), melee, Spacer(1, 4), armour, Spacer(1, 5),

             Paragraph("Close combat", Q_H)] + bullets([
        "Starts when a model Moves into contact (a Move: it can trigger reactions). Resolves at once, in exchanges.",
        "Each exchange: both roll 2d6 + <b>Fight</b>. Charge +1 (first exchange); +1 per friend also touching the enemy (max +2); −1 per Suppression; −1 if Wounded.",
        "Higher wins and rolls its melee damage on the wound table. Tie: both +1 Suppression. No effect: loser still +1 Suppression.",
        "A model at 5 Suppression that loses is overpowered: Down, or captured if allowed.",
        "After each exchange the loser, then the winner, declares Continue or Break. It ends only when both break (each moves 1\" apart).",
    ])
    grenades = qtable([["Grenade", "Range", "Radius", "Effect"]] +
                      [[g["name"], f'{g["throw_range"]}"', f'{g["blast_radius"]}"', (g.get("damage") and f'{g["damage"]} damage') or g.get("effect", "")]
                       for g in C["weapons"]["thrown"]],
                      [0.85 * inch, 0.52 * inch, 0.58 * inch, half - 1.95 * inch])
    right2 = [Paragraph("Grenades and breaching", Q_H)] + bullets([
        "Throw up to double range: test 2d6 + <b>Physique</b> (−1 beyond range), 8+ lands on target. Miss: lands short by the miss (min 1\"). Natural 2: lands 2\" in front of the thrower. Zero-g: bounces past instead.",
        "Frag: one damage roll for all; step down per model's Armour; Heavy cover −1 row; same compartment ≤ 6\"×6\": +1 row.",
        "Breaching charge: place (Interact), detonate later (action). Door destroyed; within 1\": 2d6; far side within 2\": +2 Suppression, no reactions this round.",
        "Locked door: charge; breaching tool (Interact, test 8+); Tech hack (Interact, test 9+, quiet).",
    ]) + [Spacer(1, 3), grenades, Spacer(1, 5), Paragraph("Suppression, morale, casualties", Q_H)] + bullets([
        "Suppression: −1 to all rolls each (max 5). <b>Pinned at 3+</b>: only Rally, Find Cover, Move to cover/out of sight, Shoot from cover.",
        "Casualty shock: friend within 6\" goes Down or Dead: +1 Suppression (+2 if the Leader).",
        "Breaking: half a fireteam out → each tests 2d6 + <b>Nerve</b> (+1 Leader within 6\") at activation; fail = both actions moving away.",
        "Wounded markers from rows 4–5; any marker −1 to all rolls; more markers than Wounds → Down. Down: end of round 2d6 + <b>Physique</b> 8+ to rise (3 Suppression, markers = Wounds). Medic action, 2d6 + <b>Tech</b>: Down 8+ (auto with Medic), Bleeding 8+ (becomes Down), Critical 10+ (stabilised).",
    ]) + [Spacer(1, 5), Paragraph("Ship hazards", Q_H)] + bullets([
        "<b>Zero-g:</b> push off in a straight line; end on a surface or keep drifting. Unbraced −1 to shoot, throw and fight (not Void-trained).",
        "<b>Decompression:</b> each model tests 2d6 + Physique, 8+ (+2 Sealed), or is dragged d6\" to the breach; doors seal at end of round; vacuum follows.",
        "<b>Vacuum:</b> unsealed models ending an activation there go Critical. Shooting is silent. No fire or smoke.",
        "<b>Fire:</b> 2d6 damage on entering or ending there (Sealed +1 armour); spreads on 10+, out on 2–3; extinguish 2d6 + Tech, 8+.",
        "<b>Darkness:</b> seen only within 12\"; −2 beyond 6\" unless Low-light; a model that shot this round is seen at −1.",
    ])
    story += [Paragraph("Imperator Black — Quick Reference (2)", Q_TITLE), Spacer(1, 6), two_col(left2, right2)]
    doc.build(story)
    return path


if __name__ == "__main__":
    OUT.mkdir(parents=True, exist_ok=True)
    p, n = unit_cards()
    print(f"Wrote {p.relative_to(ROOT)} ({n} cards)")
    print(f"Wrote {quick_reference().relative_to(ROOT)}")
