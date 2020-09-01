using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingEnemyMovement : EnemyMovement
{


    protected override void Update()
    {
        if (player == null)
        {
            return;
        }

        Vector3 targetPosition = new Vector3(player.position.x, this.transform.position.y, player.position.z);
        this.transform.LookAt(targetPosition);
        Move();
    }

    protected override void LateUpdate()
    {
        if (player == null)
        {
            return;
        }

        Vector3 targetPosition = new Vector3(player.position.x, this.transform.position.y, player.position.z);
        this.transform.LookAt(targetPosition);
        Move();
    }

    protected override void Move()
    {
        if (Vector3.Distance(this.transform.position, player.transform.position) > stoppingDistance)
        {
            Vector3 direction = Projectile.CalculateDirection(this.transform.position, player.transform.position + 5*Vector3.up);
            controller.Move(direction * speed * Time.deltaTime);
        }
        else
        {
            Vector3 direction = Projectile.CalculateDirection(this.transform.position, this.transform.position + Vector3.forward*rightOrLeft + 0.1f*Vector3.up);
            controller.Move(direction * speed/5 * Time.deltaTime);
        }
    }
}
