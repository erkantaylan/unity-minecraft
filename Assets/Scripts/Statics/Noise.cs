using UnityEngine;

namespace Statics
{
    public static class Noise
    {
        public static float Get2dPerlin(Vector2 position, float offset, float scale)
        {
            float x = position.x / VoxelData.ChunkWidth * scale + offset;
            float y = position.y / VoxelData.ChunkWidth * scale + offset;
            return Mathf.PerlinNoise(x, y);
        }
    }
}