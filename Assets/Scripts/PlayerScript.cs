﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine.AI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Animator))]

public class PlayerScript : MonoBehaviour {

    public delegate void DeathHandler();

    public static event DeathHandler OnDeath;
	public float maxSpeed = 10f;
	public float jumpForce = 700f;
    public float springForce = 400;
    public float boostUpForce = 400;
    public float boostSideForce = 400;

    private Vector2 velocity = new Vector2(0, 0);
    public float gravity = 9.81f;

    Animator animator;

	bool isGrounded;
    private bool wasGrouneded;
    private bool sideFree;
    private bool onSpring;
    private bool boostUp;
    private bool boostSide;
    private bool boostLeft;
    private bool boostRight;
	bool isJumping = false;
	bool leftGround = false;
	bool facingRight = true;
    private bool isBall;

    public GameObject bloodExplosion;
	public Transform groundCheckLeft;
    public Transform groundCheckRight;
    private Vector3 groundCheckOffset = new Vector3(0, 0.1f, 0);

    public Transform sideCheckTop;
    public Transform sideCheckBottom;

    public Transform altSideCheckTop;
    public Transform altSideCheckBottom;

    public Transform headCheckLeft;
    public Transform headCheckRight;

    public LayerMask layersToLandOn;
	private Rigidbody2D rigidbody2D;

    private CapsuleCollider2D collider2D;

    private Vector3 startPosition;

    private float timeFromGround;

    private List<Collider2D> previousColliders = new List<Collider2D>();

	// Use this for initialization
	void Start ()
	{
		rigidbody2D = GetComponent<Rigidbody2D>();
		animator = GetComponent<Animator>();
	    collider2D = GetComponent<CapsuleCollider2D>();

	    startPosition = transform.position;
	}
	
	// Update is called once per frame
	void Update ()
	{
	    wasGrouneded = isGrounded;

	    isBall = isBall || isJumping;

	    if (transform.position.y < -6)
	    {
	        Respawn();
	    }

	    RaycastHit2D raycast = CheckGround();

	    isGrounded = !(!raycast || raycast.distance > groundCheckOffset.y + 0.01f);

	    if (onSpring)
	    {
	        velocity.y = springForce;
	        onSpring = false;
	        isJumping = false;
	        isBall = false;
	    }

	    if (boostUp)
	    {
	        isJumping = false;
	        isBall = true;
	        boostUp = false;
	        boostSide = false;
	    }

	    if (boostSide)
	    {
	        isJumping = false;
	        isBall = true;
	    }


	    if (!isGrounded)
	    {
	        velocity.y -= gravity * Time.deltaTime;

	        if (raycast && raycast.distance - groundCheckOffset.y <= -(velocity.y * Time.deltaTime))
	        {
	            velocity.y = -(raycast.distance - groundCheckOffset.y) / Time.deltaTime;

	            if (raycast.collider.CompareTag("Spring"))
	            {
	                var spring = raycast.collider.GetComponent<Spring>();
	                spring.OnPlayerCollision();
	                onSpring = true;
	            }
	        }

	        raycast = CheckHead();

	        if (raycast && raycast.distance <= velocity.y * Time.deltaTime)
	        {
	            velocity.y = raycast.distance / Time.deltaTime;
	        }

	        if (isJumping && velocity.y > 0 && Input.GetButtonUp("Jump"))
	        {
	            velocity.y /= 2;
	        }

	        if (Input.GetButtonDown("Jump") && timeFromGround < 0.15f && !isJumping)
	        {
	            velocity.y += jumpForce;
	            isJumping = true;
	        }

	        timeFromGround += Time.deltaTime;
	    }
	    else
	    {

	        isJumping = false;
	        isBall = false;
	        timeFromGround = 0;

	        if (velocity.y < 0)
	        {
	            velocity.y = 0;
	        }

	        raycast = CheckGround();

	        if (raycast)
	        {

                //if (raycast.collider.CompareTag("Spring"))
                switch(raycast.collider.tag)
                {
                    case "Spring":
    	            {
    	                var spring = raycast.collider.GetComponent<Spring>();
    	                spring.OnPlayerCollision();
    	                onSpring = true;
    	            } break;
                    case "Spike":
                    {
                        // ...
                    } break;
                }


	            if (raycast.distance <= groundCheckOffset.y + Mathf.Epsilon)
	            {
	                velocity.y += (groundCheckOffset.y - raycast.distance);
	            }
	        }

	        if (Input.GetButtonDown("Jump"))
	        {
	            velocity.y += jumpForce;
	            isJumping = true;
	        }
	    }

	    ///
	    /// Horizontal movement
	    ///

	    var move = Input.GetAxisRaw("Horizontal");

	    if ( move > 0 && !facingRight
	         || move < 0 && facingRight )
	    {
	        Flip ();
	    }

	    if (Mathf.Abs(velocity.x) <= maxSpeed)
	    {
	        velocity.x = move * maxSpeed;
	    }
	    else
	    {
	        if (Mathf.Abs(velocity.x) - maxSpeed > gravity * Time.deltaTime)
	        {
	            velocity.x -= Mathf.Sign(velocity.x) * gravity * Time.deltaTime;
	        }
	        else
	        {
	            velocity.x = maxSpeed;

	            if (boostSide)
	            {
	                boostSide = false;
	            }
	        }
	    }


	    raycast = CheckSide(velocity.x);

	    var blocked = raycast && raycast.distance <= Mathf.Abs(velocity.x * Time.deltaTime);

	    if (blocked)
	    {
	        velocity.x = raycast.distance * Mathf.Sign(velocity.x) / Time.deltaTime;
	    }

	    if (boostSide)
	    {
	        //velocity.y = 0;
	    }

	    transform.position += (Vector3)velocity * Time.deltaTime;

	    ///
	    /// Collisions
	    ///
	    var colliders =
	        Physics2D.OverlapCapsuleAll(transform.position + (Vector3) collider2D.offset, collider2D.size, collider2D.direction, 0);
	    foreach (var collider in colliders)
	    {
	        Debug.Log(collider.name);
	        if (collider.CompareTag("Pickup"))
	        {
	            startPosition = collider.transform.position;

	            var pickup = collider.GetComponent<Pickup>();
	            pickup.OnPlayerCollision();
	        }
	        else if (collider.CompareTag("Spike"))
	        {
	            Respawn();
	        }

	        if (previousColliders.Contains(collider)) continue;

	        if (collider.CompareTag("BoostUp"))
	        {
	            Debug.Log("What");

	            boostUp = true;

	            velocity.y = boostUpForce;
	            velocity.y += collider.transform.position.y - transform.position.y;
	        }
	        else if (collider.CompareTag("BoostLeft"))
	        {
	            boostSide = true;

	            velocity.x = -boostSideForce;
	            //velocity.x -= other.transform.position.x - transform.position.x;
	        }
	        else if (collider.CompareTag("BoostRight"))
	        {
	            boostSide = true;

	            velocity.x = boostSideForce;
	            //velocity.x += other.transform.position.x - transform.position.x;
	        }
	    }

	    previousColliders = colliders.ToList();

	    animator.SetFloat("Speed", Mathf.Abs(velocity.x / maxSpeed));
		animator.SetBool("IsGrounded", isGrounded );
		animator.SetBool("IsJumping", isBall );

		if (wasGrouneded && !isGrounded)
		{
			leftGround = true;
			animator.SetTrigger("LeftGround");
		}


	}

