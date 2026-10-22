# Alpha rules PDF: handoff for the build agent

**For:** an agent (or person) building the closed-alpha player packet for Imperator Black.
**From:** the project's design lead, 2026-10-22. **Owner:** RJ. **Rules version:** v0.5.
**Output:** one PDF, `ImperatorBlack_alpha-rules-v0.5.pdf`, plus nothing else unless asked.

Read this whole file before starting. Most of the work is assembling and typesetting files that already exist; **do not rewrite rules text**.

## 1. What the packet is

A private playtest packet for a small, closed group of testers RJ knows. It holds the core rules and missions (chapters 1–13), the forces, the printable play aids, three starter scenarios and two sample missions, and a feedback form.

**Not in this alpha:** chapter 14 (campaign), chapter 15 (troop values and building troops), the campaign sheets and the troop builder. They are drafts that RJ hasn't played yet. Leave them out entirely; don't mention them beyond what `welcome.md` already says.

## 2. Getting the files

The repository is `github.com/campberj/imperator-black` (branch `main`). RJ will give you access or attach the files. Everything you need is in:

| Folder | What |
|---|---|
| `docs/alpha/` | This handoff, `welcome.md` (front matter) and `feedback-form.md` (back matter) |
| `rules/` | The rules chapters, one Markdown file each (`01-…` to `13-…`) |
| `docs/print/` | Every printable aid, already laid out as PDFs (letter size) |
| `docs/print/decks/`, `docs/print/missions/` | Sample deck layouts and sample missions |
| `docs/design/glossary.md` | Optional glossary (see section 5) |

Pull the latest `main` before building: the content changes often, and the PDFs in `docs/print/` are regenerated from it.

## 3. Contents and order

| # | Section | Source | Notes |
|---|---|---|---|
| 1 | Cover | Write it | Title "Imperator Black", subtitle "Closed alpha · Rules v0.5", the line "Private playtest. Not for distribution." and the date. No artwork needed; if you add any, it must contain no occult, demonic or supernatural imagery (a firm project rule), and no third-party logos or characters. |
| 2 | Welcome | `docs/alpha/welcome.md` | Verbatim. |
| 3 | Contents | Generate | Chapter and appendix titles with page numbers. |
| 4 | Chapters 1–13 | `rules/01-core-concepts.md` … `rules/13-missions.md` | In order. Each chapter starts on a new page. See section 4 for the clean-up rules. |
| 5 | Glossary (optional) | `docs/design/glossary.md` | Include only if every entry matches the current rules; drop entries that mention the app or future phases. If unsure, leave it out. |
| 6 | Appendix A: Quick reference | `docs/print/ImperatorBlack_quick-reference.pdf` **pages 1–3 only** | Page 4 is the campaign after-action page: leave it out. |
| 7 | Appendix B: Solo opponent cards | `docs/print/ImperatorBlack_behavior-cards.pdf` | All pages. |
| 8 | Appendix C: Unit cards | In this order: `ImperatorBlack_generic-cards.pdf`, `ImperatorBlack_unit-cards.pdf` (Task Force Boone and the opposition), `ImperatorBlack_republic-cards.pdf`, `ImperatorBlack_near-present-cards.pdf` | Cards are 4 to a page with cut marks; keep them at 100% scale. |
| 9 | Appendix D: Building boards | `docs/print/ImperatorBlack_deck-builder.pdf`, `docs/print/ImperatorBlack_land-battles.pdf` | |
| 10 | Appendix E: Starter scenarios | Gym 1, 2, 3: each `…-sheet.pdf` followed by its `…-cards.pdf` (`gym-01-first-breach`, `gym-02-silence-the-relay`, `gym-03-wipe-race`) | Use `gym-03-wipe-race-cards.pdf`, not the `-and-unit-cards` set. |
| 11 | Appendix F: Sample decks | `docs/print/decks/*.pdf` (four files) | |
| 12 | Appendix G: Sample missions | `docs/print/missions/ImperatorBlack_mission-ship-42-sheet.pdf` and `-cards.pdf`, then `mission-land-11` the same way | |
| 13 | Appendix H: Mission dashboard | `docs/print/ImperatorBlack_mission-dashboard.pdf` | Blank, one page. |
| 14 | Feedback form | `docs/alpha/feedback-form.md` | Typeset like the chapters, on its own pages, ending the packet. |

Appendix PDFs are already final layouts: insert their pages as they are (no re-typesetting, no scaling except to fit letter if needed). Give each appendix a title page or a running header with its letter and name.

## 4. Clean-up rules for the chapters

The chapters were written in a working repository, so a few things must change in the packet. **Change only these; leave every rule, number and table as written.**

1. **Table markers.** Remove the HTML comments `<!-- table:… -->` and `<!-- /table -->`; keep the tables between them.
2. **Repository paths.** Replace references to files with references inside the packet:

