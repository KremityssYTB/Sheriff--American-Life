# 3D Browser Playtest

Open the GitHub Pages preview on iPhone Safari or iPadOS Safari after enabling Pages with GitHub Actions.

- Three.js 0.180.0 loaded as an ES module from jsDelivr
- WebGL 2 rendering
- Touch directional pad, sprint, drag-to-look camera
- Enter/exit patrol SUV and simplified throttle/steering controls
- Duty toggle, emergency lights, dispatch calls, and location interactions
- Procedural Ocala district, traffic, buildings, greenery, deputy, and patrol SUV

This is a standalone browser prototype. It is not a Unity WebGL build, does not execute Unity C# code, does not share runtime state with the Unity project, and is not a native iOS/iPadOS app. It requires an internet connection for the Three.js CDN and a browser with WebGL 2.

The scene is a schematic placeholder, not a geographically accurate Ocala map. The vehicle handling and gameplay actions are simplified browser-side implementations. Do not use this preview to validate Unity physics, performance, or native device behavior.
