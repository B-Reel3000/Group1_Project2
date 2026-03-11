using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class PlayerController : MonoBehaviour
{
    [Header("References")]
    public Transform cameraPivot;
    public GameObject reticle;

    [Header("Animation")]
    public Animator animator;
    public string speedParam = "Speed";
    public string isAimingParam = "IsAiming";

    [Header("Revolver")]
    public GameObject revolverObject;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip hammerCockClip;

    [Header("Cinemachine Cameras")]
    public Unity.Cinemachine.CinemachineCamera exploreCam;
    public Unity.Cinemachine.CinemachineCamera aimCam;

    [Header("Move")]
    public float moveSpeed = 5f;

    [Header("Look")]
    public float sensitivityX = 0.12f;
    public float sensitivityY = 0.10f;
    public float minPitch = -35f;
    public float maxPitch = 70f;

    [Header("Camera Priorities (Higher = Active)")]
    public int explorePriority = 20;
    public int aimPriority = 10;

    [Header("Aim Movement Remap")]
    [Tooltip("If true, aim mode uses side-on controls for the right-facing aim animation.")]
    public bool useSideAimMovement = true;

    Rigidbody rb;
    Vector2 moveInput;
    float pitch;
    bool isAiming;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.useGravity = true;

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        ApplyAimState(false, true);
    }

    void Update()
    {
        if (Keyboard.current != null)
        {
            if (Keyboard.current.rKey.wasPressedThisFrame)
                ApplyAimState(true);

            if (Keyboard.current.qKey.wasPressedThisFrame)
                ApplyAimState(false);
        }

        moveInput = ReadMoveKeys();

        if (animator != null)
            animator.SetFloat(speedParam, moveInput.magnitude);

        if (Mouse.current != null)
        {
            Vector2 delta = Mouse.current.delta.ReadValue();

            // Keep your existing yaw/pitch behavior
            transform.Rotate(0f, delta.x * sensitivityX, 0f);

            pitch -= delta.y * sensitivityY;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

            if (cameraPivot != null)
                cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }
    }

    void FixedUpdate()
    {
        Vector3 moveDir = isAiming && useSideAimMovement
            ? GetSideAimMoveDirection(moveInput)
            : GetNormalMoveDirection(moveInput);

        Vector3 nextPos = rb.position + moveDir * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(nextPos);
    }

    Vector3 GetNormalMoveDirection(Vector2 input)
    {
        if (input.sqrMagnitude < 0.0001f)
            return Vector3.zero;

        Vector3 move = transform.forward * input.y + transform.right * input.x;

        if (move.sqrMagnitude > 1f)
            move.Normalize();

        return move;
    }

    Vector3 GetSideAimMoveDirection(Vector2 input)
    {
        if (input.sqrMagnitude < 0.0001f)
            return Vector3.zero;

        // Side-on remap for "character looks right while aiming"
        // W = right
        // S = left
        // A = forward
        // D = backward
        Vector3 move =
            transform.right * input.y +
            (-transform.forward) * input.x;

        if (move.sqrMagnitude > 1f)
            move.Normalize();

        return move;
    }

    void ApplyAimState(bool aiming, bool force = false)
    {
        if (!force && isAiming == aiming) return;

        bool wasAiming = isAiming;
        isAiming = aiming;

        if (!wasAiming && aiming && audioSource != null && hammerCockClip != null)
            audioSource.PlayOneShot(hammerCockClip);

        if (exploreCam != null)
            exploreCam.Priority = aiming ? aimPriority : explorePriority;

        if (aimCam != null)
            aimCam.Priority = aiming ? explorePriority : aimPriority;

        if (reticle != null)
            reticle.SetActive(aiming);

        if (animator != null)
            animator.SetBool(isAimingParam, aiming);

        if (revolverObject != null)
            revolverObject.SetActive(aiming);
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