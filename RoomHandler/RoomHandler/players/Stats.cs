public class Stats {
    public short hp;
    public short attack;
    public short upgradedAttack;

    public float range;
    public byte upgradedRange;
    public byte fieldOfView;

    public byte swordArmor;
    public byte arrowArmor;
    public byte upgradedSwordArmor;
    public byte upgradedArrowArmor;

    public byte level;

    public Stats(short hp, short attack, short upgradedAttack, float range, byte fieldOfView, byte swordArmor, byte arrowArmor,
        byte upgradedSwordArmor, byte upgradedArrowArmor, byte level) {
        this.hp = hp;
        this.attack = attack;
        this.upgradedAttack = upgradedAttack;

        this.range = range;
        this.fieldOfView = fieldOfView;
        this.swordArmor = swordArmor;
        this.arrowArmor = arrowArmor;

        this.upgradedSwordArmor = upgradedSwordArmor;
        this.upgradedArrowArmor = upgradedArrowArmor;

        this.level = level;
    }
}