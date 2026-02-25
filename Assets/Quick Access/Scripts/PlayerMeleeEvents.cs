using UnityEngine;

public class PlayerMeleeEvents : MonoBehaviour
{
    public MeleeHitbox[] fists;

    public void EnableFists()
    {
        foreach (var f in fists) if (f != null) f.active = true;
    }

    public void DisableFists()
    {
        foreach (var f in fists) if (f != null) f.active = false;
    }
}