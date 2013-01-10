using UnityEngine;
using System.Collections;

// Require these components when using this script
[RequireComponent(typeof (Animator))]
[RequireComponent(typeof (CapsuleCollider))]
[RequireComponent(typeof (Rigidbody))]
public class BotControlScript : MonoBehaviour, INetworkListener
{
	[System.NonSerialized]					
	public float lookWeight;					// the amount to transition when using head look
	
	public Transform HeadTarget;				// a transform to Lerp the camera to during head look
	
	public float animSpeed = 1.5f;				// a public setting for overall animator animation speed
	public float lookSmoother = 3f;				// a smoothing setting for camera motion
	public bool useCurves;						// a setting for teaching purposes to show use of curves

	
	private Animator anim;							// a reference to the animator on the character
	private CapsuleCollider col;					// a reference to the capsule collider of the character
	

	static int idleState = Animator.StringToHash("Base Layer.Idle");	
	static int locoState = Animator.StringToHash("Base Layer.Locomotion");			// these integers are references to our animator's states
	static int jumpState = Animator.StringToHash("Base Layer.Jump");				// and are used to check state for various actions to occur
	static int jumpDownState = Animator.StringToHash("Base Layer.JumpDown");		// within our FixedUpdate() function below
	static int fallState = Animator.StringToHash("Base Layer.Fall");
	static int rollState = Animator.StringToHash("Base Layer.Roll");
	static int waveState = Animator.StringToHash("Layer2.Wave");

    PlayerMovePackage.Direction currentDirection = PlayerMovePackage.Direction.Stop;

	void Start ()
	{		
		// initialising reference variables
		anim = GetComponent<Animator>();					  
		col = GetComponent<CapsuleCollider>();
		
		lookWeight = 1;
		
		if(anim.layerCount ==2)
			anim.SetLayerWeight(1, 1);
		
		Client.Instance.AddListener(this);
	}

    void FixedUpdate()
    {
        PlayerMovePackage.Direction dir = PlayerMovePackage.Direction.Stop;

        if (Input.GetKey(Options.Controls.Forward))
            dir = dir.Add(PlayerMovePackage.Direction.Up);
        if (Input.GetKey(Options.Controls.Backward))
            dir = dir.Add(PlayerMovePackage.Direction.Back);
        if (Input.GetKeyDown(Options.Controls.StrafeLeft))
            dir = dir.Add(PlayerMovePackage.Direction.Left);
        if (Input.GetKeyDown(Options.Controls.StrafeRight))
            dir = dir.Add(PlayerMovePackage.Direction.Right);

        if (currentDirection != dir)
        {
            PlayerMovePackage pmp = new PlayerMovePackage(transform.root.position, transform.root.rotation.eulerAngles, dir);
            Client.Instance.SendData(pmp);
			
			
			print ("Changed: " + dir);
        }

        currentDirection = dir;
    }

    public void OnDataReceived(DataPackage dp)
    {
        PlayerMovePackage pmp = dp as PlayerMovePackage;
        if(pmp == null)
            return;

        //set position
        transform.root.position = pmp.Position;

        //set rotation
        float newRotX = pmp.Rotation.x - transform.root.rotation.eulerAngles.x;
        float newRotY = pmp.Rotation.y - transform.root.rotation.eulerAngles.y;
        float newRotZ = pmp.Rotation.z - transform.root.rotation.eulerAngles.z;
        transform.root.Rotate(newRotX, newRotY, newRotZ);

        //set direction
        if (pmp.Dir.Has(PlayerMovePackage.Direction.Stop))
            anim.SetFloat("Speed", 0);
        if (pmp.Dir.Has(PlayerMovePackage.Direction.Up))
            anim.SetFloat("Speed", 1);
        if (pmp.Dir.Has(PlayerMovePackage.Direction.Back))
            anim.SetFloat("Speed", -1);

        anim.speed = animSpeed;
        anim.SetLookAtWeight(lookWeight);

        //float h = Input.GetAxis("Horizontal");				// setup h variable as our horizontal input axis
        //float v = Input.GetAxis("Vertical");				// setup v variables as our vertical input axis		
        //print(h + " | " + v);
        //anim.SetFloat("Speed", v);							// set our animator's float parameter 'Speed' equal to the vertical input axis				
        //anim.SetFloat("Direction", h); 						// set our animator's float parameter 'Direction' equal to the horizontal input axis		
        //anim.speed = animSpeed;								// set the speed of our animator to the public variable 'animSpeed'
        //anim.SetLookAtWeight(lookWeight);					// set the Look At Weight - amount to use look at IK vs using the head's animation	}
    }
}
