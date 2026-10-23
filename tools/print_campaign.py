#!/usr/bin/env python3
"""Print the campaign sheets (rules chapter 14): campaign status with the arc map, force roster, and reference.

Usage (from the repo root): python3 tools/print_campaign.py
Writes docs/print/ImperatorBlack_campaign-sheets.pdf. Every table comes from content/campaign.yaml.

Short arcs: python3 tools/print_campaign.py --arc docs/scenarios/arcs/<arc>/arc.yaml
Writes docs/print/arcs/ImperatorBlack_arc-<id>.pdf: the story, the mission flow, the short arc rules and the roster.
"""
import sys
from pathlib import Path

import yaml

from reportlab.lib import colors
from reportlab.lib.pagesizes import letter
from reportlab.lib.units import inch
from reportlab.platypus import Flowable, PageBreak, Paragraph, SimpleDocTemplate, Spacer, Table, TableStyle

sys.path.insert(0, str(Path(__file__).resolve().parent))
import print_sheets as ps  # noqa: E402

CP = ps.C["campaign"]
MISSIONS = {m["id"]: m["name"] for m in ps.C["missions"]["missions"]}
VESSELS = {v["id"]: v["name"] for v in ps.C["layouts"]["vessels"]}
ENVS = {e["id"]: e["name"] for e in ps.C["terrain"]["environments"]}

H1 = ps.style(17, "Body-Bold", leading=20)
H2 = ps.style(10, "Body-Bold", leading=12, space_after=2)
B = ps.style(8, leading=9.8)
S = ps.style(6.8, leading=8.2, colour=ps.MUTED)
SHADE = colors.HexColor("#e8ecef")
W = letter[0] - 1.1 * inch


def boxes(labels, widths, sub=None, height=0.28 * inch, shade=()):
    rows = [[Paragraph(f"<b>{x}</b>", ps.style(7.5, align=1)) for x in labels]]
    if sub:
        rows.append([Paragraph(x, ps.style(5.6, leading=6.6, colour=ps.MUTED, align=1)) for x in sub])
    t = Table(rows, colWidths=widths, rowHeights=[height] + ([None] if sub else []))
    st = [("GRID", (0, 0), (-1, 0), 0.6, ps.INK), ("VALIGN", (0, 0), (-1, -1), "MIDDLE"),
          ("LEFTPADDING", (0, 0), (-1, -1), 1), ("RIGHTPADDING", (0, 0), (-1, -1), 1)]
    st += [("BACKGROUND", (i, 0), (i, 0), SHADE) for i in shade]
    t.setStyle(TableStyle(st))
    return t


def grid(head, rows, widths, blank=0, row_h=None, font=B):
    data = [[Paragraph(f"<b>{h}</b>", S) for h in head]] + [[Paragraph(str(c), font) for c in r] for r in rows]
    data += [[""] * len(head) for _ in range(blank)]
    t = Table(data, colWidths=widths, rowHeights=[None] + [row_h] * (len(data) - 1) if row_h else None, repeatRows=1)
    t.setStyle(TableStyle([("GRID", (0, 0), (-1, -1), 0.4, ps.RULE), ("BACKGROUND", (0, 0), (-1, 0), SHADE),
                           ("VALIGN", (0, 0), (-1, -1), "TOP"), ("TOPPADDING", (0, 0), (-1, -1), 1.5),
                           ("BOTTOMPADDING", (0, 0), (-1, -1), 1.5)]))
    return t


