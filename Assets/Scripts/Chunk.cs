using System.Collections.Generic;
using Models;
using UnityEngine;

public class Chunk
{
    private readonly GameObject chunkObject;

    private readonly MeshFilter meshFilter;

    private readonly MeshRenderer meshRenderer;
    private readonly List<int> triangles = new List<int>();
    private readonly List<Vector2> uvs = new List<Vector2>();
    private readonly List<Vector3> vertices = new List<Vector3>();

    private readonly byte[,,] voxelMap;

    private readonly World world;

    public ChunkCoordinates Coordinates;

    private int vertexIndex;

    public Chunk(World world, ChunkCoordinates coordinates)
    {
        this.world = world;
        Coordinates = coordinates;
        chunkObject = new GameObject();
        
        chunkObject.transform.position = new Vector3(
            coordinates.X * VoxelData.ChunkWidth,
            0f,
            coordinates.Z * VoxelData.ChunkWidth);
        
        meshFilter = chunkObject.AddComponent<MeshFilter>();
        meshRenderer = chunkObject.AddComponent<MeshRenderer>();

        meshRenderer.material = world.material;
        chunkObject.transform.SetParent(world.transform);

        chunkObject.name = $"Chunk({coordinates.X}, {coordinates.Z})";

        voxelMap = PopulateVoxelMap();
        CreateMeshData();
        CreateMesh();
    }

    public bool IsActive
    {
        get => chunkObject.activeSelf;
        set => chunkObject.SetActive(value);
    }

    public Vector3Int Position => Vector3Int.CeilToInt(chunkObject.transform.position);


    private byte[,,] PopulateVoxelMap()
    {
        var map = new byte[VoxelData.ChunkWidth, VoxelData.ChunkHeight, VoxelData.ChunkWidth];
        for (var y = 0; y < VoxelData.ChunkHeight; y++)
        {
            for (var x = 0; x < VoxelData.ChunkWidth; x++)
            {
                for (var z = 0; z < VoxelData.ChunkWidth; z++)
                {
                    map[x, y, z] = world.GetVoxelId(new Vector3Int(x, y, z) + Position);
                }
            }
        }

        return map;
    }

    private void CreateMeshData()
    {
        for (var y = 0; y < VoxelData.ChunkHeight; y++)
        {
            for (var x = 0; x < VoxelData.ChunkWidth; x++)
            {
                for (var z = 0; z < VoxelData.ChunkWidth; z++)
                {
                    AddVoxelDataToChunk(new Vector3Int(x, y, z));
                }
            }
        }
    }

    private bool CheckVoxel(Vector3Int position)
    {
        int x = position.x;
        int y = position.y;
        int z = position.z;
        Debug.Log($"{nameof(position)}:{position}");
        Debug.Log($"{nameof(Position)}:{Position}");

        if (!IsVoxelInChunk(x, y, z))
        {
            //return world.blockTypes[World.GetVoxelId(position + Position)].isSolid;

            byte voxelId = world.GetVoxelId(position + Position);


            byte index0 = voxelMap[x, y, z];


            Debug.Log($"{nameof(voxelId)}:{voxelId}");
            Debug.Log($"{nameof(index0)}:{index0}");
            Debug.Log($"{nameof(world.blockTypes)} lenght:{world.blockTypes.Length}");


            return world.blockTypes[index0].isSolid;
        }

        return world.blockTypes[voxelMap[x, y, z]].isSolid;
    }

    private static bool IsVoxelInChunk(int x, int y, int z)
    {
        // return x >= 0 && x < VoxelData.ChunkWidth
        //  && y >= 0 && y < VoxelData.ChunkHeight
        //  && z >= 0 && z < VoxelData.ChunkWidth;

        if (x < 0 || x > VoxelData.ChunkWidth - 1
         || y < 0 || y > VoxelData.ChunkHeight - 1
         || z < 0 || z > VoxelData.ChunkWidth - 1)
        {
            return false;
        }

        return true;
    }


    private void AddVoxelDataToChunk(Vector3Int position)
    {
        int firstDimention = VoxelData.VoxelTriangles.GetLength(0);
        int secondDimentin = VoxelData.VoxelTriangles.GetLength(1);
        Debug.Log($"{nameof(firstDimention)}:{firstDimention}");
        for (var i = 0; i < firstDimention; i++)
        {
            Vector3Int faceCheck = position + VoxelData.FaceChecks[i];
            Debug.Log($"{nameof(faceCheck)}:{faceCheck}");
            if (!CheckVoxel(faceCheck))
            {
                for (var j = 0; j < secondDimentin; j++)
                {
                    vertices.Add(position + VoxelData.VoxelVertices[VoxelData.VoxelTriangles[i, j]]);
                }

                byte blockId = voxelMap[position.x, position.y, position.z];
                AddTexture(world.blockTypes[blockId].GetTextureId(i));
                AddTriangles();
                vertexIndex += 4;
            }
        }
    }

    private void AddTriangles()
    {
        triangles.Add(vertexIndex);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 2);
        triangles.Add(vertexIndex + 2);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 3);
    }

    private void CreateMesh()
    {
        var mesh = new Mesh
        {
            vertices = vertices.ToArray(), triangles = triangles.ToArray(), uv = uvs.ToArray()
        };

        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
    }

    private void AddTexture(int textureId)
    {
        // ReSharper disable once PossibleLossOfFraction
        float y = textureId / VoxelData.TextureAtlasSizeBlock;
        float x = textureId % 4 * VoxelData.NormalizedBlockTextureSize;

        y *= VoxelData.NormalizedBlockTextureSize;

        y = 1f - y - VoxelData.NormalizedBlockTextureSize;

        uvs.Add(new Vector2(x, y));
        uvs.Add(new Vector2(x, y + VoxelData.NormalizedBlockTextureSize));
        uvs.Add(new Vector2(x + VoxelData.NormalizedBlockTextureSize, y));
        uvs.Add(new Vector2(x + VoxelData.NormalizedBlockTextureSize, y + VoxelData.NormalizedBlockTextureSize));
    }
}