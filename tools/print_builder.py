#!/usr/bin/env python3
"""Print the troop builder (rules chapter 15): a worksheet and the cost tables, from content/values.yaml.

Usage (from the repo root): python3 tools/print_builder.py
Writes docs/print/ImperatorBlack_troop-builder.pdf.
"""
import sys
from pathlib import Path

from reportlab.lib import colors
from reportlab.lib.pagesizes import letter
from reportlab.lib.units import inch
from reportlab.platypus import PageBreak, Paragraph, SimpleDocTemplate, Spacer, Table, TableStyle

sys.path.insert(0, str(Path(__file__).resolve().parent))
import print_sheets as ps  # noqa: E402
import troop_values as tvm  # noqa: E402

V = tvm.V
H1 = ps.style(16, "Body-Bold", leading=19)
H2 = ps.style(9.5, "Body-Bold", leading=11.5, space_after=2)
B = ps.style(7.8, leading=9.4)
S = ps.style(6.6, leading=7.8, colour=ps.MUTED)
SHADE = colors.HexColor("#e8ecef")
W = letter[0] - 1.0 * inch
HALF = (W - 0.25 * inch) / 2
NAMES = {a["id"]: a["name"] for a in ps.C["core"]["attributes"]}
fmt = lambda x: f"{x:g}" if x == int(x) else f"{x:.1f}"


def grid(rows, widths, head=True):
    data = [[Paragraph(str(c), S if (head and i == 0) else B) for c in r] for i, r in enumerate(rows)]
    t = Table(data, colWidths=widths, repeatRows=1 if head else 0)
    st = [("GRID", (0, 0), (-1, -1), 0.4, ps.RULE), ("VALIGN", (0, 0), (-1, -1), "TOP"),
          ("TOPPADDING", (0, 0), (-1, -1), 1.2), ("BOTTOMPADDING", (0, 0), (-1, -1), 1.2)]
    if head:
        st.append(("BACKGROUND", (0, 0), (-1, 0), SHADE))
    t.setStyle(TableStyle(st))
    return t


def two(left, right):
    t = Table([[left, right]], colWidths=[HALF, HALF])
    t.setStyle(TableStyle([("VALIGN", (0, 0), (-1, -1), "TOP"), ("LEFTPADDING", (0, 0), (-1, -1), 0),
                           ("RIGHTPADDING", (0, 0), (0, 0), 0.25 * inch)]))
    return t


def worksheet():
    lines = [("1", "Baseline (every attribute 0, Move 5\", Wounds 1, Size 2)", "10"),
             ("2", "Physique ___ × 2   Nerve ___ × 2   Tech ___ × 1", ""),
             ("3", "Move: (___\" − 5) × 2", ""),
             ("4", "Ranged weapon ______________ at Shoot ___ (weapon table)", ""),
             ("5", "Melee weapon ______________ at Fight ___ (melee table)", ""),
             ("6", "Gear, grenades, skills (× the named attribute's multiplier where shown)", ""),
             ("", "______________________________________________", ""),
             ("", "______________________________________________", ""),
             ("7", "Subtotal (lines 1–6)", ""),
             ("8", "× survival multiplier (Armour ___, Wounds ___)", ""),
             ("9", "× 1.3 if Automated", ""),
             ("10", "Troop Value (round to the nearest whole number)", ""),
             ("11", "Requisition: TV ÷ 10, rounded up", "")]
    rows = [["", "Line", "Value"]] + [[a, b, c] for a, b, c in lines]
    t = Table([[Paragraph(str(c), S if i == 0 else B) for c in r] for i, r in enumerate(rows)],
              colWidths=[0.3 * inch, W - 1.3 * inch, 1.0 * inch], rowHeights=[None] + [0.27 * inch] * len(lines))
    t.setStyle(TableStyle([("GRID", (0, 0), (-1, -1), 0.4, ps.RULE), ("BACKGROUND", (0, 0), (-1, 0), SHADE),
                           ("VALIGN", (0, 0), (-1, -1), "MIDDLE"), ("BACKGROUND", (2, 10), (2, 10), SHADE)]))
    return t


