//激光属性配置
public struct LaserConfig{
    public LaserType type;
    public float damage;
    public float width;
    public float rotateSpeed;
    public LaserConfig(LaserType type,float damage,float width,float rotateSpeed){
        this.type=type;
        this.damage=damage;
        this.width=width;
        this.rotateSpeed=rotateSpeed;
    }
}
