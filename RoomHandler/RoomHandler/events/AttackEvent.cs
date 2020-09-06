using DarkRift.Server;
using System;
using System.Collections.Generic;
using System.Linq;

class AttackEvent {
    private LinkedList<Tuple<ushort, byte, byte>> attackers;

    public AttackEvent(Tuple<ushort, byte, byte> initialAttacker) {
        this.attackers = new LinkedList<Tuple<ushort, byte, byte>>();
        this.attackers.AddLast(initialAttacker);
    }

    public void isDeadAfterInflictedAttacks(ref Unit victim, Dictionary<byte, IClient> players) {
        foreach(Tuple<ushort, byte, byte> attacker in this.attackers) {
            Player player = RoomMaster.players[players[attacker.Item2]];

            Stats stats = player.playerStats.map(attacker.Item3);
            short attack = (short)(stats.attack + stats.upgradedAttack);

            victim.currentHp -= attack; // inflict damage
            if (victim.currentHp <= 0) {
                victim.currentHp = 0; // normalize
                break;
            }
        }
    }

    public int countAttackers {
        get {
            return this.attackers.Count;
        }
    }

    public void addAttacker(ref Tuple<ushort, byte, byte> attacker) {
        this.attackers.AddLast(attacker);
    }

    /*public void updateAttacker(Tuple<ushort, byte, byte> oldAttacker, short newAttack) {
        this.attackers.Remove(oldAttacker);
        this.attackers.AddLast(new Tuple<ushort, byte, byte>(oldAttacker.Item1, oldAttacker.Item2, newAttack));
    }*/

    public void removeAttacker(Tuple<ushort, byte, byte> attacker) {
        try {
            this.attackers.Remove(attacker);
        } catch (InvalidOperationException) {
            Console.WriteLine("Can't find this attacker for removing");
        }
    }

    public bool hasAttacker(ushort unitId, byte playerId) {
        return this.attackers.Any(attacker => attacker.Item1 == unitId && attacker.Item2 == playerId);
    }
}
