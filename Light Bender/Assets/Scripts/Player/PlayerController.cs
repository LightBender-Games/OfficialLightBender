﻿using System.Collections;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using Hashtable = ExitGames.Client.Photon.Hashtable;
public class PlayerController : MonoBehaviourPunCallbacks,IDamageable
{
    [SerializeField] GameObject cameraHolder;
    
    [SerializeField] float mouseSensivity, sprintSpeed, walkSpeed, jumpForce, smoothTime;

    [SerializeField] float respawnTime;
    
    [SerializeField]  Item[] items;
    
    [SerializeField] ProgressBarPro _progressBarPro;
    
    [SerializeField]  ProgressBarPro munitionsSlider;

    [SerializeField]  GameObject munitionObject;
    
    [SerializeField] GameObject health;

    int itemIndex;
    int previousItemIndex = -1;

    int oresHolded;
    public bool hasOre => oresHolded != 0;

    public bool IsLocal => isLocal;

    public bool isLocal;

    private bool canRespawn = true;
     
    float verticalLookRotation;
    bool grounded;
    Vector3 smoothMoveVelocity;
    Vector3 moveAmount;

    Rigidbody rb;

    public PhotonView Phv;

    private Animator animator;

    PlayerManager playerManager;
    
    public GameObject lastShooter;
    
    public TextMeshProUGUI blueScoreText;
    public TextMeshProUGUI redScoreText;
    
    Renderer[] visuals;
    int team;

    public bool PlayerOnlyLook;

    private const float maxHealth = 100f;
    public float currentHealth = maxHealth;

    private SingleShot singleshot;

    public Chat chat;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        Phv = GetComponent<PhotonView>();
        //playerManager = PhotonView.Find((int)Phv.InstantiationData[0]).GetComponent<PlayerManager>();

        if (!PlayerManager.players.Contains(this))
        {
            PlayerManager.players.Add(this);
        }
        // playerController is instantiated on each machine in the room. 
        // by doing this, it will locally, on each machine in the room 
        // (PhotonNetwork.Instantiate() creates the object for all machines)
        // add the object to the players. 
        
