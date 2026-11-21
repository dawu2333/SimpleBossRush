using UnityEngine;

//子弹功能枚举：
// 已实现：普通子弹
// 未实现：碎片子弹/爆炸子弹/追踪子弹
public enum BulletBehavior{NormalBullet,FragmentedBullet,ExplosiveBullet,TrackingBullet};
//子弹形状枚举
public enum BulletShape{Round,Rect,Head,Arrow}
public enum Faction{ Player, Boss }//子弹阵营
public enum ObstacleMode{None,SingleUse,Persistent}
//子弹属性配置
public struct BulletConfig:IAttackConfig{
    public BulletBehavior behavior;
    public float damage;
    public float speed;
    public ObstacleMode mode;
    public float size;
    public Color color;
    public BulletShape shape;
    public BulletConfig(BulletBehavior behavior, float damage, float speed, ObstacleMode mode, float size, Color color,BulletShape shape){
        this.behavior = behavior;
        this.damage = damage;
        this.speed = speed;
        this.mode = mode;
        this.size = size;
        this.color = color;
        this.shape=shape;
    }
}
