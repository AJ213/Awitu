using UnityEngine;

public abstract class Gravity : MonoBehaviour
{
    [SerializeField] protected float gravityConstant = 9.81f;
    
    [SerializeField] protected Transform groundChecker = default;
    [SerializeField] protected LayerMask groundMask = default;
    [SerializeField] protected float groundDistance = 0.4f;

    public bool IsGrounded => isGrounded;
    [SerializeField] protected bool isGrounded = false;
    
    [SerializeField] protected CharacterController controller = default;


    public Vector3 Velocity => velocity;
    [SerializeField] protected Vector3 velocity = default;

    protected abstract void Update();

    protected void Fall()
    {
        velocity.y -= gravityConstant * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
}