class ArcMap(Flowable):
    """The arc map template: the ship, three columns of node boxes (3, 2, target) and two spare boxes."""

    def __init__(self, width, height=2.9 * inch):
        super().__init__()
        self.width, self.height = width, height

    def wrap(self, *_):
        return self.width, self.height

    def node(self, c, x, y, w, h, title, tier, fixed=None):
        c.setStrokeColor(ps.INK)
        c.setLineWidth(0.8)
        c.setFillColor(colors.white)
        c.rect(x, y, w, h, fill=1, stroke=1)
        c.setFillColor(ps.INK)
        c.setFont("Body-Bold", 8)
        c.drawString(x + 4, y + h - 10, title)
        c.setFont("Body", 6.5)
        c.setFillColor(ps.MUTED)
        c.drawRightString(x + w - 4, y + h - 10, f"tier {tier}" if tier else "tier __")
        c.setFillColor(ps.INK)
        c.drawString(x + 4, y + h - 22, fixed or "Type: ______________________")
        c.drawString(x + 4, y + 13, "□ revealed   □ taken   Fail: □ □")
        c.drawString(x + 4, y + 3, "Turn struck: ___  Grade: ______")

    def draw(self):
        c, w, h = self.canv, self.width, self.height
        bw, bh = 1.55 * inch, 0.62 * inch
        xs = [0.95 * inch, 2.85 * inch, 4.75 * inch]
        cols = [[h - 0.15 * inch - bh - i * (bh + 0.22 * inch) for i in range(3)],
                [h - 0.6 * inch - bh - i * (bh + 0.45 * inch) for i in range(2)],
                [h / 2 - bh / 2 + 0.25 * inch]]
        ship = (0.05 * inch, h / 2 - 0.35 * inch + 0.25 * inch, 0.7 * inch, 0.7 * inch)
        # fixed links: ship to column 1, column 2 to the target
        c.setStrokeColor(ps.MUTED)
        c.setLineWidth(1)
        for y in cols[0]:
            c.line(ship[0] + ship[2], ship[1] + ship[3] / 2, xs[0], y + bh / 2)
        for y in cols[1]:
            c.line(xs[1] + bw, y + bh / 2, xs[2], cols[2][0] + bh / 2)
        c.setDash(2, 2)
        for y1 in cols[0]:   # possible links column 1 -> 2: draw the ones the dice give in ink
            for y2 in cols[1]:
                c.line(xs[0] + bw, y1 + bh / 2, xs[1], y2 + bh / 2)
        c.setDash()
        c.setFillColor(ps.FACTION_COLOUR["tfb"])
        c.roundRect(*ship, 6, fill=1, stroke=0)
        c.setFillColor(colors.white)
        c.setFont("Body-Bold", 9)
        c.drawCentredString(ship[0] + ship[2] / 2, ship[1] + ship[3] / 2 - 3, "SHIP")
        n = 1
        for ci, (x, ys) in enumerate(zip(xs, cols)):
            tier = CP["arc"]["columns"][ci]["tier"]
            for y in ys:
                if CP["arc"]["columns"][ci].get("target"):
                    self.node(c, x, y, bw, bh, f"{n}. Target", tier, "Operations hub: ship / land")
                else:
                    self.node(c, x, y, bw, bh, f"{n}.", tier)
                n += 1
        # spare boxes for holding sites and opportunities
        for y in (h - 0.15 * inch - bh, 0.3 * inch):   # above and below the target
            self.node(c, xs[2], y, bw, bh, "Extra node", None)
        c.setFillColor(ps.MUTED)
        c.setFont("Body", 6.5)
        c.drawString(0.05 * inch, 0.12 * inch, "Dashed: possible links. Ink the ones the dice give.")
        c.drawString(0.05 * inch, 0.02 * inch, "Extra nodes: holding sites, opportunities.")


