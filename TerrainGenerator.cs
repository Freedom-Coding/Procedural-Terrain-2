using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TerrainGenerator : MonoBehaviour
{
    [SerializeField] int xSize = 10;
    [SerializeField] int zSize = 10;

    [SerializeField] int xOffset;
    [SerializeField] int zOffset;

    [SerializeField] float noiseScale = 0.03f;
    [SerializeField] float heightMultiplier = 7;

    [SerializeField] int octavesCount = 1;
    [SerializeField] float lacunarity = 2f;
    [SerializeField] float persistance = 0.5f;

    [SerializeField] List<Layer> terrainLayers = new List<Layer>();

    [SerializeField] Material mat;

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private MeshCollider meshCollider;
    private Mesh mesh;

    void Start()
    {

    }

    void Update()
    {

    }

    public void GenerateTerrain()
    {
        CreateMesh();
        GenerateMesh();
        GenerateTexture();
    }


    private void CreateMesh()
    {
        if (GetComponent<MeshFilter>() == null)
        {
            meshFilter = gameObject.AddComponent<MeshFilter>();
        }
        if (GetComponent<MeshRenderer>() == null)
        {
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
        }
        if (GetComponent<MeshCollider>() == null)
        {
            meshCollider = gameObject.AddComponent<MeshCollider>();
        }

        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        meshCollider = GetComponent<MeshCollider>();

        mesh = new Mesh();
        meshFilter.mesh = mesh;
        meshRenderer.material = mat;
    }

    private void GenerateMesh()
    {
        //VERTICES
        Vector3[] vertices = new Vector3[(xSize + 1) * (zSize + 1)];

        int i = 0;
        for(int z = 0; z <= zSize; z++)
        {
            for(int x = 0; x <= xSize; x++)
            {
                float yPos = 0;

                for (int o = 0; o < octavesCount; o++)
                {
                    float frequency = Mathf.Pow(lacunarity, o);
                    float amplitude = Mathf.Pow(persistance, o);

                    yPos += Mathf.PerlinNoise((x + xOffset) * noiseScale * frequency, (z + zOffset) * noiseScale * frequency) * amplitude;
                }

                yPos *= heightMultiplier;

                vertices[i] = new Vector3(x, yPos, z);
                i++;
            }
        }

        //TRIANGLES
        int[] triangles = new int[xSize * zSize * 6];

        int vertex = 0;
        int triangleIndex = 0;

        for(int z = 0; z < zSize; z++)
        {
            for(int x = 0; x < xSize; x++)
            {
                triangles[triangleIndex + 0] = vertex + 0;
                triangles[triangleIndex + 1] = vertex + xSize + 1;
                triangles[triangleIndex + 2] = vertex + 1;

                triangles[triangleIndex + 3] = vertex + 1;
                triangles[triangleIndex + 4] = vertex + xSize + 1;
                triangles[triangleIndex + 5] = vertex + xSize + 2;

                vertex++;
                triangleIndex += 6;
            }
            vertex++;
        }

        mesh.Clear();

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        meshCollider.sharedMesh = mesh;
    }

    private void GenerateTexture()
    {
        float minTerrainHeight = mesh.bounds.min.y + transform.position.y - 0.1f;
        float maxTerrainHeight = mesh.bounds.max.y + transform.position.y + 0.1f;

        mat.SetFloat("minTerrainHeight", minTerrainHeight);
        mat.SetFloat("maxTerrainHeight", maxTerrainHeight);

        //Layer count
        int layersCount = terrainLayers.Count;
        mat.SetInt("numTextures", layersCount);

        //Layer heights
        float[] heights = new float[layersCount];
        int index = 0;
        foreach(Layer l in terrainLayers)
        {
            heights[index] = l.startHeight;
            index++;
        }
        mat.SetFloatArray("terrainHeights", heights);

        //Layer textures
        Texture2DArray textures = new Texture2DArray(512, 512, layersCount, TextureFormat.RGBA32, true);

        for (int i = 0; i < layersCount; i++)
        {
            textures.SetPixels(terrainLayers[i].texture.GetPixels(), i);
        }

        textures.Apply();
        mat.SetTexture("terrainTextures", textures);
    }

    [System.Serializable]
    class Layer
    {
        public Texture2D texture;
        [Range(0, 1)] public float startHeight;
    }
}
