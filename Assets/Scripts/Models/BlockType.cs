using System;
using UnityEngine;

namespace Models
{
    [Serializable]
    public class BlockType
    {
        public string blockName;
        public bool isSolid;

        // Back, Front, Top, Bottom, Left, Right

        public int GetTextureId(int faceIndex)
        {
            switch (faceIndex)
            {
                case 0:
                    return backFaceTexture;
                case 1:
                    return frontFaceTexture;
                case 2:
                    return topFaceTexture;
                case 3:
                    return bottomFaceTexture;
                case 4:
                    return leftFaceTexture;
                case 5:
                    return rightFaceTexture;
                default:
                    Debug.Log("Error in GetTextureID; invalid face index");
                    return 0;
            }
        }

        #region Texture Values

        [Header("Texture Values")]
        public int backFaceTexture;

        public int bottomFaceTexture;
        public int frontFaceTexture;
        public int leftFaceTexture;
        public int rightFaceTexture;
        public int topFaceTexture;

        #endregion
    }
}