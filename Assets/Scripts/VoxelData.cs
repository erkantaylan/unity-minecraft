using UnityEngine;

public static class VoxelData
{
    public const int ChunkWidth = 5;
    public const int ChunkHeight = 15;
    public const int WorldSizeInChunks = 100;

    public static int WorldSizeInVoxels => WorldSizeInChunks * ChunkWidth;

    public const int TextureAtlasSizeBlock = 4;

    public const int ViewDistanceInChunks = 5;
    
    public static readonly Vector3[] VoxelVertices =
    {
        new Vector3(0.0f, 0.0f, 0.0f),
        new Vector3(1.0f, 0.0f, 0.0f),
        new Vector3(1.0f, 1.0f, 0.0f),
        new Vector3(0.0f, 1.0f, 0.0f),
        new Vector3(0.0f, 0.0f, 1.0f),
        new Vector3(1.0f, 0.0f, 1.0f),
        new Vector3(1.0f, 1.0f, 1.0f),
        new Vector3(0.0f, 1.0f, 1.0f)
    };

    public static readonly int[,] VoxelTriangles =
    {
        //back, front, top, bottom, left, right
        
        {
            0,
            3,
            1,
            2
        }, //back face
        {
            5,
            6,
            4,
            7
        }, //front face
        {
            3,
            7,
            2,
            6
        }, // top face
        {
            1,
            5,
            0,
            4
        }, //bottom face
        {
            4,
            7,
            0,
            3
        }, // left face
        {
            1,
            2,
            5,
            6
        } //right face
    };

    public static readonly Vector2[] VoxelUvs =
    {
        new Vector2(0.0f, 0.0f),
        new Vector2(0.0f, 1.0f),
        new Vector2(1.0f, 0.0f),
        new Vector2(1.0f, 1.0f)
    };

    public static readonly Vector3Int[] FaceChecks =
    {
        new Vector3Int(0, 0, -1),
        new Vector3Int(0, 0, 1),
        new Vector3Int(0, 1, 0),
        new Vector3Int(0, -1, 0),
        new Vector3Int(-1, 0, 0),
        new Vector3Int(1, 0, 0)
    };

    public const float NormalizedBlockTextureSize = 1f / TextureAtlasSizeBlock;
}