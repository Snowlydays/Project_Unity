using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class PlayerController : NetworkBehaviour
{
    private void Update()
    {
        if (IsOwner == false)
        {
            return;
        }

        Vector2 direction = new Vector2()
        {
            x = Input.GetAxisRaw("Horizontal"),
            y = Input.GetAxisRaw("Vertical")
        };
        float moveSpeed = 3f;
        transform.Translate(direction * moveSpeed * Time.deltaTime);
    }
}