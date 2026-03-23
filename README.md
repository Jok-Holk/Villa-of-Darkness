# Villa of Darkness — Biệt Thự Bóng Tối

> *A Vietnamese horror survival game built in Unity 3D.*

---

## Overview

**Villa of Darkness** is a first-person horror survival and puzzle game set in an abandoned French-colonial mansion in Đà Lạt, Vietnam. The game tells its story across four chapters, each taking place in a different time period — 2000, 1970, 1990, and 2020 — as four young people separately enter the mansion, none of them knowing what the others left behind.

Each character inherits the items found by those who came before. The first three chapters end in death. Only the fourth has a chance at something else.

---

## Gameplay

- **First-person exploration** with no traditional HUD — all feedback is environmental
- **Puzzle system** unique to each chapter, built around the three inherited items
- **Two ghost types** with distinct AI and mechanics:
  - *Ma Vú Dài* — patrols and hunts by sound and sight, blocked by salt and closed doors
  - *Ma Da* — lives in reflective surfaces, triggered by the player's gaze
- **Sanity system** expressed through post-processing effects, not UI bars
- **Three endings** in Chapter 4, determined by how much of the story the player uncovered

---

## Art Style

Low-polygon 3D with a deliberate PS1/PS2 aesthetic: sub-256px textures, CRT scanline overlay, film grain, and vertex jitter. The visual style is a design choice, not a limitation.

---

## Tech Stack

| | |
|---|---|
| Engine | Unity 2022 LTS (URP) |
| Language | C# |
| AI Navigation | Unity NavMesh |
| Post-processing | URP Volume + Custom Shader Graph |
| Audio | Unity Audio Mixer with sanity-based snapshot blending |
| Version Control | Git / GitHub |

---

## Team

Final project — FPT Polytechnic, class GA20303.

| Role | Member |
|---|---|
| Lead Developer / Game Director | P1 |
| Gameplay Systems | P2 |
| UI & Audio | P3 |
| Level Design | P4 |
| Event & Trigger Systems | P5 |
| UI & Scene Flow | P6 |

---

## Status

🚧 In active development

---

## License

This project is for educational purposes as part of a graduation requirement at FPT Polytechnic. All original code and design belong to the team.
