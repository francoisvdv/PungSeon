using UnityEngine;
using System;
using System.Collections;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(Rigidbody))]
public class Player : MonoBehaviour, INetworkListener
{
    public float animSpeed = 1.5f;				// a public setting for overall animator animation speed

    public System.Net.IPAddress PlayerIP = Client.GetLocalIPAddress();
    public bool IsControlled { get { return PlayerIP.Equals(Client.GetLocalIPAddress()); } }
	public bool AlwaysFireLaserBeams = false;

	[NonSerialized]
	public int Health;

	Transform laserTarget;
	
	Transform eyesL;
	Transform eyesR;
	
	Transform laserBeamL;
	Transform laserBeamR;

    Animator anim;							                                // a reference to the animator on the character
    CapsuleCollider col;					                                // a reference to the capsule collider of the character
    static int idleState = Animator.StringToHash("Base Layer.Idle");
    static int locoState = Animator.StringToHash("Base Layer.Locomotion");			// these integers are references to our animator's states
    static int jumpState = Animator.StringToHash("Base Layer.Jump");				// and are used to check state for various actions to occur
    static int jumpDownState = Animator.StringToHash("Base Layer.JumpDown");		// within our FixedUpdate() function below
    static int fallState = Animator.StringToHash("Base Layer.Fall");
    static int rollState = Animator.StringToHash("Base Layer.Roll");
    static int waveState = Animator.StringToHash("Layer2.Wave");

    bool hit;
    RaycastHit hitInfo; //updated once per Update(), containing hitInfo about what the laser target has hit

    PlayerMovePackage.Direction currentDirection = PlayerMovePackage.Direction.Stop;
    bool firing = false;


	// Use this for initialization
	void Start ()
	{	
		Health = Options.StartingHealth;
		
		laserTarget = transform.Search("LaserTarget");

		eyesL = transform.Search("EyeL");
		eyesR = transform.Search("EyeR");
		
		laserBeamL = eyesL.Find("LaserParticles");
		laserBeamR = eyesR.Find("LaserParticles");

        // initialising reference variables
        anim = GetComponent<Animator>();
        col = GetComponent<CapsuleCollider>();

        anim.SetLookAtWeight(1);
        if (anim.layerCount == 2)
            anim.SetLayerWeight(1, 1);

        Client.Instance.AddListener(this);
	}

	void Update ()
	{
		//direction is (pointB - pointA).normalized
		Vector3 start = calculateCentroid(eyesL.position, eyesR.position);
		Ray r = new Ray(start, (laserTarget.position - start).normalized);
		hit = Physics.Raycast(r, out hitInfo, Options.LaserTargetDistance);
		if(hit)
			laserTarget.localPosition = new Vector3(laserTarget.localPosition.x, laserTarget.localPosition.y, hitInfo.distance);
		else
			laserTarget.localPosition = new Vector3(laserTarget.localPosition.x, laserTarget.localPosition.y, Options.LaserTargetDistance);
		
		if(AlwaysFireLaserBeams)
			SetLasersEnabled(true);
	}
    void FixedUpdate()
    {
        if (!IsControlled)
            return;

        HandleMovement();
        HandleFiring();
        HandleFlagPickup();
    }

	Vector3 calculateCentroid(params Vector3[] centerPoints)
	{
		var centroid = new Vector3(0,0,0);
		var numPoints = centerPoints.Length;
		
		foreach(var v in centerPoints)
		{
			centroid += v;
		}

		centroid /= numPoints;

		return centroid;
	}
	
	ParticleRenderer GetParticleRenderer(Transform t)
	{
		return t.GetComponent(typeof(ParticleRenderer)) as ParticleRenderer;
	}
	void SetLasersEnabled(bool enable)
	{
		ParticleRenderer r1 = laserBeamL.GetComponent(typeof(ParticleRenderer)) as ParticleRenderer;
		ParticleRenderer r2 = laserBeamR.GetComponent(typeof(ParticleRenderer)) as ParticleRenderer;
		
		if(r1 != null)
			r1.enabled = enable;
		if(r2 != null)
			r2.enabled = enable;
	}

    void HandleMovement()
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

            print("Changed: " + dir);
        }

        currentDirection = dir;
    }
	void HandleFiring()
	{	
		if(Input.GetKeyDown(Options.Controls.Fire))
		{
			SetLasersEnabled(true);
			firing = true;
		}
		else if(Input.GetKeyUp (Options.Controls.Fire))
		{
			SetLasersEnabled(false);
			firing = false;
		}
		
		if(!firing || !hit)
			return;
		
		Fire();
	}
	void HandleFlagPickup()
	{
		PickUpFlag();
	}
	void HandleFlagDelivery()
	{
		DeliverFlag();
	}
	void HandleBlockEffectApplication()
	{
		ApplyBlockEffect(0);
	}
	
	void Die()
	{
		GameObject.Destroy(this.gameObject);
	}
	
	void Fire()
	{
        Player otherPlayer = hitInfo.transform.GetComponent<Player>();
		if(otherPlayer == null)
			return;
		
		//apparantly there's a hit
		
		ParticleSystem ps = laserTarget.GetComponent<ParticleSystem>();
		//if(!ps.isPlaying)
			ps.Play();
		
		otherPlayer.UpdateHealth(otherPlayer.Health - 1);
	}
	public void UpdateHealth(int newAmount)
	{
		Health = newAmount;
		
		if(Health <= 0)
			Die();
	}
	void PickUpFlag()	
	{
	}
	void DeliverFlag()
	{
	}
	void ApplyBlockEffect(int id)
	{
	}

    public void OnDataReceived(DataPackage dp)
    {
        PlayerMovePackage pmp = dp as PlayerMovePackage;
        if (pmp == null)
            return;

        if (transform.root == transform)
            print("YEP");

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

        //float h = Input.GetAxis("Horizontal");				// setup h variable as our horizontal input axis
        //float v = Input.GetAxis("Vertical");				// setup v variables as our vertical input axis		
        //print(h + " | " + v);
        //anim.SetFloat("Speed", v);							// set our animator's float parameter 'Speed' equal to the vertical input axis				
        //anim.SetFloat("Direction", h); 						// set our animator's float parameter 'Direction' equal to the horizontal input axis		
        //anim.speed = animSpeed;								// set the speed of our animator to the public variable 'animSpeed'
        //anim.SetLookAtWeight(lookWeight);					// set the Look At Weight - amount to use look at IK vs using the head's animation	}
    }
}