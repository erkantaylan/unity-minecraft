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

    private readonly byte[,,] voxelMap = new byte[VoxelData.ChunkWidth, VoxelData.ChunkHeight, VoxelData.ChunkWidth];

    private readonly World world;

    public ChunkCoordinate ChunkCoordinate;

    private int vertexIndex;

    public Chunk(ChunkCoordinate chunkCoordinate, World world)
    {
        ChunkCoordinate = chunkCoordinate;
        chunkObject = new GameObject();
        chunkObject.transform.position = new Vector3(
            ChunkCoordinate.X * VoxelData.ChunkWidth,
            0f,
            ChunkCoordinate.Z * VoxelData.ChunkWidth);

        meshRenderer = chunkObject.AddComponent<MeshRenderer>();
        meshFilter = chunkObject.AddComponent<MeshFilter>();
        this.world = world;

        chunkObject.transform.SetParent(this.world.transform);
        meshRenderer.material = this.world.material;

        chunkObject.name = $"{ChunkCoordinate.X}, {ChunkCoordinate.Z}";

        PopulateVoxelMap();
        CreateMeshData();
        CreateMesh();
    }

    public bool IsActive
    {
        get => chunkObject.activeSelf;
        set => chunkObject.SetActive(value);
    }

    private Vector3 Position => chunkObject.transform.position;

    private static bool IsVoxelInChunk(int x, int y, int z)
    {
        return 
            x >= 0 && x < VoxelData.ChunkWidth
         && y >= 0 && y < VoxelData.ChunkHeight
         && z >= 0 && z < VoxelData.ChunkWidth;
    }

    private void PopulateVoxelMap()
    {
        for (var y = 0; y < VoxelData.ChunkHeight; y++)
        {
            for (var x = 0; x < VoxelData.ChunkWidth; x++)
            {
                for (var z = 0; z < VoxelData.ChunkWidth; z++)
                {
                    voxelMap[x, y, z] = World.GetVoxelId(new Vector3(x, y, z) + Position);
                }
            }
        }
    }

    private void CreateMeshData()
    {
        for (var y = 0; y < VoxelData.ChunkHeight; y++)
        {
            for (var x = 0; x < VoxelData.ChunkWidth; x++)
            {
                for (var z = 0; z < VoxelData.ChunkWidth; z++)
                {
                    AddVoxelDataToChunk(new Vector3(x, y, z));
                }
            }
        }
    }

    public byte GetVoxelFromMap(Vector3 pos)
    {
        pos -= Position;

        return voxelMap[(int) pos.x, (int) pos.y, (int) pos.z];
    }

    private bool CheckVoxel(Vector3 pos)
    {
        int x = Mathf.FloorToInt(pos.x);
        int y = Mathf.FloorToInt(pos.y);
        int z = Mathf.FloorToInt(pos.z);

        // If position is outside of this chunk...
        if (!IsVoxelInChunk(x, y, z))
        {
            return world.blocktypes[World.GetVoxelId(pos + Position)].isSolid;
        }

        return world.blocktypes[voxelMap[x, y, z]].isSolid;
    }

    private void AddVoxelDataToChunk(Vector3 pos)
    {
        for (var p = 0; p < 6; p++)
        {
            if (!CheckVoxel(pos + VoxelData.FaceChecks[p]))
            {
                byte blockId = voxelMap[(int) pos.x, (int) pos.y, (int) pos.z];

                vertices.Add(pos + VoxelData.VoxelVerts[VoxelData.VoxelTris[p, 0]]);
                vertices.Add(pos + VoxelData.VoxelVerts[VoxelData.VoxelTris[p, 1]]);
                vertices.Add(pos + VoxelData.VoxelVerts[VoxelData.VoxelTris[p, 2]]);
                vertices.Add(pos + VoxelData.VoxelVerts[VoxelData.VoxelTris[p, 3]]);

                AddTexture(world.blocktypes[blockId].GetTextureId(p));

                triangles.Add(vertexIndex);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 1);
                triangles.Add(vertexIndex + 3);
                vertexIndex += 4;
            }
        }
    }

    public void CreateMesh()
    {
        var mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();

        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
    }

    private void AddTexture(int textureId)
    {
        float y = textureId / VoxelData.TextureAtlasSizeInBlocks;
        float x = textureId - y * VoxelData.TextureAtlasSizeInBlocks;

        x *= VoxelData.NormalizedBlockTextureSize;
        y *= VoxelData.NormalizedBlockTextureSize;

        y = 1f - y - VoxelData.NormalizedBlockTextureSize;

        uvs.Add(new Vector2(x, y));
        uvs.Add(new Vector2(x, y + VoxelData.NormalizedBlockTextureSize));
        uvs.Add(new Vector2(x + VoxelData.NormalizedBlockTextureSize, y));
        uvs.Add(new Vector2(x + VoxelData.NormalizedBlockTextureSize, y + VoxelData.NormalizedBlockTextureSize));
    }
}