namespace TD
{
    public enum BattleType
    {
        One = 1,
        Two = 2
    }

    public enum SkillEffectType
    {
        None,
        Ad,
        Ap,
        Buff,
    }
    public enum BuffEffectType
    {
        None,
        Dizz,
        SubMoveSpeed,
        AddMoveSpeed,
        AddHp,
        AddMp
    }

    public enum BattleTagType
    {
        //不能移动
        DontMove,
        //不能使用技能
        DontUseSkill,
        // 不能作为攻击目标 - 不能选中
        DontAttackedTarget
    }

    public enum AttribuiteType
    {
        MoveSpeed,
        Hp,
        Ad,
        Ap,
        AdDef,
        ApDef,
        Mp,
    }

    public enum AttributeCalcType
    {
        Base,
        BaseRatio,
        BaseAdd,
        AffterRatio,
        AffterAdd,
    }
}