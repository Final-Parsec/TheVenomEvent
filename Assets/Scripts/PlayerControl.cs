using UnityEngine;
using System.Collections;
using NetworkView = UnityEngine.NetworkView;

[RequireComponent(typeof(NetworkView))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(AudioSource))]

public class PlayerControl : MonoBehaviour
{
	[HideInInspector]
	public bool facingRight = true;			// For determining which way the player is currently facing.
	[HideInInspector]
	public bool jump = false;				// Condition for whether the player should jump.
	[HideInInspector]
	public bool inControl = false;


	public float moveForce = 365f;			// Amount of force added to move the player left and right.
	public float maxSpeed = 5f;				// The fastest the player can travel in the x axis.
	public AudioClip[] jumpClips;			// Array of clips for when the player jumps.
	public float jumpForce = 1000f;			// Amount of force added when the player jumps.
    public string playerName = "DefaultPlayerName";
    public AudioClip[] taunts;				// Array of clips for when the player taunts.
	public float tauntProbability = 50f;	// Chance of a taunt happening.
	public float tauntDelay = 1f;			// Delay for when the taunt should happen.


	private int tauntIndex;					// The index of the taunts array indicating the most recent taunt.
	private Transform groundCheck;			// A position marking where to check if the player is grounded.
	private bool grounded = false;			// Whether or not the player is grounded.
	private Animator anim;					// Reference to the player's animator component.
	private NetworkView netView;
	private Rigidbody2D rigidBody;


	void Awake()
	{
		// Setting up references.
		groundCheck = transform.Find("groundCheck");
		anim = GetComponent<Animator>();
		netView = this.GetComponent<UnityEngine.NetworkView>();
		rigidBody = GetComponent<Rigidbody2D>();
	}


	void Update()
	{

		if(inControl)
		{
			// The player is grounded if a linecast to the groundcheck position hits anything on the ground layer.
			grounded = Physics2D.Linecast(transform.position, groundCheck.position, 1 << LayerMask.NameToLayer("Ground"));  

			// If the jump button is pressed and the player is grounded then the player should jump.
			if(Input.GetButtonDown("Jump") && grounded)
				jump = true;
		}
	}


	void FixedUpdate ()
	{
		if(inControl)
		{

			// Cache the horizontal input.
			float h = Input.GetAxis("Horizontal");

			// The Speed animator parameter is set to the absolute value of the horizontal input.
			anim.SetFloat("Speed", Mathf.Abs(h));

			// move
			if(h * rigidBody.velocity.x < maxSpeed)
			{
				Vector2 force = Vector2.right * h * moveForce;
				MoveHorizontal(force.x, force.y);
			}

			// change the players facing direction
			if(h > 0 && !facingRight || h < 0 && facingRight)
			{
				Flip();
			}

			// jump
			if(this.jump)
			{
				jump = false;
				Jump();
			}

			netView.RPC("updatePlayer", RPCMode.OthersBuffered, transform.position, new Vector3(rigidBody.velocity.x, rigidBody.velocity.y, 0f));
		}
	}

	void MoveHorizontal(float x, float y)
	{


		// add a force to the player.
		rigidBody.AddForce(new Vector2(x, y));
		
		// If the player's horizontal velocity is greater than the maxSpeed
		if(Mathf.Abs(rigidBody.velocity.x) > maxSpeed)
		{
			// set the player's velocity to the maxSpeed in the x axis.
			rigidBody.velocity = new Vector2(Mathf.Sign(rigidBody.velocity.x) * maxSpeed, rigidBody.velocity.y);
		}
	}

    void Jump()
    {
        // Set the Jump animator trigger parameter.
        anim.SetTrigger("Jump");

        // Play a random jump audio clip.
        int i = Random.Range(0, jumpClips.Length);
        AudioSource.PlayClipAtPoint(jumpClips[i], transform.position);

        // Add a vertical force to the player.
		rigidBody.AddForce(new Vector2(0f, jumpForce));
    }

	void Flip()
	{
		// Switch the way the player is labelled as facing.
		facingRight = !facingRight;

		// Multiply the player's x local scale by -1.
		Vector3 theScale = transform.localScale;
		theScale.x *= -1;
		transform.localScale = theScale;
	}

	[RPC]
	void updatePlayer(Vector3 position, Vector3 velocity){
		transform.position = position;
		rigidBody.velocity = velocity;
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
}
