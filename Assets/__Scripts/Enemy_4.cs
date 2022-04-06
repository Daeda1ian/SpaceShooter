using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Enemy_4 ��������� �� ������� ��������, �������� ��������� ����� �� ������ � ������������ � ���.
/// ���������� �� �����, �������� ������ ��������� ����� � ���������� ���������, ���� ����� �� ��������� ���
/// </summary>
 
[System.Serializable]
public class Part {
    public string name;           //��� ���� �����
    public float health;          //������� ��������� ���� �����
    public string[] protectedBy;  //������ �����, ���������� ���

    [HideInInspector]
    public GameObject go;         //������� ������ ���� �����
    [HideInInspector] 
    public Material mat;          //�������� ��� ����������� �����������
}


public class Enemy_4 : Enemy {
    [Header("Set in Inspector: Enemy_4")]
    public Part[] parts;         //������ ������, ������������ �������

    private Vector3 p0, p1;      //��� ����� ������������
    private float timeStart;     //����� �������� ����� �������
    private float duration = 4;  //����������������� �����������

    void Start() {
        p0 = p1 = pos;           //��������� ������� ��� ������� � Main.SpawnEnemy(), ������� ������� �� ��� ��������� �������� � po � p1
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

        timeStart = Time.time;    //�������� �����
    }

    public override void Move() {
        float u = (Time.time - timeStart) / duration;
        if (u >= 1) {
            InitMovement();
            u = 0;
        }
        u = 1 - Mathf.Pow(1 - u, 2);    //��������� ������� ����������
        pos = (1 - u) * p0 + u * p1;    //������� �������� ������������
    }

    //��� ��� ������� ��������� ����� ����� � ������� parts � �� ����� ��� �� ������ �� ������� ������
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

    // ��� ������� ���������� true, ���� ������ ����� ����������
    bool Destroyed(GameObject go) {
        return(Destroyed(FindPart(go)));
    }

    bool Destroyed(string n) {
        return (Destroyed(FindPart(n)));
    }

    bool Destroyed(Part part) {      
        if (part == null) {          // ���� ������ �� ����� �� ���� ��������
            return true;             // ������� true (�� ����: ��, ���� ����������)
        }
        return (part.health <= 0);
    }

    // ���������� � ������� ������ ���� �����, � �� ���� �������
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
                if (!bndCheck.isOnScreen) {      // ���� ������� �� ��������� ������, �� ���������� ���
                    Destroy(other);
                    break;
                }

                // �������� ��������� �������
                GameObject goHit = collision.contacts[0].thisCollider.gameObject;
                Part prtHit = FindPart(goHit);
                if (prtHit == null) {
                    goHit = collision.contacts[0].thisCollider.gameObject;
                    prtHit = FindPart(goHit);
                }

                // ���������, �������� �� ��� ��� ����� �������
                if (prtHit.protectedBy != null) {
                    foreach (string s in prtHit.protectedBy) {
                        if (!Destroyed(s)) {               // ���� ���� �� ���� �� ���������� ������ ��� �� ���������, �� �� �������� ����������� ���� �����
                            Destroy(other);                // ���������� ������ ProjectileHero
                            return;                        // �����, �� ��������� Enemy_4
                        }
                    }
                }

                prtHit.health -= Main.GetWeaponDefinition(p.type).damageOnHit;   // ��� ����� �� ��������, ������� �� �����������
                ShowLocalizedDamage(prtHit.mat);           // �������� ������ ��������� � �����
                if (prtHit.health <= 0) { 
                    prtHit.go.SetActive(false);            // ������ ���������� ����� ������� �������������� ������������ �����
                }

                // ���������, ��� �� ������� ��������� ��������
                bool allDestroyed = true;                  // ������������, ��� ��������
                foreach (Part prt in parts) {
                    if (!Destroyed(prt)) {                 // ���� �����-�� ����� ��� ����������
                        allDestroyed = false;              // ...�������� false � allDestroyed
                        break;                             // � �������� ���� foreach
                    }
                }
                if (allDestroyed) {                        // ���� ������� �������� ���������
                    Main.S.ShipDestroyed(this);            // ��������� ������-�������� Main, ��� ���� ������� ��������
                    Destroy(this.gameObject);
                }
                Destroy(other);                            // ���������� ������
                break;
        }
    }
}
