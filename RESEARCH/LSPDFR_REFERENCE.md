# LSPDFR Reference Research for SHERIFF: AMERICAN LIFE

Research date: 2026-11-25

## What LSPDFR is

LSPDFR (LSPD First Response) is a police simulation modification for the PC version of Grand Theft Auto V. It transforms GTA V into a law-enforcement experience centered on patrol, AI callouts, pursuits, and player-directed enforcement. The official site describes customization of police agencies and vehicles as part of its creative scope.

Official references:
- https://www.lcpdfr.com/lspdfr/
- https://www.lcpdfr.com/lspdfr/04/install/
- https://github.com/LMSDev/LSPDFR-API

## Key design patterns to learn from

1. **Patrol-first gameplay:** The player can patrol freely rather than being forced through a linear mission chain.
2. **Dispatch and callouts:** Incidents give patrol a purpose and vary what the player encounters.
3. **Traffic enforcement:** Vehicle stops create a repeatable interaction loop: observe, initiate stop, approach, investigate, decide, and resolve.
4. **Pursuits:** Suspect behavior and pursuit escalation create dynamic responses.
5. **Arrest and suspect handling:** The player needs tools to detain, search where legally/gameplay appropriate, transport, and process suspects.
6. **Player discretion:** The player makes enforcement choices and is not limited to one scripted outcome.
7. **Extensibility/customization:** The LSPDFR ecosystem demonstrates the value of configurable agencies, vehicles, uniforms, lighting, and add-on callouts.
8. **Plugin/API ecosystem:** LSPDFR has an official API repository with documentation, examples, extensions, and helper utilities for plugin developers.

## SHERIFF: AMERICAN LIFE implementation direction

Use LSPDFR as a design reference, not as code or asset content to copy. SHERIFF is an original Unity 6 URP game for iPhone 16e with a Florida setting and mobile-first controls.

Planned systems inspired by the gameplay patterns:
- Free patrol in first-person and third-person
- Mobile-friendly dispatch and call selection
- Randomized and authored callouts
- Traffic stops and driver interactions
- Pursuit AI and backup requests
- Suspect detainment, arrest, transport, and booking
- Reports, evidence, warrants, and investigations
- Configurable sheriff/police agencies, uniforms, and patrol vehicles
- Civilian and emergency-service activity that continues outside missions

## Mobile-specific adaptation

Do not copy the PC mod's interaction complexity literally. Build concise touch-first interactions:
- Contextual interaction wheel and clear action prompts
- Large, accessible buttons for stop lights/siren, radio, backup, and interaction
- Short interaction sequences with optional detailed panels
- AI and traffic simulation scaled by distance and device performance
- Event-driven callouts rather than many expensive always-running scripts
- Save/load and progression systems designed for short mobile sessions

## Important technical distinction

LSPDFR is a PC GTA V modification and depends on GTA V plus RAGE Plugin Hook. Its official installation instructions place LSPDFR files and plugins in the GTA V directory and launch through RAGEPluginHook.exe. It is not a Unity package and cannot be installed into SHERIFF. We should independently implement equivalent gameplay systems in Unity.

## License and asset boundary

The LSPDFR user agreement states that its software is licensed for use with GTA V. Do not copy LSPDFR binaries, source, assets, or GTA V assets into SHERIFF. Use the public documentation as design research and create original code, models, audio, UI, and content.

## Sources checked

- Official LSPDFR overview: https://www.lcpdfr.com/lspdfr/
- Official installation guide: https://www.lcpdfr.com/lspdfr/04/install/
- Official API repository: https://github.com/LMSDev/LSPDFR-API
- LSPDFR About / user agreement: https://www.lcpdfr.com/lspdfr/about/
