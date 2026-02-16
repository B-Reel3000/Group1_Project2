using UnityEngine;

public class AmmoDropOnDeath : MonoBehaviour
{
    public int ammoAmount = 2;

    private void OnTriggerEnter(Collider other)
    {
        PlayerGun gun = other.GetComponentInParent<PlayerGun>();
        if (gun != null)
        {
            gun.AddAmmo(ammoAmount);
            Destroy(gameObject);
        }
    }
}