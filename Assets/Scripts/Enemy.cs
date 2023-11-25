using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal.Profiling.Memory.Experimental.FileFormat;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public enum Type { A, B, C, D };
    public Type enumyType;
    public int maxHealth;
    public int curHealth;
    public Transform target;
    public BoxCollider meleeArea;
    public GameObject bullet;
    public bool isChase;
    public bool isAttack;
    public bool isDead;

    public Rigidbody rigid;
    public BoxCollider boxCollider;
    public MeshRenderer[] meshe;
    public NavMeshAgent nav;
    public Animator anim;
    private IEnumerable<MeshRenderer> meshs;

    void Awake()
    {
        rigid = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
        meshe = GetComponentsInChildren<MeshRenderer>();
        nav = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animator>();

        if(enumyType != Type.D)
        Invoke("ChaseStart", 2);
    }

    void ChaseStart()
    {
        isChase = true;
        anim.SetBool("isWalk", true);
    }
    void Update()
    {
        if (nav.enabled && enumyType != Type.D) {
            nav.SetDestination(target.position);
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

    void Targerting()
    {
        if(!isDead && enumyType != Type.D) { 
       
        }
        float targetRadius = 1.5f;
        float targetRange = 3f;

        switch (enumyType) {
            case Type.A:
                targetRadius = 1f;
                targetRange = 12f;
                break;

            case Type.B:
                targetRadius = 0.5f;
                targetRange = 25f;
                break;

            case Type.C:
                break;
        }

        RaycastHit[] rayHits = Physics.SphereCastAll(transform.position, targetRadius, transform.forward, targetRange, LayerMask.GetMask("Player"));

        if(rayHits.Length > 0 && !isAttack) {

        }
    }

    IEnumerator Attack()
    {
        isChase = false;
        isAttack = true;
        anim.SetBool("isAttack", true);

        switch (enumyType) {
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
                GameObject instantBullet = Instantiate(bullet, transform.position, transform.rotation);
                Rigidbody rigidBullet = instantBullet.GetComponent<Rigidbody>();
                rigidBullet.velocity = transform.forward * 20;

                yield return new WaitForSeconds(2f);
                break;
        }

        isChase = true;
        isAttack = false;
        anim.SetBool("isAttack", false);
    }

    void FixedUpdate()
    {
        Targerting();
        FreezeVelocity();
    }

    void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Melee") {
            Weapon weapon = other.GetComponent<Weapon>();
            curHealth -= weapon.damage;
            Vector3 reactVec = transform.position = other.transform.position;

            StartCoroutine(OnDamage(reactVec, false));
        }
        else if (other.tag == "Bullets") {
            Bullets bullet = other.GetComponent<Bullets>();
            curHealth -= bullet.damage;
            Vector3 reactVec = transform.position = other.transform.position;
            Destroy(other.gameObject);

            StartCoroutine(OnDamage(reactVec, false));
        }
    }
    public void HitByGrenade(Vector3 explosionPos)
    {
        curHealth -= 100;
        Vector3 reactVec = transform.position - explosionPos;
        StartCoroutine(OnDamage(reactVec, true));
    }
    IEnumerator OnDamage(Vector3 reactVec, bool isGrenade)
    {
        foreach (MeshRenderer mesh in meshs)
            mesh.material.color = Color.red;

        yield return new WaitForSeconds(0.1f);

        if (curHealth > 0) {
            foreach (MeshRenderer mesh in meshs)
                mesh.material.color = Color.white;
        }
        else {
            foreach (MeshRenderer mesh in meshs)
                mesh.material.color = Color.gray;

            gameObject.layer = 14;
            isDead = true;
            isChase = false;
            nav.enabled = false;
            anim.SetTrigger("doDie");

            if (isGrenade)
            {
                reactVec = reactVec.normalized;
                reactVec += Vector3.up * 3;

                rigid.freezeRotation = false;
                rigid.AddForce(reactVec * 5, ForceMode.Impulse);
                rigid.AddTorque(reactVec * 15, ForceMode.Impulse);
            }
            reactVec = reactVec.normalized;
            reactVec += Vector3.up;
            rigid.AddForce(reactVec * 5, ForceMode.Impulse);

            if(enumyType != Type.D)
                Destroy(gameObject, 4);
        }
    }
}
