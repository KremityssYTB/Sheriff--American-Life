# SHERIFF: AMERICAN LIFE — World Design

## World direction
- Setting: a fictionalized, Florida-based game world using real Florida geography as reference.
- Long-term target map footprint: 30 km × 30 km (900 km²), subject to performance and streaming tests.
- Development approach: build and validate one county/region at a time; do not attempt to populate the whole world at once.
- Mobile target: iPhone 16e; Unity 6 with Universal Render Pipeline (URP).
- Use public geographic data for road alignments, waterways, terrain, and public landmarks. Create original game assets and avoid copying proprietary game maps, commercial imagery, or protected third-party assets.

## County development order

### 1. Marion County — first playable county
Primary city: Ocala.
Planned places and environments:
- Ocala downtown and surrounding neighborhoods
- Silver Springs
- Belleview
- Dunnellon
- McIntosh
- Ocklawaha
- Rural roads, horse farms, ranches, pine woods, lakes, and wetlands
- Historic downtown buildings and Fort King as a landmark reference

### 2. Hillsborough County — Tampa region
Primary city: Tampa.
Required districts:
- Ybor City — a distinct historic district, with brick streets, cigar-industry heritage, historic storefronts, restaurants, and nightlife areas
- Downtown Tampa and waterfront
- Channelside
- West Tampa
- Residential neighborhoods and commercial corridors
- Suburban and rural outskirts

Ybor City is a required named district, not an optional generic Tampa neighborhood.

### 3. Orange County — Orlando region
### 4. Miami-Dade County — Miami region
### 5. Lee County — Fort Myers region
### 6. Volusia County — Daytona Beach region
### 7. Duval County — Jacksonville region
### 8. Leon County — Tallahassee region

## World-building requirements for each county
- Road network based on real-world alignments where practical
- Town centers, residential streets, and commercial corridors
- Florida-style homes, apartment buildings, and rural properties
- Gas stations, restaurants, convenience stores, shops, and other businesses
- Historic districts and public landmarks
- Farms, forests, lakes, rivers, wetlands, and coastline where geographically appropriate
- Sheriff, police, fire, and EMS facilities
- Traffic, pedestrians, civilian activities, and emergency call locations
- Mobile optimization: streamed world sections, LODs, occlusion culling, baked lighting, and configurable NPC/traffic density

## Implementation rule
Complete a small, playable vertical slice of Marion County first. Validate driving, walking, navigation, streaming, and performance on the iPhone target before expanding the region. Add later counties as separate streamed regions with planned highway connections.
