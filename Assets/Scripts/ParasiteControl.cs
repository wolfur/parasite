using UnityEngine;
using System.Collections;

public class ParasiteControl : MonoBehaviour
{
	[HideInInspector]
	public bool facingRight = true;			// For determining which way the player is currently facing.
	[HideInInspector]
	public bool jump = false;				// Condition for whether the player should jump.


	public float moveForce = 365f;			// Amount of force added to move the player left and right.
	public float maxSpeed = 5f;				// The fastest the player can travel in the x axis.
	public AudioClip[] jumpClips;			// Array of clips for when the player jumps.
	public float jumpForce = 1000f;			// Amount of force added when the player jumps.
	public AudioClip[] taunts;				// Array of clips for when the player taunts.
	public float tauntProbability = 50f;	// Chance of a taunt happening.
	public float tauntDelay = 1f;			// Delay for when the taunt should happen.


	private int tauntIndex;					// The index of the taunts array indicating the most recent taunt.
	private Transform groundCheck;			// A position marking where to check if the player is grounded.
	private bool grounded = false;			// Whether or not the player is grounded.
	private Animator anim;					// Reference to the player's animator component.
    FollowPlayer healthFollowPlayer;
    CameraFollow cameraFollow;
    Rigidbody2D rigidbody;

    Collider2D collider1;
    Collider2D collider2;

	void Awake()
	{
		// Setting up references.
		groundCheck = transform.Find("groundCheck");
		anim = GetComponent<Animator>();
        healthFollowPlayer = GameObject.FindObjectOfType<FollowPlayer>();
	    cameraFollow = FindObjectOfType<CameraFollow>();
	    rigidbody = GetComponent<Rigidbody2D>();
	}


	void Update()
	{
	    healthFollowPlayer.player = transform;

		// The player is grounded if a linecast to the groundcheck position hits anything on the ground layer.
		grounded = Physics2D.Linecast(transform.position, groundCheck.position, 1 << LayerMask.NameToLayer("Ground"));  

        //Once the parasite hits the ground the decay starts
        if (this.GetComponent<PlayerHealth>().decay == false && grounded)
        {
            this.GetComponent<PlayerHealth>().decay = true;
        }

		// If the jump button is pressed and the player is grounded then the player should jump.
		if(Input.GetButtonDown("Jump") && grounded)
			jump = true;
	}

    void OnCollisionEnter2D (Collision2D col)
    {
        // If the colliding gameobject is an Enemy...
        var go = col.gameObject;
        var host = go.GetComponent<IHost>();
        if (host != null)
        {
            gameObject.SetActive(false);
            var parasiteHealth = GetComponent<PlayerHealth>();
            parasiteHealth.enabled = false;

            var hostHealth = go.GetComponent<PlayerHealth>();
            hostHealth.enabled = true;
            healthFollowPlayer.player = go.transform;
            cameraFollow.player = go.transform;
            host.TakeControl(this);
        }
    }

    public void ReleaseControl(IHost host, Vector3 releasePoint, Vector2? launchDirection = null, float launchForce = 0f)
    {
        var hostControl = ((MonoBehaviour)host);
        hostControl.enabled = false;
        var go = hostControl.gameObject;
        var hostHealth = go.GetComponent<PlayerHealth>();
        hostHealth.enabled = false;
        healthFollowPlayer.player = transform;
        cameraFollow.player = transform;

        transform.position = releasePoint;
        gameObject.SetActive(true);
        collider1 = this.GetComponent<Collider2D>();
        collider2 = go.GetComponent<Collider2D>();
        Physics2D.IgnoreCollision(collider1,collider2 );
        Invoke("ReenableColliders", .25f);
        

        if (launchDirection.HasValue)
        {
            var finalLaunchDirection = launchDirection.Value;
            if (Mathf.Abs(Vector2.Dot(finalLaunchDirection, Vector2.right)) >= 0.9)
            {
                finalLaunchDirection += Vector2.up;
                finalLaunchDirection.Normalize();
            }


            rigidbody.AddForce(finalLaunchDirection * launchForce);
        }


        var parasiteHealth = GetComponent<PlayerHealth>();
        parasiteHealth.enabled = true;
    }

	void FixedUpdate ()
	{
		// Cache the horizontal input.
		float h = Input.GetAxis("Horizontal");

		// The Speed animator parameter is set to the absolute value of the horizontal input.
		anim.SetFloat("Speed", Mathf.Abs(h));

	    if (grounded)
	    {
	        // If the player is changing direction (h has a different sign to velocity.x) or hasn't reached maxSpeed yet...
	        if (h * rigidbody.velocity.x < maxSpeed)
	            rigidbody.AddForce(Vector2.right * h * moveForce);

	        // If the player's horizontal velocity is greater than the maxSpeed...
	        if (Mathf.Abs(rigidbody.velocity.x) > maxSpeed)
	            rigidbody.velocity = new Vector2(Mathf.Sign(rigidbody.velocity.x) * maxSpeed, rigidbody.velocity.y);
	    }

	    // If the input is moving the player right and the player is facing left...
		if(h > 0 && !facingRight)
			// ... flip the player.
			Flip();
		// Otherwise if the input is moving the player left and the player is facing right...
		else if(h < 0 && facingRight)
			// ... flip the player.
			Flip();

		// If the player should jump...
		if(jump)
		{
			// Set the Jump animator trigger parameter.
			anim.SetTrigger("Jump");

			// Play a random jump audio clip.
			int i = Random.Range(0, jumpClips.Length);
			AudioSource.PlayClipAtPoint(jumpClips[i], transform.position);

			// Add a vertical force to the player.
			rigidbody.AddForce(new Vector2(0f, jumpForce));

			// Make sure the player can't jump again until the jump conditions from Update are satisfied.
			jump = false;
		}
	}
	
	
	void Flip ()
	{
		// Switch the way the player is labelled as facing.
		facingRight = !facingRight;

		// Multiply the player's x local scale by -1.
		Vector3 theScale = transform.localScale;
		theScale.x *= -1;
		transform.localScale = theScale;
	}


	public IEnumerator Taunt()
	{
		// Check the random chance of taunting.
		float tauntChance = Random.Range(0f, 100f);
		if(tauntChance > tauntProbability)
		{
			// Wait for tauntDelay number of seconds.
			yield return new WaitForSeconds(tauntDelay);

			// If there is no clip currently playing.
			if(!GetComponent<AudioSource>().isPlaying)
			{
				// Choose a random, but different taunt.
				tauntIndex = TauntRandom();

				// Play the new taunt.
				GetComponent<AudioSource>().clip = taunts[tauntIndex];
				GetComponent<AudioSource>().Play();
			}
		}
	}


	int TauntRandom()
	{
		// Choose a random index of the taunts array.
		int i = Random.Range(0, taunts.Length);

		// If it's the same as the previous taunt...
		if(i == tauntIndex)
			// ... try another random taunt.
			return TauntRandom();
		else
			// Otherwise return this index.
			return i;
	}

    private void ReenableColliders()
    {
        Physics2D.IgnoreCollision(collider1, collider2, false);
    }
}
