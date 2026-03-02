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
        rb.freezeRotation = true;
        rb.useGravity = true;

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        SetAim(false);
    }

    void Update()
    {
        if (Keyboard.current != null)
        {
            if (Keyboard.current.rKey.wasPressedThisFrame)
                SetAim(true);

            if (Keyboard.current.qKey.wasPressedThisFrame)
                SetAim(false);
        }

        moveInput = ReadMoveKeys();

        if (animator != null)
            animator.SetFloat(speedParam, moveInput.magnitude);

        if (Mouse.current != null)
        {
            Vector2 delta = Mouse.current.delta.ReadValue();

            transform.Rotate(0f, delta.x * sensitivityX, 0f);

            pitch -= delta.y * sensitivityY;
            pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
            cameraPivot.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }
    }

    void FixedUpdate()
    {
        Vector3 dir = transform.forward * moveInput.y + transform.right * moveInput.x;
        if (dir.sqrMagnitude > 1f) dir.Normalize();

        Vector3 nextPos = rb.position + dir * moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(nextPos);
    }

    void SetAim(bool aiming)
    {
        if (isAiming == aiming) return;

        isAiming = aiming;

        // Hammer cock sound ONLY when entering aim
        if (aiming && audioSource != null && hammerCockClip != null)
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