    private RaycastHit2D CheckSide(float sign)
    {
        var rays = new List<RaycastHit2D>
        {
            Physics2D.Raycast(sideCheckBottom.position, Vector2.right * Mathf.Sign(sign), 10,
                1 << LayerMask.NameToLayer("Solid")),
            Physics2D.Raycast(sideCheckTop.position, Vector2.right * Mathf.Sign(sign), 10,
                1 << LayerMask.NameToLayer("Solid")),
            Physics2D.Raycast(altSideCheckBottom.position, Vector2.right * Mathf.Sign(sign), 10,
                1 << LayerMask.NameToLayer("Solid")),
            Physics2D.Raycast(altSideCheckTop.position, Vector2.right * Mathf.Sign(sign), 10,
                1 << LayerMask.NameToLayer("Solid"))
        };

        return rays.Where(ray => ray)
            .OrderBy(ray => ray.distance)
            .FirstOrDefault();
    }

    private RaycastHit2D CheckGround()
    {
        var left = Physics2D.Raycast(groundCheckLeft.position, Vector2.down, 10,
            1 << LayerMask.NameToLayer("Solid"));

        var right = Physics2D.Raycast(groundCheckRight.position, Vector2.down, 10,
            1 << LayerMask.NameToLayer("Solid"));

        if (left && right)
        {
            return left.distance > right.distance ? right : left;
        }

        return left ? left : right;
    }

    private RaycastHit2D CheckHead()
    {
        var left = Physics2D.Raycast(headCheckLeft.position, Vector2.up, 10,
            1 << LayerMask.NameToLayer("Solid"));

        var right = Physics2D.Raycast(headCheckRight.position, Vector2.up, 10,
            1 << LayerMask.NameToLayer("Solid"));

        if (left && right)
        {
            return left.distance > right.distance ? right : left;
        }

        return left ? left : right;
    }

    /*private void OnCollisionEnter2D(Collision2D collision)
    {
        var other = collision.collider;

        if (other.CompareTag("Spring"))
        {
            if (collision.contacts[0].point.y < transform.position.y)
            {
                isJumping = true;
                leftGround = false;
                rigidbody2D.AddForce(new Vector2(0, springForce));
            }
        }
        else if (other.CompareTag("Tile"))
        {
            if (collision.contacts[0].point.y < transform.position.y)
            {
                isGrounded = true;
            }
        }
    }*/

	void Flip ()
	{
		Vector3 tmp = transform.localScale;
		tmp.x *= -1;
		transform.localScale = tmp;

		facingRight = !facingRight;
	}

    private void Respawn()
    {
        isJumping = false;
        isBall = false;

        if (OnDeath != null)
            OnDeath();

        Instantiate(bloodExplosion, transform.position, Quaternion.identity);
        transform.position = startPosition;
        velocity = Vector2.zero;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log(other.tag);

        if (other.CompareTag("BoostUp"))
        {
            boostUp = true;

            velocity.y = boostUpForce;
            velocity.y += other.transform.position.y - transform.position.y;
        }
        else if (other.CompareTag("BoostLeft"))
        {
            boostSide = true;

            velocity.x = -boostSideForce;
            //velocity.x -= other.transform.position.x - transform.position.x;
        }
        else if (other.CompareTag("BoostRight"))
        {
            boostSide = true;

            velocity.x = boostSideForce;
            //velocity.x += other.transform.position.x - transform.position.x;
        }
    }
}
