public enum LaserType{NormalLaser,RotateLaser};
public enum LaserVisual{NormalPre,Normal,RotatePre,Rotate}
//激光属性配置
public struct LaserConfig:IAttackConfig{
    public LaserType type;
    public float damage;
    public float width;
    public float rotateSpeed;
    public LaserConfig(LaserType type,float damage,float width,float rotateSpeed=0){
        this.type=type;
        this.damage=damage;
        this.width=width;
        this.rotateSpeed=rotateSpeed;
    }
}
