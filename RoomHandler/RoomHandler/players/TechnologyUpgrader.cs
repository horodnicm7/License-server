class TechnologyUpgrader {
    public static void upgrade(ref Player player, byte upgradeTag) {
        switch (upgradeTag) {
            case UpgradeTags.SWORD_UPGRADE1:
                player.playerStats.upgradeAttack(EntityType.SWORDSMAN, 1);
                player.playerStats.upgradeAttack(EntityType.ELITE_SWORDSMAN, 1);
                player.playerStats.upgradeAttack(EntityType.CAVALRY_SWORD, 1);
                break;
            case UpgradeTags.SWORD_UPGRADE2:
                player.playerStats.upgradeAttack(EntityType.SWORDSMAN, 2);
                player.playerStats.upgradeAttack(EntityType.ELITE_SWORDSMAN, 2);
                player.playerStats.upgradeAttack(EntityType.CAVALRY_SWORD, 2);
                break;
            case UpgradeTags.SWORD_UPGRADE3:
                player.playerStats.upgradeAttack(EntityType.SWORDSMAN, 4);
                player.playerStats.upgradeAttack(EntityType.ELITE_SWORDSMAN, 4);
                player.playerStats.upgradeAttack(EntityType.CAVALRY_SWORD, 4);
                break;
            case UpgradeTags.ARROW_FIRE_UPGRADE1:
                player.playerStats.upgradeAttack(EntityType.ARCHER, 1);
                break;
            case UpgradeTags.ARROW_UPGRADE1:
                player.playerStats.upgradeAttack(EntityType.ARCHER, 1);
                break;
            case UpgradeTags.ARROW_UPGRADE2:
                player.playerStats.upgradeAttack(EntityType.ARCHER, 2);
                break;
            case UpgradeTags.ARROW_UPGRADE3:
                player.playerStats.upgradeAttack(EntityType.ARCHER, 3);
                break;
            case UpgradeTags.INFANTRY_ARMOR_UPGRADE1:
                player.playerStats.upgradeSwordArmor(EntityType.SWORDSMAN, 1);
                player.playerStats.upgradeSwordArmor(EntityType.SPEARMAN, 1);
                player.playerStats.upgradeSwordArmor(EntityType.ELITE_SWORDSMAN, 1);
                break;
            case UpgradeTags.INFANTRY_ARMOR_UPGRADE2:
                player.playerStats.upgradeSwordArmor(EntityType.SWORDSMAN, 1);
                player.playerStats.upgradeSwordArmor(EntityType.SPEARMAN, 1);
                player.playerStats.upgradeSwordArmor(EntityType.ELITE_SWORDSMAN, 1);
                player.playerStats.upgradeArrowArmor(EntityType.SWORDSMAN, 1);
                player.playerStats.upgradeArrowArmor(EntityType.SPEARMAN, 1);
                player.playerStats.upgradeArrowArmor(EntityType.ELITE_SWORDSMAN, 1);
                break;
            case UpgradeTags.INFANTRY_ARMOR_UPGRADE3:
                player.playerStats.upgradeSwordArmor(EntityType.SWORDSMAN, 2);
                player.playerStats.upgradeSwordArmor(EntityType.SPEARMAN, 2);
                player.playerStats.upgradeSwordArmor(EntityType.ELITE_SWORDSMAN, 2);
                player.playerStats.upgradeArrowArmor(EntityType.SWORDSMAN, 2);
                player.playerStats.upgradeArrowArmor(EntityType.SPEARMAN, 2);
                player.playerStats.upgradeArrowArmor(EntityType.ELITE_SWORDSMAN, 2);
                break;

            case UpgradeTags.SWORDSMAN_LEVEL1:
                player.playerStats.upgradeLevel(EntityType.SWORDSMAN, 1);
                player.playerStats.upgradeStandardAttack(EntityType.SWORDSMAN, 10);
                player.playerStats.upgradeHp(EntityType.SWORDSMAN, 85);
                player.playerStats.upgradeStandardSwordArmor(EntityType.SWORDSMAN, 2);
                player.playerStats.upgradeStandardArrowArmor(EntityType.SWORDSMAN, 0);
                break;
            case UpgradeTags.SWORDSMAN_LEVEL2:
                player.playerStats.upgradeLevel(EntityType.SWORDSMAN, 2);
                player.playerStats.upgradeStandardAttack(EntityType.SWORDSMAN, 12);
                player.playerStats.upgradeHp(EntityType.SWORDSMAN, 90);
                player.playerStats.upgradeStandardSwordArmor(EntityType.SWORDSMAN, 2);
                player.playerStats.upgradeStandardArrowArmor(EntityType.SWORDSMAN, 1);
                break;
            case UpgradeTags.SWORDSMAN_LEVEL3:
                player.playerStats.upgradeLevel(EntityType.SWORDSMAN, 3);
                break;
            case UpgradeTags.SPEARMAN_LEVEL1:
                player.playerStats.upgradeLevel(EntityType.SPEARMAN, 1);
                player.playerStats.upgradeStandardAttack(EntityType.SPEARMAN, 11);
                player.playerStats.upgradeHp(EntityType.SWORDSMAN, 60);
                player.playerStats.upgradeStandardSwordArmor(EntityType.SWORDSMAN, 1);
                player.playerStats.upgradeStandardArrowArmor(EntityType.SWORDSMAN, 1);
                break;
            case UpgradeTags.SPEARMAN_LEVEL2:
                player.playerStats.upgradeLevel(EntityType.SPEARMAN, 2);
                break;
            case UpgradeTags.ELITE_SWORDSMAN_LEVEL1:
                player.playerStats.upgradeLevel(EntityType.ELITE_SWORDSMAN, 1);
                player.playerStats.upgradeStandardAttack(EntityType.ELITE_SWORDSMAN, 16);
                player.playerStats.upgradeHp(EntityType.SWORDSMAN, 95);
                player.playerStats.upgradeStandardSwordArmor(EntityType.SWORDSMAN, 3);
                player.playerStats.upgradeStandardArrowArmor(EntityType.SWORDSMAN, 2);
                break;
        }
    }
}
