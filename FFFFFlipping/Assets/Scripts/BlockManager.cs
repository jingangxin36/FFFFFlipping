using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockManager : MonoBehaviour {

    public static BlockManager instance;
    public Transform[] blockPrefabs;
    public Transform blocksContainter;
    public float blocksPositionOffset;
    public int maxSizeInSight;

    private int mBlocksLength;
    private float mLastBlockZ;

    void Awake() {
        instance = this;
        mLastBlockZ = 0 - blocksPositionOffset;
        mBlocksLength = blockPrefabs.Length;
    }

    void Start() {
        if (mBlocksLength > 0) {
            for (int i = 0; i < maxSizeInSight; i++) {
                //依次从数组中取出block prefab
                AddNewBlock(blockPrefabs[i % mBlocksLength]);
            }
        }
    }
    /// <summary>
    /// 将指定的block移到地图尾部
    /// </summary>
    /// <param name="newBlock">指定的block</param>
    public void ChangeBlockPosition(Transform newBlock) {
        var newBlockPos = newBlock.position;
        newBlockPos.z = blocksPositionOffset + mLastBlockZ;
        newBlock.position = newBlockPos;
        mLastBlockZ = newBlockPos.z;
    }
    /// <summary>
    /// 实例化新的block并配置
    /// </summary>
    /// <param name="block">指定的block</param>
    private void AddNewBlock(Transform block) {
        var newBlock = Instantiate(block);
        newBlock.parent = blocksContainter;
        ChangeBlockPosition(newBlock);
    }

}
