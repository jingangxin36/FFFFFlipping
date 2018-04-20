
using UnityEngine;

public class CoinCell : Cell {
    public CoinCell() : base(CellType.COIN, 1f) {
    }

    public override void ReleaseIntoPool() {
        CoinPool.Instance.Release(gameObject);
    }

    public override void OnPickUp() {
        AchievementManager.Instance.PickUp(Type);
        ReleaseIntoPool();
    }
}