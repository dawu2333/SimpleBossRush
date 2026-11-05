public struct LaserConfig{
    public LaserType type;
    public int damage;
    public float width;
    public float rotateSpeed;
    public LaserConfig(LaserType type,int damage,float width,float rotateSpeed){
        this.type=type;
        this.damage=damage;
        this.width=width;
        this.rotateSpeed=rotateSpeed;
    }
}