def status_page():
    st = CP["standing"]
    story = [Paragraph("Campaign status", H1), Paragraph("Campaign: ______________________   Force: ______________________   Arc: ___", B),
             Paragraph("Imperator Black · chapter 14 (draft) · in a dry-erase sleeve or in pencil.", S), Spacer(1, 6)]
    story += [Paragraph(f"Campaign turn (an arc lasts {CP['arc']['turns']})", H2),
              boxes([str(i) for i in range(1, CP["arc"]["turns"] + 1)], [W / CP["arc"]["turns"]] * CP["arc"]["turns"]), Spacer(1, 5)]
    story += [Paragraph("Standing", H2),
              boxes([f'{x["level"]}' for x in st], [W / 7] * 7,
                    [f'{x["name"]} · income {x["income"]}' + (f' · support {x["support"]}' if x["support"] else "") for x in st], shade=(0,)),
              Spacer(1, 5)]
    ri = Table([[Paragraph("<b>Requisition</b>", B), "", Paragraph("<b>Intel</b>", B), boxes([str(i) for i in range(7)], [0.33 * inch] * 7, height=0.24 * inch),
                 Paragraph("<b>Evidence</b>", B), boxes([str(i) for i in range(7)], [0.33 * inch] * 7, height=0.24 * inch)]],
               colWidths=[0.8 * inch, 0.9 * inch, 0.45 * inch, 2.4 * inch, 0.65 * inch, 2.4 * inch])
    ri.setStyle(TableStyle([("BOX", (1, 0), (1, 0), 0.8, ps.INK), ("VALIGN", (0, 0), (-1, -1), "MIDDLE"),
                            ("LEFTPADDING", (0, 0), (-1, -1), 2)]))
    story += [ri, Paragraph("Evidence at 6: the Directorate is exposed (next arc against it).", S), Spacer(1, 6)]
    story += [Paragraph("Arc map", H2), ArcMap(W), Spacer(1, 6)]
    rows = []
    for sh in CP["ship"]:
        rows.append([Paragraph(f"<b>{sh['name']}</b>", B)] +
                    [Paragraph(f"□ <b>{i}</b> ({lv['cost']}): {lv['text']}", S) for i, lv in enumerate(sh["levels"], 1)])
    t = Table(rows, colWidths=[1.0 * inch] + [(W - 1.0 * inch) / 3] * 3)
    t.setStyle(TableStyle([("GRID", (0, 0), (-1, -1), 0.4, ps.RULE), ("VALIGN", (0, 0), (-1, -1), "TOP")]))
    story += [Paragraph("The ship", H2), t, Spacer(1, 6)]
    steps = " · ".join(f'<b>{i}. {s["step"]}</b>' for i, s in enumerate(CP["turn"], 1))
    story += [Paragraph("Each turn: " + steps, B)]
    return story


def roster_page():
    story = [PageBreak(), Paragraph("Force roster", H1),
             Paragraph(f"Up to {CP['start']['roster_max']} soldiers. An advance every {CP['advances']['every']} XP. "
                       "Out: turns still to miss (tick down at Upkeep).", S), Spacer(1, 4)]
    head = ["Name", "Troop and team", "Rank", "XP", "Advances taken", "Cit.", "Injuries", "Out", "Notes"]
    widths = [1.15 * inch, 1.2 * inch, 0.55 * inch, 0.4 * inch, 1.45 * inch, 0.4 * inch, 1.0 * inch, 0.35 * inch, W - 6.5 * inch]
    story += [grid(head, [], widths, blank=CP["start"]["roster_max"], row_h=0.36 * inch), Spacer(1, 8)]
    story += [Paragraph("Unit honours", H2),
              grid(["Team", "Marks (3 for an honour)", "Honour"], [], [2.2 * inch, 1.6 * inch, W - 3.8 * inch], blank=3, row_h=0.3 * inch),
              Paragraph(" · ".join(f"<b>{h['name']}:</b> {h['text']}" for h in CP["honour_list"]), S), Spacer(1, 8)]
    two = Table([[[Paragraph("Roll of honour", H2), grid(["Name", "Turn", "Where"], [], [1.5 * inch, 0.45 * inch, 1.6 * inch], blank=6, row_h=0.26 * inch)],
                  [Paragraph("Medals and prisoners", H2), grid(["Name", "Medal / prisoner taken", "Turn"], [], [1.3 * inch, 1.8 * inch, 0.45 * inch], blank=6, row_h=0.26 * inch)]]],
                colWidths=[W / 2, W / 2])
    two.setStyle(TableStyle([("VALIGN", (0, 0), (-1, -1), "TOP"), ("LEFTPADDING", (0, 0), (-1, -1), 0)]))
    story += [two]
    return story


