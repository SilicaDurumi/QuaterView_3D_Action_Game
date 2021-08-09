using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    public enum Type { Melee, Range };
    public Type type;
    public int damage;
    public float rateOfAttack; //rateOfAttack = 0 => double barrel shotgun
    public int maxAmmo;
    public int currentAmmo;

    public BoxCollider meleeArea;
    public TrailRenderer trailEffect;
    public Transform bulletPosition;
    public GameObject bullet;
    public Transform bulletCasePosition;
    public GameObject bulletCase;

    public void Use()
    {
        if(type == Type.Melee)
        {
            StopCoroutine("Swing");
            StartCoroutine("Swing");
        }
        else if(type == Type.Range && currentAmmo > 0)
        {
            currentAmmo--;
            StartCoroutine("Shot");
        }
    }
    IEnumerator Swing()
    {
        //execute 1
        yield return new WaitForSeconds(0.1f); //0.1 sec wait
        meleeArea.enabled = true;
        trailEffect.enabled = true;

        // execute 2
        yield return new WaitForSeconds(0.3f); //0.5 sec wait
        meleeArea.enabled = false;

        // execute 3
        yield return new WaitForSeconds(0.3f); //1.0 sec wait
        trailEffect.enabled = false;
    }

    IEnumerator Shot()
    {
        GameObject intantBuellet = Instantiate(bullet, bulletPosition.position, bulletPosition.rotation);
        Rigidbody bulletRigid = intantBuellet.GetComponent<Rigidbody>();
        bulletRigid.velocity = bulletPosition.forward * 300;

        yield return null;

        GameObject intantBuelletCase = Instantiate(bulletCase, bulletCasePosition.position, bulletCasePosition.rotation);
        Rigidbody bulletCaseRigid = intantBuelletCase.GetComponent<Rigidbody>();
        Vector3 caseVec = bulletCasePosition.forward * Random.Range(-3, -2) + Vector3.up * Random.Range(2, 3);

        bulletCaseRigid.AddForce(caseVec, ForceMode.Impulse);
        bulletCaseRigid.AddTorque(Vector3.up * 10, ForceMode.Impulse);
    }
}
