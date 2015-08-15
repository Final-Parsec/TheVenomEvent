using UnityEngine;
using System.Collections;

public class Pistol : Gun
{
	//public Rigidbody2D bullet;				// Prefab of the bullet.
	public float speed = 200f;				// The speed the bullet will fire at.

	private PlayerControl playerCtrl;		// Reference to the PlayerControl script.
	private Animator anim;					// Reference to the Animator component.
	private Vector3 mousePosition;
	public Rigidbody2D projectile;
	
	void Awake()
	{
		// Setting up the references.
		mousePosition = new Vector3 (0, 0, 0);
		anim = transform.root.gameObject.GetComponent<Animator>();
		playerCtrl = transform.root.GetComponent<PlayerControl>();
	}
	
	
	void Update ()
	{
		RotateToFace (gameObject);
	}
	
	void Shoot(Vector3 projectileDirection){
		GetComponent<AudioSource>().Play();

		Rigidbody2D bulletInstance = Instantiate(projectile, transform.position, Quaternion.Euler(new Vector3(0,0,0))) as Rigidbody2D;

		bulletInstance.AddForce(projectileDirection * speed, 0);
	}
	
	public void RotateToFace(GameObject target){
		if (Camera.current == null) 
		{ 
			//			Debug.Log("camera is not set.");
			return; 
		}
		
		mousePosition = Camera.current.ScreenToWorldPoint(new Vector3(Input.mousePosition.x,Input.mousePosition.y, Input.mousePosition.z - Camera.current.transform.position.z));
		
		//Rotates toward the mouse
		float tempAngle = Mathf.Atan2 ((mousePosition.y - transform.position.y), (mousePosition.x - transform.position.x)) * Mathf.Rad2Deg;
		if (Mathf.Abs (tempAngle) > 90)
			tempAngle += 180;
		//fix aberration when mouse is on left side of player
		if (!playerCtrl.facingRight)
			tempAngle *= -1;
		
		target.transform.eulerAngles = new Vector3 (0, 0, tempAngle);
	}

	private double DegreeToRadian(double angle)
	{
		return System.Math.PI * angle / 180.0;
	}


}
