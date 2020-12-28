using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Weapon
{
    public string wepName;
    public float damage;
    public float RPS; //rounds per second
    [HideInInspector]
    public float currentCD;
    public GameObject bullet;
    public GameObject displayGraphic;
    public Transform gunpoint;
    public float range;
}

public class MainPlayerScripts : MonoBehaviour
{
    [Header("Setup")]
    public GameObject player;
    public Rigidbody playerRB;
    public Transform cameraPivot;
    public Vector3 offset;
    [Tooltip("set to negative for camera to follow cursor, positive to oppose cursor")]
    public float mouseOffsetStr;
    [Tooltip("affects camera angle tracking.")]
    public float angleOffsetStr;
    public Text devUI;


    [Header("Player Stats")]
    [Tooltip("All player-based values are set here.")]
    public float playerSpeed;
    [Range(0,2)]
    [Tooltip("How much you are slowed by walking in a direction the cursor is not aiming in. 2 prevents all motion, 1 will stop the player when walking backwards, 0 is no slowdown.")]
    public float walkMod;
    public float jumpStr; //jump strength, no particular unit
    [Range(0,10)]
    public float jumpCD; //time between jumps, in seconds
    [HideInInspector]
    private float jumpcooldown;
    private float aimAngle;


    [Header("Weapons Systems")]
    public Weapon[] weapons;
    public int selectedWep;
    private int prevSelected;

    public List<KeyCode> weaponKeys = new List<KeyCode>{ KeyCode.Alpha1, KeyCode.Alpha2, KeyCode.Alpha3, KeyCode.Alpha4 };



    void Start()
    {
        Application.targetFrameRate = 60; //60 FPS so my computer isn't trying to get 2000

        foreach(Weapon wep in weapons)
        {
            wep.displayGraphic.SetActive(false);
        }
        weapons[selectedWep].displayGraphic.SetActive(true);
    }




    void Update()

    {
        SetCameraAngle();
        UpdateStats();
        DoMovement();

        SelectWep();
        FireWep();
         
    }


    void SetCameraAngle()
    {
        //Coordinate system: +X is right, +Z is up. Rotation about +y axis is CW (opposite of math convention)

        Vector3 mouseOffset = mouseOffsetStr * new Vector3(Input.mousePosition.x / (Screen.width) - 0.5f, 0, Input.mousePosition.y / (Screen.height) - 0.5f);
        aimAngle = Mathf.Atan2(Input.mousePosition.y - Screen.height * 0.5f, Input.mousePosition.x - Screen.width * 0.5f); // aim is based off of the center of the screen, should be fine for most things
        player.transform.localEulerAngles = new Vector3(0, -aimAngle * Mathf.Rad2Deg + 90, 0);
        transform.position = player.transform.position + offset - mouseOffset; //set camera position

        cameraPivot.localEulerAngles = new Vector3(90 - mouseOffset.z * angleOffsetStr, 0, -90); //dizzying camera angles
        transform.localEulerAngles = new Vector3(-mouseOffset.x * angleOffsetStr, 0, 90);
    }

    void UpdateStats()
    {
        jumpcooldown = Mathf.Max(0, jumpcooldown - Time.deltaTime);

        if (weapons[selectedWep].currentCD < 0)//this way it will let you fire fullspeed
            weapons[selectedWep].currentCD = 0;

        if (weapons[selectedWep].currentCD > 0) //but you will lose any rounds you don't fire while the mouse is down
            weapons[selectedWep].currentCD -= Time.deltaTime;
    }

    void DoMovement()
    {
        //sas4's movement code was pretty bad. Still fun.
        float speed = playerSpeed * Time.deltaTime;
        Vector3 positionShift = Vector3.zero;

        if (Input.GetKey(KeyCode.W)) //move up
            positionShift += new Vector3(0, 0, speed);
        if (Input.GetKey(KeyCode.D)) //move right
            positionShift += new Vector3(speed, 0, 0);
        if (Input.GetKey(KeyCode.S)) //move down
            positionShift += new Vector3(0, 0, -speed);
        if (Input.GetKey(KeyCode.A)) //move left
            positionShift += new Vector3(-speed, 0, 0);


        if (positionShift != Vector3.zero)
        {
            //playerRB.drag = 1;
            float walkDir = Mathf.Atan2(positionShift.z, positionShift.x);
            playerRB.velocity += positionShift * MovementMod(aimAngle - walkDir);
        }
        //else
        //{
        //    //playerRB.drag = 5;
        //}
        if (Input.GetKey(KeyCode.Space) && jumpcooldown <= 0)
        {
            playerRB.velocity += new Vector3(0, jumpStr, 0);
            jumpcooldown += jumpCD;
        }

    }

    void SelectWep()
    {
        for(int i=0; i<weaponKeys.Count; i++)
        {
            if (Input.GetKeyDown(weaponKeys[i]))
                selectedWep = i;
        }
        selectedWep = Mathf.Min(selectedWep, weapons.Length-1);
        if(selectedWep != prevSelected) //player changed weapons
        {

            weapons[prevSelected].displayGraphic.SetActive(false);
            weapons[selectedWep].displayGraphic.SetActive(true);
            //switch the graphics
            prevSelected = selectedWep;
            Debug.Log("switched weapons.");
        }

    }

    void FireWep()
    {
        if (Input.GetMouseButton(0))
        { 
            while(weapons[selectedWep].currentCD <= 0)
            {
                weapons[selectedWep].currentCD += Mathf.Max(1/weapons[selectedWep].RPS, 0.001f); //10k per second cap
                GameObject shot = Instantiate(weapons[selectedWep].bullet, weapons[selectedWep].bullet.transform.position, weapons[selectedWep].bullet.transform.rotation);
                float shotAngle = player.transform.localEulerAngles.y; //ehh....
                RaycastHit hit;
                shot.transform.localEulerAngles = new Vector3(shot.transform.localEulerAngles.x, shotAngle, shot.transform.localEulerAngles.z);
                if (Physics.Raycast(weapons[selectedWep].gunpoint.position, player.transform.TransformDirection(Vector3.forward), out hit, weapons[selectedWep].range))
                {
                    shot.transform.position = weapons[selectedWep].gunpoint.position;
                    shot.GetComponent<Timeout>().SetBulletScale(hit.distance);
                        Debug.Log("Found an object - distance: " + hit.distance);
                }
                else
                {
                    shot.transform.position = weapons[selectedWep].gunpoint.position;
                    shot.GetComponent<Timeout>().SetBulletScale(weapons[selectedWep].range);
                }
                    
                
            }
        }
    }

    private float MovementMod(float anglediff)
    {
        if (anglediff > Mathf.PI)
            anglediff -= 2 * Mathf.PI;
        else if (anglediff < -Mathf.PI)
            anglediff += 2 * Mathf.PI;

        return Mathf.Max(0, 1 - 0.5f * Mathf.Abs(anglediff / Mathf.PI) * walkMod); //ehh... it works

    }


}