def reference_pages():
    story = [PageBreak(), Paragraph("Campaign reference", H1), Spacer(1, 4)]
    nodes = []
    for n in CP["nodes"]:
        where = VESSELS[n["vessel"]] if n["setting"] == "ship" else ENVS[n["environment"]] + " (land)"
        nodes.append([n["roll"], n["name"], where, MISSIONS[n["mission"]], n["reward"]])
    for n in CP["special_nodes"]:
        nodes.append(["—", n["name"], n["setting"], MISSIONS[n["mission"]], n["reward"]])
    story += [Paragraph("Node table", H2), grid(["d6", "Node", "Board", "Mission", "Reward when taken"], nodes,
                                                 [0.3 * inch, 1.0 * inch, 1.5 * inch, 1.4 * inch, W - 4.2 * inch]), Spacer(1, 5)]
    story += [Paragraph("Map", H2)] + [Paragraph(f"{i}. {x}", B) for i, x in enumerate(CP["arc"]["procedure"], 1)] + \
             [Paragraph("• " + x, B) for x in CP["arc"]["taking"] + CP["arc"]["end"]] + [Spacer(1, 5)]
    story += [Paragraph("After-action rewards", H2), grid(["Result", "Reward"], [[r["result"], r["text"]] for r in CP["rewards"]], [1.9 * inch, W - 1.9 * inch]),
              Spacer(1, 5)]
    inj = [[r["roll"], r["name"], r["text"]] for r in CP["injuries"]]
    mods = "; ".join(f'{m["value"]:+d} {m["text"][0].lower() + m["text"][1:]}' for m in CP["injury_modifiers"])
    story += [Paragraph("Casualties and injuries", H2)] + [Paragraph("• " + x, B) for x in CP["casualties"]] + \
             [Spacer(1, 2), grid(["2d6", "Injury", "Effect"], inj, [0.45 * inch, 1.0 * inch, W - 1.45 * inch]),
              Paragraph(f"Modifiers: {mods}. 2 or less reads as 2; 12 or more as 12.", S), Spacer(1, 5)]
    xp = [[f'+{x["amount"]}', x["text"]] for x in CP["xp"]]
    adv = [[r["roll"], r["text"]] for r in CP["advances"]["table"]]
    two = Table([[[Paragraph("XP per mission", H2), grid(["XP", "For"], xp, [0.35 * inch, W / 2 - 0.5 * inch])],
                  [Paragraph(f"Advance (every {CP['advances']['every']} XP)", H2), grid(["d6", "Advance"], adv, [0.3 * inch, W / 2 - 0.45 * inch]),
                   Paragraph(CP["advances"]["text"][1], S)]]], colWidths=[W / 2, W / 2])
    two.setStyle(TableStyle([("VALIGN", (0, 0), (-1, -1), "TOP"), ("LEFTPADDING", (0, 0), (-1, -1), 0)]))
    story += [two, Spacer(1, 5)]
    story += [Paragraph("Events (2d6, end of each turn)", H2), grid(["2d6", "Event", "Effect"], [[e["roll"], e["name"], e["text"]] for e in CP["events"]],
                                                                    [0.4 * inch, 1.4 * inch, W - 1.8 * inch]), Spacer(1, 5)]
    story += [Paragraph("Intel and support", H2),
              grid(["Cost", "Intel use", "Effect"], [[x["cost"], x["name"], x["text"]] for x in CP["intel_uses"]], [0.4 * inch, 1.6 * inch, W - 2.0 * inch]),
              Spacer(1, 3),
              grid(["Support asset", "Effect"], [[x["name"], x["text"]] for x in CP["support"]], [1.6 * inch, W - 1.6 * inch]), Spacer(1, 5)]
    story += [Paragraph("Requisition", H2), grid(["Item", "Cost"], [[c["item"], c["cost"]] for c in CP["costs"]], [W - 1.2 * inch, 1.2 * inch]),
              Paragraph("A soldier's Requisition is its Troop Value ÷ 10, rounded up (chapter 15).", S)]
    return story


