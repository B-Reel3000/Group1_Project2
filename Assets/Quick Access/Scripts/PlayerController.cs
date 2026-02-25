using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class PlayerController : MonoBehaviour
{
    [Header("References")]
    public Transform cameraPivot;          // Player/CameraPivot
    public GameObject reticle;             // UI reticle (only active while aiming)

    [Header("Animation")]
    public Animator animator;              // drag model Animator here
    public string speedParam = "Speed";    // float in Animator

    [Header("Cinemachine Cameras (Optional)")]
    public Unity.Cinemachine.CinemachineCamera exploreCam;
    public Unity.Cinemachine.CinemachineCamera aimCam;

    [Header("Move")]
    public float moveSpeed = 5f;

    [Header("Steps")]
    public float stepHeight = 0.35f;
    public float stepCheckDistance = 0.40f;
    public float stepUpSpeed = 6f;
    public LayerMask groundLayers = ~0;

    [Header("Look")]
    public float sensitivityX = 0.12f;
    public float sensitivityY = 0.10f;
    public float minPitch = -35f;
    public float maxPitch = 70f;

    [Header("Aim")]
    public int explorePriority = 20;
    public int aimPriority = 10;

    Rigidbody rb;
    Vector2 moveInput;
    float pitch;
    bool isAiming;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.freezeRotation = true;

        rb.useGravity = true;
        rb.isKinematic = false;
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (cameraPivot != null)
            pitch = NormalizeAngle(cameraPivot.localEulerAngles.x);

        SetAim(false);
    }

    void Update()
    {
        if (cameraPivot == null) return;

        // Aim toggle
        if (Keyboard.current != null)
        {
            if (Keyboard.current.rKey.wasPressedThisFrame) SetAim(true);
            if (Keyboard.current.qKey.wasPressedThisFrame) SetAim(false);
        }

        // Movement input (stored for FixedUpdate)
        moveInput = ReadMoveKeys();

        // Update animator speed (Idle/Walk)
        if (animator != null)
            animator.SetFloat(speedParam, moveInput.magnitude);

        // Mouse look
        if (Mouse.current != null)
        {
            Vector2 delta = Mouse.current.delta.ReadValue();

            float yaw = delta.x * sensitivityX;
            transform.Rotate(0f, yaw, 0f, Space.World);

            pitch -= delta.y * sensitivityY;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
            cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }
    }

    void FixedUpdate()
    {
        Vector3 dir = (transform.forward * moveInput.y + transform.right * moveInput.x);
        if (dir.sqrMagnitude > 1f) dir.Normalize();

        Vector3 delta = dir * moveSpeed * Time.fixedDeltaTime;

        StepClimb(delta);

        Vector3 nextPos = rb.position + new Vector3(delta.x, 0f, delta.z);
        rb.MovePosition(nextPos);
    }

    void StepClimb(Vector3 horizontalDelta)
    {
        if (horizontalDelta.sqrMagnitude < 0.00001f) return;

        Vector3 dir = horizontalDelta.normalized;

        Vector3 lowerOrigin = transform.position + Vector3.up * 0.05f;
        Vector3 upperOrigin = transform.position + Vector3.up * (stepHeight + 0.05f);

        bool lowerHit = Physics.Raycast(lowerOrigin, dir, out RaycastHit lower, stepCheckDistance, groundLayers);
        bool upperHit = Physics.Raycast(upperOrigin, dir, stepCheckDistance, groundLayers);

        if (lowerHit && !upperHit)
        {
            rb.position += Vector3.up * (stepUpSpeed * Time.fixedDeltaTime);
        }
    }

    void SetAim(bool aiming)
    {
        isAiming = aiming;

        if (exploreCam != null)
            exploreCam.Priority = aiming ? aimPriority : explorePriority;

        if (aimCam != null)
            aimCam.Priority = aiming ? explorePriority : aimPriority;

        if (reticle != null)
            reticle.SetActive(aiming);
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

    float NormalizeAngle(float angle)
    {
        while (angle > 180f) angle -= 360f;
        while (angle < -180f) angle += 360f;
        return angle;
    }

    public bool IsAiming => isAiming;
}