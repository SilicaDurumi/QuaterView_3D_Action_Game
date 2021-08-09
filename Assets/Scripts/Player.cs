using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public GameObject[] weapons;
    public GameObject[] grenades;
    public GameObject grenadeObject;
    public bool[] hasWeapons;
    public float speed;
    public Camera followCamera;
    public GameManager manager;

    public AudioSource jumpSound;

    public int ammo;
    public int coin;
    public int health;
    public int score;
    public int hasGrenades;
    
    public int maxAmmo;
    public int maxCoin;
    public int maxHealth;
    public int maxhasGrenades;

    int dodgeCount = 0;
    int equipWeaponIndex = -1;

    float hAxis;
    float vAxis;

    float attackDelay;

    bool jumpKeyDown;
    bool interactionKeyDown;
    bool attackKeyDown;
    bool grenadeKeyDown;
    bool reloadKeyDown;
    bool walkKeyDown;
    bool swapKey1Down;
    bool swapKey2Down;
    bool swapKey3Down;

    bool isJumping;
    bool isDodge;
    bool isSwap;
    bool isAttackReady = true;
    bool isReload;
    bool isBorder;
    bool isDamage = false;
    bool isShop = false;
    bool isDead = false;

    Vector3 moveVec;
    Vector3 dodgeVec;

    Rigidbody rigid;
    Animator animator;
    MeshRenderer[] meshs;

    GameObject nearObject;
    public Weapon equipWeapon;


    private void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();
        meshs = GetComponentsInChildren<MeshRenderer>();

        /*        PlayerPrefs.SetInt("MaxScore", 112500);
        */
        Debug.Log(PlayerPrefs.GetInt("MaxScore"));
    }


    // Update is called once per frame
    void Update()
    {
        GetInput();
        Move();
        Turn();
        Jump();
        Dodge();
        Interaction();
        Swap();
        Attack();
        Reload();
        Grenade();
    }

    void GetInput()
    {
        hAxis = Input.GetAxisRaw("Horizontal");
        vAxis = Input.GetAxisRaw("Vertical");
        walkKeyDown = Input.GetButton("Walk");
        jumpKeyDown = Input.GetButtonDown("Jump");
        interactionKeyDown = Input.GetButtonDown("Interaction");
        attackKeyDown = Input.GetButton("Attack1");
        grenadeKeyDown = Input.GetButtonDown("Attack2");
        reloadKeyDown = Input.GetButtonDown("Reload");
        swapKey1Down = Input.GetButtonDown("Swap1");
        swapKey2Down = Input.GetButtonDown("Swap2");
        swapKey3Down= Input.GetButtonDown("Swap3");
    }

    void Move()
    {
        moveVec = new Vector3(hAxis, 0, vAxis).normalized;

        if (isDodge)
            moveVec = dodgeVec;

        if (isSwap || isReload || isDead)
            moveVec = Vector3.zero;

        if(!isBorder)
            transform.position += moveVec * speed * (walkKeyDown ? 0.3f : 1f) * Time.deltaTime;

        animator.SetBool("isRun", moveVec != Vector3.zero);
        animator.SetBool("isWalk", walkKeyDown);

    }

    void Turn()
    {
        transform.LookAt(transform.position + moveVec);

        if (attackKeyDown && !isDead)
        {
            Ray ray = followCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit rayHit;
            if (Physics.Raycast(ray, out rayHit, 100) && !isDodge)
            {
                Vector3 nextVec = rayHit.point - transform.position;
                nextVec.y = 0;
                transform.LookAt(transform.position + nextVec);
            }
        }
    }

    void Jump()
    {
        if (jumpKeyDown && moveVec == Vector3.zero && !isJumping && !isDodge && !isReload && !isDead)
        {
            rigid.AddForce(Vector3.up * 15, ForceMode.Impulse);
            animator.SetBool("isJumping", true);
            animator.SetTrigger("doJump");
            isJumping = true;

            jumpSound.Play();
        }

    }

    void Attack()
    {
        if (equipWeapon == null)
            return;

        attackDelay += Time.deltaTime;
        isAttackReady = equipWeapon.rateOfAttack < attackDelay;

        if(attackKeyDown && isAttackReady && !isDodge && !isSwap && !isReload && !isShop && !isDead)
        {
            equipWeapon.Use();
            animator.SetTrigger(equipWeapon.type == Weapon.Type.Melee ? "doSwing" : "doShot");
            attackDelay = 0;
        }
    }

    void Reload()
    {
        if (equipWeapon == null)
            return;

        if (equipWeapon.currentAmmo == equipWeapon.maxAmmo)
            return;

        if (equipWeapon.type == Weapon.Type.Melee)
            return;

        if (ammo == 0)
            return;

        if(reloadKeyDown && !isDodge && !isJumping && !isSwap && isAttackReady && !isReload && !isShop && !isDead)
        {
            animator.SetTrigger("doReload");
            isReload = true;

            Invoke("ReloadOut", 1.5f);
        }

    }

    void ReloadOut()
    {
        int reloadAmmo = ammo < equipWeapon.maxAmmo ? ammo : equipWeapon.maxAmmo;
        equipWeapon.currentAmmo = reloadAmmo;
        ammo -= reloadAmmo;
        isReload = false;
    }

    void Grenade()
    {
        if(hasGrenades == 0)
            return;
        
        if(grenadeKeyDown && !isReload && !isSwap && !isShop && !isDead)
        {
            Ray ray = followCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit rayHit;
            if (Physics.Raycast(ray, out rayHit, 100))
            {
                Vector3 nextVec = rayHit.point - transform.position;
                nextVec.y = 10;

                GameObject instantGrenade = Instantiate(grenadeObject, transform.position, transform.rotation);
                Rigidbody rigidGrenade = instantGrenade.GetComponent<Rigidbody>();
                rigidGrenade.AddForce(nextVec, ForceMode.Impulse);
                rigidGrenade.AddTorque(Vector3.back * 10, ForceMode.Impulse);

                hasGrenades--;
                grenades[hasGrenades].SetActive(false);
            }
        }
    }

    void Dodge()
    {
        if (jumpKeyDown && moveVec != Vector3.zero && !isJumping && !isDodge && dodgeCount < 2 && !isShop && !isDead)
        {
            dodgeCount += 1;
            dodgeVec = moveVec;
            speed *= 2;
            animator.SetTrigger("doDodge");
            isDodge = true;
            Invoke("DodgeOut", 0.75f); // 1.5f
        }
        else if (jumpKeyDown && moveVec != Vector3.zero && !isJumping && !isDodge && dodgeCount == 2 && !isShop && !isDead)
        {
            dodgeCount += 1;
            dodgeVec = moveVec;
            speed *= 0.5f;
            animator.SetTrigger("doDodge");
            isDodge = true;
            Invoke("DodgeOut", 1.5f); // 1.5f
            Invoke("DodgeCoolDown", 2.5f);
        }
    }

    void DodgeCoolDown()
    {
        speed *= 4;
    }

    IEnumerator DodgeStamina(float delayTime)
    {
        //execute 1
        yield return new WaitForSeconds(delayTime);

        if (dodgeCount > 0 && dodgeCount <= 3)
        {
            dodgeCount -= 1;
        }
    }

    void DodgeOut()
    {
        speed *= 0.5f;
        isDodge = false;
        StartCoroutine("DodgeStamina", 2);
    }


    void Swap()
    {
        if (swapKey1Down && (!hasWeapons[0] || equipWeaponIndex == 0) || 
           (swapKey2Down && (!hasWeapons[1] || equipWeaponIndex == 1))||
           (swapKey3Down && (!hasWeapons[2] || equipWeaponIndex == 2)))
            return;

        int weaponIndex = -1;
        if (swapKey1Down) weaponIndex = 0;
        if (swapKey2Down) weaponIndex = 1;
        if (swapKey3Down) weaponIndex = 2;

        if((swapKey1Down || swapKey2Down || swapKey3Down) && !isJumping && !isDodge && !isShop && !isDead)
        {
            if(equipWeapon != null) 
                equipWeapon.gameObject.SetActive(false);

            equipWeaponIndex = weaponIndex;
            equipWeapon = weapons[weaponIndex].GetComponent<Weapon>();
            equipWeapon.gameObject.SetActive(true);

            animator.SetTrigger("doSwap");

            isSwap = true;

            Invoke("SwapOut", 0.4f);
        }
    }

    void SwapOut()
    {
        isSwap = false;
    }
    void Interaction()
    {
        if(interactionKeyDown && nearObject != null && !isJumping && !isDead) { 
            if(nearObject.tag == "Weapon")
            {
                Item item = nearObject.GetComponent<Item>();
                int weaponIndex = item.value;
                hasWeapons[weaponIndex] = true;

                Destroy(nearObject);
            }
            else if(nearObject.tag == "Shop")
            {
                Shop shop = nearObject.GetComponent<Shop>();
                shop.Enter(this);
                isShop = true;
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.tag == "Floor")
        {
            animator.SetBool("isJumping", false);
            isJumping = false;
        }
    }

    void StopToWall()
    {
        Debug.DrawRay(transform.position, transform.forward * 5, Color.green);
        isBorder = Physics.Raycast(transform.position, transform.forward, 5, LayerMask.GetMask("Wall"));
    }
    
    void FreezeRotation()
    {
        rigid.angularVelocity = Vector3.zero;
    }
    void FixedUpdate()
    {
        FreezeRotation();
        StopToWall();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Item")
        {
            Item item = other.GetComponent<Item>();
            Debug.Log(item.type);
            switch (item.type)
            {
                case Item.Type.Ammo:
                    ammo += item.value;
                    if (ammo > maxAmmo)
                        ammo = maxAmmo;
                    break;
                case Item.Type.Coin:
                    coin += item.value;
                    if (coin > maxCoin)
                        coin = maxCoin;
                    break;
                case Item.Type.Heart:
                    health += item.value;
                    if (health > maxHealth)
                        health = maxHealth;
                    break;
                case Item.Type.Grenade:
                    if (hasGrenades == maxhasGrenades)
                        return;
                    grenades[hasGrenades].SetActive(true);
                    hasGrenades += item.value;
                    break;
            }
            Destroy(other.gameObject);
        }
        else if (other.tag == "EnemyBullet") { 
            if (!isDamage)
            {
                Bullet enemyBullet = other.GetComponent<Bullet>();
                health -= enemyBullet.damage;

                bool isBossAttack = (other.name == "Boss Melee Area");
                StartCoroutine(OnDamage(isBossAttack));
            }
            if (other.GetComponent<Rigidbody>() != null)
                Destroy(other.gameObject);
        }
    }

    IEnumerator OnDamage(bool isBossAttack)
    {
        isDamage = true;
        foreach(MeshRenderer mesh in meshs)
        {
            mesh.material.color = Color.yellow;
        }
        if (isBossAttack)
            rigid.AddForce(transform.forward * -25, ForceMode.Impulse);

        if (health <= 0 && !isDead)
            OnDie();

        yield return new  WaitForSeconds(1f);

        isDamage = false;
        foreach(MeshRenderer mesh in meshs)
        {
            mesh.material.color = Color.white;
        }
        if (isBossAttack)
            rigid.velocity = Vector3.zero;

    }
    
    void OnDie()
    {
        animator.SetTrigger("doDie");
        isDead = true;
        manager.GameOver();
    }
    void OnTriggerStay(Collider other)
    {
        if (other.tag == "Weapon" || other.tag == "Shop")
            nearObject = other.gameObject;

    }
    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Weapon")
            nearObject = null;
        else if(other.tag == "Shop")
        {
            Shop shop = nearObject.GetComponent<Shop>();
            shop.Exit();
            isShop = false;
            nearObject = null;
        }
    }
}
