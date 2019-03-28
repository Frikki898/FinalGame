﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OgreController : MonoBehaviour
{
    public float sideWaysSpeed;
    private Rigidbody2D rigid;
    public float jumpForce;
    public bool isGrounded;
    public float movementSpeed;
    public float runSpeed;
    public float carrySpeed;
	private Animator animator = null;
	private BoxScript touchingBox = null;
    private BoxScript holdingBox = null;
    private float ychange = 1;
	private float animSpeed = 1;
	private bool facingLeft = false;
    private float fromFloatingBox;

    private enum grabSide
    {
        left,
        right,
        none
    };

    private grabSide gSide = grabSide.none;

	// Start is called before the first frame update
	void Start()
    {
		transform.eulerAngles = new Vector2(0, 100);

        foreach(Collider2D col1 in FindObjectOfType<GnomeController>().GetComponents<Collider2D>())
        {
            foreach (Collider2D col2 in this.GetComponents<Collider2D>())
            {
                Physics2D.IgnoreCollision(col1, col2);
            }
        }
		
		animator = GetComponent<Animator>();
		rigid = this.GetComponent<Rigidbody2D>();
        movementSpeed = runSpeed;
        rigid.centerOfMass = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        if(holdingBox != null)
        {
            if (Mathf.Abs(holdingBox.GetComponent<Rigidbody2D>().velocity.y) >= ychange || Mathf.Abs(rigid.velocity.y) >= ychange)
            {
                Debug.Log("break from here");
                movementSpeed = runSpeed;
                holdingBox.rb.constraints = RigidbodyConstraints2D.FreezeRotation;
                holdingBox.beingHeld = false;
                holdingBox.ogreSelection.SetActive(false);
                holdingBox = null;
            }
        }
        //Debug.Log(gSide);

        if (Input.GetKey(KeyCode.A))
		{
            if (isGrounded)
            {
                rigid.velocity += Vector2.left * Time.deltaTime * movementSpeed;
            }
            else
            {
                rigid.velocity += Vector2.left * Time.deltaTime * movementSpeed * 0.5f;
            }

            if (!facingLeft)
			{
				if (holdingBox != null)
				{
                    if(gSide == grabSide.left)
                    {
                        //Debug.Log("pulling");
                        holdingBox.rb.velocity += Vector2.left * Time.deltaTime * 5;
                    }
                    else
                    {
                        rigid.velocity += Vector2.left * Time.deltaTime * 5;
                    }
					holdingBox.rb.velocity += Vector2.left * Time.deltaTime * movementSpeed;
                    animSpeed = -0.6f;
				}
				else
				{
					facingLeft = true;
					rigid.transform.eulerAngles = new Vector2(0, -80);
					animSpeed = 1f;
				}
			}
			else
			{
				if (holdingBox != null)
				{
                    if (gSide == grabSide.left)
                    {
                        holdingBox.rb.velocity += Vector2.left * Time.deltaTime * 5;
                    }
                    else
                    {
                        rigid.velocity += Vector2.left * Time.deltaTime * 5;
                    }
                    holdingBox.rb.velocity += Vector2.left * Time.deltaTime * movementSpeed;
                    animSpeed = 0.6f;
				}
				else
				{
					animSpeed = 1f;
				}
			}
		}
		else if (Input.GetKey(KeyCode.D))
		{
            
            if (isGrounded)
            {
                rigid.velocity += Vector2.right * Time.deltaTime * movementSpeed;
            }
            else
            {
                rigid.velocity += Vector2.right * Time.deltaTime * movementSpeed * 0.5f;
            }

            if (facingLeft)
			{
				if (holdingBox != null)
				{
                    if (gSide == grabSide.right)
                    {
                        holdingBox.rb.velocity += Vector2.right * Time.deltaTime * 5;
                        Debug.Log("pulling");
                    }
                    else
                    {
                        rigid.velocity += Vector2.right * Time.deltaTime * 5;
                    }
                    holdingBox.rb.velocity += Vector2.right * Time.deltaTime * movementSpeed;
                    animSpeed = -0.6f;
				}
				else
				{
					facingLeft = false;
					rigid.transform.eulerAngles = new Vector2(0, 100);
					animSpeed = 1f;
				}
			}
			else
			{
				if (holdingBox != null)
				{
                    if (gSide == grabSide.right)
                    {
                        holdingBox.rb.velocity += Vector2.right * Time.deltaTime * 5;
                        Debug.Log("pulling");
                    }
                    else
                    {
                        rigid.velocity += Vector2.right * Time.deltaTime * 5;
                    }
                    holdingBox.rb.velocity += Vector2.right * Time.deltaTime * movementSpeed;
					animSpeed = 0.6f;
				}
				else
				{
					animSpeed = 1f;
				}
			}
		}

		if (Input.GetKeyDown(KeyCode.W) && isGrounded)
        {
            if(holdingBox == null)
            {
                isGrounded = false;
                rigid.velocity += Vector2.up * jumpForce;
                Debug.Log(isGrounded);
            }
        }
        if(Input.GetKeyDown(KeyCode.E))
        {
			animSpeed = 0;
			grabClosest();
        }

		if (!Input.anyKey)
		{
			if (isGrounded)
				animSpeed = 0;
		}

		if (isGrounded)
		{
			animator.SetFloat("mainSpeed", animSpeed);
		}
		else
		{
			animator.SetFloat("mainSpeed", 1.5f);
		}

		
	}

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.GetComponent<BoxScript>() != null)
        {
            Debug.Log("Touching a box");

            Vector3 center = collision.collider.bounds.center;
            Vector3 contactPoint = collision.contacts[0].point;
            
            if (contactPoint.x > center.x + collision.gameObject.transform.localScale.x / 2)
            {
                touchingBox = collision.gameObject.GetComponent<BoxScript>();
                if (holdingBox == null)
                {
                    gSide = grabSide.right;
                }
                Debug.Log("To the right");
            }
            if (contactPoint.x < center.x - collision.gameObject.transform.localScale.x / 2)
            {
                touchingBox = collision.gameObject.GetComponent<BoxScript>();
                if(holdingBox == null)
                {
                    gSide = grabSide.left;
                }
                Debug.Log("To the left");
            }
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        Debug.Log("exiting");
        isGrounded = false;
        BoxScript b = collision.gameObject.GetComponent<BoxScript>();
        if(b != null)
        {
            Debug.Log(b.name);
            //Debug.Log("getting in here");
            if (b == holdingBox)    
            {
                if (Vector3.Distance(this.transform.position, holdingBox.transform.position) > fromFloatingBox + 1)
                {
                    grabClosest();
                }
                //Debug.Log("not getting in here");
                //releasing the box if i move away form it
                //grabClosest();
            }
            //Debug.Log("stopped touching box");
            touchingBox = null;
        }
    }

    public void grabClosest()
    {
        if(holdingBox == null)
        {
            if (touchingBox != null)
            {
                if(touchingBox.BoxType == BoxScript.BoxTypes.wood)
                {
                    movementSpeed = carrySpeed;
                    holdingBox = touchingBox;
                    holdingBox.beingHeld = true;
                    holdingBox.rb.constraints = RigidbodyConstraints2D.FreezeRotation;
                    holdingBox.rb.mass = 1;
                    holdingBox.ogreSelection.SetActive(true);
                    fromFloatingBox = Vector3.Distance(this.transform.position, holdingBox.transform.position);
                } 
                else if(touchingBox.BoxType == BoxScript.BoxTypes.steel)
                {
                    movementSpeed = carrySpeed;
                    holdingBox = touchingBox;
                    holdingBox.beingHeld = true;
                    holdingBox.rb.constraints = RigidbodyConstraints2D.FreezeRotation;
                    holdingBox.rb.mass = 1;
                    holdingBox.ogreSelection.SetActive(true);
                    fromFloatingBox = Vector3.Distance(this.transform.position, holdingBox.transform.position);
                }
                else if(touchingBox.BoxType == BoxScript.BoxTypes.magic)
                {
                    Debug.Log("Cannot pick up magic");
                    //todo: add visual feedback that cube cant be picked up
                }
                else if(touchingBox.BoxType == BoxScript.BoxTypes.wood)
                {
                    Debug.Log("Needs help to move this");
                    //todo: add visual feedback that he needs help
                }
            }
        }
        else
        {
            movementSpeed = runSpeed;
            holdingBox.beingHeld = false;
            holdingBox.rb.mass = 10;
            holdingBox.ogreSelection.SetActive(false);
            holdingBox = null;
        }
    }


    private float lastFrameVelo;
    void OnCollisionStay2D()
    {
        if (rigid.velocity.y == 0)
        {
            isGrounded = true;
        }
        lastFrameVelo = rigid.velocity.y;
    }
}

