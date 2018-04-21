using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//todo 试试配置表???

public class BlockManager : Singleton<BlockManager> {

    public Transform[] blockPrefabs;
    public Transform blocksContainter;
    public Transform gameObjectPool;

    public EnemyPool enemyPool;
    public CoinPool coinPool;
    public TrapPool trapPool;

    public float blocksPositionOffset;
    public int maxSizeInSight;
    public int maxRow;

    private float mLastBlockZ;
    private int mBlocksLength;
    private int mCurrentRowIndex;
    private CellType[,] mCellTypes;


    protected override void Awake() {
        base.Awake();
        mLastBlockZ = 0 - blocksPositionOffset;
        mBlocksLength = blockPrefabs.Length;
        mCellTypes = new CellType[3, maxRow];
    }


    void Start() {
        int tempCount = (int)(enemyPool.probability * 3 * maxRow);
        GenerateRandomMap(tempCount, CellType.ENEMY);

        tempCount = (int)(coinPool.probability * 3 * maxRow);
        GenerateRandomMap(tempCount, CellType.COIN);

        tempCount = (int)(trapPool.probability * 3 * maxRow);
        GenerateRandomMap(tempCount, CellType.TRAP);

        if (mBlocksLength > 0) {
            for (int i = 0; i < maxSizeInSight; i++) {
                //依次从数组中取出block prefab
                AddNewBlock(blockPrefabs[i % mBlocksLength]);
            }
        }
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

    /// <summary>
    /// 将指定的block移到地图尾部
    /// </summary>
    /// <param name="newBlock">指定的block</param>
    public void ChangeBlockPosition(Transform newBlock) {
        //todo 需要根据配置表实例化道具
        GenerateCell();
        var newBlockPos = newBlock.position;
        newBlockPos.z = blocksPositionOffset + mLastBlockZ;
        newBlock.position = newBlockPos;
        mLastBlockZ = newBlockPos.z;

        mCurrentRowIndex++;
        //todo 关卡设置
        if (mCurrentRowIndex == maxRow) {
            Debug.Log("过关了");
            mCurrentRowIndex = 0;
        }

    }

    /// <summary>
    /// 生成随机地图配置表
    /// </summary>
    /// <param name="count">cell的数量</param>
    /// <param name="targetType">cell的类别</param>
    private void GenerateRandomMap(int count, CellType targetType) {
        for (int i = 0; i < count; i++) {
            int tempXIndex;
            int tempYIndex;
            do {
                tempYIndex = UnityEngine.Random.Range(1, maxRow);
                tempXIndex = UnityEngine.Random.Range(0, 3);
            } while (mCellTypes[tempXIndex, tempYIndex] != CellType.NULL);
            mCellTypes[tempXIndex, tempYIndex] = targetType;
        }
    }

    /// <summary>
    /// 根据地图配置表生成相应的cell
    /// </summary>
    private void GenerateCell() {
        for (int i = 0; i < 3; i++) {
            GameObject tempCell;
            switch (mCellTypes[i, mCurrentRowIndex]) {
                case CellType.ENEMY:
                    tempCell = enemyPool.Take();
                    tempCell.transform.position = GetCellPosition(i);
                    tempCell.transform.parent = enemyPool.gameObject.transform;
                    break;
                case CellType.COIN:
                    tempCell = coinPool.Take();
                    tempCell.transform.position = GetCellPosition(i);
                    tempCell.transform.parent = coinPool.gameObject.transform;
                    break;
                case CellType.TRAP:
                    tempCell = trapPool.Take();
                    tempCell.transform.position = GetCellPosition(i);
                    tempCell.transform.parent = trapPool.gameObject.transform;
                    break;
            }
        }
    }

    /// <summary>
    /// 获取即将生成的cell的位置
    /// </summary>
    /// <param name="xIndex">目标cell的x轴索引</param>
    /// <returns></returns>
    private Vector3 GetCellPosition(int xIndex) {
        float cellX = (xIndex - 1) * blocksPositionOffset;
        float cellZ = mLastBlockZ + blocksPositionOffset;
        return new Vector3(cellX, 0, cellZ);
    }

}
