using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[ExecuteInEditMode]
public class WorldGenerator : MonoBehaviour
{
    public static WorldGenerator Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
    }

    public WorldBiom worldBiom;
    public Vector3Int chunkSizes;
    public Transform ChunkParent;
    public Material chunkGroundMaterial;
    public Material chunkWaterMaterial;

    [Space(30)]
    public bool generate = false;
    public bool deleteChunks = false;

    private float[,] chunkGroundHeight;
    private int[,,] chunkObjectParts;
    private int lastChunkId;
    private Vector2 chunkPositionOffset;
    private Vector3 lastChunkPosition = Vector3.zero;
    private Vector3 lastPlayerPosition = Vector3.zero;

    private int chunkToPlayer = 10;

    private Mesh GetGroundMesh(WorldBiom biom, Vector3 positionOffset)
    {
        var myMesh = new MyMeshData();

        List<MyVector3> vertices = new List<MyVector3>();
        List<int> triangles = new List<int>();
        List<MyVector2> uv = new List<MyVector2>();
        List<MyVector3> normals = new List<MyVector3>();
        for (int i = 0; i <= chunkSizes.x; i++)
        {
            for (int j = 0; j < chunkSizes.z; j++)
            {
                float ii = (float)i / (chunkSizes.x - 1);
                float jj = (float)j / (chunkSizes.z - 1);

                float height = biom.groundHeight.Evaluate(jj);
                height += Mathf.PerlinNoise(ii * 10 + chunkPositionOffset.x + positionOffset.z, jj * 10 + chunkPositionOffset.y + positionOffset.x) * biom.groundHeightVariation.Evaluate(jj);
                height = Mathf.Max(biom.minGroundHeight.Evaluate(jj), height);
                if(i != chunkSizes.x)
                    chunkGroundHeight[i, j] = height;

                height *= chunkSizes.y;
                vertices.Add(new MyVector3(j, height, i));
                uv.Add(new MyVector2(jj, ii));
                normals.Add(Vector3.up);

                int pos = vertices.Count - 1;

                if (i != 0 && j != 0)
                {
                    triangles.Add(pos);
                    triangles.Add(pos - chunkSizes.z - 1);
                    triangles.Add(pos - 1);

                    triangles.Add(pos);
                    triangles.Add(pos - chunkSizes.z);
                    triangles.Add(pos - chunkSizes.z - 1);
                }
            }
        }

        myMesh.vertices = vertices.ToArray();
        myMesh.triangles = triangles.ToArray();
        myMesh.uv = uv.ToArray();
        myMesh.normals = normals.ToArray();

        var mesh = (Mesh)myMesh;
        mesh.RecalculateNormals();
        return mesh;
    }

    private void CreateGround(WorldBiom biom, Transform chunk)
    {
        var ground = new GameObject("ground").transform;
        ground.parent = chunk;
        ground.localPosition = Vector3.zero;

        var meshRenderer = ground.gameObject.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = chunkGroundMaterial;
        var meshFilter = ground.gameObject.AddComponent<MeshFilter>();
        meshFilter.mesh = GetGroundMesh(biom, chunk.localPosition);
        ground.gameObject.AddComponent<MeshCollider>().sharedMesh = meshFilter.sharedMesh;

        var water = new GameObject("water").transform;
        water.parent = chunk;
        water.localPosition = Vector3.zero;
        meshRenderer = water.gameObject.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = chunkWaterMaterial;
        meshFilter = water.gameObject.AddComponent<MeshFilter>();
        meshFilter.mesh = new MyMeshData()
        {
            vertices = new[]
            {
                new MyVector3(0, 0, 0),
                new MyVector3(chunkSizes.z, 0, 0),
                new MyVector3(chunkSizes.z, 0, chunkSizes.x),
                new MyVector3(0, 0, chunkSizes.x),
            },
            triangles = new[]
            {
                0, 2, 1,
                0, 3, 2
            }
        };
    }

    private WorldObjectSpace InstantiateWorldObject(WorldObjectSpace prefab, Transform parent = null)
    {
        var newObj = Instantiate(prefab, parent);
        for (int i = 0; i < newObj.ancors.Length; i++)
        {
            for (int j = 0; j < newObj.ancors[i].exitPoints.Length; j++)
            {
                for (int k = 0; k < newObj.ancors[i].exitPoints[j].exitObjects.Length; k++)
                    if (newObj.ancors[i].exitPoints[j].exitObjects[k].exitObject == newObj)
                        newObj.ancors[i].exitPoints[j].exitObjects[k].exitObject = prefab.ancors[i].exitPoints[j].exitObjects[k].exitObject;
            }
        }
        return newObj;
    }

    private void ExtendBiomObject(WorldObjectSpace obj, int chainCount, int minChainCount, Transform objP, Transform lastExitPointoint = null)
    {
        foreach (var offsets in obj.parts)
            offsets.ApplyRandom();
        int enterPointIndex = 0;
        if (lastExitPointoint != null)
        {
            string exitName = lastExitPointoint.name.Split(' ').Last();
            for (int i = 0; i < obj.ancors.Length; i++)
                if (obj.ancors[i].entryPoint.transform.name.EndsWith(exitName))
                {
                    enterPointIndex = i;
                    break;
                }
            var entryPoint = obj.ancors[enterPointIndex].entryPoint;
            obj.transform.rotation *= Quaternion.Inverse(entryPoint.rotation * Quaternion.Inverse(lastExitPointoint.rotation));
            obj.transform.position += lastExitPointoint.position - entryPoint.position;
        }
        obj.transform.parent = objP;

        if (chainCount < 0)
            return;
        if (obj.ancors == null || obj.ancors.Length == 0)
            return;

        for (int i = 0; i < obj.ancors[enterPointIndex].exitPoints.Length; i++)
        {
            var exit = obj.ancors[enterPointIndex].exitPoints[i];
            if (exit.exitAncor == null)
                continue;
            if (exit.exitObjects.Length == 0)
                continue;
            if (Random.value > exit.probability && (i > 0 || minChainCount < 1))
                continue;

            var maxChainCount = chainCount;
            if (exit.maxChainLenght > 0 && exit.maxChainLenght < maxChainCount)
                maxChainCount = exit.maxChainLenght;
            if (maxChainCount <= 0 && exit.forceCreation)
                maxChainCount = 1;

            var minChainCount2 = minChainCount;
            if (exit.minChainLenght > minChainCount2)
                minChainCount2 = exit.minChainLenght;

            var maxValue = 0f;
            foreach (var pos in exit.exitObjects)
                maxValue += pos.probability;
            var prob = Random.value * maxValue;
            foreach (var pos in exit.exitObjects)
            {
                if (prob > pos.probability)
                {
                    prob -= pos.probability;
                    continue;
                }
                var newObj = InstantiateWorldObject(pos.exitObject, exit.exitAncor);
                ExtendBiomObject(newObj, maxChainCount - 1, minChainCount2 - 1, objP, exit.exitAncor);
                break;
            }
        }
    }

    private void CreateObject(WorldBiom.BiomObjects obj, int sliceIndex, Transform objP)
    {
        obj.slicesUntillCanCreate = Random.Range(obj.minSlicesUntillNext, obj.maxSlicesUntillNext);

        var newObj = InstantiateWorldObject(obj.biomObject, objP);
        var pos = new Vector3(Random.Range(obj.minPosition.x, obj.maxPosition.x), Random.Range(obj.minPosition.y, obj.maxPosition.y), 0);
        pos.z = sliceIndex;
        pos.y *= chunkSizes.y;
        pos.x *= chunkSizes.z;
        pos.y = Mathf.Max(pos.y, chunkGroundHeight[sliceIndex, (int)pos.x] * chunkSizes.y);
        newObj.transform.localPosition = pos;
        if (obj.flipXAxes)
            newObj.transform.localScale = new Vector3(-newObj.transform.localScale.x,
                                                        newObj.transform.localScale.y,
                                                        newObj.transform.localScale.z);

        ExtendBiomObject(newObj, 100, 0, objP);
    }

    private void CreateWorldObjects(WorldBiom biom, Transform chunk)
    {
        var objectsP = new GameObject("objects").transform;
        objectsP.parent = chunk;
        objectsP.localPosition = Vector3.zero;

        foreach (var obj in biom.biomObjects)
            obj.slicesUntillCanCreate = -1;

        for (int i = 0; i < chunkSizes.x; i++)
        {
            foreach (var obj in biom.biomObjects)
            {
                if (obj.slicesUntillCanCreate > 0)
                {
                    obj.slicesUntillCanCreate--;
                    continue;
                }

                var overProbability = (int)obj.probabilityPerSlice;
                if (overProbability > 0)
                    for (int _ = Random.Range(0, overProbability + 1); _ > -1 ; _--)
                        CreateObject(obj, i, objectsP);
                var underProbability = obj.probabilityPerSlice - overProbability;
                if (Random.value < underProbability)
                    CreateObject(obj, i, objectsP);
            }
        }
    }

    private Transform CreateChunk(WorldBiom biom, Vector3 positionOffset)
    {
        var chunk = new GameObject("chunk " + lastChunkId++).transform;
        chunk.parent = ChunkParent;

        CreateGround(biom, chunk);
        CreateWorldObjects(biom, chunk);

        chunk.localPosition = positionOffset;
        return chunk;
    }

    public void CheckNewPlayerPosition(Vector3 position)
    {
        lastPlayerPosition = position;
        if (Vector3.Distance(lastPlayerPosition, lastChunkPosition) < chunkSizes.x * chunkToPlayer)
        {
            GenerateNextChunk();
            var lastChunk = ChunkParent.GetChild(0);
            if (Vector3.Distance(lastPlayerPosition, lastChunk.position) > chunkSizes.x * chunkToPlayer)
                Destroy(lastChunk.gameObject);
        }
    }

    private void GenerateNextChunk()
    {
        lastChunkPosition.z += chunkSizes.x;
        CreateChunk(worldBiom, lastChunkPosition);
        chunkPositionOffset.y += chunkSizes.x;
    }

    void Start()
    {
        //while (ChunkParent.childCount > 0)
        //{
        //    var c = ChunkParent.GetChild(0);
        //    Debug.Log(c);
        //    if (Application.platform == RuntimePlatform.WindowsEditor && !Application.isPlaying)
        //        DestroyImmediate(c.gameObject);
        //    else
        //        Destroy(c.gameObject);
        //}
        generate = true;

        chunkObjectParts = new int[chunkSizes.x, chunkSizes.y, chunkSizes.z];
        lastChunkId = 0;

        chunkGroundHeight = new float[chunkSizes.x, chunkSizes.z];

        lastChunkPosition = new Vector3(0, 0, -chunkSizes.x);
        //CreateChunk(worldBiom);
    }


    void Update()
    {
        if (generate)
        {
            generate = false;
            for(int i=0;i<ChunkParent.childCount;i++)
            {
                var c = ChunkParent.GetChild(i);
                if (Application.platform == RuntimePlatform.WindowsEditor && !Application.isPlaying)
                {
                    DestroyImmediate(c.gameObject);
                    i--;
                }
                else
                    Destroy(c.gameObject);
            }

            chunkPositionOffset = new Vector2(Random.value, Random.value) * 100;
            chunkGroundHeight = new float[chunkSizes.x, chunkSizes.z];
            lastChunkPosition = new Vector3(0, 0, -chunkSizes.x);
            for (int i = 0; i < 10; i++)
            {
                GenerateNextChunk();
            }
        }
        if (deleteChunks)
        {
            deleteChunks = false;
            foreach (Transform c in ChunkParent)
                if (Application.platform == RuntimePlatform.WindowsEditor)
                    DestroyImmediate(c.gameObject);
                else
                    Destroy(c.gameObject);
        }
    }

}
