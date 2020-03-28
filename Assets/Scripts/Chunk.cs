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

    public ChunkCoordinate Coordinate;

    private int vertexIndex;

    public Chunk(World world, ChunkCoordinate coordinate)
    {
        this.world = world;
        Coordinate = coordinate;
        chunkObject = new GameObject();

        chunkObject.transform.position = new Vector3(
            coordinate.X * VoxelData.ChunkWidth,
            0f,
            coordinate.Z * VoxelData.ChunkWidth);

        meshFilter = chunkObject.AddComponent<MeshFilter>();
        meshRenderer = chunkObject.AddComponent<MeshRenderer>();

        chunkObject.transform.SetParent(world.transform);
        meshRenderer.material = world.material;

        chunkObject.name = $"Chunk({coordinate.X}, {coordinate.Z})";

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
                    map[x, y, z] = World.GetVoxelId(new Vector3Int(x, y, z) + Position);
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
        Debug.Log($"{nameof(position)}:{position}");
        Debug.Log($"{nameof(Position)}:{Position}");

        if (!IsVoxelInChunk(position.x, position.y, position.z))
        {
            return world.blockTypes[World.GetVoxelId(position + Position)].isSolid;
        }

        return world.blockTypes[voxelMap[position.x, position.y, position.z]].isSolid;
    }

    private static bool IsVoxelInChunk(int x, int y, int z)
    {
        // return x >= 0 && x < VoxelData.ChunkWidth
        //  && y >= 0 && y < VoxelData.ChunkHeight
        //  && z >= 0 && z < VoxelData.ChunkWidth;

        return x >= 0 && x < VoxelData.ChunkWidth
            && y >= 0 && y < VoxelData.ChunkHeight
            && z >= 0 && z < VoxelData.ChunkWidth;
    }


    private void AddVoxelDataToChunk(Vector3Int position)
    {
        int firstDimention = VoxelData.VoxelTriangles.GetLength(0);
        int secondDimentin = VoxelData.VoxelTriangles.GetLength(1);
        
        for (var i = 0; i < firstDimention; i++)
        {
            Vector3Int faceCheck = position + VoxelData.FaceChecks[i];
            
            if (!CheckVoxel(faceCheck))
            {
                for (var j = 0; j < secondDimentin; j++)
                {
                    vertices.Add(position + VoxelData.VoxelVertices[VoxelData.VoxelTriangles[i, j]]);
                }

                byte blockId = voxelMap[position.x, position.y, position.z];
                int textureId = world.blockTypes[blockId].GetTextureId(i);
                
                AddTexture(textureId);
                AddTriangles();
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
        vertexIndex += 4;
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