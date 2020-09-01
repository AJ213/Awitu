using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class EnemyMovement : Gravity
{
    [SerializeField] protected float speed = 5;
    public float Speed { get { return speed; } set { speed = value; } }
    [SerializeField] protected float stoppingDistance = 5;
    protected Transform player = default;
    [SerializeField] protected int rightOrLeft;
    protected void Start()
    {
        player = Player.player.transform;
        rightOrLeft = Random.Range(0, 2) * 2 - 1;
    }
    protected override void Update()
    {
        isGrounded = Physics.CheckSphere(groundChecker.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        
        Fall();
    }

    protected virtual void LateUpdate()
    {
        if(player == null)
        {
            return;
        }    

        Vector3 targetPosition = new Vector3(player.position.x, this.transform.position.y, player.position.z);
        this.transform.LookAt(targetPosition);
        Move();
    }

    protected virtual void Move()
    {

        if (Vector3.Distance(this.transform.position, player.transform.position) < stoppingDistance)
        {
            Vector3 direction = Projectile.CalculateDirection(this.transform.position, this.transform.position + Vector3.right * rightOrLeft);
            controller.Move(direction * speed/2 * Time.deltaTime);
        }
        else
        {
            Vector3 direction = Projectile.CalculateDirection(this.transform.position, player.transform.position);
            controller.Move(direction * speed * Time.deltaTime);
        }
    }
}
