using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Enemy_4 создается за верхней границей, выбирает случайную точку на экране и перемещается к ней.
/// Добравшись до места, выбирает другую случайную точку и продолжает двигаться, пока игрок не уничтожит его
/// </summary>
 
[System.Serializable]
public class Part {
    public string name;           //имя этой части
    public float health;          //степень стойкости этой части
    public string[] protectedBy;  //другие части, защищающие эту

    [HideInInspector]
    public GameObject go;         //игровой объект этой части
    [HideInInspector] 
    public Material mat;          //материал для отображения повреждений
}


public class Enemy_4 : Enemy {
    [Header("Set in Inspector: Enemy_4")]
    public Part[] parts;         //массив частей, составляющих корабль

    private Vector3 p0, p1;      //две точки интерполяции
    private float timeStart;     //время создания этого корабля
    private float duration = 4;  //продолжительность перемещения

    void Start() {
        p0 = p1 = pos;           //начальная позиция уже выбрана в Main.SpawnEnemy(), поэтому запишем ее как начальное значение в po и p1
        InitMovement();

        Transform t;
        foreach (Part part in parts) {
            t = transform.Find(part.name);
            if (t != null) {
                part.go = t.gameObject;
                part.mat = part.go.GetComponent<Renderer>().material;
            }
        }
    }

    void InitMovement() {
        pos = p1;
        float widMinRad = bndCheck.camWidth - bndCheck.radius;
        float hgtMinRad = bndCheck.camHeight - bndCheck.radius;
        p1.x = Random.Range(-widMinRad, widMinRad);
        p1.y = Random.Range(-hgtMinRad, hgtMinRad);

        timeStart = Time.time;    //сбросить время
    }

    public override void Move() {
        float u = (Time.time - timeStart) / duration;
        if (u >= 1) {
            InitMovement();
            u = 0;
        }
        u = 1 - Mathf.Pow(1 - u, 2);    //применить плавное замедление
        pos = (1 - u) * p0 + u * p1;    //простая линейная интерполяция
    }

    //эти две функции выполняют поиск части в массиве parts п по имени или по ссылке на игровой объект
    Part FindPart(string n) {
        foreach (Part part in parts) {
            if (part.name == n) {
                return (part);
            }
        }
        return(null);
    }

    Part FindPart(GameObject go) {
        foreach (Part part in parts) {
            if (part.go == go) {
                return(part);
            }
        }
        return (null);
    }

    // Эти функции возвращают true, если данная часть уничтожена
    bool Destroyed(GameObject go) {
        return(Destroyed(FindPart(go)));
    }

    bool Destroyed(string n) {
        return (Destroyed(FindPart(n)));
    }

    bool Destroyed(Part part) {      
        if (part == null) {          // Если ссылка на часть не была передана
            return true;             // Вернуть true (то есть: да, была уничтожена)
        }
        return (part.health <= 0);
    }

    // Окрашивает в красный только одну часть, а не весь корабль
    void ShowLocalizedDamage(Material m) {
        m.color = Color.red;
        damageDoneTime = Time.time + showDamageDuration;
        showingDamage = true;
    }

    void OnCollisionEnter(Collision collision) {
        GameObject other = collision.gameObject;
        switch (other.tag) {
            case "ProjectileHero":
                Projectile p = other.GetComponent<Projectile>();
                if (!bndCheck.isOnScreen) {      // Если корабль за границами экрана, не повреждать его
                    Destroy(other);
                    break;
                }

                // Поразить вражеский корабль
                GameObject goHit = collision.contacts[0].thisCollider.gameObject;
                Part prtHit = FindPart(goHit);
                if (prtHit == null) {
                    goHit = collision.contacts[0].thisCollider.gameObject;
                    prtHit = FindPart(goHit);
                }

                // Проверить, защищена ли еще эта часть корабля
                if (prtHit.protectedBy != null) {
                    foreach (string s in prtHit.protectedBy) {
                        if (!Destroyed(s)) {               // Если хотя бы одна из защищающих частей еще не разрушена, то не наносить повреждений этой части
                            Destroy(other);                // Уничтожить снаряд ProjectileHero
                            return;                        // выйти, не повреждая Enemy_4
                        }
                    }
                }

                prtHit.health -= Main.GetWeaponDefinition(p.type).damageOnHit;   // Эта часть не защищена, нанести ей повреждение
                ShowLocalizedDamage(prtHit.mat);           // Показать эффект попадания в часть
                if (prtHit.health <= 0) { 
                    prtHit.go.SetActive(false);            // Вместо разрушения всего корабля деактивировать уничтоженную часть
                }

                // Проверить, был ли корабль полностью разрушен
                bool allDestroyed = true;                  // Предположить, что разрушен
                foreach (Part prt in parts) {
                    if (!Destroyed(prt)) {                 // Если какая-то часть еще существует
                        allDestroyed = false;              // ...записать false в allDestroyed
                        break;                             // и прервать цикл foreach
                    }
                }
                if (allDestroyed) {                        // Если корабль разрушен полностью
                    Main.S.ShipDestroyed(this);            // уведомить объект-одиночку Main, что этот корабль разрушен
                    Destroy(this.gameObject);
                }
                Destroy(other);                            // Уничтожить снаряд
                break;
        }
    }
}
