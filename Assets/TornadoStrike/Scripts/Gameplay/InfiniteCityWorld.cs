using System.Collections.Generic;
using UnityEngine;

namespace TornadoStrike.Gameplay
{
    [DisallowMultipleComponent]
    public sealed class InfiniteCityWorld : MonoBehaviour
    {
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
        public Material roofMaterial;
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
        public Material warningStripeMaterial;
        public Material signMaterial;
        public Material pedestrianSkinMaterial;
        public Material pedestrianShirtMaterial;
        public Material pedestrianPantsMaterial;
        public Material pedestrianHairMaterial;
        public Material leafDarkMaterial;

        private readonly Dictionary<Vector2Int, GameObject> activeChunks = new Dictionary<Vector2Int, GameObject>();

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

            var sidewalkSize = chunkSize * 0.34f;
            var offsets = new[]
            {
                new Vector3(-chunkSize * 0.25f, 0.01f, -chunkSize * 0.25f),
                new Vector3(chunkSize * 0.25f, 0.01f, -chunkSize * 0.25f),
                new Vector3(-chunkSize * 0.25f, 0.01f, chunkSize * 0.25f),
                new Vector3(chunkSize * 0.25f, 0.01f, chunkSize * 0.25f)
            };

            for (var i = 0; i < offsets.Length; i++)
            {
                var sidewalk = Primitive($"Sidewalk_{i}", PrimitiveType.Cube, parent, sidewalkMaterial);
                sidewalk.transform.localPosition = offsets[i];
                sidewalk.transform.localScale = new Vector3(sidewalkSize, 0.08f, sidewalkSize);

                CreateSidewalkSeams(parent, offsets[i], sidewalkSize, i);
            }
        }

        private void CreateRoads(Transform parent)
        {
            var roadWidth = 4f;
            var curb = MaterialOr(curbMaterial, sidewalkMaterial);

            var horizontal = Primitive("Road_H", PrimitiveType.Cube, parent, asphaltMaterial);
            horizontal.transform.localPosition = new Vector3(0f, 0.04f, 0f);
            horizontal.transform.localScale = new Vector3(chunkSize, 0.08f, roadWidth);

            var vertical = Primitive("Road_V", PrimitiveType.Cube, parent, asphaltMaterial);
            vertical.transform.localPosition = new Vector3(0f, 0.05f, 0f);
            vertical.transform.localScale = new Vector3(roadWidth, 0.08f, chunkSize);

            var intersection = Primitive("Intersection", PrimitiveType.Cube, parent, asphaltMaterial);
            intersection.transform.localPosition = new Vector3(0f, 0.065f, 0f);
            intersection.transform.localScale = new Vector3(roadWidth + 0.25f, 0.08f, roadWidth + 0.25f);

            for (var i = -2; i <= 2; i++)
            {
                CreateRoadDash(parent, $"LaneDash_H_Left_{i}", new Vector3(i * 4f, 0.12f, -0.72f), new Vector3(1.8f, 0.025f, 0.08f));
                CreateRoadDash(parent, $"LaneDash_H_Right_{i}", new Vector3(i * 4f, 0.12f, 0.72f), new Vector3(1.8f, 0.025f, 0.08f));
                CreateRoadDash(parent, $"LaneDash_V_Left_{i}", new Vector3(-0.72f, 0.13f, i * 4f), new Vector3(0.08f, 0.025f, 1.8f));
                CreateRoadDash(parent, $"LaneDash_V_Right_{i}", new Vector3(0.72f, 0.13f, i * 4f), new Vector3(0.08f, 0.025f, 1.8f));
            }

            for (var side = -1; side <= 1; side += 2)
            {
                var curbH = Primitive($"Curb_H_{side}", PrimitiveType.Cube, parent, curb);
                curbH.transform.localPosition = new Vector3(0f, 0.16f, side * roadWidth * 0.5f);
                curbH.transform.localScale = new Vector3(chunkSize, 0.16f, 0.18f);

                var curbV = Primitive($"Curb_V_{side}", PrimitiveType.Cube, parent, curb);
                curbV.transform.localPosition = new Vector3(side * roadWidth * 0.5f, 0.17f, 0f);
                curbV.transform.localScale = new Vector3(0.18f, 0.16f, chunkSize);
            }

            CreateCrosswalk(parent, new Vector3(-2.65f, 0.15f, 0f), true);
            CreateCrosswalk(parent, new Vector3(2.65f, 0.15f, 0f), true);
            CreateCrosswalk(parent, new Vector3(0f, 0.16f, -2.65f), false);
            CreateCrosswalk(parent, new Vector3(0f, 0.16f, 2.65f), false);
        }

