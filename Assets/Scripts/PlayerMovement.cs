using UnityEngine;
using UnityEngine.InputSystem;
 
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    // [Header("Movement")]
    // [SerializeField] private float moveSpeed = 5f;
    // [SerializeField] private float acceleration = 10f;
    // [SerializeField] private float deceleration = 15f;
 
    // [Header("Shooting")]
    // [SerializeField] private GameObject bulletPrefab;
    // [SerializeField] private Transform firePoint;
    // [SerializeField] private float bulletSpeed = 15f;
    // [SerializeField] private float fireRate = 0.15f;
 
    // private Rigidbody2D rb;
    // [SerializeField] private Camera mainCamera;
 
    // private Vector2 moveInput;
    // private Vector2 mouseWorldPos;
    // private float fireCooldown;
 
    // // --- Input System callbacks ---
 
    // public void OnMove(InputValue value)
    // {
    //     moveInput = value.Get<Vector2>();
    //     mainCamera.transform.position = (Vector2)this.transform.position;
    // }
 
    // public void OnLook(InputValue value)
    // {
    //     // Receives raw screen-space pointer position
    //     Vector2 screenPos = value.Get<Vector2>();
    //     mouseWorldPos = mainCamera.ScreenToWorldPoint(screenPos);
    // }
 
    // public void OnFire(InputValue value)
    // {
    //     // OnFire is called on press; we track hold state via fireCooldown
    //     if (value.isPressed)
    //         TryShoot();
    // }
 
    // // --- Unity lifecycle ---
 
    // private void Awake()
    // {
    //     rb = GetComponent<Rigidbody2D>();
    //     rb.gravityScale = 0f;
    //     rb.freezeRotation = true;
 
    //     mainCamera = Camera.main;
    // }
 
    // private void Update()
    // {
    //     AimAtMouse();
 
    //     fireCooldown -= Time.deltaTime;
 
    //     // Hold-to-shoot: check if fire button is held each frame
    //     var fireAction = GetComponent<PlayerInput>()?.actions["Attack"];
    //     if (fireAction != null && fireAction.IsPressed())
    //         TryShoot();
    // }
 
    // private void FixedUpdate()
    // {
    //     HandleMovement();
    // }
 
    // // --- Movement ---
 
    // private void HandleMovement()
    // {
    //     Vector2 targetVelocity = moveInput.normalized * moveSpeed;
 
    //     float rate = moveInput.sqrMagnitude > 0.01f ? acceleration : deceleration;
    //     rb.linearVelocity = Vector2.MoveTowards(rb.linearVelocity, targetVelocity, rate * Time.fixedDeltaTime);
    // }
 
    // // --- Aiming ---
 
    // private void AimAtMouse()
    // {
    //     Vector2 direction = mouseWorldPos - (Vector2)transform.position;
    //     float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
    //     transform.rotation = Quaternion.Euler(0f, 0f, angle);
    // }
 
    // // --- Shooting ---
 
    // private void TryShoot()
    // {
    //     if (fireCooldown > 0f || bulletPrefab == null || firePoint == null)
    //         return;
 
    //     fireCooldown = fireRate;
 
    //     GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
 
    //     if (bullet.TryGetComponent<Rigidbody2D>(out var bulletRb))
    //     {
    //         bulletRb.linearVelocity = firePoint.right * bulletSpeed;
    //     }
 
    //     // Destroy bullet after 3 seconds to avoid memory leaks
    //     Destroy(bullet, 3f);
    // }
}