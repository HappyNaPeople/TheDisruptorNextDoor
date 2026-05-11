using UnityEngine;

public class TrapHp : MonoBehaviour
{
    [Tooltip("この罠の寿命（秒）。1HP = 1秒")]
    public float hp = 10f;

    [Tooltip("防御倍率。1.0で標準ダメージ、0.5でダメージ半減（硬い）、2.0で2倍（脆い）")]
    public float defenseMultiplier = 1.0f;

    [Tooltip("時間経過でHPを消費するかどうか（1秒1HP）")]
    public bool consumeHpOverTime = true;

    [SerializeField] bool broken = false;

    [SerializeField] GameObject breakFXPrefab;

    private Trap _trap;

    private void Start()
    {
        _trap = GetComponent<Trap>();
        // 自分のオブジェクトになければ親から探す
        if (_trap == null) _trap = GetComponentInParent<Trap>();
    }

    private void Update()
    {
        if (broken) return;

        // 落下完了などの設置準備が整ってから寿命カウントダウン開始
        if (_trap != null && !_trap.isSetup) return;

        if (consumeHpOverTime)
        {
            hp -= Time.deltaTime;
            if (hp <= 0)
            {
                Break();
            }
        }
    }

    public void TakeDamage(float value, Vector2? hitPos = null, GameObject hitFXPrefab = null, bool isRight = true)
    {
        if (broken) return;

        if (hitPos != null)
        {
            if (hitFXPrefab != null)
            {
                var hitEffect = Instantiate(hitFXPrefab, (Vector3)hitPos, Quaternion.Euler(0f, 0f, isRight ? 0f : 180f));
            }
        }

        hp -= (value * defenseMultiplier);
        if (hp <= 0)
        {
            Break();
        }
    }

    public void Break()
    {
        broken = true;
        if (_trap != null)
        {
            _trap.BrakeTheTrap();
        }
        else
        {
            Destroy(gameObject);
        }

        if (breakFXPrefab != null)
        {
            Instantiate(breakFXPrefab, transform.position, Quaternion.identity);
        }
    }
}
