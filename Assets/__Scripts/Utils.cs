using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utils : MonoBehaviour {
    
    //возращает список всех материалов в данном игровом объекте и его дочерних объектах
    static public Material[] GetAllMaterials(GameObject go) {
        Renderer[] rends = go.GetComponentsInChildren<Renderer>();  //метод, который обходит объект, все его дочерние объекты и возращает массив компонентов с типом, указанным в параметре <>
        List<Material> mats = new List<Material> ();
        foreach (Renderer rend in rends) {
            mats.Add(rend.material);          //сохраняем все материалы в список
        }
        return(mats.ToArray());               //возращаем список
    }

}