        private void CreateSidewalkSeams(Transform parent, Vector3 center, float size, int index)
        {
            var seamMaterial = MaterialOr(sidewalkLineMaterial, curbMaterial);
            var half = size * 0.5f;
            var seamY = center.y + 0.055f;

            for (var i = -1; i <= 1; i++)
            {
                var offset = i * size * 0.24f;

                var xSeam = Primitive($"SidewalkSeam_X_{index}_{i}", PrimitiveType.Cube, parent, seamMaterial);
                xSeam.transform.localPosition = new Vector3(center.x + offset, seamY, center.z);
                xSeam.transform.localScale = new Vector3(0.025f, 0.016f, half * 1.85f);
                DestroyCollider(xSeam);

                var zSeam = Primitive($"SidewalkSeam_Z_{index}_{i}", PrimitiveType.Cube, parent, seamMaterial);
                zSeam.transform.localPosition = new Vector3(center.x, seamY, center.z + offset);
                zSeam.transform.localScale = new Vector3(half * 1.85f, 0.016f, 0.025f);
                DestroyCollider(zSeam);
            }
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
            var specialCreated = false;
            if (ShouldCreateSpecial(coord))
            {
                CreateSpecialBuilding(parent, rng, coord);
                specialCreated = true;
            }

            var baseCount = specialCreated ? 5 + rng.Next(0, 3) : minBuildingsPerChunk + rng.Next(0, Mathf.Max(1, maxBuildingsPerChunk - minBuildingsPerChunk + 1));
            var buildingCount = Mathf.RoundToInt(baseCount * buildingDensity);
            for (var i = 0; i < buildingCount; i++)
            {
                var quadrantX = rng.NextDouble() < 0.5 ? -1f : 1f;
                var quadrantZ = rng.NextDouble() < 0.5 ? -1f : 1f;
                var x = quadrantX * RandomRange(rng, 4.8f, 9.5f);
                var z = quadrantZ * RandomRange(rng, 4.8f, 9.5f);
                var width = RandomRange(rng, 1.45f, 2.75f);
                var depth = RandomRange(rng, 1.35f, 2.55f);
                var height = RandomRange(rng, 1.1f, 3.35f);
                var material = i % 2 == 0 ? houseMaterialA : houseMaterialB;

                CreateHouse($"House_{i}", new Vector3(x, 0f, z), width, depth, height, parent, material, rng);
            }
        }

        private void CreateHouse(string id, Vector3 localPosition, float width, float depth, float height, Transform parent, Material material, System.Random rng)
        {
            var house = new GameObject(id);
            house.transform.SetParent(parent);
            house.transform.localPosition = localPosition;
            house.transform.localRotation = Quaternion.Euler(0f, rng.Next(0, 4) * 90f, 0f);

            var body = Primitive("Body", PrimitiveType.Cube, house.transform, material);
            body.transform.localPosition = Vector3.up * (height * 0.5f);
            body.transform.localScale = new Vector3(width, height, depth);
            DestroyCollider(body);

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

            AddFacadeBand(house.transform, "ConcreteBase", 0.13f, new Vector3(width * 1.06f, 0.18f, depth * 1.06f), MaterialOr(concreteMaterial, curbMaterial));
            AddFacadeBand(house.transform, "Cornice", height - 0.12f, new Vector3(width * 1.08f, 0.14f, depth * 1.08f), MaterialOr(concreteMaterial, roofMaterial));
            AddCornerColumns(house.transform, width, depth, height, MaterialOr(concreteMaterial, material));

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
            }

