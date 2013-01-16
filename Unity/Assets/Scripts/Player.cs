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
    [NonSerialized]
    public int Score;

	Transform laserTarget;
	
	Transform eyesL;
	Transform eyesR;
	
	Transform laserBeamL;
	Transform laserBeamR;

    GameObject gangnamObject;

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
    int fireTimer = 0;

    bool resend = false;

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

        NetworkManager.Instance.Client.AddListener(this);
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

        if (gangnamObject != null)
        {
            gangnamObject.transform.position = this.gameObject.transform.position;
            gangnamObject.transform.rotation = this.gameObject.transform.rotation;
        }
	}
    void FixedUpdate()
    {
        fireTimer++;

        if (firing && fireTimer <= 10)
            SetLasersEnabled(true);
        else
            SetLasersEnabled(false);

        if (!IsControlled)
            return;

        if (firing && fireTimer <= 10)
            Fire();

        HandleMovement();
        HandleFiring();
        HandleFlagPickup();

        if (resend)
        {
            //PlayerMovePackage pmp = new PlayerMovePackage(transform.root.rotation.eulerAngles);
            PlayerMovePackage pmp = null;
            if (sendCounter == 5)
            {
                pmp = new PlayerMovePackage(transform.root.position, transform.root.rotation.eulerAngles, currentDirection);
                sendCounter = 0;
            }
            else
                pmp = new PlayerMovePackage(transform.root.rotation.eulerAngles);

            NetworkManager.Instance.Client.SendData(pmp);
            resend = false;
        }
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
        if (Input.GetKey(Options.Controls.StrafeLeft))
            dir = dir.Add(PlayerMovePackage.Direction.Left);
        if (Input.GetKey(Options.Controls.StrafeRight))
            dir = dir.Add(PlayerMovePackage.Direction.Right);

        if (currentDirection != dir)
        {
            PlayerMovePackage pmp = new PlayerMovePackage(transform.root.position, transform.root.rotation.eulerAngles, dir);
            NetworkManager.Instance.Client.SendData(pmp);

            print("Changed: " + dir);
        }

        currentDirection = dir;
    }
	void HandleFiring()
	{
        // If you press the fire key, or when you are still firing
        if ((!firing && Input.GetKey(Options.Controls.Fire)) || (firing && fireTimer >= 20))
        {
            PlayerMovePackage pmp = new PlayerMovePackage(transform.root.position, transform.root.rotation.eulerAngles, currentDirection);
            NetworkManager.Instance.Client.SendData(pmp);

            FireWeaponPackage fwp = new FireWeaponPackage();
            fwp.Enabled = true;
            fwp.Target = GetComponentInChildren<Camera>().transform.rotation.eulerAngles.x;
            NetworkManager.Instance.Client.SendData(fwp);
        }
        else if(firing && !Input.GetKey(Options.Controls.Fire))
        {
            FireWeaponPackage fwp = new FireWeaponPackage();
            fwp.Enabled = false;
            fwp.Target = GetComponentInChildren<Camera>().transform.rotation.eulerAngles.x;
            NetworkManager.Instance.Client.SendData(fwp);
        }
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
        if (!hit)
            return;

        Player otherPlayer = hitInfo.transform.GetComponent<Player>();
		if(otherPlayer == null)
			return;

		ParticleSystem ps = laserTarget.GetComponent<ParticleSystem>();
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

    void OnTriggerEnter(Collider col)
    {
        if (!IsControlled)
            return;

        if (col.transform.root.name.Contains("Base"))
        {
            Base b = col.transform.root.gameObject.GetComponentInChildren<Base>();
            Flag f = GameManager.Instance.GetFlag(this);
            if (b == null || f == null || (b.Owner == null && GameManager.Instance.GetPlayerBase(this) != null) ||
                (b.Owner != null && b.Owner != this))
                return;
            
            if (b.Owner == null)
            {
                //capture base

                BaseCapturePackage bcp = new BaseCapturePackage();
                bcp.PlayerIP = Client.GetLocalIPAddress();
                bcp.BaseId = b.BaseId;
                NetworkManager.Instance.Client.SendData(bcp);
            }
            
            FlagPackage fp = new FlagPackage();
            fp.Event = FlagPackage.FlagEvent.Capture;
            fp.FlagId = f.FlagId;
            NetworkManager.Instance.Client.SendData(fp);
        }
        else if (col.gameObject.name.Contains("Flag"))
        {
            Flag f = col.GetComponentInChildren<Flag>();
            if (GameManager.Instance.GetFlag(this) != null || f == null || f.Owner != null)
                return;

            FlagPackage fp = new FlagPackage();
            fp.Event = FlagPackage.FlagEvent.PickUp;
            fp.FlagId = f.FlagId;
            NetworkManager.Instance.Client.SendData(fp);
        }
    }

    int sendCounter = 0;
    public void OnDataReceived(DataPackage dp)
    {
        if (dp is TokenChangePackage)
        {
            if (sendCounter == 0)
            {
                resend = true;
                sendCounter = 0;
            }
            else
                sendCounter++;
        }
        else if (dp is PlayerMovePackage && dp.SenderRemoteIPEndpoint.Address.Equals(PlayerIP))
        {
            PlayerMovePackage pmp = (PlayerMovePackage)dp;

            if (!pmp.RotationOnly)
            {
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

                if (pmp.Dir == PlayerMovePackage.Direction.Stop)
                {
                    gangnamObject = (GameObject)Instantiate(GameManager.Instance.gangnamPrefab);
                    gangnamObject.transform.position = this.gameObject.transform.position;
                    gangnamObject.transform.rotation = this.gameObject.transform.rotation;
                    gangnamObject.transform.localScale += this.gameObject.transform.localScale;
                    this.GetComponentInChildren<SkinnedMeshRenderer>().enabled = false;

                    SkinnedMeshRenderer thisMR = (SkinnedMeshRenderer)this.GetComponentInChildren(typeof(SkinnedMeshRenderer));
                    SkinnedMeshRenderer gangnamMR = (SkinnedMeshRenderer)gangnamObject.GetComponentInChildren(typeof(SkinnedMeshRenderer));
                    gangnamMR.material = thisMR.material;
                }
                else
                {
                    this.GetComponentInChildren<SkinnedMeshRenderer>().enabled = true;
                    Destroy(gangnamObject);
                }

                anim.speed = animSpeed;

                //float h = Input.GetAxis("Horizontal");				// setup h variable as our horizontal input axis
                //float v = Input.GetAxis("Vertical");				// setup v variables as our vertical input axis		
                //print(h + " | " + v);
                //anim.SetFloat("Speed", v);							// set our animator's float parameter 'Speed' equal to the vertical input axis				
                //anim.SetFloat("Direction", h); 						// set our animator's float parameter 'Direction' equal to the horizontal input axis		
                //anim.speed = animSpeed;								// set the speed of our animator to the public variable 'animSpeed'
                //anim.SetLookAtWeight(lookWeight);					// set the Look At Weight - amount to use look at IK vs using the head's animation	}
            }
            else if(!IsControlled)
            {
                //set rotation
                float newRotX = pmp.Rotation.x - transform.root.rotation.eulerAngles.x;
                float newRotY = pmp.Rotation.y - transform.root.rotation.eulerAngles.y;
                float newRotZ = pmp.Rotation.z - transform.root.rotation.eulerAngles.z;
                transform.root.Rotate(newRotX, newRotY, newRotZ);
            }
        }
        else if (dp is FlagPackage && dp.SenderRemoteIPEndpoint.Address.Equals(PlayerIP))
        {
            FlagPackage fp = (FlagPackage)dp;
            if (fp.Event == FlagPackage.FlagEvent.Capture)
                Score += 3;
            else if (fp.Event == FlagPackage.FlagEvent.Drop)
                Score--;
        }
        else if (dp is FireWeaponPackage && dp.SenderRemoteIPEndpoint.Address.Equals(PlayerIP))
        {
            FireWeaponPackage fwp = (FireWeaponPackage)dp;

            //Set laser target
            Camera c = GetComponentInChildren<Camera>();
            float newRotX = fwp.Target - c.transform.rotation.eulerAngles.x;
            c.transform.Rotate(newRotX, 0, 0);

            firing = fwp.Enabled;
            fireTimer = 0;

            //if (Input.GetKeyDown(Options.Controls.Fire) || firing)
            //{
            //    // Update the fire timer
            //    fireTimer++;
            //    if (fireTimer == 1)
            //    {
            //        print("Network send");

            //    }
            //    // As long as it is at the "fire the lazor" state
            //    if (fireTimer >= 0 && fireTimer <= 10)
            //    {
            //        // Enable the laser
            //        SetLasersEnabled(true);
            //        firing = true;
            //    }
            //}
            //// If the key is held up
            //if (Input.GetKeyUp(Options.Controls.Fire))
            //{
            //    // Reset timer, and set firing to false
            //    fireTimer = 0;
            //    SetLasersEnabled(false);
            //    firing = false;
            //}
            //else if (fireTimer > 10)
            //{
            //    if (fireTimer > 20)
            //    {
            //        fireTimer = 0;
            //    }
            //    SetLasersEnabled(false);
            //}

            //if (!firing || !hit)
            //    return;

            //Fire();
        }
    }
}