using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Это перечисление всех возможных типов оружия.
/// Также включает тип "shield", чтобы дать возможность совершенствовать защиту.
/// Аббревеатурой [HP] ниже отмечены элементы, не реализованные пока что
/// </summary>

public enum WeaponType {
    none,      // по умолчанию/нет оружия
    blaster,   // простой бластер
    spread,    // веерная пушка, стреляющая несколькими снарядами
    phaser,    // [HP] волновой фазер
    missile,   // [HP] самонаводящиеся ракеты
    laser,     // [HP] наносит повреждения при долговременном воздействии
    shield     // увеличивает shieldLevel
}

/// <summary>
/// Класс WeaponDefinition позволяет настраивать свойства конкретного оружия в инспекторе.
/// Для этого класс Main будет хранить массив элементов типа weaponDefinition
/// </summary>

[System.Serializable]
public class WeaponDefinition {
    public WeaponType type = WeaponType.none;
    public string letter;                         // буква на кубике, изображающем бонус
    public Color color = Color.white;             // цвет ствола оружия и кубика бонуса
    public GameObject projectilePrefab;           // шаблон снарядов
    public Color projectileColor = Color.white;
    public float damageOnHit = 0;                 // разрушительная мощность
    public float continuousDamage = 0;            // степень разрушения в секунду (для Laser)
    public float delayBetweenShots = 0;
    public float velocity = 20;                   // скорость полета снарядов
}


public class Weapon : MonoBehaviour {
    static public Transform PROJECTILE_ANCHOR;

    [Header("Set Dynamically")] [SerializeField]
    private WeaponType _type = WeaponType.none;
    public WeaponDefinition def;
    public GameObject collar;
    public float lastShotTime;
    private Renderer collarRend;

    void Start () {
        collar = transform.Find("Collar").gameObject;
        collarRend = collar.GetComponent<Renderer>();

        SetType(_type);                     //вызвать SetType(), чтобы заменить тип оружия по умолчанию WeaponType.none
        //динамически создать точку привязки для всех снарядов
        if (PROJECTILE_ANCHOR == null) {
            GameObject go = new GameObject("_ProjectileAnchor");
            PROJECTILE_ANCHOR = go.transform;
        }
        // найти fireDelegate в корневом игровом объекте
        GameObject rootGo = transform.root.gameObject;
        if (rootGo.GetComponent<Hero>() != null) {
            rootGo.GetComponent<Hero>().fireDelegate += Fire;
        }
    }

    public WeaponType type {
        get {
            return (_type);
        }
        set {
            SetType(value);
        }
    }

    public void SetType(WeaponType wt) {
        _type = wt;
        if (type == WeaponType.none) {
            this.gameObject.SetActive(false);
        } else {
            this.gameObject.SetActive(true);
        }
        def = Main.GetWeaponDefinition(_type);
        collarRend.material.color = def.color;
        lastShotTime = 0;
    }

    public void Fire() {
        if (!gameObject.activeInHierarchy) return;               //если объект неактивен, то не стрелять

        if (Time.time - lastShotTime < def.delayBetweenShots) {  //если с момента последнего выстрела прошло меньше времени, чем delayBetweenShots, выстрел не производится
            return;
        }

        Projectile p;
        Vector3 vel = Vector3.up * def.velocity;                 //задается начальная скорость полеты вверх
        if (transform.up.y < 0) {
            vel.y = -vel.y;
        }
        switch (type) {                           //варианты поведения для каждого из типов оружия
            case WeaponType.blaster:
                p = MakeProjectile();             //стреляет один раз, вызывая метод MakeProjectile
                p.rigid.velocity = vel;           //устанавливает скорость снаряда
                break;

            case WeaponType.spread:               //здесь создаются три разных снаряда
                p = MakeProjectile();
                p.rigid.velocity = vel;
                p = MakeProjectile();
                //когда экземпляр Quaternion умножается на Vector3, он поворачивает вектор Vector3
                p.transform.rotation = Quaternion.AngleAxis(10, Vector3.back);
                p.rigid.velocity = p.transform.rotation * vel;
                p = MakeProjectile();
                p.transform.rotation = Quaternion.AngleAxis(-10, Vector3.back);
                p.rigid.velocity = p.transform.rotation * vel;
                break;
        }
    }

    //Метод MakeProjectile() создает экземпляры шаблона, хранящегося в WeaponDefinition, и возвращает ссылку на экземпляр класса Projectile.
    public Projectile MakeProjectile() {
        GameObject go = Instantiate<GameObject>(def.projectilePrefab);
        if (transform.parent.gameObject.tag == "Hero") {  //в зависимости, каким кораблем произведен выстре, снаряду назначается соответствующий тег и физ. уровень
            go.tag = "ProjectileHero";
            go.layer = LayerMask.NameToLayer("ProjectileHero");
        } else {
            go.tag = "ProjectileEnemy";
            go.layer = LayerMask.NameToLayer("ProjectileEnemy");
        }
        go.transform.position = collar.transform.position;
        go.transform.SetParent(PROJECTILE_ANCHOR, true);    //назначаем родителем всех последующих снарядов _ProjectileAnchor
        Projectile p = go.GetComponent<Projectile>();
        p.type = type;
        lastShotTime = Time.time;      //Полю lastShotTime присваивается текущее время, что предотвращает возможность повторного выстрела раньше, чем через def.delayBetweenShots секунд
        return (p);
    }
}
