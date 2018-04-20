using UnityEngine;

public class EnemyCell : Cell {
    public EnemyCell() : base(CellType.ENEMY, 1f) {
    }

    public override void ReleaseIntoPool() {
       EnemyPool.Instance.Release(gameObject);
    }

    public override void OnPickUp() {
        AchievementManager.Instance.PickUp(Type);
        ReleaseIntoPool();
    }
}