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
        public int minBuildingsPerChunk = 8;
        public int maxBuildingsPerChunk = 13;
        public int streetPropsPerChunk = 12;

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
            CreateBuildings(chunk.transform, rng, coord);
            CreateStreetProps(chunk.transform, rng);
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

            var door = Primitive("Door", PrimitiveType.Cube, house.transform, MaterialOr(blackMaterial, roofMaterial));
            door.transform.localPosition = new Vector3(0f, 0.38f, -depth * 0.51f);
            door.transform.localScale = new Vector3(width * 0.22f, 0.72f, 0.05f);
            DestroyCollider(door);

            var rows = Mathf.Clamp(Mathf.RoundToInt(height / 0.85f), 1, 4);
            for (var row = 0; row < rows; row++)
            {
                var y = 0.72f + row * 0.68f;
                AddWindow(house.transform, new Vector3(-width * 0.28f, y, -depth * 0.52f), new Vector3(width * 0.18f, 0.24f, 0.04f));
                AddWindow(house.transform, new Vector3(width * 0.28f, y, -depth * 0.52f), new Vector3(width * 0.18f, 0.24f, 0.04f));
                AddWindow(house.transform, new Vector3(-width * 0.28f, y, depth * 0.52f), new Vector3(width * 0.18f, 0.24f, 0.04f));
                AddWindow(house.transform, new Vector3(width * 0.28f, y, depth * 0.52f), new Vector3(width * 0.18f, 0.24f, 0.04f));
            }

            if (rng.NextDouble() < 0.45)
            {
                var tank = Primitive("RoofTank", PrimitiveType.Cylinder, house.transform, MaterialOr(whiteMaterial, markerMaterial));
                tank.transform.localPosition = new Vector3(width * 0.22f, height + 0.42f, depth * 0.12f);
                tank.transform.localScale = new Vector3(0.22f, 0.18f, 0.22f);
                DestroyCollider(tank);
            }
        }

        private void AddWindow(Transform parent, Vector3 localPosition, Vector3 localScale)
        {
            var window = Primitive("Window", PrimitiveType.Cube, parent, glassMaterial);
            window.transform.localPosition = localPosition;
            window.transform.localScale = localScale;
            DestroyCollider(window);
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

            if (specialType == 0)
            {
                AddStack(building.transform, new Vector3(-1.7f, 3.3f, 1.25f));
                AddStack(building.transform, new Vector3(1.7f, 3.15f, 1.25f));
                AddSign(building.transform, "BoltSign", new Vector3(0f, 2.9f, -2.38f), new Vector3(1.2f, 0.38f, 0.08f), roadLineMaterial);
            }
            else if (specialType == 1)
            {
                AddSign(building.transform, "PoliceSign", new Vector3(0f, 3.05f, -2.38f), new Vector3(2.2f, 0.42f, 0.08f), whiteMaterial);
                AddSign(building.transform, "BlueLight", new Vector3(-0.55f, 3.48f, -0.9f), new Vector3(0.38f, 0.2f, 0.38f), policeMaterial);
                AddSign(building.transform, "RedLight", new Vector3(0.55f, 3.48f, -0.9f), new Vector3(0.38f, 0.2f, 0.38f), fireStationMaterial);
            }
            else
            {
                AddSign(building.transform, "GarageDoorA", new Vector3(-1.2f, 0.88f, -2.38f), new Vector3(1.25f, 1.35f, 0.08f), whiteMaterial);
                AddSign(building.transform, "GarageDoorB", new Vector3(1.2f, 0.88f, -2.38f), new Vector3(1.25f, 1.35f, 0.08f), whiteMaterial);
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

            var canopy = Primitive("Canopy", PrimitiveType.Sphere, tree.transform, treeCanopyMaterial);
            canopy.transform.localPosition = Vector3.up * (height * 0.9f);
            canopy.transform.localScale = Vector3.one * RandomRange(rng, 0.85f, 1.15f);
            DestroyCollider(canopy);

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

            var pole = Primitive("Pole", PrimitiveType.Cylinder, lamp.transform, lampPoleMaterial);
            pole.transform.localPosition = Vector3.up * 0.8f;
            pole.transform.localScale = new Vector3(0.08f, 0.8f, 0.08f);
            DestroyCollider(pole);

            var light = Primitive("Light", PrimitiveType.Sphere, lamp.transform, lampLightMaterial);
            light.transform.localPosition = new Vector3(0.18f, 1.68f, 0f);
            light.transform.localScale = new Vector3(0.28f, 0.22f, 0.28f);
            DestroyCollider(light);

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

            var cabin = Primitive("Cabin", PrimitiveType.Cube, vehicle.transform, glassMaterial);
            cabin.transform.localPosition = Vector3.up * (isBus ? 0.95f : 0.68f) + Vector3.right * (isBus ? 0.25f : -0.1f);
            cabin.transform.localScale = isBus ? new Vector3(2.75f, 0.38f, 1.05f) : new Vector3(0.82f, 0.38f, 0.78f);
            DestroyCollider(cabin);

            for (var x = -1; x <= 1; x += 2)
            {
                for (var z = -1; z <= 1; z += 2)
                {
                    var wheel = Primitive("Wheel", PrimitiveType.Cylinder, vehicle.transform, MaterialOr(blackMaterial, asphaltMaterial));
                    wheel.transform.localPosition = new Vector3(x * (isBus ? 1.25f : 0.55f), 0.18f, z * 0.5f);
                    wheel.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                    wheel.transform.localScale = new Vector3(0.18f, 0.08f, 0.18f);
                    DestroyCollider(wheel);
                }
            }

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