        // just have to test it out, but it SHOULD work
        
        
        // DEBUG
        /*Debug.Log("Displaying PlayerManager.players on machine of player "+name+":");
        for (int i = 0; i < PlayerManager.players.Count; i++)
        {
            Debug.Log("PLayer Name:" + PlayerManager.players[i].name);
        }*/
    }

    void Start()
    {
        //Debug.Log("Starting");
        if (Phv.IsMine)
        {
            //Debug.Log("Phv is mine");
            EquipItem(0);
            animator = GetComponent<Animator>();
            health.SetActive(true);
            munitionObject.SetActive(true);
            PauseMenu.isleft = true;
            
            if (items[itemIndex] is SingleShot)
            {
                singleshot = (SingleShot) items[itemIndex];
                singleshot.PlayerOwner = this;
                //Debug.Log("Name " + items[itemIndex].name);
                //Debug.Log(singleshot.nbballes + " :::: " + singleshot.nbinit);
            }
            
            visuals = GetComponentsInChildren<Renderer>();
            team = (int)PhotonNetwork.LocalPlayer.CustomProperties["Team"];
            Debug.Log("Instantiation is finished");
        }
        else
        {
            Debug.Log("Destroy component");
            Debug.Log("Owner name of phv: "+Phv.Owner.NickName);
            Destroy(GetComponentInChildren<Camera>().gameObject);
            Destroy(rb);
        }
    }
    
    void Update()
    {
        //Debug.Log(PauseMenu.GameIsPaused + "  <>  " + Phv.IsMine );
        if (!Phv.IsMine || PauseMenu.GameIsPaused)
            return;
        
        
        Look();
        if (!PlayerOnlyLook)
        {
            Move();
            Jump();
        }

        for (int i = 0; i < items.Length; i++)
        {
            if (Input.GetKeyDown((i + 1).ToString()))
            {
                EquipItem(i);
                break;
            }
        }

        if (Input.GetAxisRaw("Mouse ScrollWheel") < 0f)
        {
            if (itemIndex >= items.Length - 1)
            {
                EquipItem(0);
                Debug.Log("Equip item");
            }
            else
            {
                EquipItem(itemIndex + 1);
            }
        }
        else if (Input.GetAxisRaw("Mouse ScrollWheel") > 0f)
        {
            if (itemIndex <= items.Length - 1)
            {
                EquipItem(items.Length - 1);
            }
            else
            {
                EquipItem(itemIndex - 1);
            }
           
        }

        if (Input.GetMouseButton(0))
        {
            items[itemIndex].Use();
        }

        /*if (transform.position.y < -10f)
        {
            Die();
        }*/
        if (Input.GetKeyDown("1"))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        if (Input.GetKeyDown("2"))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (Input.GetKeyDown("r"))
        {
            StartCoroutine(singleshot.Reload(singleshot.secondsToReload));
        }
    }

    void Move()
    {
        //Debug.Log("Movement activated");
        Vector3 moveDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;

        moveAmount = Vector3.SmoothDamp(moveAmount, moveDir * (Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed),
            ref smoothMoveVelocity, smoothTime);
        
        bool isWalking = animator.GetBool("IsWalking");
        bool pressedwalk = Input.GetKey("w");
        
        bool isRunning = animator.GetBool("IsRunning");
        bool pressedrun = Input.GetKey(KeyCode.LeftShift);
        
        bool isLeft = animator.GetBool("IsLeftWalk");
        bool isRight = animator.GetBool("IsRightWalk");
        bool pressedleft = Input.GetKey("a");;
        bool pressedright = Input.GetKey("d");;
        
        bool isDance = animator.GetBool("IsDance");
        bool presseddance = Input.GetKey("l");;
        //Debug.Log("Movement");
        
        // walk
        if (!isWalking && pressedwalk)
        {
            animator.SetBool("IsWalking",true);
            //Debug.Log("Walking");
        }
        if (isWalking && !pressedwalk)
        {
            animator.SetBool("IsWalking",false);
        }
        // run
        if(!isRunning && pressedrun)
        {
            animator.SetBool("IsRunning",true);
        }
        if (isRunning && !pressedrun)
        {
            animator.SetBool("IsRunning",false);
        }
        // left
        if(!isLeft && pressedleft)
        {
            animator.SetBool("IsLeftWalk",true);
        }
        if (isLeft && !pressedleft)
        {
            animator.SetBool("IsLeftWalk",false);
        }
        // right
        if(!isRight && pressedright)
        {
            animator.SetBool("IsRightWalk",true);
        }
        if (isRight && !pressedright)
        {
            animator.SetBool("IsRightWalk",false);
        }
        
        //dance
        if(!isDance && presseddance)
        {
            animator.SetBool("IsDance",true);
        }
        if (isDance && !presseddance)
        {
            animator.SetBool("IsDance",false);
        }
    }
    void Look()
    {
        transform.Rotate(Vector3.up * (Input.GetAxisRaw("Mouse X") * mouseSensivity));

        verticalLookRotation += Input.GetAxisRaw("Mouse Y") * mouseSensivity;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -90f, 90);
        
        cameraHolder.transform.localEulerAngles = Vector3.left * verticalLookRotation;
    }

    void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && grounded)
        {
            rb.AddForce(transform.up * jumpForce);
        }
    }

    void EquipItem(int _index)
    {
        if (_index == previousItemIndex)
            return;
        
        itemIndex = _index;

        items[itemIndex].itemGameObject.SetActive(true);

        if (previousItemIndex != -1)
        {
            items[previousItemIndex].itemGameObject.SetActive(false);
        }

        
        if (items[itemIndex] is SingleShot)
        {
            singleshot = (SingleShot) items[itemIndex];
            munitionsSlider.SetValue(singleshot.nbballes, singleshot.nbinit);
        }
        else
        {
            Debug.LogError("ERROR");
        }
            
            
        //Debug.Log(singleshot.nbballes + "*/*/*/*/" + singleshot.nbinit);

        previousItemIndex = itemIndex;
        
        if (Phv.IsMine)
        {
            Hashtable hash = new Hashtable {{"itemindex", itemIndex}};
            PhotonNetwork.LocalPlayer.SetCustomProperties(hash);
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (!Phv.IsMine && targetPlayer == Phv.Owner)
        {
            EquipItem((int) changedProps["itemindex"]);
        }
    }

    private void ResetAnimator()
    {
        animator.SetBool("isWalking",false);
        animator.SetBool("IsRunning",false);
        animator.SetBool("IsLeftWalk",false);
        animator.SetBool("IsRightWalk",false);
        animator.SetBool("IsDance",false);
    }

    public void SetGroundedState(bool _grounded)
    {
        grounded = _grounded; 
    }
    public void AddOre(int oresToAdd)
    {
        oresHolded += oresToAdd;
    }
    public int GetOresBeingHeld()
    {
        return oresHolded;
    }
    public void RemoveOres()
    {
        oresHolded = 0;
    }
    public bool GetOnlyLook()
    {
        return PlayerOnlyLook;
    }
    public void SetOnlyLook(bool onlyLook)
    {
        PlayerOnlyLook = onlyLook;
        ResetAnimator();
        rb.velocity = Vector3.zero;
    }
    
    public void SetTeam(int Team)
    {
        team = Team;
    }

    public int GetTeam()
    {
        return team;
    }
    
    void FixedUpdate()
    {
        if (PlayerOnlyLook)
        {
            rb.velocity = Vector3.zero;
            // we do not want the player to move if the player stopped
        }
        if (!Phv.IsMine || PauseMenu.GameIsPaused || PlayerOnlyLook)
            return;

        rb.MovePosition(rb.position + transform.TransformDirection(moveAmount) * Time.fixedDeltaTime);
    }
     
     public void TakeDamage(float damage) // juste sur le shooter
     {
         Phv.RPC("RPC_TakeDamage", RpcTarget.All,damage);
     }
    
     IEnumerator Respawn(float respawnWait)
     {
         canRespawn = false;
         // overflow protection for respawn
         
         SetRenderers(false);

         currentHealth = 100;
         PlayerManager.scores[(team+1)%2] += 1;
         PlayerManager.UpdateScores();
         if (oresHolded != 0)
         {
             SendChatMessage("System",
                 lastShooter.name +" killed " + name);
             RemoveOres();
         }
         
         GetComponent<PlayerController>().enabled = false;
         Transform spawn = SpawnManager.instance.GetTeamSpawn(team);
         transform.position = spawn.position;
         transform.rotation = spawn.rotation;
         GetComponent<PlayerController>().enabled = true;
         
         SendChatMessage("System",
             lastShooter.name +" killed " + name);
         
         yield return new WaitForSeconds(respawnWait);

         currentHealth = 100; 
         // just in case someone manages to shoot the player when waiting
         
         _progressBarPro.SetValue(100f,100f);

         SetRenderers(true);
         canRespawn = true;
     }

     void SetRenderers(bool state)
     {
         foreach (var renderer in visuals)
         {
             renderer.enabled = state;
         }
     }

     public void SendChatMessage(string sender, string message)
     {
         Phv.RPC("SendChat",RpcTarget.All,sender,message);
     }


     [PunRPC]
     void RPC_TakeDamage(float damage)
     {
         if (!Phv.IsMine)
             return;

         currentHealth -= damage;
         _progressBarPro.SetValue(currentHealth,100f);
         if (currentHealth <= 0 && canRespawn)
         {
             StartCoroutine(Respawn(respawnTime));
         }
     }
     
     [PunRPC]
     void SendChat(string sender, string message)
     {
         ChatMessage m = new ChatMessage(sender,message);

         GameManager.chatMessages.Insert(0, m);
         if(GameManager.chatMessages.Count > 8)
         {
             GameManager.chatMessages.RemoveAt(GameManager.chatMessages.Count - 1);
         }

         Chat.chatMessages = GameManager.chatMessages;
         // responsible for the synchronisation of all messages
     }
}