using System.Collections.Generic;
using UnityEngine;

namespace TornadoStrike.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class InfiniteCityWorld : MonoBehaviour
    {
        private const float RoadWidth = 4.6f;
        private const float RoadHalf = RoadWidth * 0.5f;
        private const float SidewalkWidth = 1.2f;
        private const float SidewalkCenter = RoadHalf + SidewalkWidth * 0.5f;
        private const float LaneOffset = 1.05f;
        private const float RoundaboutRadius = 5.05f;
        private const float RoundaboutRoadWidth = 2.05f;

        [Header("Streaming")]
        public Transform target;
        [Min(8f)] public float chunkSize = 24f;
        [Range(1, 5)] public int activeRadius = 2;
        public int seed = 314159;

        [Header("Design Density")]
        [Range(0.5f, 2f)] public float buildingDensity = 1.15f;
        [Range(0.5f, 2.5f)] public float vehicleDensity = 1.25f;
        [Range(0.5f, 2.5f)] public float streetPropDensity = 1.45f;
        [Range(0.2f, 2f)] public float pedestrianDensity = 1.05f;
        [Range(0f, 2f)] public float surfaceDetailDensity = 1f;
        public int minBuildingsPerChunk = 8;
        public int maxBuildingsPerChunk = 13;
        public int streetPropsPerChunk = 12;
        public int pedestriansPerChunk = 5;

        [Header("Materials")]
        public Material grassMaterial;
        public Material asphaltMaterial;
        public Material roadLineMaterial;
        public Material sidewalkMaterial;
        public Material curbMaterial;
        public Material carMaterial;
        public Material busMaterial;
        public Material houseMaterialA;
        public Material houseMaterialB;
        public Material houseCandyPinkMaterial;
        public Material houseMintMaterial;
        public Material houseCreamMaterial;
        public Material houseTrimMaterial;
        public Material roofMaterial;
        public Material roofHighlightMaterial;
        public Material glassMaterial;
        public Material whiteMaterial;
        public Material blackMaterial;
        public Material treeTrunkMaterial;
        public Material treeCanopyMaterial;
        public Material lampPoleMaterial;
        public Material lampLightMaterial;
        public Material powerPlantMaterial;
        public Material policeMaterial;
        public Material fireStationMaterial;
        public Material markerMaterial;

        [Header("Realistic City Detail Materials")]
        public Material concreteMaterial;
        public Material brickMaterial;
        public Material metalMaterial;
        public Material roadWearMaterial;
        public Material sidewalkLineMaterial;
        public Material windowFrameMaterial;
        public Material carGlassMaterial;
        public Material tireMaterial;
        public Material headlightMaterial;
        public Material tailLightMaterial;
        public Material vehicleTrimMaterial;
        public Material warningStripeMaterial;
        public Material signMaterial;
        public Material pedestrianSkinMaterial;
        public Material pedestrianShirtMaterial;
        public Material pedestrianPantsMaterial;
        public Material pedestrianHairMaterial;
        public Material leafDarkMaterial;

        private readonly Dictionary<Vector2Int, GameObject> activeChunks = new Dictionary<Vector2Int, GameObject>();

        private struct BuildingLot
        {
            public Vector3 position;
            public float yaw;
            public float maxWidth;
            public float maxDepth;
            public float minHeight;
            public float maxHeight;

            public BuildingLot(Vector3 position, float yaw, float maxWidth, float maxDepth, float minHeight, float maxHeight)
            {
                this.position = position;
                this.yaw = yaw;
                this.maxWidth = maxWidth;
                this.maxDepth = maxDepth;
                this.minHeight = minHeight;
                this.maxHeight = maxHeight;
            }
        }

        private void Awake()
        {
            if (target == null)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    target = player.transform;
                }
            }
        }

        private void Start()
        {
            RefreshChunks();
        }

        private void Update()
        {
            RefreshChunks();
        }

        private void RefreshChunks()
        {
            if (target == null)
            {
                return;
            }

            var center = WorldToChunk(target.position);
            var needed = new HashSet<Vector2Int>();

            for (var x = -activeRadius; x <= activeRadius; x++)
            {
                for (var y = -activeRadius; y <= activeRadius; y++)
                {
                    var coord = new Vector2Int(center.x + x, center.y + y);
                    needed.Add(coord);

                    if (!activeChunks.ContainsKey(coord))
                    {
                        activeChunks.Add(coord, CreateChunk(coord));
                    }
                }
            }

            var stale = new List<Vector2Int>();
            foreach (var pair in activeChunks)
            {
                if (!needed.Contains(pair.Key))
                {
                    stale.Add(pair.Key);
                }
            }

            foreach (var coord in stale)
            {
                Destroy(activeChunks[coord]);
                activeChunks.Remove(coord);
            }
        }

        private Vector2Int WorldToChunk(Vector3 position)
        {
            return new Vector2Int(
                Mathf.FloorToInt((position.x + chunkSize * 0.5f) / chunkSize),
                Mathf.FloorToInt((position.z + chunkSize * 0.5f) / chunkSize));
        }

        private GameObject CreateChunk(Vector2Int coord)
        {
            var chunk = new GameObject($"CityChunk_{coord.x}_{coord.y}");
            chunk.transform.SetParent(transform);
            chunk.transform.position = new Vector3(coord.x * chunkSize, 0f, coord.y * chunkSize);

            var rng = new System.Random(Hash(coord));
            CreateGround(chunk.transform);
            CreateRoads(chunk.transform);
            CreateRoadSurfaceDetails(chunk.transform, rng);
            CreateBuildings(chunk.transform, rng, coord);
            CreateStreetProps(chunk.transform, rng);
            CreatePedestrians(chunk.transform, rng);
            CreateVehicles(chunk.transform, rng);

            return chunk;
        }

        private void CreateGround(Transform parent)
        {
            var ground = Primitive("Ground", PrimitiveType.Cube, parent, grassMaterial);
            ground.transform.localPosition = new Vector3(0f, -0.05f, 0f);
            ground.transform.localScale = new Vector3(chunkSize, 0.1f, chunkSize);

            for (var side = -1; side <= 1; side += 2)
            {
                var horizontalSidewalk = Primitive($"Sidewalk_H_{side}", PrimitiveType.Cube, parent, sidewalkMaterial);
                horizontalSidewalk.transform.localPosition = new Vector3(0f, 0.015f, side * SidewalkCenter);
                horizontalSidewalk.transform.localScale = new Vector3(chunkSize, 0.08f, SidewalkWidth);
                CreatePaverSeams(parent, horizontalSidewalk.transform.localPosition, horizontalSidewalk.transform.localScale, true, $"H_{side}");

                var verticalSidewalk = Primitive($"Sidewalk_V_{side}", PrimitiveType.Cube, parent, sidewalkMaterial);
                verticalSidewalk.transform.localPosition = new Vector3(side * SidewalkCenter, 0.02f, 0f);
                verticalSidewalk.transform.localScale = new Vector3(SidewalkWidth, 0.08f, chunkSize);
                CreatePaverSeams(parent, verticalSidewalk.transform.localPosition, verticalSidewalk.transform.localScale, false, $"V_{side}");
            }

            CreateCornerGreenBands(parent);
        }

        private void CreateRoads(Transform parent)
        {
            var curb = MaterialOr(curbMaterial, sidewalkMaterial);

            var horizontal = Primitive("Road_H", PrimitiveType.Cube, parent, asphaltMaterial);
            horizontal.transform.localPosition = new Vector3(0f, 0.04f, 0f);
            horizontal.transform.localScale = new Vector3(chunkSize, 0.08f, RoadWidth);

            var vertical = Primitive("Road_V", PrimitiveType.Cube, parent, asphaltMaterial);
            vertical.transform.localPosition = new Vector3(0f, 0.05f, 0f);
            vertical.transform.localScale = new Vector3(RoadWidth, 0.08f, chunkSize);

            var intersection = Primitive("Intersection", PrimitiveType.Cube, parent, asphaltMaterial);
            intersection.transform.localPosition = new Vector3(0f, 0.065f, 0f);
            intersection.transform.localScale = new Vector3(RoadWidth + 0.25f, 0.08f, RoadWidth + 0.25f);

            for (var i = -2; i <= 2; i++)
            {
                CreateRoadDash(parent, $"LaneDash_H_{i}", new Vector3(i * 4f, 0.12f, 0f), new Vector3(1.65f, 0.025f, 0.08f));
                CreateRoadDash(parent, $"LaneDash_V_{i}", new Vector3(0f, 0.13f, i * 4f), new Vector3(0.08f, 0.025f, 1.65f));
            }

            for (var side = -1; side <= 1; side += 2)
            {
                var curbH = Primitive($"Curb_H_{side}", PrimitiveType.Cube, parent, curb);
                curbH.transform.localPosition = new Vector3(0f, 0.16f, side * RoadHalf);
                curbH.transform.localScale = new Vector3(chunkSize, 0.16f, 0.18f);

                var curbV = Primitive($"Curb_V_{side}", PrimitiveType.Cube, parent, curb);
                curbV.transform.localPosition = new Vector3(side * RoadHalf, 0.17f, 0f);
                curbV.transform.localScale = new Vector3(0.18f, 0.16f, chunkSize);
            }

            CreateCrosswalk(parent, new Vector3(-(RoadHalf + 0.62f), 0.15f, 0f), true);
            CreateCrosswalk(parent, new Vector3(RoadHalf + 0.62f, 0.15f, 0f), true);
            CreateCrosswalk(parent, new Vector3(0f, 0.16f, -(RoadHalf + 0.62f)), false);
            CreateCrosswalk(parent, new Vector3(0f, 0.16f, RoadHalf + 0.62f), false);
            CreateLaneDirectionArrows(parent);
            CreateRoundabout(parent);
        }

        private void CreateLaneDirectionArrows(Transform parent)
        {
            CreateRoadArrow(parent, "Arrow_H_East", new Vector3(-7.2f, 0.16f, -LaneOffset), 0f);
            CreateRoadArrow(parent, "Arrow_H_West", new Vector3(7.2f, 0.16f, LaneOffset), 180f);
            CreateRoadArrow(parent, "Arrow_V_North", new Vector3(LaneOffset, 0.17f, -7.2f), -90f);
            CreateRoadArrow(parent, "Arrow_V_South", new Vector3(-LaneOffset, 0.17f, 7.2f), 90f);
        }

        private void CreateRoadArrow(Transform parent, string name, Vector3 localPosition, float yaw)
        {
            var arrowRoot = new GameObject(name);
            arrowRoot.transform.SetParent(parent);
            arrowRoot.transform.localPosition = localPosition;
            arrowRoot.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);

            var shaft = Primitive("ArrowShaft", PrimitiveType.Cube, arrowRoot.transform, MaterialOr(whiteMaterial, roadLineMaterial));
            shaft.transform.localPosition = Vector3.zero;
            shaft.transform.localScale = new Vector3(0.58f, 0.018f, 0.08f);
            DestroyCollider(shaft);

            var headLeft = Primitive("ArrowHeadLeft", PrimitiveType.Cube, arrowRoot.transform, MaterialOr(whiteMaterial, roadLineMaterial));
            headLeft.transform.localPosition = new Vector3(0.36f, 0f, -0.12f);
            headLeft.transform.localRotation = Quaternion.Euler(0f, -35f, 0f);
            headLeft.transform.localScale = new Vector3(0.34f, 0.018f, 0.08f);
            DestroyCollider(headLeft);

            var headRight = Primitive("ArrowHeadRight", PrimitiveType.Cube, arrowRoot.transform, MaterialOr(whiteMaterial, roadLineMaterial));
            headRight.transform.localPosition = new Vector3(0.36f, 0f, 0.12f);
            headRight.transform.localRotation = Quaternion.Euler(0f, 35f, 0f);
            headRight.transform.localScale = new Vector3(0.34f, 0.018f, 0.08f);
            DestroyCollider(headRight);
        }

        private void CreatePaverSeams(Transform parent, Vector3 center, Vector3 size, bool horizontal, string id)
        {
            var seamMaterial = MaterialOr(sidewalkLineMaterial, curbMaterial);
            var seamY = center.y + 0.055f;
            var count = Mathf.Max(2, Mathf.FloorToInt((horizontal ? size.x : size.z) / 2.4f));

            for (var i = -count; i <= count; i++)
            {
                var offset = i * 2.4f;

                var seam = Primitive($"SidewalkSeam_{id}_{i}", PrimitiveType.Cube, parent, seamMaterial);
                seam.transform.localPosition = horizontal ? new Vector3(center.x + offset, seamY, center.z) : new Vector3(center.x, seamY, center.z + offset);
                seam.transform.localScale = horizontal ? new Vector3(0.025f, 0.016f, size.z * 0.82f) : new Vector3(size.x * 0.82f, 0.016f, 0.025f);
                DestroyCollider(seam);
            }
        }

        private void CreateCornerGreenBands(Transform parent)
        {
            var parkMaterial = MaterialOr(leafDarkMaterial, grassMaterial);
            for (var x = -1; x <= 1; x += 2)
            {
                for (var z = -1; z <= 1; z += 2)
                {
                    var park = Primitive($"CornerPark_{x}_{z}", PrimitiveType.Cube, parent, parkMaterial);
                    park.transform.localPosition = new Vector3(x * 8.25f, 0.005f, z * 8.25f);
                    park.transform.localScale = new Vector3(5.2f, 0.045f, 5.2f);
                    DestroyCollider(park);
                }
            }
        }

        private void CreateRoundabout(Transform parent)
        {
            var segmentCount = 28;
            var circumference = Mathf.PI * 2f * RoundaboutRadius;
            var segmentLength = circumference / segmentCount * 0.78f;

            for (var i = 0; i < segmentCount; i++)
            {
                var angle = (i / (float)segmentCount) * Mathf.PI * 2f;
                var tangentYaw = -angle * Mathf.Rad2Deg;
                var road = Primitive($"RoundaboutRoad_{i}", PrimitiveType.Cube, parent, asphaltMaterial);
                road.transform.localPosition = new Vector3(Mathf.Cos(angle) * RoundaboutRadius, 0.105f, Mathf.Sin(angle) * RoundaboutRadius);
                road.transform.localRotation = Quaternion.Euler(0f, tangentYaw, 0f);
                road.transform.localScale = new Vector3(segmentLength, 0.065f, RoundaboutRoadWidth);
                DestroyCollider(road);

                if (i % 2 == 0)
                {
                    var dash = Primitive($"RoundaboutDash_{i}", PrimitiveType.Cube, parent, roadLineMaterial);
                    dash.transform.localPosition = new Vector3(Mathf.Cos(angle) * RoundaboutRadius, 0.155f, Mathf.Sin(angle) * RoundaboutRadius);
                    dash.transform.localRotation = Quaternion.Euler(0f, tangentYaw, 0f);
                    dash.transform.localScale = new Vector3(segmentLength * 0.34f, 0.02f, 0.07f);
                    DestroyCollider(dash);
                }
            }

            var island = Primitive("RoundaboutIsland", PrimitiveType.Cylinder, parent, MaterialOr(leafDarkMaterial, grassMaterial));
            island.transform.localPosition = new Vector3(0f, 0.12f, 0f);
            island.transform.localScale = new Vector3(2.05f, 0.08f, 2.05f);
            DestroyCollider(island);

            var rim = Primitive("RoundaboutIslandRim", PrimitiveType.Cylinder, parent, MaterialOr(curbMaterial, sidewalkMaterial));
            rim.transform.localPosition = new Vector3(0f, 0.16f, 0f);
            rim.transform.localScale = new Vector3(2.3f, 0.035f, 2.3f);
            DestroyCollider(rim);
        }

        private void CreateRoadSurfaceDetails(Transform parent, System.Random rng)
        {
            var detailCount = Mathf.RoundToInt(5 * surfaceDetailDensity);
            for (var i = 0; i < detailCount; i++)
            {
                var horizontal = rng.NextDouble() < 0.5;
                var lane = rng.NextDouble() < 0.5 ? -0.6f : 0.6f;
                var along = RandomRange(rng, -chunkSize * 0.42f, chunkSize * 0.42f);
                var patch = Primitive($"RoadPatch_{i}", PrimitiveType.Cube, parent, MaterialOr(roadWearMaterial, asphaltMaterial));
                patch.transform.localPosition = horizontal ? new Vector3(along, 0.135f, lane) : new Vector3(lane, 0.14f, along);
                patch.transform.localScale = horizontal ? new Vector3(RandomRange(rng, 0.75f, 1.7f), 0.018f, RandomRange(rng, 0.18f, 0.42f)) : new Vector3(RandomRange(rng, 0.18f, 0.42f), 0.018f, RandomRange(rng, 0.75f, 1.7f));
                DestroyCollider(patch);
            }

            for (var i = 0; i < 2; i++)
            {
                var manhole = Primitive($"Manhole_{i}", PrimitiveType.Cylinder, parent, MaterialOr(metalMaterial, blackMaterial));
                manhole.transform.localPosition = new Vector3(RandomSigned(rng, 0.45f, 1.35f), 0.16f, RandomSigned(rng, 0.45f, 1.35f));
                manhole.transform.localScale = new Vector3(0.34f, 0.018f, 0.34f);
                DestroyCollider(manhole);
            }

            for (var side = -1; side <= 1; side += 2)
            {
                CreateStormDrain(parent, new Vector3(RandomRange(rng, -chunkSize * 0.32f, chunkSize * 0.32f), 0.19f, side * 2.12f), true);
                CreateStormDrain(parent, new Vector3(side * 2.12f, 0.2f, RandomRange(rng, -chunkSize * 0.32f, chunkSize * 0.32f)), false);
            }
        }

        private void CreateStormDrain(Transform parent, Vector3 localPosition, bool horizontal)
        {
            var drain = Primitive($"StormDrain_{localPosition.x:0.0}_{localPosition.z:0.0}", PrimitiveType.Cube, parent, MaterialOr(metalMaterial, blackMaterial));
            drain.transform.localPosition = localPosition;
            drain.transform.localScale = horizontal ? new Vector3(0.58f, 0.025f, 0.11f) : new Vector3(0.11f, 0.025f, 0.58f);
            DestroyCollider(drain);

            for (var i = -1; i <= 1; i++)
            {
                var grate = Primitive("DrainGrate", PrimitiveType.Cube, parent, MaterialOr(blackMaterial, asphaltMaterial));
                grate.transform.localPosition = localPosition + (horizontal ? new Vector3(i * 0.16f, 0.025f, 0f) : new Vector3(0f, 0.025f, i * 0.16f));
                grate.transform.localScale = horizontal ? new Vector3(0.035f, 0.016f, 0.13f) : new Vector3(0.13f, 0.016f, 0.035f);
                DestroyCollider(grate);
            }
        }

        private void CreateRoadDash(Transform parent, string name, Vector3 localPosition, Vector3 localScale)
        {
            var dash = Primitive(name, PrimitiveType.Cube, parent, roadLineMaterial);
            dash.transform.localPosition = localPosition;
            dash.transform.localScale = localScale;
        }

        private void CreateCrosswalk(Transform parent, Vector3 center, bool verticalStripes)
        {
            for (var i = -2; i <= 2; i++)
            {
                var stripe = Primitive($"Crosswalk_{center.x:0.0}_{center.z:0.0}_{i}", PrimitiveType.Cube, parent, whiteMaterial);
                stripe.transform.localPosition = center + (verticalStripes ? new Vector3(0f, 0f, i * 0.36f) : new Vector3(i * 0.36f, 0f, 0f));
                stripe.transform.localScale = verticalStripes ? new Vector3(0.32f, 0.025f, 0.22f) : new Vector3(0.22f, 0.025f, 0.32f);
            }
        }

        private void CreateBuildings(Transform parent, System.Random rng, Vector2Int coord)
        {
            var lots = BuildBuildingLots(rng);
            var usedSpecialLot = new Vector3(float.MaxValue, 0f, float.MaxValue);
            var specialCreated = false;
            if (ShouldCreateSpecial(coord))
            {
                var specialLot = PickSpecialLot(coord);
                CreateSpecialBuilding(parent, coord, specialLot);
                usedSpecialLot = specialLot.position;
                specialCreated = true;
            }

            var baseCount = specialCreated ? 7 + rng.Next(0, 3) : minBuildingsPerChunk + rng.Next(0, Mathf.Max(1, maxBuildingsPerChunk - minBuildingsPerChunk + 1));
            var buildingCount = Mathf.Min(lots.Count, Mathf.RoundToInt(baseCount * buildingDensity));
            var created = 0;
            for (var i = 0; i < lots.Count && created < buildingCount; i++)
            {
                var lot = lots[i];
                if ((lot.position - usedSpecialLot).sqrMagnitude < 15.5f)
                {
                    continue;
                }

                var width = RandomRange(rng, lot.maxWidth * 0.82f, lot.maxWidth);
                var depth = RandomRange(rng, lot.maxDepth * 0.82f, lot.maxDepth);
                var height = RandomRange(rng, lot.minHeight, lot.maxHeight);
                var material = PickCandyHouseMaterial(created);

                CreateHouse($"House_{created}", lot.position, lot.yaw, width, depth, height, parent, material, rng);
                created++;
            }
        }

        private Material PickCandyHouseMaterial(int index)
        {
            switch (index % 4)
            {
                case 0:
                    return MaterialOr(houseMaterialA, houseCreamMaterial);
                case 1:
                    return MaterialOr(houseMaterialB, houseMintMaterial);
                case 2:
                    return MaterialOr(houseCandyPinkMaterial, houseMaterialA);
                default:
                    return MaterialOr(houseMintMaterial, houseMaterialB);
            }
        }

        private List<BuildingLot> BuildBuildingLots(System.Random rng)
        {
            var lots = new List<BuildingLot>();
            for (var x = -1; x <= 1; x += 2)
            {
                for (var z = -1; z <= 1; z += 2)
                {
                    lots.Add(new BuildingLot(new Vector3(x * 5.45f, 0f, z * 5.2f), YawTowardCenter(x, z), 1.7f, 1.55f, 1.35f, 2.4f));
                    lots.Add(new BuildingLot(new Vector3(x * 8.55f, 0f, z * 5.45f), z > 0 ? 0f : 180f, 2.05f, 1.7f, 1.55f, 3.1f));
                    lots.Add(new BuildingLot(new Vector3(x * 5.55f, 0f, z * 8.45f), x > 0 ? 90f : -90f, 1.8f, 2.05f, 1.45f, 2.85f));
                    lots.Add(new BuildingLot(new Vector3(x * 8.65f, 0f, z * 8.55f), YawTowardCenter(x, z), 2.28f, 2.08f, 1.8f, 3.6f));
                }
            }

            for (var i = 0; i < lots.Count; i++)
            {
                var swap = rng.Next(i, lots.Count);
                var current = lots[i];
                lots[i] = lots[swap];
                lots[swap] = current;
            }

            return lots;
        }

        private BuildingLot PickSpecialLot(Vector2Int coord)
        {
            var hash = Mathf.Abs(Hash(coord));
            var x = (hash & 1) == 0 ? -1 : 1;
            var z = (hash & 2) == 0 ? -1 : 1;
            return new BuildingLot(new Vector3(x * 7.35f, 0f, z * 7.35f), YawTowardCenter(x, z), 5.2f, 4.7f, 2.9f, 2.9f);
        }

        private static float YawTowardCenter(int x, int z)
        {
            if (Mathf.Abs(x) > Mathf.Abs(z))
            {
                return x > 0 ? 90f : -90f;
            }

            return z > 0 ? 0f : 180f;
        }

        private void CreateHouse(string id, Vector3 localPosition, float yaw, float width, float depth, float height, Transform parent, Material material, System.Random rng)
        {
            var house = new GameObject(id);
            house.transform.SetParent(parent);
            house.transform.localPosition = localPosition;
            house.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);

            var trimMaterial = MaterialOr(houseTrimMaterial, concreteMaterial);
            var lotPad = Primitive("LotPad", PrimitiveType.Cube, house.transform, MaterialOr(sidewalkMaterial, trimMaterial));
            lotPad.transform.localPosition = new Vector3(0f, 0.02f, 0f);
            lotPad.transform.localScale = new Vector3(width + 0.55f, 0.055f, depth + 0.55f);
            DestroyCollider(lotPad);

            var body = Primitive("Body", PrimitiveType.Cube, house.transform, material);
            body.transform.localPosition = Vector3.up * (height * 0.5f);
            body.transform.localScale = new Vector3(width, height, depth);
            DestroyCollider(body);
            AddCandyFacadeDetails(house.transform, width, depth, height, rng);

            var collider = house.AddComponent<BoxCollider>();
            collider.center = body.transform.localPosition;
            collider.size = body.transform.localScale;

            var absorbable = house.AddComponent<Absorbable>();
            absorbable.absorbableId = "city_house";
            absorbable.category = AbsorbableCategory.Building;
            absorbable.localizationKey = "object_house";
            absorbable.requiredRadius = 1.25f + height * 0.32f + Mathf.Max(width, depth) * 0.08f;
            absorbable.growthValue = 0.05f + height * 0.018f + width * depth * 0.004f;
            absorbable.scoreValue = 14 + Mathf.RoundToInt(height * 8f + width * depth * 2f);

            var roof = Primitive("Roof", PrimitiveType.Cube, house.transform, roofMaterial);
            roof.transform.localPosition = new Vector3(0f, height + 0.13f, 0f);
            roof.transform.localScale = new Vector3(width * 1.12f, 0.26f, depth * 1.12f);
            DestroyCollider(roof);
            AddGabledRoof(house.transform, width, depth, height);
            AddRoofAccessories(house.transform, width, depth, height, rng);

            AddFacadeBand(house.transform, "ConcreteBase", 0.13f, new Vector3(width * 1.06f, 0.18f, depth * 1.06f), trimMaterial);
            AddFacadeBand(house.transform, "Cornice", height - 0.12f, new Vector3(width * 1.08f, 0.14f, depth * 1.08f), trimMaterial);
            AddCornerColumns(house.transform, width, depth, height, trimMaterial);

            var door = Primitive("Door", PrimitiveType.Cube, house.transform, MaterialOr(blackMaterial, roofMaterial));
            door.transform.localPosition = new Vector3(0f, 0.38f, -depth * 0.545f);
            door.transform.localScale = new Vector3(width * 0.22f, 0.72f, 0.05f);
            DestroyCollider(door);

            var doorFrame = Primitive("DoorFrame", PrimitiveType.Cube, house.transform, MaterialOr(windowFrameMaterial, concreteMaterial));
            doorFrame.transform.localPosition = new Vector3(0f, 0.42f, -depth * 0.525f);
            doorFrame.transform.localScale = new Vector3(width * 0.32f, 0.82f, 0.035f);
            DestroyCollider(doorFrame);

            var rows = Mathf.Clamp(Mathf.RoundToInt(height / 0.85f), 1, 4);
            for (var row = 0; row < rows; row++)
            {
                var y = 0.72f + row * 0.68f;
                AddWindow(house.transform, new Vector3(-width * 0.28f, y, -depth * 0.52f), new Vector3(width * 0.18f, 0.24f, 0.04f));
                AddWindow(house.transform, new Vector3(width * 0.28f, y, -depth * 0.52f), new Vector3(width * 0.18f, 0.24f, 0.04f));
                AddWindow(house.transform, new Vector3(-width * 0.28f, y, depth * 0.52f), new Vector3(width * 0.18f, 0.24f, 0.04f));
                AddWindow(house.transform, new Vector3(width * 0.28f, y, depth * 0.52f), new Vector3(width * 0.18f, 0.24f, 0.04f));
                AddWindow(house.transform, new Vector3(-width * 0.52f, y, -depth * 0.18f), new Vector3(0.04f, 0.22f, depth * 0.16f));
                AddWindow(house.transform, new Vector3(-width * 0.52f, y, depth * 0.18f), new Vector3(0.04f, 0.22f, depth * 0.16f));
                AddWindow(house.transform, new Vector3(width * 0.52f, y, -depth * 0.18f), new Vector3(0.04f, 0.22f, depth * 0.16f));
                AddWindow(house.transform, new Vector3(width * 0.52f, y, depth * 0.18f), new Vector3(0.04f, 0.22f, depth * 0.16f));
            }

            if (rng.NextDouble() < 0.52)
            {
                AddStripedAwning(house.transform, width, depth);
            }

            if (rng.NextDouble() < 0.42)
            {
                var ac = Primitive("WallAcUnit", PrimitiveType.Cube, house.transform, MaterialOr(metalMaterial, whiteMaterial));
                ac.transform.localPosition = new Vector3(width * 0.51f, 0.95f, -depth * 0.18f);
                ac.transform.localScale = new Vector3(0.08f, 0.26f, 0.38f);
                DestroyCollider(ac);
            }

            if (rng.NextDouble() < 0.45)
            {
                var tank = Primitive("RoofTank", PrimitiveType.Cylinder, house.transform, MaterialOr(whiteMaterial, markerMaterial));
                tank.transform.localPosition = new Vector3(width * 0.22f, height + 0.42f, depth * 0.12f);
                tank.transform.localScale = new Vector3(0.22f, 0.18f, 0.22f);
                DestroyCollider(tank);
            }

            if (rng.NextDouble() < 0.36)
            {
                var vent = Primitive("RoofVent", PrimitiveType.Cube, house.transform, MaterialOr(metalMaterial, whiteMaterial));
                vent.transform.localPosition = new Vector3(-width * 0.24f, height + 0.34f, -depth * 0.08f);
                vent.transform.localScale = new Vector3(0.26f, 0.24f, 0.3f);
                DestroyCollider(vent);
            }
        }

        private void AddCandyFacadeDetails(Transform parent, float width, float depth, float height, System.Random rng)
        {
            var trim = MaterialOr(houseTrimMaterial, concreteMaterial);
            var cream = MaterialOr(houseCreamMaterial, trim);
            var accent = rng.NextDouble() < 0.5 ? MaterialOr(houseCandyPinkMaterial, houseMaterialA) : MaterialOr(roadLineMaterial, houseMaterialA);

            AddFacePlate(parent, "FrontCandyInset", new Vector3(0f, height * 0.48f, -depth * 0.526f), new Vector3(width * 0.78f, height * 0.46f, 0.035f), cream);
            AddFacePlate(parent, "BackCandyInset", new Vector3(0f, height * 0.48f, depth * 0.526f), new Vector3(width * 0.78f, height * 0.46f, 0.035f), cream);
            AddFacePlate(parent, "ShopSignPanel", new Vector3(0f, Mathf.Min(height - 0.42f, 1.24f), -depth * 0.555f), new Vector3(width * 0.58f, 0.18f, 0.05f), accent);

            for (var side = -1; side <= 1; side += 2)
            {
                AddFacePlate(parent, "FrontCandyPost", new Vector3(side * width * 0.42f, height * 0.5f, -depth * 0.55f), new Vector3(0.08f, height * 0.82f, 0.055f), trim);
                AddFacePlate(parent, "BackCandyPost", new Vector3(side * width * 0.42f, height * 0.5f, depth * 0.55f), new Vector3(0.08f, height * 0.82f, 0.055f), trim);
                AddFacePlate(parent, "SideCandyPost", new Vector3(side * width * 0.55f, height * 0.5f, 0f), new Vector3(0.055f, height * 0.78f, depth * 0.12f), trim);

                var planter = Primitive("CandyWindowPlanter", PrimitiveType.Cube, parent, MaterialOr(leafDarkMaterial, treeCanopyMaterial));
                planter.transform.localPosition = new Vector3(side * width * 0.28f, 0.53f, -depth * 0.57f);
                planter.transform.localScale = new Vector3(width * 0.2f, 0.07f, 0.07f);
                DestroyCollider(planter);
            }

            var lowerStripe = Primitive("CandyLowerStripe", PrimitiveType.Cube, parent, accent);
            lowerStripe.transform.localPosition = new Vector3(0f, 0.31f, -depth * 0.565f);
            lowerStripe.transform.localScale = new Vector3(width * 0.88f, 0.08f, 0.055f);
            DestroyCollider(lowerStripe);
        }

        private void AddFacePlate(Transform parent, string name, Vector3 localPosition, Vector3 localScale, Material material)
        {
            var plate = Primitive(name, PrimitiveType.Cube, parent, material);
            plate.transform.localPosition = localPosition;
            plate.transform.localScale = localScale;
            DestroyCollider(plate);
        }

        private void AddGabledRoof(Transform parent, float width, float depth, float height)
        {
            var leftPlane = Primitive("RoofPlaneLeft", PrimitiveType.Cube, parent, roofMaterial);
            leftPlane.transform.localPosition = new Vector3(0f, height + 0.3f, -depth * 0.2f);
            leftPlane.transform.localRotation = Quaternion.Euler(-14f, 0f, 0f);
            leftPlane.transform.localScale = new Vector3(width * 1.2f, 0.12f, depth * 0.68f);
            DestroyCollider(leftPlane);

            var rightPlane = Primitive("RoofPlaneRight", PrimitiveType.Cube, parent, roofMaterial);
            rightPlane.transform.localPosition = new Vector3(0f, height + 0.3f, depth * 0.2f);
            rightPlane.transform.localRotation = Quaternion.Euler(14f, 0f, 0f);
            rightPlane.transform.localScale = new Vector3(width * 1.2f, 0.12f, depth * 0.68f);
            DestroyCollider(rightPlane);

            var ridge = Primitive("RoofRidge", PrimitiveType.Cube, parent, MaterialOr(whiteMaterial, roofMaterial));
            ridge.transform.localPosition = new Vector3(0f, height + 0.48f, 0f);
            ridge.transform.localScale = new Vector3(width * 1.12f, 0.08f, 0.1f);
            DestroyCollider(ridge);

            var tileMaterial = MaterialOr(roofHighlightMaterial, roofMaterial);
            for (var i = -1; i <= 1; i++)
            {
                var z = i * depth * 0.2f;
                var tile = Primitive("RoofCandyTileRow", PrimitiveType.Cube, parent, tileMaterial);
                tile.transform.localPosition = new Vector3(0f, height + 0.43f - Mathf.Abs(i) * 0.06f, z);
                tile.transform.localRotation = Quaternion.Euler(i < 0 ? -14f : 14f, 0f, 0f);
                tile.transform.localScale = new Vector3(width * 1.08f, 0.035f, 0.045f);
                DestroyCollider(tile);
            }
        }

        private void AddRoofAccessories(Transform parent, float width, float depth, float height, System.Random rng)
        {
            var chimney = Primitive("CandyChimney", PrimitiveType.Cube, parent, MaterialOr(brickMaterial, roofMaterial));
            chimney.transform.localPosition = new Vector3(-width * 0.27f, height + 0.62f, depth * 0.18f);
            chimney.transform.localScale = new Vector3(0.18f, 0.42f, 0.2f);
            DestroyCollider(chimney);

            var chimneyCap = Primitive("CandyChimneyCap", PrimitiveType.Cube, parent, MaterialOr(houseTrimMaterial, whiteMaterial));
            chimneyCap.transform.localPosition = chimney.transform.localPosition + Vector3.up * 0.23f;
            chimneyCap.transform.localScale = new Vector3(0.28f, 0.08f, 0.28f);
            DestroyCollider(chimneyCap);

            if (rng.NextDouble() < 0.58)
            {
                var skylight = Primitive("RoofSkylight", PrimitiveType.Cube, parent, MaterialOr(carGlassMaterial, glassMaterial));
                skylight.transform.localPosition = new Vector3(width * 0.18f, height + 0.5f, -depth * 0.18f);
                skylight.transform.localRotation = Quaternion.Euler(-14f, 0f, 0f);
                skylight.transform.localScale = new Vector3(0.36f, 0.055f, 0.24f);
                DestroyCollider(skylight);
            }
        }

        private void AddStripedAwning(Transform parent, float width, float depth)
        {
            var awning = Primitive("StorefrontAwning", PrimitiveType.Cube, parent, MaterialOr(signMaterial, roadLineMaterial));
            awning.transform.localPosition = new Vector3(0f, 0.86f, -depth * 0.59f);
            awning.transform.localScale = new Vector3(width * 0.72f, 0.12f, 0.32f);
            DestroyCollider(awning);

            for (var i = -1; i <= 1; i++)
            {
                var stripe = Primitive("AwningCreamStripe", PrimitiveType.Cube, parent, MaterialOr(houseTrimMaterial, whiteMaterial));
                stripe.transform.localPosition = new Vector3(i * width * 0.18f, 0.93f, -depth * 0.605f);
                stripe.transform.localScale = new Vector3(width * 0.08f, 0.035f, 0.34f);
                DestroyCollider(stripe);
            }
        }

        private void AddWindow(Transform parent, Vector3 localPosition, Vector3 localScale)
        {
            var frameMaterial = MaterialOr(windowFrameMaterial, concreteMaterial);
            var frame = Primitive("WindowFrame", PrimitiveType.Cube, parent, frameMaterial);
            frame.transform.localPosition = OffsetTowardFace(localPosition, 0.004f);
            frame.transform.localScale = localScale + new Vector3(0.08f, 0.08f, 0.012f);
            DestroyCollider(frame);

            var window = Primitive("Window", PrimitiveType.Cube, parent, glassMaterial);
            window.transform.localPosition = OffsetTowardFace(localPosition, 0.018f);
            window.transform.localScale = localScale;
            DestroyCollider(window);

            var onZFace = Mathf.Abs(localPosition.z) >= Mathf.Abs(localPosition.x);
            var sill = Primitive("WindowSill", PrimitiveType.Cube, parent, MaterialOr(houseTrimMaterial, frameMaterial));
            sill.transform.localPosition = OffsetTowardFace(localPosition + Vector3.down * (localScale.y * 0.72f), 0.026f);
            sill.transform.localScale = onZFace ? new Vector3(localScale.x + 0.12f, 0.04f, 0.065f) : new Vector3(0.065f, 0.04f, localScale.z + 0.12f);
            DestroyCollider(sill);

            var shine = Primitive("WindowGloss", PrimitiveType.Cube, parent, MaterialOr(whiteMaterial, frameMaterial));
            shine.transform.localPosition = OffsetTowardFace(localPosition + Vector3.up * (localScale.y * 0.24f), 0.029f);
            shine.transform.localScale = onZFace ? new Vector3(localScale.x * 0.54f, 0.035f, 0.022f) : new Vector3(0.022f, 0.035f, localScale.z * 0.54f);
            DestroyCollider(shine);
        }

        private void AddFacadeBand(Transform parent, string name, float y, Vector3 localScale, Material material)
        {
            var band = Primitive(name, PrimitiveType.Cube, parent, material);
            band.transform.localPosition = new Vector3(0f, y, 0f);
            band.transform.localScale = localScale;
            DestroyCollider(band);
        }

        private void AddCornerColumns(Transform parent, float width, float depth, float height, Material material)
        {
            for (var x = -1; x <= 1; x += 2)
            {
                for (var z = -1; z <= 1; z += 2)
                {
                    var column = Primitive("CornerColumn", PrimitiveType.Cube, parent, material);
                    column.transform.localPosition = new Vector3(x * width * 0.49f, height * 0.5f, z * depth * 0.49f);
                    column.transform.localScale = new Vector3(0.09f, height * 0.96f, 0.09f);
                    DestroyCollider(column);
                }
            }
        }

        private static Vector3 OffsetTowardFace(Vector3 localPosition, float distance)
        {
            if (Mathf.Abs(localPosition.z) >= Mathf.Abs(localPosition.x))
            {
                return localPosition + Vector3.forward * Mathf.Sign(localPosition.z) * distance;
            }

            return localPosition + Vector3.right * Mathf.Sign(localPosition.x) * distance;
        }

        private void CreateSpecialBuilding(Transform parent, Vector2Int coord, BuildingLot lot)
        {
            var specialType = Mathf.Abs(Hash(coord)) % 3;
            var name = specialType == 0 ? "PowerPlant" : specialType == 1 ? "PoliceStation" : "FireStation";
            var material = specialType == 0 ? powerPlantMaterial : specialType == 1 ? policeMaterial : fireStationMaterial;
            var slotKind = specialType == 0 ? SceneSlotKind.PowerPlant : specialType == 1 ? SceneSlotKind.PoliceStation : SceneSlotKind.FireStation;
            var key = specialType == 0 ? "slot_power_plant" : specialType == 1 ? "slot_police_station" : "slot_fire_station";
            var required = specialType == 0 ? 3.65f : specialType == 1 ? 2.95f : 3.05f;

            var slot = new GameObject($"{name}_Slot");
            slot.transform.SetParent(parent);
            slot.transform.localPosition = lot.position;
            var sceneSlot = slot.AddComponent<SceneSlot>();
            sceneSlot.slotId = $"{name}_{coord.x}_{coord.y}";
            sceneSlot.kind = slotKind;
            sceneSlot.displayNameKey = key;
            sceneSlot.recommendedTier = Mathf.RoundToInt(required);

            var building = new GameObject(name);
            building.transform.SetParent(slot.transform);
            building.transform.localPosition = Vector3.zero;
            building.transform.localRotation = Quaternion.Euler(0f, lot.yaw, 0f);

            var body = Primitive("Body", PrimitiveType.Cube, building.transform, material);
            body.transform.localPosition = Vector3.up * 1.45f;
            body.transform.localScale = new Vector3(lot.maxWidth, 2.9f, lot.maxDepth);
            DestroyCollider(body);

            var collider = building.AddComponent<BoxCollider>();
            collider.center = body.transform.localPosition;
            collider.size = body.transform.localScale;

            var absorbable = building.AddComponent<Absorbable>();
            absorbable.absorbableId = name;
            absorbable.category = AbsorbableCategory.SpecialBuilding;
            absorbable.localizationKey = key;
            absorbable.requiredRadius = required;
            absorbable.growthValue = specialType == 0 ? 0.34f : 0.29f;
            absorbable.scoreValue = specialType == 0 ? 110 : 92;
            absorbable.isSpecialSlot = true;
            absorbable.slotKey = sceneSlot.slotId;

            AddSpecialBuildingFacade(building.transform, lot.maxWidth, lot.maxDepth, 2.9f, specialType);
            AddSpecialBuildingIdentityDetails(building.transform, lot.maxWidth, lot.maxDepth, specialType);

            if (specialType == 0)
            {
                AddPowerPlantTank(building.transform, new Vector3(-2.05f, 0.82f, -1.35f), 0.58f, 0.78f);
                AddPowerPlantTank(building.transform, new Vector3(2.05f, 0.82f, -1.35f), 0.58f, 0.78f);
                AddSign(building.transform, "HazardStripeA", new Vector3(-1.1f, 1.18f, -2.42f), new Vector3(0.82f, 0.12f, 0.08f), MaterialOr(warningStripeMaterial, roadLineMaterial));
                AddSign(building.transform, "HazardStripeB", new Vector3(1.1f, 1.18f, -2.42f), new Vector3(0.82f, 0.12f, 0.08f), MaterialOr(warningStripeMaterial, roadLineMaterial));
                AddStack(building.transform, new Vector3(-1.7f, 3.3f, 1.25f));
                AddStack(building.transform, new Vector3(1.7f, 3.15f, 1.25f));
                AddSign(building.transform, "BoltSign", new Vector3(0f, 2.9f, -2.38f), new Vector3(1.2f, 0.38f, 0.08f), roadLineMaterial);
            }
            else if (specialType == 1)
            {
                AddSign(building.transform, "GlassEntrance", new Vector3(0f, 0.82f, -2.42f), new Vector3(1.2f, 1.35f, 0.08f), MaterialOr(carGlassMaterial, glassMaterial));
                AddSign(building.transform, "PoliceSign", new Vector3(0f, 3.05f, -2.38f), new Vector3(2.2f, 0.42f, 0.08f), whiteMaterial);
                AddSign(building.transform, "BlueLight", new Vector3(-0.55f, 3.48f, -0.9f), new Vector3(0.38f, 0.2f, 0.38f), policeMaterial);
                AddSign(building.transform, "RedLight", new Vector3(0.55f, 3.48f, -0.9f), new Vector3(0.38f, 0.2f, 0.38f), fireStationMaterial);
                AddRoofAntenna(building.transform, new Vector3(2.2f, 3.8f, 1.2f));
            }
            else
            {
                AddSign(building.transform, "GarageDoorA", new Vector3(-1.2f, 0.88f, -2.38f), new Vector3(1.25f, 1.35f, 0.08f), whiteMaterial);
                AddSign(building.transform, "GarageDoorB", new Vector3(1.2f, 0.88f, -2.38f), new Vector3(1.25f, 1.35f, 0.08f), whiteMaterial);
                AddGarageDoorRibs(building.transform, -1.2f);
                AddGarageDoorRibs(building.transform, 1.2f);
                AddFireStationTower(building.transform);
                AddStack(building.transform, new Vector3(2.3f, 3.45f, 1.35f));
            }

            var marker = Primitive($"{name}_Marker", PrimitiveType.Cylinder, slot.transform, markerMaterial);
            marker.transform.localPosition = Vector3.up * 3.5f;
            marker.transform.localScale = new Vector3(1.35f, 0.05f, 1.35f);
            DestroyCollider(marker);
        }

        private void AddStack(Transform parent, Vector3 localPosition)
        {
            var stack = Primitive("Stack", PrimitiveType.Cylinder, parent, MaterialOr(blackMaterial, markerMaterial));
            stack.transform.localPosition = localPosition;
            stack.transform.localScale = new Vector3(0.32f, 0.9f, 0.32f);
            DestroyCollider(stack);
        }

        private void AddSign(Transform parent, string name, Vector3 localPosition, Vector3 localScale, Material material)
        {
            var sign = Primitive(name, PrimitiveType.Cube, parent, material);
            sign.transform.localPosition = localPosition;
            sign.transform.localScale = localScale;
            DestroyCollider(sign);
        }

        private void AddSpecialBuildingFacade(Transform parent, float width, float depth, float height, int specialType)
        {
            AddFacadeBand(parent, "SpecialBase", 0.15f, new Vector3(width * 1.04f, 0.22f, depth * 1.04f), MaterialOr(concreteMaterial, curbMaterial));
            AddFacadeBand(parent, "SpecialRoofTrim", height + 0.05f, new Vector3(width * 1.08f, 0.2f, depth * 1.08f), MaterialOr(metalMaterial, roofMaterial));

            var windowMaterial = specialType == 0 ? MaterialOr(carGlassMaterial, glassMaterial) : glassMaterial;
            for (var x = -1; x <= 1; x++)
            {
                AddWindow(parent, new Vector3(x * 1.35f, 1.95f, -depth * 0.52f), new Vector3(0.62f, 0.34f, 0.045f));
                AddWindow(parent, new Vector3(x * 1.35f, 1.95f, depth * 0.52f), new Vector3(0.62f, 0.34f, 0.045f));
            }

            if (windowMaterial != glassMaterial)
            {
                var strip = Primitive("IndustrialWindowStrip", PrimitiveType.Cube, parent, windowMaterial);
                strip.transform.localPosition = new Vector3(0f, 2.38f, depth * 0.53f);
                strip.transform.localScale = new Vector3(width * 0.68f, 0.2f, 0.045f);
                DestroyCollider(strip);
            }
        }

        private void AddSpecialBuildingIdentityDetails(Transform parent, float width, float depth, int specialType)
        {
            var trim = MaterialOr(houseTrimMaterial, concreteMaterial);
            var accent = specialType == 0 ? MaterialOr(warningStripeMaterial, roadLineMaterial) : specialType == 1 ? policeMaterial : fireStationMaterial;

            AddSign(parent, "SpecialFrontColorBand", new Vector3(0f, 2.68f, -depth * 0.545f), new Vector3(width * 0.82f, 0.16f, 0.06f), accent);
            AddSign(parent, "SpecialCreamRoofCap", new Vector3(0f, 3.08f, 0f), new Vector3(width * 0.82f, 0.12f, depth * 0.82f), trim);

            if (specialType == 0)
            {
                for (var x = -1; x <= 1; x += 2)
                {
                    var pipe = Primitive("PowerPipeRun", PrimitiveType.Cylinder, parent, MaterialOr(metalMaterial, trim));
                    pipe.transform.localPosition = new Vector3(x * width * 0.26f, 1.08f, depth * 0.54f);
                    pipe.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                    pipe.transform.localScale = new Vector3(0.055f, width * 0.16f, 0.055f);
                    DestroyCollider(pipe);
                }
            }
            else if (specialType == 1)
            {
                var shield = Primitive("PoliceShieldBadge", PrimitiveType.Cube, parent, MaterialOr(vehicleTrimMaterial, whiteMaterial));
                shield.transform.localPosition = new Vector3(0f, 2.35f, -depth * 0.565f);
                shield.transform.localScale = new Vector3(0.54f, 0.62f, 0.055f);
                DestroyCollider(shield);

                var shieldInset = Primitive("PoliceShieldInset", PrimitiveType.Cube, parent, policeMaterial);
                shieldInset.transform.localPosition = new Vector3(0f, 2.35f, -depth * 0.595f);
                shieldInset.transform.localScale = new Vector3(0.34f, 0.42f, 0.055f);
                DestroyCollider(shieldInset);
            }
            else
            {
                for (var rail = -1; rail <= 1; rail += 2)
                {
                    var ladderRail = Primitive("FireLadderRail", PrimitiveType.Cube, parent, MaterialOr(vehicleTrimMaterial, whiteMaterial));
                    ladderRail.transform.localPosition = new Vector3(width * 0.46f, 1.72f, rail * 0.18f);
                    ladderRail.transform.localScale = new Vector3(0.055f, 1.72f, 0.04f);
                    DestroyCollider(ladderRail);
                }

                for (var rung = 0; rung < 5; rung++)
                {
                    var ladderRung = Primitive("FireLadderRung", PrimitiveType.Cube, parent, MaterialOr(vehicleTrimMaterial, whiteMaterial));
                    ladderRung.transform.localPosition = new Vector3(width * 0.46f, 0.82f + rung * 0.3f, 0f);
                    ladderRung.transform.localScale = new Vector3(0.06f, 0.04f, 0.42f);
                    DestroyCollider(ladderRung);
                }
            }
        }

        private void AddPowerPlantTank(Transform parent, Vector3 localPosition, float radius, float height)
        {
            var tank = Primitive("PowerTank", PrimitiveType.Cylinder, parent, MaterialOr(metalMaterial, powerPlantMaterial));
            tank.transform.localPosition = localPosition;
            tank.transform.localScale = new Vector3(radius, height * 0.5f, radius);
            DestroyCollider(tank);

            var cap = Primitive("PowerTankCap", PrimitiveType.Cylinder, parent, MaterialOr(concreteMaterial, whiteMaterial));
            cap.transform.localPosition = localPosition + Vector3.up * (height * 0.55f);
            cap.transform.localScale = new Vector3(radius * 1.05f, 0.06f, radius * 1.05f);
            DestroyCollider(cap);
        }

        private void AddGarageDoorRibs(Transform parent, float x)
        {
            for (var i = 0; i < 4; i++)
            {
                var rib = Primitive("GarageDoorRib", PrimitiveType.Cube, parent, MaterialOr(metalMaterial, curbMaterial));
                rib.transform.localPosition = new Vector3(x, 0.42f + i * 0.26f, -2.43f);
                rib.transform.localScale = new Vector3(1.24f, 0.035f, 0.035f);
                DestroyCollider(rib);
            }
        }

        private void AddRoofAntenna(Transform parent, Vector3 localPosition)
        {
            var mast = Primitive("RoofAntenna", PrimitiveType.Cylinder, parent, MaterialOr(metalMaterial, lampPoleMaterial));
            mast.transform.localPosition = localPosition;
            mast.transform.localScale = new Vector3(0.04f, 0.55f, 0.04f);
            DestroyCollider(mast);

            var dish = Primitive("AntennaDish", PrimitiveType.Cube, parent, MaterialOr(carGlassMaterial, glassMaterial));
            dish.transform.localPosition = localPosition + new Vector3(0.16f, 0.42f, 0f);
            dish.transform.localScale = new Vector3(0.3f, 0.18f, 0.04f);
            dish.transform.localRotation = Quaternion.Euler(0f, 22f, 0f);
            DestroyCollider(dish);
        }

        private void AddFireStationTower(Transform parent)
        {
            var tower = Primitive("FireLookoutTower", PrimitiveType.Cube, parent, fireStationMaterial);
            tower.transform.localPosition = new Vector3(-2.25f, 3.6f, 1.05f);
            tower.transform.localScale = new Vector3(0.95f, 1.45f, 0.9f);
            DestroyCollider(tower);

            var towerRoof = Primitive("FireTowerRoof", PrimitiveType.Cube, parent, MaterialOr(roofMaterial, blackMaterial));
            towerRoof.transform.localPosition = new Vector3(-2.25f, 4.42f, 1.05f);
            towerRoof.transform.localScale = new Vector3(1.08f, 0.18f, 1.03f);
            DestroyCollider(towerRoof);
        }

        private void CreateStreetProps(Transform parent, System.Random rng)
        {
            var lampPositions = new[]
            {
                new Vector3(-8.6f, 0f, SidewalkCenter),
                new Vector3(8.6f, 0f, SidewalkCenter),
                new Vector3(-8.6f, 0f, -SidewalkCenter),
                new Vector3(8.6f, 0f, -SidewalkCenter),
                new Vector3(SidewalkCenter, 0f, -8.6f),
                new Vector3(SidewalkCenter, 0f, 8.6f),
                new Vector3(-SidewalkCenter, 0f, -8.6f),
                new Vector3(-SidewalkCenter, 0f, 8.6f)
            };

            for (var i = 0; i < lampPositions.Length; i++)
            {
                CreateLampPost($"LampPost_{i}", lampPositions[i], Quaternion.Euler(0f, LampYawTowardRoad(lampPositions[i]), 0f), parent);
            }

            var treePositions = new[]
            {
                new Vector3(-10.1f, 0f, -10.1f),
                new Vector3(10.1f, 0f, -10.1f),
                new Vector3(-10.1f, 0f, 10.1f),
                new Vector3(10.1f, 0f, 10.1f),
                new Vector3(-4.75f, 0f, -7.15f),
                new Vector3(4.75f, 0f, -7.15f),
                new Vector3(-4.75f, 0f, 7.15f),
                new Vector3(4.75f, 0f, 7.15f)
            };

            var treeCount = Mathf.Min(treePositions.Length, Mathf.RoundToInt(treePositions.Length * Mathf.Clamp(streetPropDensity, 0.5f, 1.4f)));
            for (var i = 0; i < treeCount; i++)
            {
                CreateTree($"Tree_{i}", treePositions[i] + new Vector3(RandomRange(rng, -0.2f, 0.2f), 0f, RandomRange(rng, -0.2f, 0.2f)), parent, rng);
            }

            var hydrants = new[]
            {
                new Vector3(-(RoadHalf + 0.8f), 0f, -(RoadHalf + 0.8f)),
                new Vector3(RoadHalf + 0.8f, 0f, -(RoadHalf + 0.8f)),
                new Vector3(-(RoadHalf + 0.8f), 0f, RoadHalf + 0.8f),
                new Vector3(RoadHalf + 0.8f, 0f, RoadHalf + 0.8f)
            };

            for (var i = 0; i < hydrants.Length; i++)
            {
                CreateHydrant($"Hydrant_{i}", hydrants[i], parent);
            }

            var benches = new[]
            {
                new Vector3(-9.6f, 0f, -6.1f),
                new Vector3(9.6f, 0f, -6.1f),
                new Vector3(-9.6f, 0f, 6.1f),
                new Vector3(9.6f, 0f, 6.1f)
            };

            for (var i = 0; i < benches.Length; i++)
            {
                CreateBench($"Bench_{i}", benches[i], parent, Quaternion.Euler(0f, benches[i].z > 0f ? 180f : 0f, 0f));
            }
        }

        private void CreateTree(string id, Vector3 localPosition, Transform parent, System.Random rng)
        {
            var tree = new GameObject(id);
            tree.transform.SetParent(parent);
            tree.transform.localPosition = localPosition;

            var height = RandomRange(rng, 1.15f, 1.75f);
            var trunk = Primitive("Trunk", PrimitiveType.Cylinder, tree.transform, treeTrunkMaterial);
            trunk.transform.localPosition = Vector3.up * (height * 0.35f);
            trunk.transform.localScale = new Vector3(0.16f, height * 0.35f, 0.16f);
            DestroyCollider(trunk);

            for (var ring = 0; ring < 3; ring++)
            {
                var barkBand = Primitive("BarkBand", PrimitiveType.Cylinder, tree.transform, MaterialOr(brickMaterial, treeTrunkMaterial));
                barkBand.transform.localPosition = Vector3.up * (0.22f + ring * height * 0.18f);
                barkBand.transform.localScale = new Vector3(0.165f, 0.018f, 0.165f);
                DestroyCollider(barkBand);
            }

            for (var branch = 0; branch < 3; branch++)
            {
                var angle = branch * 120f + RandomRange(rng, -18f, 18f);
                var branchObject = Primitive("Branch", PrimitiveType.Cylinder, tree.transform, MaterialOr(treeTrunkMaterial, brickMaterial));
                branchObject.transform.localPosition = Vector3.up * (height * 0.62f);
                branchObject.transform.localRotation = Quaternion.Euler(55f, angle, 0f);
                branchObject.transform.localScale = new Vector3(0.045f, 0.38f, 0.045f);
                DestroyCollider(branchObject);
            }

            var canopy = Primitive("Canopy", PrimitiveType.Sphere, tree.transform, treeCanopyMaterial);
            canopy.transform.localPosition = Vector3.up * (height * 0.9f);
            canopy.transform.localScale = Vector3.one * RandomRange(rng, 0.85f, 1.15f);
            DestroyCollider(canopy);

            var canopyHighlight = Primitive("CanopyHighlight", PrimitiveType.Sphere, tree.transform, MaterialOr(houseMintMaterial, treeCanopyMaterial));
            canopyHighlight.transform.localPosition = new Vector3(-0.18f, height * 1.05f, -0.16f);
            canopyHighlight.transform.localScale = Vector3.one * 0.38f;
            DestroyCollider(canopyHighlight);

            for (var lobe = 0; lobe < 3; lobe++)
            {
                var angle = (lobe * 120f + 35f) * Mathf.Deg2Rad;
                var leaf = Primitive("CanopyLobe", PrimitiveType.Sphere, tree.transform, lobe == 1 ? MaterialOr(leafDarkMaterial, treeCanopyMaterial) : treeCanopyMaterial);
                leaf.transform.localPosition = new Vector3(Mathf.Cos(angle) * 0.24f, height * RandomRange(rng, 0.82f, 1.02f), Mathf.Sin(angle) * 0.24f);
                leaf.transform.localScale = Vector3.one * RandomRange(rng, 0.52f, 0.72f);
                DestroyCollider(leaf);
            }

            var collider = tree.AddComponent<BoxCollider>();
            collider.center = Vector3.up * (height * 0.75f);
            collider.size = new Vector3(1.15f, height * 1.25f, 1.15f);

            var absorbable = tree.AddComponent<Absorbable>();
            absorbable.absorbableId = "city_tree";
            absorbable.category = AbsorbableCategory.Nature;
            absorbable.localizationKey = "object_tree";
            absorbable.requiredRadius = 1.05f;
            absorbable.growthValue = 0.018f;
            absorbable.scoreValue = 6;
        }

        private void CreateLampPost(string id, Vector3 localPosition, Quaternion localRotation, Transform parent)
        {
            var lamp = new GameObject(id);
            lamp.transform.SetParent(parent);
            lamp.transform.localPosition = localPosition;
            lamp.transform.localRotation = localRotation;

            var basePlate = Primitive("BasePlate", PrimitiveType.Cylinder, lamp.transform, MaterialOr(metalMaterial, lampPoleMaterial));
            basePlate.transform.localPosition = Vector3.up * 0.06f;
            basePlate.transform.localScale = new Vector3(0.18f, 0.06f, 0.18f);
            DestroyCollider(basePlate);

            for (var i = 0; i < 4; i++)
            {
                var angle = i * Mathf.PI * 0.5f;
                var bolt = Primitive("BaseBolt", PrimitiveType.Cube, lamp.transform, MaterialOr(vehicleTrimMaterial, whiteMaterial));
                bolt.transform.localPosition = new Vector3(Mathf.Cos(angle) * 0.13f, 0.14f, Mathf.Sin(angle) * 0.13f);
                bolt.transform.localScale = new Vector3(0.04f, 0.035f, 0.04f);
                DestroyCollider(bolt);
            }

            var pole = Primitive("Pole", PrimitiveType.Cylinder, lamp.transform, lampPoleMaterial);
            pole.transform.localPosition = Vector3.up * 0.8f;
            pole.transform.localScale = new Vector3(0.08f, 0.8f, 0.08f);
            DestroyCollider(pole);

            for (var band = 0; band < 3; band++)
            {
                var poleBand = Primitive("PoleCandyBand", PrimitiveType.Cylinder, lamp.transform, MaterialOr(signMaterial, roadLineMaterial));
                poleBand.transform.localPosition = Vector3.up * (0.48f + band * 0.32f);
                poleBand.transform.localScale = new Vector3(0.086f, 0.018f, 0.086f);
                DestroyCollider(poleBand);
            }

            var arm = Primitive("LampArm", PrimitiveType.Cube, lamp.transform, MaterialOr(metalMaterial, lampPoleMaterial));
            arm.transform.localPosition = new Vector3(0.22f, 1.55f, 0f);
            arm.transform.localScale = new Vector3(0.45f, 0.055f, 0.055f);
            DestroyCollider(arm);

            var shade = Primitive("LampShade", PrimitiveType.Cube, lamp.transform, MaterialOr(metalMaterial, blackMaterial));
            shade.transform.localPosition = new Vector3(0.48f, 1.48f, 0f);
            shade.transform.localScale = new Vector3(0.35f, 0.12f, 0.24f);
            DestroyCollider(shade);

            var light = Primitive("Light", PrimitiveType.Sphere, lamp.transform, lampLightMaterial);
            light.transform.localPosition = new Vector3(0.48f, 1.38f, 0f);
            light.transform.localScale = new Vector3(0.24f, 0.12f, 0.2f);
            DestroyCollider(light);

            var glowRim = Primitive("LightGlowRim", PrimitiveType.Sphere, lamp.transform, MaterialOr(whiteMaterial, lampLightMaterial));
            glowRim.transform.localPosition = new Vector3(0.48f, 1.38f, 0f);
            glowRim.transform.localScale = new Vector3(0.28f, 0.035f, 0.24f);
            DestroyCollider(glowRim);

            var banner = Primitive("StreetBanner", PrimitiveType.Cube, lamp.transform, MaterialOr(signMaterial, roadLineMaterial));
            banner.transform.localPosition = new Vector3(-0.08f, 1.08f, 0f);
            banner.transform.localScale = new Vector3(0.045f, 0.42f, 0.28f);
            DestroyCollider(banner);

            var collider = lamp.AddComponent<BoxCollider>();
            collider.center = Vector3.up * 0.85f;
            collider.size = new Vector3(0.75f, 1.8f, 0.55f);

            var absorbable = lamp.AddComponent<Absorbable>();
            absorbable.absorbableId = "city_lamp_post";
            absorbable.category = AbsorbableCategory.Prop;
            absorbable.localizationKey = "object_lamp_post";
            absorbable.requiredRadius = 0.95f;
            absorbable.growthValue = 0.014f;
            absorbable.scoreValue = 5;
        }

        private void CreateHydrant(string id, Vector3 localPosition, Transform parent)
        {
            var hydrant = new GameObject(id);
            hydrant.transform.SetParent(parent);
            hydrant.transform.localPosition = localPosition;

            var body = Primitive("Body", PrimitiveType.Cylinder, hydrant.transform, fireStationMaterial);
            body.transform.localPosition = Vector3.up * 0.23f;
            body.transform.localScale = new Vector3(0.18f, 0.23f, 0.18f);
            DestroyCollider(body);

            var cap = Primitive("Cap", PrimitiveType.Sphere, hydrant.transform, roadLineMaterial);
            cap.transform.localPosition = Vector3.up * 0.5f;
            cap.transform.localScale = Vector3.one * 0.28f;
            DestroyCollider(cap);

            for (var side = -1; side <= 1; side += 2)
            {
                var nozzle = Primitive("SideNozzle", PrimitiveType.Cylinder, hydrant.transform, MaterialOr(metalMaterial, fireStationMaterial));
                nozzle.transform.localPosition = new Vector3(side * 0.2f, 0.3f, 0f);
                nozzle.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                nozzle.transform.localScale = new Vector3(0.07f, 0.12f, 0.07f);
                DestroyCollider(nozzle);
            }

            var collider = hydrant.AddComponent<BoxCollider>();
            collider.center = Vector3.up * 0.28f;
            collider.size = new Vector3(0.55f, 0.62f, 0.55f);

            var absorbable = hydrant.AddComponent<Absorbable>();
            absorbable.absorbableId = "city_hydrant";
            absorbable.category = AbsorbableCategory.Prop;
            absorbable.localizationKey = "object_hydrant";
            absorbable.requiredRadius = 0.85f;
            absorbable.growthValue = 0.012f;
            absorbable.scoreValue = 4;
        }

        private void CreateBench(string id, Vector3 localPosition, Transform parent, Quaternion localRotation)
        {
            var bench = new GameObject(id);
            bench.transform.SetParent(parent);
            bench.transform.localPosition = localPosition;
            bench.transform.localRotation = localRotation;

            var seat = Primitive("Seat", PrimitiveType.Cube, bench.transform, MaterialOr(roofMaterial, houseMaterialA));
            seat.transform.localPosition = Vector3.up * 0.35f;
            seat.transform.localScale = new Vector3(1.2f, 0.18f, 0.38f);
            DestroyCollider(seat);

            var back = Primitive("Back", PrimitiveType.Cube, bench.transform, MaterialOr(roofMaterial, houseMaterialA));
            back.transform.localPosition = new Vector3(0f, 0.62f, 0.24f);
            back.transform.localScale = new Vector3(1.2f, 0.42f, 0.14f);
            DestroyCollider(back);

            for (var x = -1; x <= 1; x += 2)
            {
                var leg = Primitive("BenchLeg", PrimitiveType.Cube, bench.transform, MaterialOr(metalMaterial, blackMaterial));
                leg.transform.localPosition = new Vector3(x * 0.42f, 0.18f, -0.08f);
                leg.transform.localScale = new Vector3(0.08f, 0.34f, 0.08f);
                DestroyCollider(leg);

                var slat = Primitive("BenchSlat", PrimitiveType.Cube, bench.transform, MaterialOr(concreteMaterial, curbMaterial));
                slat.transform.localPosition = new Vector3(0f, 0.68f, 0.16f + x * 0.08f);
                slat.transform.localScale = new Vector3(1.12f, 0.045f, 0.045f);
                DestroyCollider(slat);
            }

            var collider = bench.AddComponent<BoxCollider>();
            collider.center = Vector3.up * 0.42f;
            collider.size = new Vector3(1.25f, 0.75f, 0.72f);

            var absorbable = bench.AddComponent<Absorbable>();
            absorbable.absorbableId = "city_bench";
            absorbable.category = AbsorbableCategory.Prop;
            absorbable.localizationKey = "object_bench";
            absorbable.requiredRadius = 0.9f;
            absorbable.growthValue = 0.012f;
            absorbable.scoreValue = 4;
        }

        private void CreatePedestrians(Transform parent, System.Random rng)
        {
            var spawns = new[]
            {
                new BuildingLot(new Vector3(-6.7f, 0f, SidewalkCenter), 90f, 0f, 0f, 0f, 0f),
                new BuildingLot(new Vector3(6.7f, 0f, -SidewalkCenter), -90f, 0f, 0f, 0f, 0f),
                new BuildingLot(new Vector3(SidewalkCenter, 0f, 6.7f), 180f, 0f, 0f, 0f, 0f),
                new BuildingLot(new Vector3(-SidewalkCenter, 0f, -6.7f), 0f, 0f, 0f, 0f, 0f),
                new BuildingLot(new Vector3(-10.3f, 0f, 3.7f), 180f, 0f, 0f, 0f, 0f),
                new BuildingLot(new Vector3(10.3f, 0f, -3.7f), 0f, 0f, 0f, 0f, 0f)
            };

            var count = Mathf.Min(spawns.Length, Mathf.RoundToInt(pedestriansPerChunk * pedestrianDensity));
            for (var i = 0; i < count; i++)
            {
                var jitter = new Vector3(RandomRange(rng, -0.16f, 0.16f), 0f, RandomRange(rng, -0.16f, 0.16f));
                CreatePedestrian($"Pedestrian_{i}", spawns[i].position + jitter, Quaternion.Euler(0f, spawns[i].yaw, 0f), parent, rng);
            }
        }

        private void CreatePedestrian(string id, Vector3 localPosition, Quaternion localRotation, Transform parent, System.Random rng)
        {
            var pedestrian = new GameObject(id);
            pedestrian.transform.SetParent(parent);
            pedestrian.transform.localPosition = localPosition;
            pedestrian.transform.localRotation = localRotation;

            var shirtMaterial = rng.NextDouble() < 0.5 ? pedestrianShirtMaterial : MaterialOr(signMaterial, pedestrianShirtMaterial);

            var torso = Primitive("Torso", PrimitiveType.Cube, pedestrian.transform, shirtMaterial);
            torso.transform.localPosition = Vector3.up * 0.86f;
            torso.transform.localScale = new Vector3(0.28f, 0.48f, 0.18f);
            DestroyCollider(torso);

            var shirtStripe = Primitive("ShirtStripe", PrimitiveType.Cube, pedestrian.transform, MaterialOr(houseTrimMaterial, whiteMaterial));
            shirtStripe.transform.localPosition = new Vector3(0f, 0.9f, -0.095f);
            shirtStripe.transform.localScale = new Vector3(0.22f, 0.045f, 0.025f);
            DestroyCollider(shirtStripe);

            var head = Primitive("Head", PrimitiveType.Sphere, pedestrian.transform, pedestrianSkinMaterial);
            head.transform.localPosition = Vector3.up * 1.2f;
            head.transform.localScale = Vector3.one * 0.22f;
            DestroyCollider(head);

            for (var side = -1; side <= 1; side += 2)
            {
                var eye = Primitive("Eye", PrimitiveType.Cube, pedestrian.transform, MaterialOr(blackMaterial, pedestrianHairMaterial));
                eye.transform.localPosition = new Vector3(side * 0.055f, 1.22f, -0.18f);
                eye.transform.localScale = new Vector3(0.035f, 0.028f, 0.02f);
                DestroyCollider(eye);
            }

            var hair = Primitive("Hair", PrimitiveType.Sphere, pedestrian.transform, MaterialOr(pedestrianHairMaterial, blackMaterial));
            hair.transform.localPosition = new Vector3(0f, 1.31f, 0f);
            hair.transform.localScale = new Vector3(0.23f, 0.12f, 0.23f);
            DestroyCollider(hair);

            var backpack = Primitive("Backpack", PrimitiveType.Cube, pedestrian.transform, MaterialOr(signMaterial, pedestrianShirtMaterial));
            backpack.transform.localPosition = new Vector3(0f, 0.86f, 0.12f);
            backpack.transform.localScale = new Vector3(0.22f, 0.34f, 0.08f);
            DestroyCollider(backpack);

            for (var side = -1; side <= 1; side += 2)
            {
                var arm = Primitive("Arm", PrimitiveType.Cylinder, pedestrian.transform, pedestrianSkinMaterial);
                arm.transform.localPosition = new Vector3(side * 0.2f, 0.86f, 0f);
                arm.transform.localRotation = Quaternion.Euler(12f, 0f, side * 8f);
                arm.transform.localScale = new Vector3(0.035f, 0.24f, 0.035f);
                DestroyCollider(arm);

                var leg = Primitive("Leg", PrimitiveType.Cylinder, pedestrian.transform, pedestrianPantsMaterial);
                leg.transform.localPosition = new Vector3(side * 0.07f, 0.38f, 0f);
                leg.transform.localScale = new Vector3(0.04f, 0.32f, 0.04f);
                DestroyCollider(leg);

                var foot = Primitive("Foot", PrimitiveType.Cube, pedestrian.transform, MaterialOr(blackMaterial, asphaltMaterial));
                foot.transform.localPosition = new Vector3(side * 0.07f, 0.08f, 0.04f);
                foot.transform.localScale = new Vector3(0.09f, 0.05f, 0.18f);
                DestroyCollider(foot);
            }

            var collider = pedestrian.AddComponent<BoxCollider>();
            collider.center = Vector3.up * 0.7f;
            collider.size = new Vector3(0.48f, 1.35f, 0.42f);

            var absorbable = pedestrian.AddComponent<Absorbable>();
            absorbable.absorbableId = "city_pedestrian";
            absorbable.category = AbsorbableCategory.Prop;
            absorbable.localizationKey = "object_pedestrian";
            absorbable.requiredRadius = 0.82f;
            absorbable.growthValue = 0.009f;
            absorbable.scoreValue = 3;
        }

        private void CreateVehicles(Transform parent, System.Random rng)
        {
            var laneSpawns = new[]
            {
                new BuildingLot(new Vector3(-9.1f, 0f, -LaneOffset), 0f, 0f, 0f, 0f, 0f),
                new BuildingLot(new Vector3(8.8f, 0f, LaneOffset), 180f, 0f, 0f, 0f, 0f),
                new BuildingLot(new Vector3(-6.4f, 0f, LaneOffset), 180f, 0f, 0f, 0f, 0f),
                new BuildingLot(new Vector3(6.6f, 0f, -LaneOffset), 0f, 0f, 0f, 0f, 0f),
                new BuildingLot(new Vector3(LaneOffset, 0f, -9.0f), -90f, 0f, 0f, 0f, 0f),
                new BuildingLot(new Vector3(-LaneOffset, 0f, 8.9f), 90f, 0f, 0f, 0f, 0f),
                new BuildingLot(new Vector3(LaneOffset, 0f, 6.2f), -90f, 0f, 0f, 0f, 0f),
                new BuildingLot(new Vector3(-LaneOffset, 0f, -6.2f), 90f, 0f, 0f, 0f, 0f)
            };

            var vehicleCount = Mathf.Min(laneSpawns.Length, Mathf.RoundToInt((5 + rng.Next(0, 3)) * vehicleDensity));
            for (var i = 0; i < vehicleCount; i++)
            {
                var isBus = i == 1 || (i == 5 && rng.NextDouble() < 0.45);
                var jitter = Quaternion.Euler(0f, laneSpawns[i].yaw, 0f) * new Vector3(RandomRange(rng, -0.22f, 0.22f), 0f, 0f);
                CreateVehicle(isBus ? $"Bus_{i}" : $"Car_{i}", laneSpawns[i].position + jitter, Quaternion.Euler(0f, laneSpawns[i].yaw, 0f), parent, isBus);
            }
        }

        private void CreateVehicle(string id, Vector3 localPosition, Quaternion localRotation, Transform parent, bool isBus)
        {
            var vehicle = new GameObject(id);
            vehicle.transform.SetParent(parent);
            vehicle.transform.localPosition = localPosition;
            vehicle.transform.localRotation = localRotation;

            var vehicleMaterial = MaterialOr(isBus ? busMaterial : carMaterial, carMaterial);
            var glass = MaterialOr(carGlassMaterial, glassMaterial);

            var body = Primitive("Body", PrimitiveType.Cube, vehicle.transform, vehicleMaterial);
            body.transform.localPosition = Vector3.up * (isBus ? 0.46f : 0.3f);
            body.transform.localScale = isBus ? new Vector3(3.45f, 0.82f, 1.16f) : new Vector3(1.55f, 0.52f, 0.9f);
            DestroyCollider(body);

            var chassis = Primitive("LowerChassis", PrimitiveType.Cube, vehicle.transform, MaterialOr(metalMaterial, blackMaterial));
            chassis.transform.localPosition = Vector3.up * (isBus ? 0.2f : 0.18f);
            chassis.transform.localScale = isBus ? new Vector3(3.55f, 0.18f, 1.2f) : new Vector3(1.62f, 0.14f, 0.94f);
            DestroyCollider(chassis);

            var cabin = Primitive("Cabin", PrimitiveType.Cube, vehicle.transform, glass);
            cabin.transform.localPosition = Vector3.up * (isBus ? 0.95f : 0.68f) + Vector3.right * (isBus ? 0.25f : -0.1f);
            cabin.transform.localScale = isBus ? new Vector3(2.75f, 0.38f, 1.05f) : new Vector3(0.82f, 0.38f, 0.78f);
            DestroyCollider(cabin);

            if (isBus)
            {
                var roof = Primitive("BusWhiteRoof", PrimitiveType.Cube, vehicle.transform, MaterialOr(whiteMaterial, concreteMaterial));
                roof.transform.localPosition = new Vector3(-0.1f, 1.17f, 0f);
                roof.transform.localScale = new Vector3(2.55f, 0.055f, 0.88f);
                DestroyCollider(roof);

                for (var x = -1.15f; x <= 1.15f; x += 0.58f)
                {
                    var roofWindow = Primitive("BusRoofWindow", PrimitiveType.Cube, vehicle.transform, glass);
                    roofWindow.transform.localPosition = new Vector3(x, 1.205f, 0f);
                    roofWindow.transform.localScale = new Vector3(0.36f, 0.035f, 0.58f);
                    DestroyCollider(roofWindow);
                }

                for (var x = -1; x <= 1; x++)
                {
                    AddVehicleWindow(vehicle.transform, new Vector3(x * 0.82f, 0.96f, -0.61f), new Vector3(0.48f, 0.28f, 0.04f));
                    AddVehicleWindow(vehicle.transform, new Vector3(x * 0.82f, 0.96f, 0.61f), new Vector3(0.48f, 0.28f, 0.04f));
                }

                var door = Primitive("BusDoor", PrimitiveType.Cube, vehicle.transform, glass);
                door.transform.localPosition = new Vector3(1.32f, 0.56f, -0.62f);
                door.transform.localScale = new Vector3(0.36f, 0.72f, 0.045f);
                DestroyCollider(door);

                var routeSign = Primitive("RouteSign", PrimitiveType.Cube, vehicle.transform, MaterialOr(signMaterial, roadLineMaterial));
                routeSign.transform.localPosition = new Vector3(1.55f, 0.94f, -0.63f);
                routeSign.transform.localScale = new Vector3(0.62f, 0.18f, 0.04f);
                DestroyCollider(routeSign);
            }
            else
            {
                var hood = Primitive("Hood", PrimitiveType.Cube, vehicle.transform, vehicleMaterial);
                hood.transform.localPosition = new Vector3(0.52f, 0.48f, 0f);
                hood.transform.localScale = new Vector3(0.5f, 0.12f, 0.82f);
                DestroyCollider(hood);

                var trunk = Primitive("Trunk", PrimitiveType.Cube, vehicle.transform, vehicleMaterial);
                trunk.transform.localPosition = new Vector3(-0.58f, 0.47f, 0f);
                trunk.transform.localScale = new Vector3(0.42f, 0.11f, 0.78f);
                DestroyCollider(trunk);

                var windshield = Primitive("TopWindshield", PrimitiveType.Cube, vehicle.transform, glass);
                windshield.transform.localPosition = new Vector3(0.3f, 0.9f, 0f);
                windshield.transform.localScale = new Vector3(0.32f, 0.04f, 0.72f);
                DestroyCollider(windshield);

                var rearWindow = Primitive("TopRearWindow", PrimitiveType.Cube, vehicle.transform, glass);
                rearWindow.transform.localPosition = new Vector3(-0.48f, 0.82f, 0f);
                rearWindow.transform.localScale = new Vector3(0.28f, 0.035f, 0.66f);
                DestroyCollider(rearWindow);

                var roofHighlight = Primitive("RoofHighlight", PrimitiveType.Cube, vehicle.transform, MaterialOr(whiteMaterial, concreteMaterial));
                roofHighlight.transform.localPosition = new Vector3(-0.08f, 0.925f, -0.26f);
                roofHighlight.transform.localScale = new Vector3(0.44f, 0.025f, 0.055f);
                DestroyCollider(roofHighlight);

                for (var side = -1; side <= 1; side += 2)
                {
                    var mirror = Primitive("SideMirror", PrimitiveType.Cube, vehicle.transform, MaterialOr(blackMaterial, metalMaterial));
                    mirror.transform.localPosition = new Vector3(0.28f, 0.6f, side * 0.56f);
                    mirror.transform.localScale = new Vector3(0.16f, 0.08f, 0.045f);
                    DestroyCollider(mirror);
                }

                AddVehicleWindow(vehicle.transform, new Vector3(0.08f, 0.72f, -0.47f), new Vector3(0.58f, 0.24f, 0.04f));
                AddVehicleWindow(vehicle.transform, new Vector3(0.08f, 0.72f, 0.47f), new Vector3(0.58f, 0.24f, 0.04f));
            }

            var frontBumper = Primitive("FrontBumper", PrimitiveType.Cube, vehicle.transform, MaterialOr(whiteMaterial, concreteMaterial));
            frontBumper.transform.localPosition = new Vector3(isBus ? 1.78f : 0.86f, isBus ? 0.53f : 0.42f, 0f);
            frontBumper.transform.localScale = new Vector3(0.08f, 0.09f, isBus ? 0.92f : 0.64f);
            DestroyCollider(frontBumper);

            var rearBumper = Primitive("RearBumper", PrimitiveType.Cube, vehicle.transform, MaterialOr(blackMaterial, metalMaterial));
            rearBumper.transform.localPosition = new Vector3(isBus ? -1.78f : -0.86f, isBus ? 0.49f : 0.39f, 0f);
            rearBumper.transform.localScale = new Vector3(0.08f, 0.08f, isBus ? 0.9f : 0.62f);
            DestroyCollider(rearBumper);
            AddVehicleSurfaceDetails(vehicle.transform, isBus, vehicleMaterial);

            for (var x = -1; x <= 1; x += 2)
            {
                for (var z = -1; z <= 1; z += 2)
                {
                    var wheel = Primitive("Wheel", PrimitiveType.Cylinder, vehicle.transform, MaterialOr(tireMaterial, blackMaterial));
                    wheel.transform.localPosition = new Vector3(x * (isBus ? 1.25f : 0.55f), 0.18f, z * 0.5f);
                    wheel.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                    wheel.transform.localScale = new Vector3(0.18f, 0.08f, 0.18f);
                    DestroyCollider(wheel);

                    var hub = Primitive("WheelHub", PrimitiveType.Cylinder, vehicle.transform, MaterialOr(metalMaterial, whiteMaterial));
                    hub.transform.localPosition = wheel.transform.localPosition + new Vector3(0f, 0f, z * 0.01f);
                    hub.transform.localRotation = wheel.transform.localRotation;
                    hub.transform.localScale = new Vector3(0.08f, 0.085f, 0.08f);
                    DestroyCollider(hub);
                }
            }
            AddWheelArchAndTireDetails(vehicle.transform, isBus);

            AddVehicleLights(vehicle.transform, isBus);

            if (isBus)
            {
                var stripe = Primitive("RouteStripe", PrimitiveType.Cube, vehicle.transform, roadLineMaterial);
                stripe.transform.localPosition = new Vector3(0f, 0.54f, -0.6f);
                stripe.transform.localScale = new Vector3(3.2f, 0.08f, 0.04f);
                DestroyCollider(stripe);
            }

            var collider = vehicle.AddComponent<BoxCollider>();
            collider.center = body.transform.localPosition;
            collider.size = body.transform.localScale;

            var absorbable = vehicle.AddComponent<Absorbable>();
            absorbable.absorbableId = isBus ? "city_bus" : "city_car";
            absorbable.category = AbsorbableCategory.Vehicle;
            absorbable.localizationKey = isBus ? "object_bus" : "object_car";
            absorbable.requiredRadius = isBus ? 2.05f : 1.08f;
            absorbable.growthValue = isBus ? 0.09f : 0.034f;
            absorbable.scoreValue = isBus ? 28 : 9;
        }

        private void AddVehicleSurfaceDetails(Transform parent, bool isBus, Material vehicleMaterial)
        {
            var trim = MaterialOr(vehicleTrimMaterial, whiteMaterial);
            var dark = MaterialOr(blackMaterial, metalMaterial);
            var glass = MaterialOr(carGlassMaterial, glassMaterial);

            if (isBus)
            {
                var windowBelt = Primitive("BusWindowBelt", PrimitiveType.Cube, parent, dark);
                windowBelt.transform.localPosition = new Vector3(0f, 0.95f, -0.625f);
                windowBelt.transform.localScale = new Vector3(3.15f, 0.09f, 0.035f);
                DestroyCollider(windowBelt);

                for (var i = -2; i <= 2; i++)
                {
                    var divider = Primitive("BusWindowDivider", PrimitiveType.Cube, parent, trim);
                    divider.transform.localPosition = new Vector3(i * 0.55f, 0.96f, -0.65f);
                    divider.transform.localScale = new Vector3(0.035f, 0.34f, 0.04f);
                    DestroyCollider(divider);
                }

                var routeFront = Primitive("BusFrontRoutePlate", PrimitiveType.Cube, parent, MaterialOr(signMaterial, roadLineMaterial));
                routeFront.transform.localPosition = new Vector3(1.8f, 0.92f, 0f);
                routeFront.transform.localScale = new Vector3(0.055f, 0.22f, 0.5f);
                DestroyCollider(routeFront);

                for (var i = -1; i <= 1; i++)
                {
                    var vent = Primitive("BusRoofVent", PrimitiveType.Cube, parent, MaterialOr(metalMaterial, trim));
                    vent.transform.localPosition = new Vector3(i * 0.75f, 1.28f, 0.34f);
                    vent.transform.localScale = new Vector3(0.38f, 0.055f, 0.16f);
                    DestroyCollider(vent);
                }
            }
            else
            {
                var hoodStripe = Primitive("HoodPaintHighlight", PrimitiveType.Cube, parent, trim);
                hoodStripe.transform.localPosition = new Vector3(0.55f, 0.56f, 0f);
                hoodStripe.transform.localScale = new Vector3(0.34f, 0.035f, 0.055f);
                DestroyCollider(hoodStripe);

                var grille = Primitive("FrontGrille", PrimitiveType.Cube, parent, dark);
                grille.transform.localPosition = new Vector3(0.91f, 0.43f, 0f);
                grille.transform.localScale = new Vector3(0.045f, 0.14f, 0.36f);
                DestroyCollider(grille);

                var license = Primitive("FrontLicensePlate", PrimitiveType.Cube, parent, trim);
                license.transform.localPosition = new Vector3(0.94f, 0.33f, 0f);
                license.transform.localScale = new Vector3(0.035f, 0.075f, 0.25f);
                DestroyCollider(license);

                var rearLicense = Primitive("RearLicensePlate", PrimitiveType.Cube, parent, trim);
                rearLicense.transform.localPosition = new Vector3(-0.94f, 0.31f, 0f);
                rearLicense.transform.localScale = new Vector3(0.035f, 0.07f, 0.24f);
                DestroyCollider(rearLicense);

                var cabinSplit = Primitive("CabinSplitLine", PrimitiveType.Cube, parent, MaterialOr(vehicleMaterial, trim));
                cabinSplit.transform.localPosition = new Vector3(-0.1f, 0.94f, 0f);
                cabinSplit.transform.localScale = new Vector3(0.055f, 0.035f, 0.72f);
                DestroyCollider(cabinSplit);

                var rearGlassTint = Primitive("RearGlassTint", PrimitiveType.Cube, parent, glass);
                rearGlassTint.transform.localPosition = new Vector3(-0.68f, 0.72f, 0f);
                rearGlassTint.transform.localScale = new Vector3(0.05f, 0.18f, 0.58f);
                DestroyCollider(rearGlassTint);
            }

            for (var z = -1; z <= 1; z += 2)
            {
                var sideTrim = Primitive(isBus ? "BusSideChromeLine" : "CarSideChromeLine", PrimitiveType.Cube, parent, trim);
                sideTrim.transform.localPosition = new Vector3(0f, isBus ? 0.5f : 0.42f, z * (isBus ? 0.63f : 0.5f));
                sideTrim.transform.localScale = new Vector3(isBus ? 3.05f : 1.34f, 0.045f, 0.035f);
                DestroyCollider(sideTrim);
            }
        }

        private void AddWheelArchAndTireDetails(Transform parent, bool isBus)
        {
            var archMaterial = MaterialOr(blackMaterial, tireMaterial);
            var trim = MaterialOr(vehicleTrimMaterial, whiteMaterial);
            var xDistance = isBus ? 1.25f : 0.55f;
            var zDistance = 0.51f;

            for (var x = -1; x <= 1; x += 2)
            {
                for (var z = -1; z <= 1; z += 2)
                {
                    var arch = Primitive("WheelArch", PrimitiveType.Cube, parent, archMaterial);
                    arch.transform.localPosition = new Vector3(x * xDistance, isBus ? 0.38f : 0.34f, z * zDistance);
                    arch.transform.localScale = new Vector3(isBus ? 0.48f : 0.34f, 0.08f, 0.055f);
                    DestroyCollider(arch);

                    var tread = Primitive("TireTreadMark", PrimitiveType.Cube, parent, trim);
                    tread.transform.localPosition = new Vector3(x * xDistance, 0.18f, z * 0.59f);
                    tread.transform.localScale = new Vector3(0.035f, 0.09f, 0.12f);
                    DestroyCollider(tread);
                }
            }
        }

        private void AddVehicleWindow(Transform parent, Vector3 localPosition, Vector3 localScale)
        {
            var window = Primitive("VehicleWindow", PrimitiveType.Cube, parent, MaterialOr(carGlassMaterial, glassMaterial));
            window.transform.localPosition = localPosition;
            window.transform.localScale = localScale;
            DestroyCollider(window);
        }

        private void AddVehicleLights(Transform parent, bool isBus)
        {
            var frontX = isBus ? 1.78f : 0.82f;
            var rearX = isBus ? -1.78f : -0.82f;
            var halfWidth = isBus ? 0.48f : 0.34f;
            var y = isBus ? 0.44f : 0.34f;

            for (var z = -1; z <= 1; z += 2)
            {
                var headlight = Primitive("Headlight", PrimitiveType.Cube, parent, MaterialOr(headlightMaterial, whiteMaterial));
                headlight.transform.localPosition = new Vector3(frontX, y, z * halfWidth);
                headlight.transform.localScale = new Vector3(0.045f, 0.12f, 0.16f);
                DestroyCollider(headlight);

                var tailLight = Primitive("TailLight", PrimitiveType.Cube, parent, MaterialOr(tailLightMaterial, fireStationMaterial));
                tailLight.transform.localPosition = new Vector3(rearX, y, z * halfWidth);
                tailLight.transform.localScale = new Vector3(0.045f, 0.12f, 0.14f);
                DestroyCollider(tailLight);
            }
        }

        private Vector3 RandomBlockPosition(System.Random rng)
        {
            return new Vector3(RandomSigned(rng, 4.35f, 10.3f), 0f, RandomSigned(rng, 4.35f, 10.3f));
        }

        private Vector3 RandomRoadEdgePosition(System.Random rng)
        {
            var horizontalRoad = rng.NextDouble() < 0.5;
            var along = RandomRange(rng, -chunkSize * 0.42f, chunkSize * 0.42f);
            var edge = rng.NextDouble() < 0.5 ? -2.55f : 2.55f;
            return horizontalRoad ? new Vector3(along, 0f, edge) : new Vector3(edge, 0f, along);
        }

        private static float LampYawTowardRoad(Vector3 localPosition)
        {
            if (Mathf.Abs(localPosition.z) >= Mathf.Abs(localPosition.x))
            {
                return localPosition.z > 0f ? 90f : -90f;
            }

            return localPosition.x > 0f ? 180f : 0f;
        }

        private Vector3 RandomSidewalkPosition(System.Random rng)
        {
            var alongRoad = rng.NextDouble() < 0.5;
            var along = RandomRange(rng, -chunkSize * 0.42f, chunkSize * 0.42f);
            var edge = (rng.NextDouble() < 0.5 ? -1f : 1f) * RandomRange(rng, 3.05f, 3.95f);
            return alongRoad ? new Vector3(along, 0f, edge) : new Vector3(edge, 0f, along);
        }

        private bool ShouldCreateSpecial(Vector2Int coord)
        {
            if (coord == Vector2Int.zero)
            {
                return false;
            }

            return Mathf.Abs(Hash(coord)) % 8 == 0;
        }

        private int Hash(Vector2Int coord)
        {
            unchecked
            {
                var hash = seed;
                hash = hash * 73856093 ^ coord.x;
                hash = hash * 19349663 ^ coord.y;
                return hash;
            }
        }

        private static float RandomRange(System.Random rng, float min, float max)
        {
            return min + (float)rng.NextDouble() * (max - min);
        }

        private static float RandomSigned(System.Random rng, float min, float max)
        {
            return RandomRange(rng, min, max) * (rng.NextDouble() < 0.5 ? -1f : 1f);
        }

        private static Material MaterialOr(Material primary, Material fallback)
        {
            return primary != null ? primary : fallback;
        }

        private static void DestroyCollider(GameObject go)
        {
            var collider = go.GetComponent<Collider>();
            if (collider != null)
            {
#if UNITY_EDITOR
                if (!Application.isPlaying)
                {
                    UnityEngine.Object.DestroyImmediate(collider);
                    return;
                }
#endif
                UnityEngine.Object.Destroy(collider);
            }
        }

        private static GameObject Primitive(string name, PrimitiveType type, Transform parent, Material material)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent);

            var renderer = go.GetComponent<Renderer>();
            if (renderer != null && material != null)
            {
                renderer.sharedMaterial = material;
            }

            return go;
        }
    }
}
