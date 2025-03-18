using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5.0f;
    public float jumpForce = 4.0f;
    public float gravity = -9.81f;

    [Header("Camera Settings")]
    public float mouseSensitivity = 100.0f;
    public Transform playerCamera;
    public float maxLookAngle = 90f;
    public float minLookAngle = -90f;

    [Header("Dash Settings")]
    public float dashSpeed = 10f;          // 冲刺速度
    public float dashDuration = 0.5f;      // 冲刺持续时间
    public float dashCooldown = 1.5f;      // 冷却时间
    public float dashFOV = 80f;            // 冲刺时相机视野
    
    private CharacterController _controller;
    private Vector3 _velocity;
    private float _xRotation;
    private float originalSpeed;
    private float originalFOV;
    private bool isDashing;
    private float dashCooldownTimer;




    void Start()
    {
        _controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        originalSpeed = moveSpeed;
        originalFOV = playerCamera.GetComponent<Camera>().fieldOfView;

        // 自动查找子物体中的摄像机（2024年最佳实践）
        if (playerCamera == null && transform.childCount > 0)
        {
            playerCamera = transform.GetChild(0);
        }

    }

    void Update()
    {
        HandleMovement();
        HandleCamera();
        HandleDash();
    }

    private void HandleDash()
    {
        // 冷却计时
        if (dashCooldownTimer > 0)
        {
            dashCooldownTimer -= Time.deltaTime;
        }

        // 冲刺触发（左Shift键）
        if (Input.GetKeyDown(KeyCode.LeftShift) && !isDashing && dashCooldownTimer <= 0)
        {
            StartCoroutine(PerformDash());
        }
    }

    private IEnumerator PerformDash()
    {
        // 状态初始化
        isDashing = true;
        moveSpeed = dashSpeed;
        playerCamera.GetComponent<Camera>().fieldOfView = dashFOV;

        // 保持冲刺方向（基于当前移动输入）
        Vector3 dashDirection = transform.forward * Input.GetAxis("Vertical") +
                              transform.right * Input.GetAxis("Horizontal");
        if (dashDirection == Vector3.zero) dashDirection = transform.forward;
        if (dashDirection == Vector3.zero)
            dashDirection = transform.forward;
        else
            dashDirection = dashDirection.normalized;

        // 冲刺执行
        float startTime = Time.time;
        while (Time.time < startTime + dashDuration)
        {

            _controller.Move(dashDirection.normalized * dashSpeed * Time.deltaTime);
            yield return null;
        }

        // 恢复状态
        moveSpeed = originalSpeed;
        Camera.main.fieldOfView = originalFOV;
        isDashing = false;
        dashCooldownTimer = dashCooldown;
    }
    private void HandleCamera()
    {
        // 鼠标输入
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // 垂直视角控制（摄像机旋转）
        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, minLookAngle, maxLookAngle);
        playerCamera.localRotation = Quaternion.Euler(_xRotation, 0, 0);

        // 水平旋转（玩家本体旋转）
        transform.Rotate(Vector3.up * mouseX);
    }

    private void HandleMovement()
    {
        if (isDashing) return;

        // 地面检测
        bool isGrounded = _controller.isGrounded;
        if (isGrounded && _velocity.y < 0)
        {
            _velocity.y = -2f; // 轻微下压确保接地
        }

        // 键盘输入
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        // 移动方向计算（基于摄像机方向）
        Vector3 move = transform.right * x + transform.forward * z;
        _controller.Move(move * moveSpeed * Time.deltaTime);

        // 跳跃（2024年改进版跳跃逻辑）
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            _velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
        }

        // 重力应用
        _velocity.y += gravity * Time.deltaTime;
        _controller.Move(_velocity * Time.deltaTime);
    }
}

