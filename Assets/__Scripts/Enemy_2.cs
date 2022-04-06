using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_2 : Enemy {

    [Header("Set in Inspector")]
    //����������, ��������� ���� ����� ������� �������������� �������� ��������
    public float sinEccentricity = 0.6f;
    public float lifeTime = 10;

    [Header("Set Dynamically: Enemy_2")]
    //Enemy_2 ���������� �������� ������������ ����� ����� �������, ������� ��������� �� ���������
    public Vector3 p0;
    public Vector3 p1;
    public float birthTime;

    void Start() {
        //������� ��������� ����� �� ����� ������� ������
        p0 = Vector3.zero;
        p0.x = -bndCheck.camWidth - bndCheck.radius;
        p0.y = Random.Range(-bndCheck.camHeight, bndCheck.camHeight);

        //������� ��������� ����� �� ������ ������� ������
        p1 = Vector3.zero;
        p1.x = bndCheck.camWidth + bndCheck.radius;
        p1.y = Random.Range(-bndCheck.camHeight,bndCheck.camHeight);

        //�������� �������� ��������� � �������� ����� �������
        if (Random.value > 0.5f) {
            p0.x *= -1;            //��������� ����� .x ������ ����� ��������� �� �� ������ ���� ������
            p1.x *= -1;
        }
        birthTime = Time.time;     //�������� � birthTime ������� �����
    }

    public override void Move() {
        //������ ����� ����������� �� ������ �������� u ����� 0 � 1
        float u = (Time.time - birthTime) / lifeTime;
        if (u > 1) {                        //���� u>1, ������, ������� ���������� ������, ��� lifeTime
            Destroy(this.gameObject);       //���� ��������� Enemy_2 �������� ���� ����
            return;
        }
        u = (u + sinEccentricity * (Mathf.Sin(u * Mathf.PI * 2))); //��������������� u ����������� �������� ������, ������������ �� ���������
        pos = (1 - u) * p0 + u * p1;        //��������������� �������������� ����� ����� �������
    }
}
