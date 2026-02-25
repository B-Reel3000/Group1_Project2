using UnityEngine;

public class MeleeHitbox : MonoBehaviour
{
    public int damage = 1;
    public string enemyTag = "Enemy";

    [HideInInspector] public bool active;

    void OnTriggerEnter(Collider other)
    {
        if (!active) return;
        if (!other.CompareTag(enemyTag)) return;

        Health h = other.GetComponentInParent<Health>();
        if (h != null)
        {
            h.TakeDamage(damage, Health.DamageType.Melee);
            active = false; // prevents multi-hits from one punch
        }
    }
}