class ArcFlow(Flowable):
    """The short arc's flow: mission 1, then 2A (Decisive or Marginal) or 2B (Failure), then mission 3."""

    def __init__(self, width, names, height=1.25 * inch):
        super().__init__()
        self.width, self.height, self.names = width, height, names

    def wrap(self, *_):
        return self.width, self.height

    def box(self, x, y, w, h, label, name):
        c = self.canv
        c.setFillColor(SHADE)
        c.setStrokeColor(ps.INK)
        c.setLineWidth(0.8)
        c.roundRect(x, y, w, h, 4, stroke=1, fill=1)
        c.setFillColor(ps.INK)
        c.setFont("Body-Bold", 9)
        c.drawCentredString(x + w / 2, y + h - 13, label)
        c.setFont("Body", 7)
        c.drawCentredString(x + w / 2, y + h - 24, name)

    def arrow(self, x1, y1, x2, y2, text):
        c = self.canv
        c.setStrokeColor(ps.INK)
        c.setLineWidth(0.8)
        c.line(x1, y1, x2, y2)
        c.setFillColor(ps.INK)
        c.circle(x2, y2, 1.8, stroke=0, fill=1)
        c.setFillColor(ps.MUTED)
        c.setFont("Body", 6.5)
        tx, ty = x1 + (x2 - x1) * 0.4, y1 + (y2 - y1) * 0.4
        c.drawCentredString(tx, ty + (4 if y2 >= y1 else -9), text)

    def draw(self):
        w, h = 1.6 * inch, 0.42 * inch
        gap = (self.width - 3 * w) / 2
        mid = self.height / 2 - h / 2
        top, low = self.height - h - 2, 2
        x1, x2, x3 = 0, w + gap, 2 * (w + gap)
        self.box(x1, mid, w, h, "Mission 1", self.names["1"])
        self.box(x2, top, w, h, "Mission 2A", self.names["2A"])
        self.box(x2, low, w, h, "Mission 2B", self.names["2B"])
        self.box(x3, mid, w, h, "Mission 3 (finale)", self.names["3"])
        self.arrow(x1 + w, mid + h * 0.7, x2, top + h / 2, "Decisive or Marginal")
        self.arrow(x1 + w, mid + h * 0.3, x2, low + h / 2, "Failure")
        self.arrow(x2 + w, top + h / 2, x3, mid + h * 0.7, "any grade")
        self.arrow(x2 + w, low + h / 2, x3, mid + h * 0.3, "any grade")


