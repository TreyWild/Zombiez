using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalZombie : MonoBehaviour
{
    
    public float maxHP;
    public float hpRegen;
    public float attackRate; //per second
    public float attackDmg;
    public float speed;
    public float jumpCooldown; //in seconds
    public float turnSpeed; // in deg/sec
    public float jumpStr;
    public float scanRange;



    public float currentHP;
    private float attackCD;
    private float jumpCD;
    private float scanCD;

    public List<string> team;
    public Rigidbody target;

    private Rigidbody self;

    private void Start()
    {
        self = gameObject.GetComponent<Rigidbody>();
    }

    void Update()
    {
        UpdateStats();

        if(scanCD == 0) //so it doesn't check 60 times a second
        {
            ScanForTarget();
            scanCD = 1 + Random.value; //between 1 and 2 seconds
        }

        MoveTowardsTarget();

    }



    void ScanForTarget()
    {
        bool noTarget = true;

        foreach (Rigidbody obj in FindObjectsOfType<Rigidbody>())
        {
            if (obj == self)
                continue;
            if (!team.Contains(obj.tag) && DistShorter(obj))
            {
                target = obj;
                noTarget = false;
            }
        }

        if (noTarget)
            target = null;
    }
    bool DistShorter(Rigidbody targ)
    {
        float targSqrDist = Vector3.SqrMagnitude(targ.transform.position - self.transform.position);
        return (target == null || target == targ|| targSqrDist <= Vector3.SqrMagnitude(target.transform.position - self.transform.position)) && targSqrDist <= scanRange*scanRange;
    }

    void MoveTowardsTarget()
    {
        if (target != null)
        {
            Vector3 motionStep = speed * transform.TransformDirection(new Vector3(0.707107f, 0, 0.707107f)) * Time.deltaTime;
            float aimDir = -Mathf.Atan2(target.position.z - self.position.z, target.position.x - self.position.x);
            aimDir = facingDir * 0.0174533f - aimDir;
            if (aimDir > 3.14159f)
                aimDir -= 6.28318f;
            else if (aimDir < -3.14159f)
                aimDir += 6.28318f;
            float turnStep = turnSpeed * Time.deltaTime;
            if (aimDir > turnStep * 0.0174533f) //turn right
            {
                facingDir -= turnStep;
                self.velocity += motionStep;
            }
            else if (aimDir < -turnStep * 0.0174533f)
            {
                facingDir += turnStep;
                self.velocity += motionStep;
            }
            else
            {
                self.velocity += motionStep;
                Leap();
            }
        }
    }

    void Leap()
    {
        if(jumpCD == 0)
        {
            jumpCD = jumpCooldown;
            self.velocity += speed/60 * transform.TransformDirection(new Vector3(0.707107f, 0, 0.707107f))*jumpStr;
        }
    }

    public float facingDir 
    {
        get { return self.transform.localEulerAngles.y - 45; }
        set { self.transform.localEulerAngles = new Vector3(self.transform.localEulerAngles.x, value + 45, self.transform.localEulerAngles.z); }
    }

    void UpdateStats()
    {
        jumpCD = Mathf.Max(0, jumpCD - Time.deltaTime);
        attackCD = Mathf.Max(0, attackCD - Time.deltaTime);
        scanCD = Mathf.Max(0, scanCD - Time.deltaTime);

        currentHP = Mathf.Min(currentHP + hpRegen * Time.deltaTime, maxHP);
    }



    public void Hit(float damage, float knockback)
    {
        //didn't get around to it yet
    }
}
