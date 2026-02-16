using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    public Transform cameraPivot;            // Player/CameraPivot (Cinemachine Tracking Target)
    public CinemachineCamera exploreCam;     // Cinemachine Camera (Explore)
    public CinemachineCamera aimCam;         // Cinemachine Camera (Aim)
    public CinemachineBrain brain;           // Drag Main Camera (has Cinemachine Brain)

    [Header("UI (optional)")]
    public GameObject reticle;

    [Header("Move")]
    public float moveSpeed = 5f;

    [Header("Look")]
    public float sensitivityX = 0.12f;
    public float sensitivityY = 0.10f;
    public float minPitch = -35f;
    public float maxPitch = 70f;

    [Header("Aim (Cinemachine Priority)")]
    public int explorePriority = 20;
    public int aimPriority = 10;

    [Header("Quickdraw Camera Switch")]
    [Tooltip("0 = instant cut. Try 0.03 - 0.08 for a snappy blend.")]
    public float quickdrawBlendTime = 0.03f;

    float pitch;
    bool isAiming;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (cameraPivot != null)
            pitch = cameraPivot.localEulerAngles.x;

        SetAim(false);
    }

    void Update()
    {
        if (cameraPivot == null) return;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.rKey.wasPressedThisFrame) SetAim(true);
            if (Keyboard.current.qKey.wasPressedThisFrame) SetAim(false);
        }

        if (Mouse.current != null)
        {
            Vector2 delta = Mouse.current.delta.ReadValue();

            // yaw rotates the player
            float yaw = delta.x * sensitivityX;
            transform.Rotate(0f, yaw, 0f, Space.World);

            // pitch rotates the pivot
            pitch -= delta.y * sensitivityY;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
            cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }

        Vector2 move = ReadMoveKeys();
        if (move.sqrMagnitude > 0.001f)
        {
            Vector3 moveDir = (transform.forward * move.y + transform.right * move.x).normalized;
            transform.position += moveDir * moveSpeed * Time.deltaTime;
        }
    }

    void SetAim(bool aiming)
    {
        isAiming = aiming;

        // Swap priorities
        if (exploreCam != null) exploreCam.Priority = aiming ? aimPriority : explorePriority;
        if (aimCam != null)     aimCam.Priority     = aiming ? explorePriority : aimPriority;

        // Optional reticle
        if (reticle != null) reticle.SetActive(aiming);

        // QUICKDRAW: force blend time (or cut)
        ApplyQuickdrawBlend();
    }

    void ApplyQuickdrawBlend()
    {
        if (brain == null) return;

        // Cinemachine Brain blend is what makes switching feel “slow”
        // Setting time to 0 makes it an instant cut.
        var blend = brain.DefaultBlend;
        blend.Time = quickdrawBlendTime;

        // If you want true “cut” behavior when time == 0:
        if (quickdrawBlendTime <= 0f)
            blend.Style = CinemachineBlendDefinition.Styles.Cut;

        brain.DefaultBlend = blend;
    }

    Vector2 ReadMoveKeys()
    {
        Vector2 v = Vector2.zero;
        if (Keyboard.current == null) return v;

        if (Keyboard.current.wKey.isPressed) v.y += 1f;
        if (Keyboard.current.sKey.isPressed) v.y -= 1f;
        if (Keyboard.current.dKey.isPressed) v.x += 1f;
        if (Keyboard.current.aKey.isPressed) v.x -= 1f;

        return v.normalized;
    }

    public bool IsAiming => isAiming;
}
