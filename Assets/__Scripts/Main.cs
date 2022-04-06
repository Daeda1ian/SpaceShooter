using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Main : MonoBehaviour {

    static public Main S;
    static Dictionary<WeaponType, WeaponDefinition> WEAP_DICT;

    [Header("Set in Inspector")]
    public GameObject[] prefabEnemies;
    public float enemySpawnPerSecond = 0.5f;       // вражеских кораблей в секунду
    public float enemyDefaultPadding = 1.5f;       // отступ для позиционирования

    public WeaponDefinition[] weaponDefinitions;
    public GameObject prefabPowerUp;               // шаблон всех бонусов
    public WeaponType[] powerUpFrequency = new WeaponType[] {  //массив типов бонусов
        WeaponType.blaster, WeaponType.blaster, WeaponType.spread, WeaponType.shield
    };

    private BoundsCheck bndCheck;

    public void ShipDestroyed(Enemy e) {               //вызывается в момент уничтожения вражеского корабля
        if (Random.value <= e.powerUpDropChance) {              //сгенерировать бонус с заданной вероятностью
            int ndx = Random.Range(0, powerUpFrequency.Length); //выбрать тип бонуса
            WeaponType puType = powerUpFrequency[ndx];          //выбрать один из элементов в powerUpFrequency

            GameObject go = Instantiate(prefabPowerUp) as GameObject; //создать экземпляр PowerUp
            PowerUp pu = go.GetComponent<PowerUp>();
            pu.SetType(puType);                                       //установить соответствующий тип WeaponType
            pu.transform.position = e.transform.position;             //gоместить в место, где находился разрушенный корабль
        }
    }

    void Awake() {
        S = this;
        bndCheck = GetComponent<BoundsCheck>();                 //ссылка на скрипт
        Invoke("SpawnEnemy", 1f/enemySpawnPerSecond);           //вызывать метод один раз в 2 секунды (по умолчанию)
        WEAP_DICT = new Dictionary<WeaponType, WeaponDefinition>();

        foreach (WeaponDefinition def in weaponDefinitions) {
            WEAP_DICT[def.type] = def;
        }
    }

    public void SpawnEnemy() {
        int ndx = Random.Range(0, prefabEnemies.Length);                  //выбрать случайный шаблон Enemy для создания
        GameObject go = Instantiate<GameObject>(prefabEnemies[ndx]);      //создаем объект врага
        float enemyPadding = enemyDefaultPadding;             //значение по умолчанию

        if (go.GetComponent<BoundsCheck>() != null) {    //если выбранный шаблон имеет компонент BoundsCheck, 
            enemyPadding = Mathf.Abs(go.GetComponent<BoundsCheck>().radius); //в роли отступа используется радиус этого компонента
        }

        //установить начальные координаты созданного вражеского корабля
        Vector3 pos = Vector3.zero;
        float xMin = -bndCheck.camWidth + enemyPadding;
        float xMax = bndCheck.camWidth - enemyPadding;
        pos.x = Random.Range(xMin, xMax);
        pos.y = bndCheck.camHeight + enemyPadding;
        go.transform.position = pos;

        Invoke("SpawnEnemy", 1f / enemySpawnPerSecond);
    }

    public void DelayedRestart(float delay) {
        Invoke("Restart", delay);              //вызвать метод Restart через delay секунд
    }

    public void Restart() {
        SceneManager.LoadScene("_Scene_0");    //перезагрузить _Scene_0, чтобы перезапустить игру
    }
    
    static public WeaponDefinition GetWeaponDefinition(WeaponType wt) {
        if (WEAP_DICT.ContainsKey(wt)) {
            return(WEAP_DICT[wt]);
        }
        return(new WeaponDefinition());
    }
}
