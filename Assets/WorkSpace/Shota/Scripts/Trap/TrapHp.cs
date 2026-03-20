using UnityEngine;

public class TrapHp : MonoBehaviour
{
    public int hp = 3;
    [SerializeField] bool broken = false;

    public void TakeDamage(int value, Vector2? hitPos = null)
    {
        if (broken) return;

        if(hitPos != null)
        {
            var hitEffect = Instantiate(Resources.Load("Prefabs/Particles/FX_Hit_01"), (Vector3)hitPos, Quaternion.identity);
        }
        
        hp -= value;
        if(hp <= 0)
        {
            Break();
        }
    }

    public void Break()
    {
        broken = true;
        Destroy(gameObject);
    }
}