            if (rng.NextDouble() < 0.52)
            {
                var awning = Primitive("StorefrontAwning", PrimitiveType.Cube, house.transform, MaterialOr(signMaterial, roadLineMaterial));
                awning.transform.localPosition = new Vector3(0f, 0.86f, -depth * 0.59f);
                awning.transform.localScale = new Vector3(width * 0.72f, 0.12f, 0.32f);
                DestroyCollider(awning);
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

        private void CreateSpecialBuilding(Transform parent, System.Random rng, Vector2Int coord)
        {
            var specialType = Mathf.Abs(Hash(coord)) % 3;
            var name = specialType == 0 ? "PowerPlant" : specialType == 1 ? "PoliceStation" : "FireStation";
            var material = specialType == 0 ? powerPlantMaterial : specialType == 1 ? policeMaterial : fireStationMaterial;
            var slotKind = specialType == 0 ? SceneSlotKind.PowerPlant : specialType == 1 ? SceneSlotKind.PoliceStation : SceneSlotKind.FireStation;
            var key = specialType == 0 ? "slot_power_plant" : specialType == 1 ? "slot_police_station" : "slot_fire_station";
            var required = specialType == 0 ? 3.65f : specialType == 1 ? 2.95f : 3.05f;

            var slot = new GameObject($"{name}_Slot");
            slot.transform.SetParent(parent);
            slot.transform.localPosition = new Vector3(RandomSigned(rng, 5.4f, 7.8f), 0f, RandomSigned(rng, 5.4f, 7.8f));
            var sceneSlot = slot.AddComponent<SceneSlot>();
            sceneSlot.slotId = $"{name}_{coord.x}_{coord.y}";
            sceneSlot.kind = slotKind;
            sceneSlot.displayNameKey = key;
            sceneSlot.recommendedTier = Mathf.RoundToInt(required);

            var building = new GameObject(name);
            building.transform.SetParent(slot.transform);
            building.transform.localPosition = Vector3.zero;

            var body = Primitive("Body", PrimitiveType.Cube, building.transform, material);
            body.transform.localPosition = Vector3.up * 1.45f;
            body.transform.localScale = new Vector3(5.5f, 2.9f, 4.7f);
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

            AddSpecialBuildingFacade(building.transform, 5.5f, 4.7f, 2.9f, specialType);

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
            var propCount = Mathf.RoundToInt(streetPropsPerChunk * streetPropDensity);
            for (var i = 0; i < propCount; i++)
            {
                var roll = rng.NextDouble();
                if (roll < 0.42)
                {
                    CreateTree($"Tree_{i}", RandomBlockPosition(rng), parent, rng);
                }
                else if (roll < 0.72)
                {
                    CreateLampPost($"LampPost_{i}", RandomRoadEdgePosition(rng), parent);
                }
                else if (roll < 0.88)
                {
                    CreateHydrant($"Hydrant_{i}", RandomRoadEdgePosition(rng), parent);
                }
                else
                {
                    CreateBench($"Bench_{i}", RandomBlockPosition(rng), parent, rng);
                }
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

        private void CreateLampPost(string id, Vector3 localPosition, Transform parent)
        {
            var lamp = new GameObject(id);
            lamp.transform.SetParent(parent);
            lamp.transform.localPosition = localPosition;

            var basePlate = Primitive("BasePlate", PrimitiveType.Cylinder, lamp.transform, MaterialOr(metalMaterial, lampPoleMaterial));
            basePlate.transform.localPosition = Vector3.up * 0.06f;
            basePlate.transform.localScale = new Vector3(0.18f, 0.06f, 0.18f);
            DestroyCollider(basePlate);

            var pole = Primitive("Pole", PrimitiveType.Cylinder, lamp.transform, lampPoleMaterial);
            pole.transform.localPosition = Vector3.up * 0.8f;
            pole.transform.localScale = new Vector3(0.08f, 0.8f, 0.08f);
            DestroyCollider(pole);

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

        private void CreateBench(string id, Vector3 localPosition, Transform parent, System.Random rng)
        {
            var bench = new GameObject(id);
            bench.transform.SetParent(parent);
            bench.transform.localPosition = localPosition;
            bench.transform.localRotation = Quaternion.Euler(0f, rng.Next(0, 4) * 90f, 0f);

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
            var count = Mathf.RoundToInt(pedestriansPerChunk * pedestrianDensity);
            for (var i = 0; i < count; i++)
            {
                var position = RandomSidewalkPosition(rng);
                CreatePedestrian($"Pedestrian_{i}", position, Quaternion.Euler(0f, RandomRange(rng, 0f, 360f), 0f), parent, rng);
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

            var head = Primitive("Head", PrimitiveType.Sphere, pedestrian.transform, pedestrianSkinMaterial);
            head.transform.localPosition = Vector3.up * 1.2f;
            head.transform.localScale = Vector3.one * 0.22f;
            DestroyCollider(head);

            var hair = Primitive("Hair", PrimitiveType.Sphere, pedestrian.transform, MaterialOr(pedestrianHairMaterial, blackMaterial));
            hair.transform.localPosition = new Vector3(0f, 1.31f, 0f);
            hair.transform.localScale = new Vector3(0.23f, 0.12f, 0.23f);
            DestroyCollider(hair);

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
            var baseCount = 4 + rng.Next(0, 4);
            var vehicleCount = Mathf.RoundToInt(baseCount * vehicleDensity);
            for (var i = 0; i < vehicleCount; i++)
            {
                var isBus = rng.NextDouble() < 0.24;
                var onHorizontal = rng.NextDouble() < 0.5;
                var laneOffset = rng.NextDouble() < 0.5 ? -0.95f : 0.95f;
                var along = RandomRange(rng, -chunkSize * 0.42f, chunkSize * 0.42f);
                var localPosition = onHorizontal ? new Vector3(along, 0f, laneOffset) : new Vector3(laneOffset, 0f, along);
                var rotation = Quaternion.Euler(0f, onHorizontal ? 90f : 0f, 0f);

                CreateVehicle(isBus ? $"Bus_{i}" : $"Car_{i}", localPosition, rotation, parent, isBus);
            }
        }

        private void CreateVehicle(string id, Vector3 localPosition, Quaternion localRotation, Transform parent, bool isBus)
        {
            var vehicle = new GameObject(id);
            vehicle.transform.SetParent(parent);
            vehicle.transform.localPosition = localPosition;
            vehicle.transform.localRotation = localRotation;

            var body = Primitive("Body", PrimitiveType.Cube, vehicle.transform, isBus ? busMaterial : carMaterial);
            body.transform.localPosition = Vector3.up * (isBus ? 0.46f : 0.3f);
            body.transform.localScale = isBus ? new Vector3(3.45f, 0.82f, 1.16f) : new Vector3(1.55f, 0.52f, 0.9f);
            DestroyCollider(body);

            var chassis = Primitive("LowerChassis", PrimitiveType.Cube, vehicle.transform, MaterialOr(metalMaterial, blackMaterial));
            chassis.transform.localPosition = Vector3.up * (isBus ? 0.2f : 0.18f);
            chassis.transform.localScale = isBus ? new Vector3(3.55f, 0.18f, 1.2f) : new Vector3(1.62f, 0.14f, 0.94f);
            DestroyCollider(chassis);

            var cabin = Primitive("Cabin", PrimitiveType.Cube, vehicle.transform, glassMaterial);
            cabin.transform.localPosition = Vector3.up * (isBus ? 0.95f : 0.68f) + Vector3.right * (isBus ? 0.25f : -0.1f);
            cabin.transform.localScale = isBus ? new Vector3(2.75f, 0.38f, 1.05f) : new Vector3(0.82f, 0.38f, 0.78f);
            DestroyCollider(cabin);

            if (isBus)
            {
                for (var x = -1; x <= 1; x++)
                {
                    AddVehicleWindow(vehicle.transform, new Vector3(x * 0.82f, 0.96f, -0.61f), new Vector3(0.48f, 0.28f, 0.04f));
                    AddVehicleWindow(vehicle.transform, new Vector3(x * 0.82f, 0.96f, 0.61f), new Vector3(0.48f, 0.28f, 0.04f));
                }

                var door = Primitive("BusDoor", PrimitiveType.Cube, vehicle.transform, MaterialOr(carGlassMaterial, glassMaterial));
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
                var hood = Primitive("Hood", PrimitiveType.Cube, vehicle.transform, carMaterial);
                hood.transform.localPosition = new Vector3(0.52f, 0.48f, 0f);
                hood.transform.localScale = new Vector3(0.5f, 0.12f, 0.82f);
                DestroyCollider(hood);

                var trunk = Primitive("Trunk", PrimitiveType.Cube, vehicle.transform, carMaterial);
                trunk.transform.localPosition = new Vector3(-0.58f, 0.47f, 0f);
                trunk.transform.localScale = new Vector3(0.42f, 0.11f, 0.78f);
                DestroyCollider(trunk);

                AddVehicleWindow(vehicle.transform, new Vector3(0.08f, 0.72f, -0.47f), new Vector3(0.58f, 0.24f, 0.04f));
                AddVehicleWindow(vehicle.transform, new Vector3(0.08f, 0.72f, 0.47f), new Vector3(0.58f, 0.24f, 0.04f));
            }

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
