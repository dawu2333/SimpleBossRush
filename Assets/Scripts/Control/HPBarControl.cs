
using UnityEngine;

//血条显示：hp 百分比 → 长度 + 位置
//1.5f 是血条的长度，0.75f 是血条的偏移量
public static class HpBarControl{
    public static void UpdateHpBar(Transform hpBar,float percentage) {
        float p=Mathf.Clamp01(percentage);
        float scaleXValue = 1.5f * p;
        float posXValue = -0.75f + 0.75f * p;
        hpBar.localPosition=new Vector3(posXValue,hpBar.localPosition.y,hpBar.localPosition.z);
        hpBar.localScale=new Vector3(scaleXValue,hpBar.localScale.y,hpBar.localScale.z);
    }

    //三形参重载，算出百分比再给二参数的方法
    public static void UpdateHpBar(Transform hpBar,float hp,float maxHp)=>UpdateHpBar(hpBar,(float)hp/maxHp);
}
