using UnityEngine;
using System.Collections;

public class RocketLauncher : MonoBehaviour
{
	public Rigidbody2D rocket;				// Prefab of the rocket.
	public float speed = 200f;				// The speed the rocket will fire at.
	
	
	private PlayerControl playerCtrl;		// Reference to the PlayerControl script.
	private Animator anim;					// Reference to the Animator component.
	
	
	
	void Awake()
	{
		// Setting up the references.
		anim = transform.root.gameObject.GetComponent<Animator>();
		playerCtrl = transform.root.GetComponent<PlayerControl>();
	}
	
	
	void Update ()
	{
		//point gun towards mouse
		RotateToFace ();
		
		
		// If the fire button is pressed...
		if(Input.GetButtonDown("Fire1"))
		{
			// ... set the animator Shoot trigger parameter and play the audioclip.
			anim.SetTrigger("Shoot");
			GetComponent<AudioSource>().Play();
			
			// If the player is facing right...
			if(playerCtrl.facingRight)
			{
				// ... instantiate the rocket facing right and set it's velocity to the right. 
				Rigidbody2D bulletInstance = Instantiate(rocket, transform.position, Quaternion.Euler(new Vector3(0,0,0))) as Rigidbody2D;
				bulletInstance.velocity = new Vector2(speed, 0);
			}
			else
			{
				// Otherwise instantiate the rocket facing left and set it's velocity to the left.
				Rigidbody2D bulletInstance = Instantiate(rocket, transform.position, Quaternion.Euler(new Vector3(0,0,180f))) as Rigidbody2D;
				bulletInstance.velocity = new Vector2(-speed, 0);
			}
		}
	}
	
	void RotateToFace(){
		if (Camera.current == null) 
		{ 
			//			Debug.Log("camera is not set.");
			return; 
		}
		
		Vector3 mousePosition = Camera.current.ScreenToWorldPoint(new Vector3(Input.mousePosition.x,Input.mousePosition.y, Input.mousePosition.z - Camera.current.transform.position.z));
		
		//Rotates toward the mouse
		float tempAngle = Mathf.Atan2 ((mousePosition.y - transform.position.y), (mousePosition.x - transform.position.x)) * Mathf.Rad2Deg;
		if (Mathf.Abs (tempAngle) > 90)
			tempAngle += 180;
		//fix aberration when mouse is on left side of player
		if (!playerCtrl.facingRight)
			tempAngle *= -1;
		
		transform.eulerAngles = new Vector3 (0, 0, tempAngle);
	}
}