def arc_sheet(path):
    arc = yaml.safe_load(Path(path).read_text())
    base = Path(path).parent
    ms = [(m, yaml.safe_load((base / m["file"]).read_text())) for m in arc["missions"]]
    names = {m["label"]: s["title"].split(": ", 1)[-1] for m, s in ms}
    opp = next(o["name"] for o in ps.C["missions"]["oppositions"] if o["id"] == arc["opposition"])
    faction = next(f["name"] for f in ps.C["factions"]["factions"] if f["id"] == arc["faction"])
    out = ps.OUT / "arcs" / f"{ps.PREFIX}arc-{arc['id']}.pdf"
    out.parent.mkdir(parents=True, exist_ok=True)
    doc = SimpleDocTemplate(str(out), pagesize=letter, leftMargin=0.55 * inch, rightMargin=0.55 * inch,
                            topMargin=0.45 * inch, bottomMargin=0.45 * inch, title=f"Imperator Black short arc: {arc['name']}", author="Imperator Black")
    story = [Paragraph(arc["name"], H1),
             Paragraph(f'Imperator Black short arc · rules v{arc["rules_version"]} · chapter 14 · {faction} against the {opp} · solo · three missions', S),
             Spacer(1, 5), Paragraph(" ".join(arc["summary"].split()), B), Spacer(1, 8), ArcFlow(W, names), Spacer(1, 8)]
    rows = []
    for m, s in ms:
        task = next(b["text"] for b in s["briefing"] if b["heading"] == "Tasking")
        rows.append([m["label"], f'<b>{names[m["label"]]}</b><br/>{task}', m["board"], f'{opp}, tier {s["tier"]}', m["force"]])
    story += [Paragraph("Missions", H2),
              grid(["", "Mission", "Board", "Opposition", "Force"], rows,
                   [0.3 * inch, 2.55 * inch, 1.95 * inch, 1.25 * inch, W - 6.05 * inch]), Spacer(1, 6)]
    nxt = []
    for m, s in ms:
        for x in s.get("next", []):
            nxt.append([m["label"], x["grade"], x["text"]])
    story += [Paragraph("What each grade leads to", H2), grid(["", "Grade", "Next"], nxt, [0.3 * inch, 0.7 * inch, W - 1.0 * inch]), Spacer(1, 6)]
    story += [Paragraph("Short arc rules (chapter 14)", H2)] + \
             [Paragraph(f"{i}. {x}", B) for i, x in enumerate(CP["short_arc"]["text"], 1)]

    # page 2: the roster and the log
    story += [PageBreak(), Paragraph(f'{arc["name"]}: roster and log', H1),
              Paragraph("Name every soldier the first time they deploy. Per mission: D if deployed, then XP earned; mark Dead, Down (and the injury roll) or Out. "
                        f"An advance every {CP['advances']['every']} XP. Count each soldier at its profile's TV.", S), Spacer(1, 4)]
    head = ["Name", "Profile", "TV", "Mission 1", "Mission 2", "Mission 3", "XP", "Advances", "Injuries / out", "Notes"]
    widths = [1.1 * inch, 1.1 * inch, 0.35 * inch, 0.62 * inch, 0.62 * inch, 0.62 * inch, 0.35 * inch, 1.05 * inch, 0.95 * inch, W - 6.76 * inch]
    story += [grid(head, [], widths, blank=arc.get("roster_rows", 12), row_h=0.34 * inch), Spacer(1, 8)]
    log = [[m["label"], names[m["label"]], "", "", ""] for m, _ in ms]
    story += [Paragraph("Mission log", H2),
              grid(["", "Mission", "Played?", "Grade", "Edge or setback carried on, citation, what happened"], log,
                   [0.3 * inch, 1.9 * inch, 0.6 * inch, 0.8 * inch, W - 3.6 * inch], row_h=0.4 * inch), Spacer(1, 8)]
    story += [Paragraph("The arc's result", H2),
              grid(["Result", "The story"], [[r["grade"], r["text"]] for r in arc["results"]], [0.9 * inch, W - 0.9 * inch]), Spacer(1, 4),
              boxes(["Finale grade", "One step better (1 and 2 both Decisive)", "One step worse (1 or 2 a Failure)", "Arc result"],
                    [W / 4] * 4, height=0.4 * inch)]
    doc.build(story)
    return out


def main():
    if "--arc" in sys.argv:
        print(f"Wrote {arc_sheet(sys.argv[sys.argv.index('--arc') + 1]).relative_to(ps.ROOT)}")
        return
    out = ps.OUT / f"{ps.PREFIX}campaign-sheets.pdf"
    ps.OUT.mkdir(parents=True, exist_ok=True)
    doc = SimpleDocTemplate(str(out), pagesize=letter, leftMargin=0.55 * inch, rightMargin=0.55 * inch,
                            topMargin=0.45 * inch, bottomMargin=0.45 * inch, title="Imperator Black campaign sheets", author="Imperator Black")
    doc.build(status_page() + roster_page() + reference_pages())
    print(f"Wrote {out.relative_to(ps.ROOT)}")


if __name__ == "__main__":
    main()
