using System;
using System.Collections.Generic;
using System.Linq;

class AttackEvent {
    private LinkedList<Tuple<ushort, byte, short>> attackers;

    public AttackEvent(Tuple<ushort, byte, short> initialAttacker) {
        this.attackers = new LinkedList<Tuple<ushort, byte, short>>();
        this.attackers.AddLast(initialAttacker);
    }

    public void isDeadAfterInflictedAttacks(ref Unit victim) {
        foreach(Tuple<ushort, byte, short> attacker in this.attackers) {
            victim.currentHp -= attacker.Item3; // inflict damage
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

    public void addAttacker(ref Tuple<ushort, byte, short> attacker) {
        this.attackers.AddLast(attacker);
    }

    public void updateAttacker(Tuple<ushort, byte, short> oldAttacker, short newAttack) {
        this.attackers.Remove(oldAttacker);
        this.attackers.AddLast(new Tuple<ushort, byte, short>(oldAttacker.Item1, oldAttacker.Item2, newAttack));
    }

    public void removeAttacker(Tuple<ushort, byte, short> attacker) {
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
