using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUp : MonoBehaviour {

    [Header("Set in Inspector")]
    public Vector2 rotMinMax = new Vector2(15, 90);
    public Vector2 drifMinMax = new Vector2(.25f, 2);
    public float lifeTime = 6f;     //время в секундах существования PowerUp
    public float fadeTime = 4f;

    [Header("Set Dynamically")]
    public WeaponType type;
    public GameObject cube;
    public TextMesh letter;
    public Vector3 rotPerSecond;
    public float birthTime;

    private Rigidbody rigid;
    private BoundsCheck bndCheck;
    private Renderer cubeRend;

    void Awake() {
        cube = transform.Find("Cube").gameObject;
        letter = GetComponent<TextMesh>();
        rigid = GetComponent<Rigidbody>();
        bndCheck = GetComponent<BoundsCheck>();
        cubeRend = GetComponent<Renderer>();

        Vector3 vel = Random.onUnitSphere;               //получить случайную скорость
        vel.z = 0;                                       //отобразить vel на плоскости XY
        vel.Normalize();                                 //нормализация устанавливает длину Vector3 равной 1м 
        vel *= Random.Range(drifMinMax.x, drifMinMax.y);
        rigid.velocity = vel;

        transform.rotation = Quaternion.identity;        //установить угол поворота R:[0, 0, 0]
        rotPerSecond = new Vector3(Random.Range(rotMinMax.x, rotMinMax.y), Random.Range(rotMinMax.x, rotMinMax.y), Random.Range(rotMinMax.x, rotMinMax.y));
        birthTime = Time.time;
    }
    void Update() {
        
        cube.transform.rotation = Quaternion.Euler(rotPerSecond * Time.time);
        float u = (Time.time - (birthTime + lifeTime)) / fadeTime;

        if (u >= 1) {
            Destroy(this.gameObject);
            return;
        }
        // Использовать u для определения альфа-значения куба и буквы
        if (u > 0) {
            Color c = cubeRend.material.color;
            c.a = 1f - u;
            cubeRend.material.color = c;
            c = letter.color;
            c.a = 1f - (u * 0.5f);
            letter.color = c;
        }
        // Если бонус полностью вышел за границу экрана, уничтожить его
        if (!bndCheck.isOnScreen) {
            Destroy(gameObject);
        }
    }

    public void SetType(WeaponType wt) {
        // Получить WeaponDefinition из Main
        WeaponDefinition def = Main.GetWeaponDefinition(wt);
        // Установить цвет дочернего куба
        cubeRend.material.color = def.color;
        //letter.color = def.color; // Букву тоже можно окрасить в тот же цвет
        letter.text = def.letter; // Установить отображаемую букву
        type = wt; // В заключение установить фактический тип
    }

    public void AbsorbedBy(GameObject target) {
        // Эта функция вызывается классом Него, когда игрок подбирает бонус
        Destroy(this.gameObject);
    }


}
