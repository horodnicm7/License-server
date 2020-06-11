﻿using System;
using System.Collections.Generic;
using System.Linq;

class AttackEvent {
    private LinkedList<Tuple<ushort, byte, short>> attackers;

    public AttackEvent(Tuple<ushort, byte, short> initialAttacker = null) {
        this.attackers = new LinkedList<Tuple<ushort, byte, short>>();
        
        if (initialAttacker != null) {
            this.attackers.AddLast(initialAttacker);
        }
    }

    public bool isDeadAfterInflictedAttacks(ref Unit victim) {
        Console.WriteLine("Apply damage from no enemies: " + this.attackers.Count);
        foreach(Tuple<ushort, byte, short> attacker in this.attackers) {
            victim.currentHp -= attacker.Item3; // inflict damage
            if (victim.currentHp <= 0) {
                victim.currentHp = 0; // normalize
                return true; // signal death
            }
        }

        return false;
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