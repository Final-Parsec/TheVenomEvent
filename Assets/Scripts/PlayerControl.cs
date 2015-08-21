using UnityEngine;
using System.Collections;
using System.Collections.Generic;
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
	public bool inControl = false;
	[HideInInspector]
	public NetworkPlayer owner;
	[HideInInspector]
	public NetworkView netView;
	[HideInInspector]
	public Vector3 serverPosition;
	[HideInInspector]
	public Quaternion serverRotation; 
	private float positionErrorThreshold = .1f;

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

	private Rigidbody2D rigidBody;
	private List<Gun> guns;
	private Gun equippedGun;

	private float horizontalInput;
	private bool jump;
	private bool shoot;
	private Vector3 mouseWorldPos;



	void Awake()
	{
		// Setting up references.
		groundCheck = transform.Find("groundCheck");
		anim = GetComponent<Animator>();
		netView = this.GetComponent<UnityEngine.NetworkView>();
		rigidBody = GetComponent<Rigidbody2D>();

		guns = new List<Gun>(GetComponentsInChildren<Gun>());
		if(guns.Count > 0)
			equippedGun = guns[0];
	}


	void Update()
	{
		if(inControl)
		{
			UpdateInput(Input.GetAxis("Horizontal"), Input.GetButtonDown("Jump"), Input.GetButtonDown("Fire1"), Input.mousePosition);

			netView.RPC("SendInput", RPCMode.Server, horizontalInput, jump, shoot, mouseWorldPos);
		}
	}


	void UpdateInput(float horizontalInput, bool jump, bool shoot, Vector3 mousePosition)
	{
		this.horizontalInput = horizontalInput;
		this.jump = jump;
		this.shoot = shoot;
		this.mouseWorldPos = Camera.main.ScreenToWorldPoint(mousePosition);
	}


	void FixedUpdate ()
	{
		if(inControl || Network.isServer)
		{
			grounded = Physics2D.Linecast(transform.position, groundCheck.position, 1 << LayerMask.NameToLayer("Ground"));
			if(jump && grounded)
			{
				jump = false;
				Jump();
			}

			// The Speed animator parameter is set to the absolute value of the horizontal input.
//			anim.SetFloat("Speed", Mathf.Abs(horizontalInput));

//			if (Camera.current != null) 
//			{ 
//				// check if mouse is on the right and player is facing left.  If yes, turn player right.
//				if(Camera.current.ScreenToWorldPoint(new Vector3(Input.mousePosition.x,Input.mousePosition.y, Input.mousePosition.z - Camera.current.transform.position.z)).x
//				   > this.transform.position.x && !this.facingRight)
//				{
//					TurnRight();
//				}
//				// check if mouse is on the left side of the player, and the player is facing left. if yes, turn player left.
//				if(Camera.current.ScreenToWorldPoint(new Vector3(Input.mousePosition.x,Input.mousePosition.y, Input.mousePosition.z - Camera.current.transform.position.z)).x
//				   < this.transform.position.x && this.facingRight)
//				{
//					TurnLeft();
//				}
//			}


			//netView.RPC("UpdatePlayer", RPCMode.Server, transform.position, new Vector3(rigidBody.velocity.x, rigidBody.velocity.y, 0f));

			Vector2 force = Vector2.right * horizontalInput * moveForce;
			MoveHorizontal(force);
			if(shoot){}
			//shoot();
			

		}
	}

	public void lerpToTarget() {
		//Debug.Log("Ran lerp!");

		var distance = Vector3.Distance(transform.position, serverPosition);
		
		//only correct if the error margin (the distance) is too extreme
		if (distance >= positionErrorThreshold) {
			float lerp = ((1f / distance) * 50f) / 100f;

			transform.position = Vector3.Lerp(transform.position, serverPosition, lerp);
			transform.rotation = Quaternion.Slerp(transform.rotation, serverRotation, lerp);
		}
	}

	void MoveHorizontal(Vector2 force)
	{
		if(!grounded || rigidBody.velocity.x > maxSpeed)
			return;

		rigidBody.AddForce(new Vector2(force.x, force.y));

		if(Mathf.Abs(rigidBody.velocity.x) > maxSpeed)
		{
			rigidBody.velocity = new Vector2(Mathf.Sign(rigidBody.velocity.x) * maxSpeed, rigidBody.velocity.y);
		}
	}

	/// <summary>
	/// Turns the player to the right.
	/// </summary>
	void TurnRight(){
		// Switch the way the player is labelled as facing.
		facingRight = true;
		
		// Multiply the player's x local scale by -1.
		Vector3 theScale = transform.localScale;
		theScale.x = Mathf.Abs (theScale.x);  //make x positive
		transform.localScale = theScale;
	}

	/// <summary>
	/// Turns the player to the right
	/// </summary>
	void TurnLeft(){
		// Switch the way the player is labelled as facing.
		facingRight = !facingRight;
		
		// Multiply the player's x local scale by -1.
		Vector3 theScale = transform.localScale;
		theScale.x = Mathf.Abs (theScale.x) * -1; //make x negative
		transform.localScale = theScale;
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

	[RPC]
	void UpdatePlayer(Vector3 position, Vector3 velocity)
	{
		if ((transform.position - position).magnitude > .5)
			transform.position = position;

		rigidBody.velocity = velocity;
	}

	[RPC]
	void SendInput(float horizontalInput, bool jump, bool shoot, Vector3 worldMousePos)
	{

		UpdateInput(horizontalInput, jump, shoot, worldMousePos);

		//Vector3 velocity = rigidBody.velocity;
		//netView.RPC("UpdatePlayer", RPCMode.Others, transform.position, velocity);
	}
}
