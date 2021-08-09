using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public enum Type { A, B, C, D};
    public Type enemyType;
    public int maxHealth;
    public int currentHealth;
    public int score;
    public GameManager manager;
    public Transform Target;
    public BoxCollider meleeArea;
    public GameObject bullet;
    public GameObject[] coins;
    public bool isChase;
    public bool isAttack;
    public bool isDead;

    public Rigidbody rigid;
    public BoxCollider boxCollider;
    public MeshRenderer[] meshs;
    public NavMeshAgent nav;
    public Animator animator;

    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
        meshs = GetComponentsInChildren<MeshRenderer>();
        nav = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();

        if(enemyType != Type.D)
            Invoke("ChaseStart", 2);
    }
    
    void ChaseStart()
    {
        isChase = true;
        animator.SetBool("isWalk", true);
    }

    void Update()
    {
        if (nav.enabled && enemyType != Type.D)
        {
            nav.SetDestination(Target.position);
            nav.isStopped = !isChase;
        }
    }

    
    void FreezeVelocity()
    {
        if (isChase)
        {
            rigid.velocity = Vector3.zero;
            rigid.angularVelocity = Vector3.zero;

        }
    }

    void Targeting()
    {   if(!isDead && enemyType != Type.D)
        {
            float targetRadius = 1.5f;
            float targetRange = 3f;

            switch (enemyType)
            {
                case Type.A:
                    targetRadius = 1.5f;
                    targetRange = 3f;
                    break;
                case Type.B:
                    targetRadius = 1f;
                    targetRange = 12f;
                    break;
                case Type.C:
                    targetRadius = 0.3f;
                    targetRange = 30f;
                    break;
            }

            RaycastHit[] rayHits =
                Physics.SphereCastAll(
                    transform.position,
                    targetRadius,
                    transform.forward,
                    targetRange,
                    LayerMask.GetMask("Player")
         );
            if (rayHits.Length > 0 && !isAttack)
            {
                StartCoroutine(Attack());
            }
        }
    }
    IEnumerator Attack()
    {
        isChase = false;
        isAttack = true;
        animator.SetBool("isAttack", true);

        switch (enemyType)
        {
            case Type.A:
                yield return new WaitForSeconds(0.2f);
                meleeArea.enabled = true;

                yield return new WaitForSeconds(1f);
                meleeArea.enabled = false;

                yield return new WaitForSeconds(1f);
                break;
            case Type.B:
                yield return new WaitForSeconds(0.1f);
                rigid.AddForce(transform.forward * 20, ForceMode.Impulse);
                meleeArea.enabled = true;

                
                yield return new WaitForSeconds(0.5f);
                rigid.velocity = Vector3.zero;
                meleeArea.enabled = false;

                yield return new WaitForSeconds(2f);
                break;
            case Type.C:
                yield return new WaitForSeconds(0.5f);
                Debug.Log(transform.position);
                
                GameObject instantBullet = Instantiate(bullet, transform.position + new Vector3(0, 2, 0), transform.rotation);
                Rigidbody rigidBullet = instantBullet.GetComponent<Rigidbody>();
                rigidBullet.velocity = transform.forward * 20;

                yield return new WaitForSeconds(2f);
                break;
        }


        isChase = true;
        isAttack = false;
        animator.SetBool("isAttack", false);
    }

    void FixedUpdate()
    {
        Targeting();
        FreezeVelocity();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Melee")
        {
            Weapon weapon = other.GetComponent<Weapon>();
            currentHealth -= weapon.damage;
            Vector3 reactVec = transform.position - other.transform.position;
            StartCoroutine(OnDamage(reactVec, false));

            Debug.Log("Melee : " + currentHealth);
        }
        else if (other.tag == "Bullet")
        {
            Bullet bullet = other.GetComponent<Bullet>();
            currentHealth -= bullet.damage;
            Vector3 reactVec = transform.position - other.transform.position;
            Destroy(other.gameObject);

            StartCoroutine(OnDamage(reactVec, false));

            Debug.Log("Range : " + currentHealth);
        }
    }
    public void HitByGrenade(Vector3 explosionPosition)
    {
        if (currentHealth <= 100)
        {
            currentHealth -= 100;
            Vector3 reactVec = transform.position - explosionPosition;
            StartCoroutine(OnDamage(reactVec, true));
            Debug.Log("Grenade : " + currentHealth);
        }
        else if(currentHealth > 101 && currentHealth <= 1000)
        {
            currentHealth -= (int)(currentHealth / 2);
            Vector3 reactVec = transform.position - explosionPosition;
            StartCoroutine(OnDamage(reactVec, true));
            Debug.Log("Grenade : " + currentHealth);
        }

        else if(currentHealth > 1000)
        {
            currentHealth -= (int)(currentHealth / 3);
            Vector3 reactVec = transform.position - explosionPosition;
            StartCoroutine(OnDamage(reactVec, true));
            Debug.Log("Grenade : " + currentHealth);
        }
    }
        IEnumerator OnDamage(Vector3 reactVec, bool isGrenade)
        {
            foreach(MeshRenderer mesh in meshs)
                mesh.material.color = Color.red;

        if (currentHealth > 0)
        {
            yield return new WaitForSeconds(0.1f);
            foreach (MeshRenderer mesh in meshs)
                mesh.material.color = Color.white;
        }
        else if(currentHealth <= 0)
        {
            gameObject.layer = 14;
            isDead = true;
            isChase = false;
            nav.enabled = false;
            isAttack = false;

            foreach (MeshRenderer mesh in meshs)
                mesh.material.color = Color.gray;

            animator.SetTrigger("doDie");
            Player player = Target.GetComponent<Player>();
            player.score += score;
            int ranCoin = Random.Range(0, 3);
            Instantiate(coins[ranCoin], transform.position, Quaternion.identity);

            switch (enemyType)
            {
                case Type.A:
                    manager.enemyCountA--;
                        break;
                case Type.B:
                    manager.enemyCountB--;
                    break;
                case Type.C:
                    manager.enemyCountC--;
                    break;
                case Type.D:
                    manager.enemyCountD--;
                    break;
            }

            if (isGrenade)
            {
                reactVec = reactVec.normalized;
                reactVec += Vector3.up;

                rigid.freezeRotation = false;
                rigid.AddForce(reactVec * 30, ForceMode.Impulse);
                rigid.AddTorque(reactVec * 15, ForceMode.Impulse);
            }
            else
            {
                reactVec = reactVec.normalized;
                reactVec += Vector3.up;
                rigid.AddForce(reactVec * 50, ForceMode.Impulse);
            }
                Destroy(gameObject, 0.5f);
        }
        }
    }