def main():
    out = ps.OUT / f"{ps.PREFIX}troop-builder.pdf"
    doc = SimpleDocTemplate(str(out), pagesize=letter, leftMargin=0.5 * inch, rightMargin=0.5 * inch,
                            topMargin=0.45 * inch, bottomMargin=0.45 * inch, title="Imperator Black troop builder", author="Imperator Black")
    m = tvm.mult
    story = [Paragraph("Troop builder", H1),
             Paragraph("Imperator Black · chapter 15 (draft) · Troop Value (TV) and Requisition. Name: ____________________  Faction: ____________  Class / rank: ____________", B),
             Spacer(1, 6)]
    stats = Table([[Paragraph(f"<b>{a['short']}</b>", ps.style(7, align=1)) for a in ps.C["core"]["attributes"]] + [Paragraph("<b>ARM</b>", ps.style(7, align=1))],
                   [""] * (len(ps.C["core"]["attributes"]) + 1)],
                  colWidths=[W / (len(ps.C["core"]["attributes"]) + 1)] * (len(ps.C["core"]["attributes"]) + 1), rowHeights=[None, 0.32 * inch])
    stats.setStyle(TableStyle([("GRID", (0, 0), (-1, -1), 0.6, ps.INK), ("BACKGROUND", (0, 0), (-1, 0), SHADE)]))
    story += [stats, Spacer(1, 6), worksheet(), Spacer(1, 8)]
    mult_t = grid([["Attribute value", "0", "1", "2", "3", "4"], ["Multiplier"] + [fmt(m(i)) for i in range(5)]],
                  [1.2 * inch] + [(HALF - 1.2 * inch) / 5] * 5)
    surv = grid([["", "Wounds 1", "Wounds 2", "Wounds 3"]] + [[f"Armour {a}"] + [f"{tvm.survival(a, w):.2f}" for w in (1, 2, 3)] for a in range(4)],
                [0.9 * inch] + [(HALF - 0.9 * inch) / 3] * 3)
    limits = [Paragraph("<b>Limits.</b> Class sets Shoot: irregular 0, trained 1, professional 2, elite 3, legendary 4+. "
                        "Fight, Physique, Nerve, Tech at most Shoot + 1, never above 4 (Physique and Tech 3 unless Augmented or Automated). "
                        "Move 5–7\" (8\" Augmented). Wounds 1; 2 for legendary heroes and Augmented elites; Automated 1/2/3 by Size. "
                        "Core kit freely; faction kit only for that faction. One Leader per team.", B)]
    story += [two([Paragraph("Multiplier", H2), mult_t, Spacer(1, 5)] + limits,
                  [Paragraph("Survival multiplier", H2), surv,
                   Paragraph("Expected hits to put the model Down (2d8, AP 0), compared with Armour 0 and Wounds 1, to the power 0.4.", S)])]
    story.append(PageBreak())

    def wtable(sel, title):
        rows = [[title, "S0", "S1", "S2", "S3", "S4"]]
        for w in sel:
            b = tvm.weapon_base(w["id"])
            rows.append([w["name"]] + [fmt(round(b * m(i), 1)) for i in range(5)])
        return grid(rows, [HALF - 5 * 0.42 * inch] + [0.42 * inch] * 5)

    ranged = ps.C["weapons"]["ranged"]
    core = [w for w in ranged if not w.get("faction")]
    fac = [w for w in ranged if w.get("faction")]
    melee = grid([["Melee weapon", "F0", "F1", "F2", "F3", "F4"]] +
                 [[w["name"]] + [fmt(round(tvm.melee_base(w["id"]) * m(i), 1)) for i in range(5)] for w in ps.C["weapons"]["melee"]],
                 [HALF - 5 * 0.42 * inch] + [0.42 * inch] * 5)

    def costs(group, names, per_level=False):
        rows = [["Item" if not per_level else "Skill", "Cost"]]
        for k, c in V[group].items():
            if k == "automated":
                continue
            per = f' × {NAMES[c["per"]]}' if c.get("per") else ""
            rows.append([names[k], fmt(c["cost"]) + (" /level" if per_level else "") + per])
        return grid(rows, [HALF - 1.1 * inch, 1.1 * inch])

    gear_names = {g["id"]: g["name"] for g in ps.C["gear"]["gear"]}
    gren_names = {g["id"]: g["name"] for g in ps.C["weapons"]["thrown"]}
    skill_names = {s["id"]: s["name"] for s in ps.C["skills"]["skills"]}
    row1 = two([Paragraph("Ranged weapons (by Shoot)", H2), wtable(core, "Core weapon")],
               [Paragraph("Faction weapons (by Shoot)", H2), wtable(fac, "Faction weapon"), Spacer(1, 4),
                Paragraph("Melee weapons (by Fight)", H2), melee])
    row2 = two([Paragraph("Gear", H2), costs("gear", gear_names), Spacer(1, 4), Paragraph("Grenades", H2), costs("grenades", gren_names)],
               [Paragraph("Skills", H2), costs("skills", skill_names, True),
                Paragraph("Skills that come with gear are paid by the gear. × Shoot / Fight / Tech / Physique: times that attribute's multiplier.", S)])
    story += [Paragraph("Troop builder: cost tables", H1), Spacer(1, 4), row1, Spacer(1, 6), row2]
    doc.build(story)
    print(f"Wrote {out.relative_to(ps.ROOT)}")


if __name__ == "__main__":
    main()
