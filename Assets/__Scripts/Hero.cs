using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hero : MonoBehaviour {

    static public Hero S;

    [Header("Set in Inspector")]
    public float speed = 30;            //����, ����������� ��������� �������
    public float rollMult = -45;
    public float pitchMult = 30;
    public float gameRestartDelay = 2f;
    public GameObject projectilePrefab;
    public float projectileSpeed = 40;
    public Weapon[] weapons;

    [Header("Set Dynamically")]
    [SerializeField]
    private float _shieldLevel = 1;

    private GameObject lastTriggerGo = null;  //������ ������ �� ��������� ������������� ������

    public delegate void WeaponFireDelegate();  //���������� ������ �������� ���� WeaponFireDelegate
    public WeaponFireDelegate fireDelegate;     //������� ���� ���� WeaponFireDelegate � ������ fireDelegate

    void Start() {                      //��� �������� �������
        S = this;
        //fireDelegate += TempFire;
        ClearWeapons();
        weapons[0].SetType(WeaponType.blaster);
//        if (S == null) {                //���������, ��� ��� �������� ����� ��� ���
//            S = this;                   //��������� ������ �� ��������
//        } else {
//            Debug.LogError("Hero.Awake() - Attempted to assign second Hero.S!");
//        }
//       fireDelegate += TempFire;
    }
    void Update() {
        float xAxis = Input.GetAxis("Horizontal");
        float yAxis = Input.GetAxis("Vertical");

        //����������� �������
        Vector3 pos = transform.position;
        pos.x += xAxis * speed * Time.deltaTime;
        pos.y += yAxis * speed * Time.deltaTime;
        transform.position = pos;

        //������� ������� 
        transform.rotation = Quaternion.Euler(yAxis * pitchMult, xAxis * rollMult, 0);

//        if (Input.GetKeyDown(KeyCode.Space)) {     //������� �������� �� ������� �������
//            TempFire();
//        }

        if (Input.GetAxis("Jump") == 1 && fireDelegate != null) {  //Input.GetAxis("Jump") ������ 1, ���� ���� ����� ������ ��� ������ �� ������
            fireDelegate();                                        //����� ��������
        }
    }

    void TempFire() {
        GameObject projGO = Instantiate<GameObject>(projectilePrefab);
        projGO.transform.position = transform.position;
        Rigidbody rigidB = projGO.GetComponent<Rigidbody>();
 //       rigidB.velocity = Vector3.up * projectileSpeed;

        Projectile proj = projGO.GetComponent<Projectile>();          //������ �� ��������� Projectile
        proj.type = WeaponType.blaster;                               //������������� ��� ������
        float tSpeed = Main.GetWeaponDefinition(proj.type).velocity;  //������ ������ �������� ������� ��� ������
        rigidB.velocity = Vector3.up * tSpeed;                        //������������� ��� �������� � ������
    }

    private void OnTriggerEnter(Collider other) {
        Transform rootT = other.gameObject.transform.root;
        GameObject go = rootT.gameObject;
        //print("Triggeres: " + go.name);

        if (go == lastTriggerGo) {    //������������� ������������� ������������ � ��� �� ��������
            return;
        }
        lastTriggerGo = go;

        if (go.tag == "Enemy") {      //���� �������� ���� ����������� � ��������� ��������
            shieldLevel--;            //��������� ������� ������ �� 1
            Destroy(go);              //� ���������� �����
        } else if (go.tag == "PowerUp") {
            AbsorbPowerUp(go);
        } else {
            print("Triggered by non-Enemy: " + go.name);
        }
    }

    public void AbsorbPowerUp(GameObject go) { 
        PowerUp pu = go.GetComponent<PowerUp>();
        switch (pu.type) {
            case WeaponType.shield:           //���� ����� ����� ��� WeaponType.shield, �� ����������� ������� ��������� ���� �� 1
                shieldLevel++;
                break;

            default:                          //����� ������ ������� ���� WeaponType � ��� ������, � �������������� � ����� default.
                if (pu.type == weapons[0].type) {      //���� ������ ���� �� ����
                    Weapon w = GetEmptyWeaponSlot();
                    if (w != null) {
                        w.SetType(pu.type);            //���������� � pu.type
                    }
                } else {                               //���� ������ ������� ����
                    ClearWeapons();
                    weapons[0].SetType(pu.type);
                }
                break;
        }
        pu.AbsorbedBy(this.gameObject);
    }

    public float shieldLevel {
        get {
            return (_shieldLevel);
        }
        set {
            _shieldLevel = Mathf.Min(value, 4);
            if (value < 0) {
                Destroy(this.gameObject);
                Main.S.DelayedRestart(gameRestartDelay);  //�������� ������� Main.S � ������������� ������������� ����
            }
        }
    }

    Weapon GetEmptyWeaponSlot() {
        for (int i = 0; i < weapons.Length; i++) {
            if (weapons[i].type == WeaponType.none) {
                return( weapons[i]);
            }
        }
        return(null);
    }

    void ClearWeapons() {
        foreach(Weapon w in weapons) {
            w.SetType(WeaponType.none);
        }
    }
}
