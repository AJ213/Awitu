using UnityEngine;

public class PlayerMovement : Gravity
{
    public float Speed { get => speed; set => speed = value; }
    [SerializeField] float speed = 10;
    public float JumpHeight { get => jumpHeight; set => jumpHeight = value; }
    [SerializeField] float jumpHeight = 3;
    public float MaxJumpCount { get => maxJumpCount; set => maxJumpCount = value; }
    [SerializeField] float maxJumpCount = 1;

    [SerializeField] float currentJumpCount = 1;
    [SerializeField] AudioSource hoverSound = default;
    [SerializeField] AudioSource jumpSound = default;
    [SerializeField] bool isHovering = false;
    [SerializeField] ParticleSystem hoverEffect = null;

    protected override void Update()
    {
        isGrounded = Physics.CheckSphere(groundChecker.position, groundDistance, groundMask);

        if(isGrounded && velocity.y < 0)
        {
            currentJumpCount = maxJumpCount;
            velocity.y = -2f;
        }
        if (!isGrounded && velocity.y < 0 && (Input.GetKey(KeyCode.LeftShift) || (Input.GetButton("Jump") && currentJumpCount < 1)) )
        {
            Hover();
        }
        else
        {
            isHovering = false;
            hoverSound.Stop();
            hoverEffect.Stop();
        }

        if (Input.GetButtonDown("Jump") && currentJumpCount > 0)
        {
            Jump();
        }

        Move();
        
        if (!isHovering)
        {
            Fall();
        }
    }

    void Move()
    {
        Vector2 input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        Vector3 move = (this.transform.right * input.x) + (this.transform.forward * input.y);
        move.Normalize();
        controller.Move(move * speed * Time.deltaTime);
    }
    void Hover()
    {
        isHovering = true;
        velocity.y = -1f;
        controller.Move(velocity * Time.deltaTime);
        hoverEffect.Play();
        if(!hoverSound.isPlaying)
            hoverSound.Play();
    }
    void Jump()
    {
        currentJumpCount--;
        if(velocity.y < 0)
        {
            velocity.y = 0;
        }
        velocity.y += Mathf.Sqrt(jumpHeight * -2f * -gravityConstant);
        jumpSound.Play();
    }
}
