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
    public float dashSpeed = 10f;          // ����ٶ�
    public float dashDuration = 0.5f;      // ��̳���ʱ��
    public float dashCooldown = 1.5f;      // ��ȴʱ��
    public float dashFOV = 80f;            // ���ʱ�����Ұ
    
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

        // �Զ������������е��������2024�����ʵ����
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
        // ��ȴ��ʱ
        if (dashCooldownTimer > 0)
        {
            dashCooldownTimer -= Time.deltaTime;
        }

        // ��̴�������Shift����
        if (Input.GetKeyDown(KeyCode.LeftShift) && !isDashing && dashCooldownTimer <= 0)
        {
            StartCoroutine(PerformDash());
        }
    }

    private IEnumerator PerformDash()
    {
        // ״̬��ʼ��
        isDashing = true;
        moveSpeed = dashSpeed;
        playerCamera.GetComponent<Camera>().fieldOfView = dashFOV;

        // ���ֳ�̷��򣨻��ڵ�ǰ�ƶ����룩
        Vector3 dashDirection = transform.forward * Input.GetAxis("Vertical") +
                              transform.right * Input.GetAxis("Horizontal");
        if (dashDirection == Vector3.zero) dashDirection = transform.forward;
        if (dashDirection == Vector3.zero)
            dashDirection = transform.forward;
        else
            dashDirection = dashDirection.normalized;

        // ���ִ��
        float startTime = Time.time;
        while (Time.time < startTime + dashDuration)
        {

            _controller.Move(dashDirection.normalized * dashSpeed * Time.deltaTime);
            yield return null;
        }

        // �ָ�״̬
        moveSpeed = originalSpeed;
        Camera.main.fieldOfView = originalFOV;
        isDashing = false;
        dashCooldownTimer = dashCooldown;
    }
    private void HandleCamera()
    {
        // �������
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // ��ֱ�ӽǿ��ƣ��������ת��
        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, minLookAngle, maxLookAngle);
        playerCamera.localRotation = Quaternion.Euler(_xRotation, 0, 0);

        // ˮƽ��ת����ұ�����ת��
        transform.Rotate(Vector3.up * mouseX);
    }

    private void HandleMovement()
    {
        if (isDashing) return;

        // ������
        bool isGrounded = _controller.isGrounded;
        if (isGrounded && _velocity.y < 0)
        {
            _velocity.y = -2f; // ��΢��ѹȷ���ӵ�
        }

        // ��������
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        // �ƶ�������㣨�������������
        Vector3 move = transform.right * x + transform.forward * z;
        _controller.Move(move * moveSpeed * Time.deltaTime);

        // ��Ծ��2024��Ľ�����Ծ�߼���
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            _velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
        }

        // ����Ӧ��
        _velocity.y += gravity * Time.deltaTime;
        _controller.Move(_velocity * Time.deltaTime);
    }
}