| In the text | Replace with |
|---|---|
| `docs/print/ImperatorBlack_quick-reference.pdf` (or "the quick reference") | "the quick reference (Appendix A)" |
| `docs/print/ImperatorBlack_republic-cards.pdf`, `…near-present-cards.pdf`, `…generic-cards.pdf`, `docs/print/` cards in general | "the unit cards (Appendix C)" |
| `docs/print/decks/…` (chapter 11) | "Appendix F" |
| `docs/print/missions/…`, `tools/generate_mission.py` (chapter 13 example) | "Appendix G" (drop the tool name and command) |
| `ImperatorBlack_mission-dashboard.pdf` | "Appendix H" |
| `docs/design/opposition-force.md`, `docs/design/air-and-void.md` | Remove the reference and its sentence fragment, keeping the rest of the sentence |
| Links to other chapters (`11-ship-layout.md` and so on) | "chapter 11" and so on, as plain text or internal PDF links |
| "(chapter 14)" or "(chapter 15)" (the campaign and troop values, not in this alpha) | "(in the campaign rules, coming in a later alpha)" |

3. **Development notes.** Remove sentences that are about the project, not the game: mentions of "the app", "Phase 3/5/7", "the simulator", "content/…", and "written but not yet played". Examples: chapter 11's "The app's generator follows exactly these steps" (drop the sentence); chapter 13's "In a campaign (Phase 7), the enemy holds the field" (keep the rule as "In a campaign, …" without "(Phase 7)"); chapter 9's mention of the troop builder phase. If removing a sentence would change a rule, keep it and flag it to RJ instead.
4. **Proposed markers.** Keep every "Proposed" label and status column. Testers need to see what's new.
5. **Do not include** `rules/README.md` (it is a development changelog), chapter 14 or chapter 15.
6. **Spelling and style.** Keep British spelling ("armour") and inches. Don't edit wording for style.

## 5. Typesetting

- **Page:** US Letter, 0.6–0.75" margins, page numbers in the footer, running header "Imperator Black · closed alpha v0.5 · not for distribution".
- **Type:** a clean sans or serif for body text at 10–11 pt; tables at 8–9 pt so the wide ones (chapter 9's troop tables, 14 columns) fit. Rotate a page to landscape only if a table can't fit any other way.
- **Tables:** header row shaded, thin rules, no table split across pages if it's under a third of a page.
- **Markdown features used:** headings, bold and italics, bullet and numbered lists, pipe tables, and `inline code` (render code as plain text in the body font, since only file names use it and those are being replaced).
- **Bookmarks:** a PDF outline with every chapter and appendix.
- **Tools:** any that produce a clean result, for example pandoc with a LaTeX or HTML engine (WeasyPrint), or reportlab. Merge the appendix PDFs with a PDF library (pypdf or qpdf). The repository's own tools use reportlab and the DejaVu Sans Condensed font if you want the packet to match the printed aids.
- **Filename:** `ImperatorBlack_alpha-rules-v0.5.pdf`. Every project PDF starts with `ImperatorBlack_`.

## 6. Generic profiles

The packet already includes **Generic forces** (chapter 9 and `ImperatorBlack_generic-cards.pdf`): nine model-agnostic profiles in four teams for common collections, such as Warhammer 40,000 guard-style infantry, heavy weapons teams, storm troopers and power-armoured heavy infantry. They were made in the project's content files so their numbers match the rules. **Do not write new profiles yourself**: troop profiles must come from the content files, or their costs, cards and rules tables will disagree. If RJ wants more (for example vehicle crews, or other common collections), ask him to request them in the design project, then rebuild.

The generic profiles use descriptive names, not trademarks ("Heavy Infantry", not a brand's name for it); keep it that way in anything you write.

## 7. Third-party conversions

The packet includes fan conversion profiles: BLKOUT (Task Force Boone, Manticor Borz, Harlow, UN Force Command), Star Wars: Legion (the Galactic Republic) and Near Present (Anchorage and Hankou). RJ has approved them for this closed group. Keep the notice at the end of `welcome.md`, and add it to the copyright page in one line: "Fan conversion profiles are unofficial; all third-party names and settings belong to their owners. Shared privately and without charge for playtesting." Don't add logos, artwork or text from those products.

## 8. Checks before handing back

- [ ] Chapters 1–13 present and in order; no chapter 14 or 15; no `rules/README.md`.
- [ ] Search the PDF text: no `<!-- table`, no `docs/`, no `tools/`, no `.md`, no `content/`, no "Phase ", no "the app " or "the app's" (note: "the approach" is fine).
- [ ] Every table in the chapters rendered as a table (spot-check chapters 4, 9, 10, 11, 12, 13).
- [ ] Quick reference: 3 pages (not 4).
- [ ] Unit cards at 100% scale (a card is 4.25" × 5.5" with cut marks).
- [ ] Bookmarks for every chapter and appendix; page numbers in the contents match.
- [ ] Cover and running header say "not for distribution".
- [ ] File named `ImperatorBlack_alpha-rules-v0.5.pdf`.
- [ ] Note the total page count and any sentence you flagged under section 4.3, and send both to RJ with the PDF.
