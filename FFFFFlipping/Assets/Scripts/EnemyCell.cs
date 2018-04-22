using UnityEngine;

public class EnemyCell : Cell {
    public EnemyCell() : base(CellType.ENEMY, 1f) {
    }

    public float turnRate;
    private float mLookLimit = 45f;
    public int visionRange ;

    protected virtual bool SeesPlayer =>
        ((base.Player != null) && (Mathf.Abs((int)(transform.position.y - Player.transform.position.y)) <= this.visionRange));

    protected virtual void Update() {
        if (Player != null) {

            if ( SeesPlayer) {

                TurnTowardPlayer();
            }
        }
    }

    public override void ReleaseIntoPool() {
       EnemyPool.Instance.Release(gameObject);
    }

    public override void OnPickUp() {
        AchievementManager.Instance.PickUp(Type);
        ReleaseIntoPool();
    }

    private void TurnTowardPlayer() {
        Vector3 position = Player.transform.position;
        position.y = transform.position.y;
        Vector3 forward = transform.position - position;
        Debug.Log(forward);

        if (forward != Vector3.zero) {
            Quaternion b = Quaternion.LookRotation(forward);
            Debug.Log(b);

            transform.rotation = Quaternion.Lerp(base.transform.rotation, b, Time.deltaTime * turnRate);
            float y = base.transform.eulerAngles.y;
            if (y > 180f) {
                y -= 360f;
            }
            base.transform.rotation = Quaternion.Euler(0f, Mathf.Clamp(y, -mLookLimit, mLookLimit), 0f);
        }
    }
}