using System.Collections.Generic;
using UnityEngine;

public class MeleeHitbox : MonoBehaviour
{
    [Header("Owner")]
    public PlayerMeleeEvents owner;

    [Header("Damage")]
    public int damage = 1;

    [Header("Audio")]
    public AudioClip impactClip;
    public float impactVolume = 1f;

    HashSet<Health> hitThisSwing = new HashSet<Health>();
    AudioSource audioSource;

    void Awake()
    {
        audioSource = GetComponentInParent<AudioSource>();
    }

    public void BeginSwing()
    {
        hitThisSwing.Clear();
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

        Health h = other.GetComponentInParent<Health>();
        if (h == null) return;

        if (hitThisSwing.Contains(h)) return;
        hitThisSwing.Add(h);

        // DAMAGE
        h.TakeDamage(damage, Health.DamageType.Melee);

        // IMPACT SOUND
        if (audioSource != null && impactClip != null)
            audioSource.PlayOneShot(impactClip, impactVolume);

        owner.OnLandedHit(h);
    }
}