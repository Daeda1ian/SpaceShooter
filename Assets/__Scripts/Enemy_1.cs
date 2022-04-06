using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_1 : Enemy {

    [Header("Set in Inspector: Enemy_1")]
    public float waveFrequency = 2;       //����� ������ ������� ����� ���������
    public float waveWidth = 4;           //������ ��������� � ������
    public float waveRotY = 45;

    private float x0;                     //��������� �������� ���������� �
    private float birthTime;
    void Start() {
        x0 = pos.x;                       //���������� ��������� ���������� � ������� Enemy_1
        birthTime = Time.time;
    }

    //Move() � ���� ������ �������� �� �������������� ����������� �� ���������, � ����� Move() � Enemy - �� ���������
    public override void Move() {         //�������������� ������� Move ����������� Enemy
        Vector3 tempPos = pos;            //��� ��� pos - ��� ��������, ������ �������� �������� pos.x, ������� ������� pos � ���� ������� Vector3, ���������� ��� ���������
        float age = Time.time - birthTime;
        float theta = Mathf.PI * 2 * age / waveFrequency;
        float sin = Mathf.Sin(theta);
        tempPos.x = x0 + waveWidth * sin;
        pos = tempPos;

        Vector3 rot = new Vector3(0, sin * waveRotY, 0);
        this.transform.rotation = Quaternion.Euler(rot);
        base.Move();                      //���������� Move() ����������� Enemy
    }
}
