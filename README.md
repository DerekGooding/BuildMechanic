# BuildMechanic

A 3D Mech Mechanic Simulator built in **MonoGame DesktopGL** and **C# (.NET 9/10)**.

Suspended in an industrial hangar gantry, battle-scarred combat mechs arrive for structural overhaul. Inspect components with an AR diagnostic scanner, unscrew 3D steel hex bolts with a power impact driver, detach armor plates, grind out fractures, lay molten weld beads, grind them flush, and torque fasteners back to combat readiness.

---

## Features
- **3D Workstation Orbit Camera**:
  - Smooth orbital controls, zooming, panning, and dedicated sector focus.
  - Press `F` to focus on the Torso repair workstation; press `Space` to reset to full hangar overview.
- **Interactive Fasteners & Armor Plating**:
  - Individual 3D hex bolts that physically thread outward when unbolted and lock into place when torqued.
  - Detachable armor plates that can be pulled off the chassis onto an inspection jig once all bolts are removed.
- **Multi-Step Crack Repair System**:
  - `Fractured` $\rightarrow$ Jagged battle crack.
  - `Prepped V-Groove` $\rightarrow$ Clean ground channel prepped by the Angle Grinder.
  - `Welded Bead` $\rightarrow$ Molten welded bead filled with the MIG Torch.
  - `Flush Finished` $\rightarrow$ Ground flush and polished to 100% structural integrity.
- **Particle VFX & Dynamic Lighting**:
  - Directional 3D spark physics with ground bouncing for the angle grinder.
  - Dynamic arc flash lighting that flickers across the mech chassis during welding.
- **Procedural Sound Bank**:
  - Cyclic impact driver rattle and torque snap clicks.
  - Abrasive steel grinding screech and disc spin-up.
  - Electric welding arc sizzle and mains hum.
  - Heavy steel plate clunks, AR scanner sweeps, and contract completion chimes.
- **Sci-Fi Diegetic HUD & Contracts**:
  - AR Scanner mode highlighting structural stress, bolt torque, and component health.
  - Contract #1: *Titan Vanguard — Torso Armor Overhaul* with active objective tracker and credit payouts.

---

## Controls

| Input | Action |
| :--- | :--- |
| `RMB + Drag` | Orbit Camera |
| `Scroll Wheel` | Zoom Camera in/out |
| `MMB / Shift + RMB` | Pan Camera |
| `F` | Focus on Torso Sector |
| `Space` | Reset Camera to Full Hangar Overview |
| `1` / `Tab` | AR Scanner Tool |
| `2` | Power Impact Driver |
| `R` | Toggle Driver Direction (`FWD` Torque / `REV` Unscrew) |
| `3` | Pry Bar / Mount Tool |
| `4` | Angle Grinder |
| `5` | MIG Welder |
| `Left Click (Hold)` | Operate Selected Tool on target (Bolt, Plate, or Crack) |
| `Enter` | Restart / Accept Contract after completion |
| `Esc` | Exit |

---

## Running the Project

Ensure the .NET SDK (8, 9, or 10) is installed:

```bash
dotnet build
dotnet run
```
