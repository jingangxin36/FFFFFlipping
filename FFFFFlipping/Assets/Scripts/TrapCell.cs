using UnityEngine;

public class TrapCell : Cell {
    public TrapCell() : base(CellType.TRAP, 1f) {
    }

    public override void ReleaseIntoPool() {
        CoinPool.Instance.Release(gameObject);
    }

    public override void OnPickUp() {
        AchievementManager.Instance.PickUp(Type);
        ReleaseIntoPool();
    }
}