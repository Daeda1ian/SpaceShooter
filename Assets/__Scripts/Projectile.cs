using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour {

    private BoundsCheck bndCheck;
    private Renderer rend;

    [Header("Set Dynamically")]
    public Rigidbody rigid;
    [SerializeField]              //делает видимым в инспекторе и возможным для изменения
    private WeaponType _type;     //взаимосвязь со свойством

    public WeaponType type { //это общедоступное свойство маскирует поле _type и обрабатывает операции присваивания ему нового значения
        get {
            return(_type);
        }
        set {
            SetType(value);
        }
    }

    void Awake() {
        bndCheck = GetComponent<BoundsCheck>();
        rend = GetComponent<Renderer>();            //метод SetType использует компонент Renderer этого объекта, поэтому нужна ссылка
        rigid = GetComponent<Rigidbody>();
    }
    void Update() {
        if (bndCheck.offUp) {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Изменяет скрытое поле _type и устанавливает цвет этого снаряда, как определено в WeaponDefinition
    /// </summary>
    /// <param name="eType">Тип WeaponType используемого оружия</param>
    public void SetType(WeaponType eType) {
        _type = eType;
        WeaponDefinition def = Main.GetWeaponDefinition(_type);
        rend.material.color = def.projectileColor;
    }
}
