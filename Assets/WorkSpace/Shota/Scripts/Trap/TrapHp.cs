using UnityEngine;

public class TrapHp : MonoBehaviour
{
    [Tooltip("この罠の寿命（秒）。1HP = 1秒")]
    public float hp = 10f;
    
    [Tooltip("防御倍率。1.0で標準ダメージ、0.5でダメージ半減（硬い）、2.0で2倍（脆い）")]
    public float defenseMultiplier = 1.0f;

    [SerializeField] bool broken = false;
    
    private Trap _trap;

    private void Start()
    {
        _trap = GetComponent<Trap>();
    }

    private void Update()
    {
        if (broken) return;
        
        // 落下完了などの設置準備が整ってから寿命カウントダウン開始
        if (_trap != null && !_trap.isSetup) return;

        hp -= Time.deltaTime;
        if (hp <= 0)
        {
            Break();
        }
    }

    public void TakeDamage(float value, Vector2? hitPos = null)
    {
        if (broken) return;

        if(hitPos != null)
        {
            var hitEffect = Instantiate(Resources.Load("Prefabs/Particles/FX_Hit_01"), (Vector3)hitPos, Quaternion.identity);
        }
        
        hp -= (value * defenseMultiplier);
        if(hp <= 0)
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
    }
}
