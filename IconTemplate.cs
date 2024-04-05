using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "IconTemplate", menuName = "Icon", order = 0)]
public class IconTemplate : ScriptableObject {
    
    public new string name;
    //public Texture2D iconColor;
    //public Texture2D iconAlpha;
    public Sprite Icon;
    public string tag;

}
