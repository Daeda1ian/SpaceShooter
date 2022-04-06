using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour {

    [Header("Set in Inspector")]
    public float speed = 10f;        //скорость в м/с
    public float fireRate = 0.3f;    //секунд между выстрелами (не используется)
    public float health = 10;
    public int score = 100;          //очки за уничтожение этого корабля

    public float showDamageDuration = 0.1f;
    public float powerUpDropChance = 1f;

    [Header("Set Dynamically: Enemy")]
    public Color[] originalColors;
    public Material[] materials;     //все материалы объекта и его потомков
    public bool showingDamage = false;
    public float damageDoneTime;     //время прекращения отображения эффекта
    public bool notifiedOfDestruction = false; 

    protected BoundsCheck bndCheck;

    private void Awake() {
        bndCheck = GetComponent<BoundsCheck>();   //получаем ссылку на компонент сценария BoundsCheck, подключенного к этому игровому объекту
        materials = Utils.GetAllMaterials(gameObject);
        originalColors = new Color[materials.Length];  //запоминаем исходные цвета объектов
        for (int i = 0; i < materials.Length; i++) {
            originalColors[i] = materials[i].color;
        }

    }
    public Vector3 pos {
        get {
            return (this.transform.position);
        }
        set {
            this.transform.position = value;
        }
    }
    void Update() {
        Move();

        if (showingDamage && Time.time > damageDoneTime) {  //если в текущий момент отображается эффект попадания и текущее время больше damageDoneTime
            UnShowDamage();                                 //то вызывается UnShowDamage
        } 

        if (bndCheck != null && bndCheck.offDown) {  //если корабль за нижней границей
            Destroy(gameObject);                       //то уничтожить его
        }
    }

    public virtual void Move() {
        Vector3 tempPos = pos;                  //получаем текущее положение данного объекта
        tempPos.y -= speed * Time.deltaTime;    //перемещаем его вниз, вдоль оси Y
        pos = tempPos;                          //обновляем координаты
    }

    /*private void OnCollisionEnter(Collision collision) {
        GameObject otherGO = collision.gameObject;
        if (otherGO.tag == "ProjectileHero") {
            Destroy(otherGO);
            Destroy(gameObject);
        } else {
            print("Enemy hit by non-ProjectileHero: " + otherGO.name);
        }
    }*/

    private void OnCollisionEnter(Collision collision) {
        GameObject otherGO = collision.gameObject;
        switch (otherGO.tag) {
            case "ProjectileHero":
                Projectile p = otherGO.GetComponent<Projectile>();
                if (!bndCheck.isOnScreen) {       //если вражеский корабль за границами экрана, не наносить ему повреждений
                    Destroy(otherGO);             //уничтожаем снаряд
                    break;
                }
                health -= Main.GetWeaponDefinition(p.type).damageOnHit;
                ShowDamage();
                if (health <= 0) {                 //поразить вражеский корабль. Получить разрушающую силу из WEAP_DICT в классе Main
                    if (!notifiedOfDestruction) {
                        Main.S.ShipDestroyed(this);
                    }
                    notifiedOfDestruction = true;
                    Destroy(this.gameObject);
                }
                Destroy(otherGO);
                break;

            default:
                print("Enemy hit by non-ProjectileHero: " + otherGO.name);
                break;
        }
    }

    void ShowDamage() {                             //отвечает за окрашивание врага в красный цвет во время попадания
        foreach (Material m in materials) {         //окраска
            m.color = Color.red;
        }
        showingDamage = true;                       //урон показан
        damageDoneTime = Time.time + showDamageDuration;  //запоминаем время окончания отображения эффекта
    }

    void UnShowDamage() {                            //возвращает все материалам исходные цвета
        for (int i = 0; i < materials.Length; i++) {
            materials[i].color = originalColors[i];
        }
        showingDamage = false;                      
    }
}
