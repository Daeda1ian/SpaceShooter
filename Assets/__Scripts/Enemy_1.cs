using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_1 : Enemy {

    [Header("Set in Inspector: Enemy_1")]
    public float waveFrequency = 2;       //число секунд полного цикла синусоиды
    public float waveWidth = 4;           //ширина синусоиды в метрах
    public float waveRotY = 45;

    private float x0;                     //начальное значение координаты Х
    private float birthTime;
    void Start() {
        x0 = pos.x;                       //установить начальную координату Х объекта Enemy_1
        birthTime = Time.time;
    }

    //Move() в этом классе отвечает за горизонтальное перемещение по синусоиде, а метод Move() в Enemy - по вертикали
    public override void Move() {         //переопределяем функцию Move суперкласса Enemy
        Vector3 tempPos = pos;            //так как pos - это свойство, нельзя напрямую изменить pos.x, поэтому получим pos в виде вектора Vector3, доступного для изменения
        float age = Time.time - birthTime;
        float theta = Mathf.PI * 2 * age / waveFrequency;
        float sin = Mathf.Sin(theta);
        tempPos.x = x0 + waveWidth * sin;
        pos = tempPos;

        Vector3 rot = new Vector3(0, sin * waveRotY, 0);
        this.transform.rotation = Quaternion.Euler(rot);
        base.Move();                      //вызывается Move() суперкласса Enemy
    }
}
