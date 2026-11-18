# FactoryVerseMR

**Build a miniature factory in your own space.**

FactoryVerseMR is a mixed-reality factory-building and quality-control experience for **Meta Quest 3**, developed in **Unity and C#**. Place and arrange modular conveyors, connect a packing machine between the fixed source and shipping stations, run a production line, and manually remove defective packages before they reach shipping.

> **Project status:** Playable prototype. Documentation, performance checks and release packaging are in progress.

<!-- Add final logo here: ![FactoryVerseMR](Media/Branding/FactoryVerseMR_Splash.png) -->
<!-- Add trailer here: [Watch the gameplay trailer](TRAILER_URL) -->

## Gameplay

1. **Place your factory:** Position, rotate and resize the miniature factory board in your physical space.
2. **Build a route:** Purchase straight or turning conveyor modules and a packing machine, then connect the source to the shipping station.
3. **Start production:** Start the line and spawn products from the source (5 credits per spawn).
4. **Inspect quality:** Products are packed as they travel through the packing machine. Defective packages are visually highlighted in red.
5. **Return rejects:** Grab defective packages and drop them into the Return Bin to remove them and earn a rejection reward.
6. **Ship good products:** Successfully deliver non-defective packed packages to earn credits. Shipping defective products incurs a penalty.

The experience runs continuously: balance factory expansion, product throughput and quality control.

## Features

- **Mixed-reality tabletop factory:** Position the factory in your real environment using Quest 3 passthrough.
- **Hands and controllers:** Interact with UI and grab modules using supported Quest 3 input.
- **Modular layout:** Snap straight and corner conveyor modules onto a configurable grid.
- **Production management:** Start, stop, reset and clear products from the line through the factory control panel.
- **Manual quality control:** Spot red-highlighted defective products and return them to the bin.
- **Credits and difficulty:** Spend credits on modules and product spawning, earn delivery rewards, and face a higher defect rate at the next difficulty level.
- **In-app guide:** Five illustrated How to Play pages, plus a Home and About screen.

## Controls

| Action | Interaction |
|---|---|
| Select UI | Poke with a tracked hand or select with a controller |
| Grab and move modules | Hand pinch/grab or controller grip |
| Move / rotate board | Grab its handle and reposition or rotate |
| Resize board | Grab the board's resize handles with both hands |
| Remove defective package | Grab the package and drop it into the Return Bin |

## Technology

- Unity 6.3 LTS, C#
- Meta Quest 3, Android
- OpenXR and Unity XR interaction/UI systems
- Mixed-reality passthrough
- Low-poly factory assets, including Kenney Factory Kit 3.0 (see asset licence and attribution before release)

## Screenshots and video

_Add real Quest 3 gameplay captures here._

Suggested captures: Home screen, factory placement, module shop, completed production line, red-highlighted defect, Return Bin interaction, and shipping/credits HUD.

<!-- Example: ![Factory line in mixed reality](Media/Screenshots/factory-line.png) -->

## Installation and build

**Hardware:** Meta Quest 3.

**For developers:** Open the Unity project with the matching Unity editor version and required XR/Android packages. Configure Android/OpenXR for Quest 3 and ensure the Home and gameplay scenes are enabled in the build scene list, with Home first. Build and deploy to the headset.

> A downloadable APK and a verified clean-build procedure have not yet been published. Do not distribute an APK until standalone headset testing is complete.

## Project structure

The Unity project separates the Home/tutorial experience from the gameplay scene. Gameplay systems cover board placement and snapping, module purchasing, production control, product movement, packing/quality state, returns, shipping and credits. Specific script and folder names may vary as the project is finalised.

## Development and credits

**Developed by Surya Veeraputhiran**  
MSc Virtual Reality, Swansea University



## Licence

**To be decided.** No open-source licence is granted by this README alone. Review third-party asset terms before choosing a repository licence.
