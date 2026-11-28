#!/usr/bin/env python3
"""Validate the data-only Florida regional world catalog."""
import json
from pathlib import Path

catalog_path = Path(__file__).resolve().parents[1] / "data" / "florida_region.json"
data = json.loads(catalog_path.read_text(encoding="utf-8"))

def require(condition: bool, message: str) -> None:
    if not condition:
        raise SystemExit(f"Florida catalog validation failed: {message}")

counties = data.get("county_roster", [])
require(len(counties) == 67, f"expected 67 counties, found {len(counties)}")
require(len(set(counties)) == 67, "county roster contains duplicate identifiers")
require(data.get("map_area_km2") == 225, "map area must be exactly 225 km²")

regions = data.get("regions", [])
region_counties = [county for region in regions for county in region.get("county_ids", [])]
require(len(region_counties) == 67, "regional county grouping must contain 67 entries")
require(len(set(region_counties)) == 67, "a county appears in multiple region groups")
require(set(region_counties) == set(counties), "regional groups do not cover the county roster")

districts = data.get("districts", [])
district_ids = {district["id"] for district in districts}
require(len(district_ids) == len(districts), "duplicate district identifier")
require(len(districts) >= 10, "expected a multi-region destination catalog")
for district in districts:
    require(district.get("county_id") in counties, f"unknown county for district {district.get('id')}")

graph = data.get("route_graph", {})
nodes = graph.get("nodes", [])
node_ids = {node["id"] for node in nodes}
require(node_ids == district_ids, "route graph nodes must match district identifiers")
require(len(nodes) == len(node_ids), "duplicate route graph node")
bounds = data["coordinate_system"]["world_bounds_m"]
require(bounds == [-7500, -7500, 7500, 7500], "world bounds must describe a 15 km square")
for node in nodes:
    require(bounds[0] <= node["x_m"] <= bounds[2], f"district {node['id']} is outside x bounds")
    require(bounds[1] <= node["z_m"] <= bounds[3], f"district {node['id']} is outside z bounds")

for edge_group in ("road_edges", "water_edges"):
    edges = graph.get(edge_group, [])
    edge_ids = [edge["id"] for edge in edges]
    require(len(edge_ids) == len(set(edge_ids)), f"duplicate identifiers in {edge_group}")
    for edge in edges:
        require(edge.get("from") in node_ids, f"unknown route origin in {edge.get('id')}")
        require(edge.get("to") in node_ids, f"unknown route destination in {edge.get('id')}")
        require(edge.get("from") != edge.get("to"), f"self-loop in {edge.get('id')}")

road_adjacency = {node_id: set() for node_id in node_ids}
for edge in graph.get("road_edges", []):
    road_adjacency[edge["from"]].add(edge["to"])
    road_adjacency[edge["to"]].add(edge["from"])
visited = set()
pending = [next(iter(node_ids))]
while pending:
    current = pending.pop()
    if current in visited:
        continue
    visited.add(current)
    pending.extend(road_adjacency[current] - visited)
require(visited == node_ids, "road network must connect every destination district")

access_points = data.get("watercraft_access_points", [])
for access in access_points:
    require(access.get("district_id") in district_ids, f"unknown district for water access {access.get('id')}")

streaming = data.get("streaming", {})
cell_size = streaming.get("cell_size_m")
require(cell_size == 500, "streaming cell size must be 500 m")
cell_count_x = (bounds[2] - bounds[0]) // cell_size
cell_count_z = (bounds[3] - bounds[1]) // cell_size
require((cell_count_x, cell_count_z) == (30, 30), "streaming grid must be 30 x 30 cells")

print(
    "Florida catalog OK:",
    f"{len(counties)} counties, {len(regions)} regions, {len(districts)} districts,",
    f"{len(graph.get('road_edges', []))} road links, {len(graph.get('water_edges', []))} water links,",
    f"{len(access_points)} water access points, {cell_count_x}x{cell_count_z} cells."
)
