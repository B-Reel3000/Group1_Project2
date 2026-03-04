using UnityEngine;

public class EnemyFistHitbox : MonoBehaviour
{
    [Header("Owner")]
    public EnemyAI_Navmesh owner;

    [Header("Damage")]
    public int damage = 1;

    [Header("Audio")]
    public AudioClip impactClip;
    public float impactVolume = 1f;

    AudioSource audioSource;
    bool hitThisSwing;

    void Awake()
    {
        if (owner == null)
            owner = GetComponentInParent<EnemyAI_Navmesh>();

        audioSource = GetComponentInParent<AudioSource>();
    }

    public void BeginSwing()
    {
        hitThisSwing = false;
    }

    void OnTriggerEnter(Collider other)
    {
        TryHit(other);
    }

    void OnTriggerStay(Collider other)
    {
        TryHit(other);
    }

    void TryHit(Collider other)
    {
        if (owner == null) return;
        if (!owner.CanDealDamage) return;
        if (hitThisSwing) return;

        PlayerHealth ph = other.GetComponentInParent<PlayerHealth>();
        if (ph == null) return;

        hitThisSwing = true;

        ph.TakeDamage(damage);

        if (audioSource != null && impactClip != null)
            audioSource.PlayOneShot(impactClip, impactVolume);
    }
}