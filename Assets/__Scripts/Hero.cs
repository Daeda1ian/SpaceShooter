using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hero : MonoBehaviour {

    static public Hero S;

    [Header("Set in Inspector")]
    public float speed = 30;            //поля, управляющие движением корабля
    public float rollMult = -45;
    public float pitchMult = 30;
    public float gameRestartDelay = 2f;
    public GameObject projectilePrefab;
    public float projectileSpeed = 40;
    public Weapon[] weapons;

    [Header("Set Dynamically")]
    [SerializeField]
    private float _shieldLevel = 1;

    private GameObject lastTriggerGo = null;  //хранит ссылку на последний столкнувшийся объект

    public delegate void WeaponFireDelegate();  //объявление нового делегата типа WeaponFireDelegate
    public WeaponFireDelegate fireDelegate;     //создать поле типа WeaponFireDelegate с именем fireDelegate

    void Start() {                      //при создании объекта
        S = this;
        //fireDelegate += TempFire;
        ClearWeapons();
        weapons[0].SetType(WeaponType.blaster);
//        if (S == null) {                //проверяем, был они присвоен ранее или нет
//            S = this;                   //сохранить ссылку на одиночку
//        } else {
//            Debug.LogError("Hero.Awake() - Attempted to assign second Hero.S!");
//        }
//       fireDelegate += TempFire;
    }
    void Update() {
        float xAxis = Input.GetAxis("Horizontal");
        float yAxis = Input.GetAxis("Vertical");

        //перемещение корабля
        Vector3 pos = transform.position;
        pos.x += xAxis * speed * Time.deltaTime;
        pos.y += yAxis * speed * Time.deltaTime;
        transform.position = pos;

        //поворот корабля 
        transform.rotation = Quaternion.Euler(yAxis * pitchMult, xAxis * rollMult, 0);

//        if (Input.GetKeyDown(KeyCode.Space)) {     //корабль стреляет по нажатию пробела
//            TempFire();
//        }

        if (Input.GetAxis("Jump") == 1 && fireDelegate != null) {  //Input.GetAxis("Jump") вернет 1, если была нажат пробел или кнопка на пульте
            fireDelegate();                                        //вызов делегата
        }
    }

    void TempFire() {
        GameObject projGO = Instantiate<GameObject>(projectilePrefab);
        projGO.transform.position = transform.position;
        Rigidbody rigidB = projGO.GetComponent<Rigidbody>();
 //       rigidB.velocity = Vector3.up * projectileSpeed;

        Projectile proj = projGO.GetComponent<Projectile>();          //ссылка на компонент Projectile
        proj.type = WeaponType.blaster;                               //устанавливаем тип оружия
        float tSpeed = Main.GetWeaponDefinition(proj.type).velocity;  //узнаем нужную скорость снарадя это оружию
        rigidB.velocity = Vector3.up * tSpeed;                        //устанавливаем эту скорость в объект
    }

    private void OnTriggerEnter(Collider other) {
        Transform rootT = other.gameObject.transform.root;
        GameObject go = rootT.gameObject;
        //print("Triggeres: " + go.name);

        if (go == lastTriggerGo) {    //гарантировать невозможность столкновения с тем же объектом
            return;
        }
        lastTriggerGo = go;

        if (go.tag == "Enemy") {      //если защитное поле столкнулось с вражеским кораблем
            shieldLevel--;            //уменьшить уровень защиты на 1
            Destroy(go);              //и уничтожить врага
        } else if (go.tag == "PowerUp") {
            AbsorbPowerUp(go);
        } else {
            print("Triggered by non-Enemy: " + go.name);
        }
    }

    public void AbsorbPowerUp(GameObject go) { 
        PowerUp pu = go.GetComponent<PowerUp>();
        switch (pu.type) {
            case WeaponType.shield:           //если бонус имеет тип WeaponType.shield, он увеличивает уровени защитного поля на 1
                shieldLevel++;
                break;

            default:                          //Бонус любого другого типа WeaponType — это оружие, и обрабатывается в ветке default.
                if (pu.type == weapons[0].type) {      //если оружие того же типа
                    Weapon w = GetEmptyWeaponSlot();
                    if (w != null) {
                        w.SetType(pu.type);            //установить в pu.type
                    }
                } else {                               //если оружие другого типа
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
                Main.S.DelayedRestart(gameRestartDelay);  //сообщить объекту Main.S о необходимости перезапустить игру
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
