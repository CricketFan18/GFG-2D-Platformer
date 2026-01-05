using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class WallJump : ObstacleBase
{
    public bool isLeft;
    List<PlayerControls> players = new List<PlayerControls>();
    private System.Action<InputAction.CallbackContext> jumpCallback;
    
    public override void AffectPlayer(GameObject player)
    {
        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();

        if (rb)
        {
            Vector2 pushDir = (isLeft)? Vector2.right + Vector2.up : Vector2.left+Vector2.up;
            rb.linearVelocity = Vector2.zero;
            rb.AddForce(pushDir * knockbackForce, ForceMode2D.Impulse);
        }
    }

    private new void OnTriggerEnter2D(Collider2D other)
    {
        PlayerControls pc =  other.GetComponent<PlayerControls>();
        if (pc)
        {
            // pc.enabled = true;
            // if(pc.GetComponent<Rigidbody2D>().linearVelocity.y != 0)pc.attachedToWall = true;
            pc.attachedToWall = true;
            jumpCallback = ctx => wallJump(pc);
            pc.jumpRef.action.performed += jumpCallback;
        }
    }

    private void wallJump(PlayerControls pc)
    {
        // pc.enabled = false;
        pc.wallJumping = true;
        pc.attachedToWall = false;
        AffectPlayer(pc.gameObject);
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        PlayerControls pc = other.GetComponent<PlayerControls>();
        if (pc)
        {
            pc.attachedToWall = false;
            pc.jumpRef.action.performed -= jumpCallback;
        }
    }
}
