using System.Collections.Generic;

public class Civilizations {
    private static Dictionary<byte, string> byteToString = new Dictionary<byte, string> {
        { Civilizations.HUMANS, "Humans" },
        { Civilizations.ORCS, "Orcs" },
        { Civilizations.HIGH_ELVES, "ELVES" }
    };

    public const byte HUMANS = 1;
    public const byte HIGH_ELVES = 2;
    public const byte ORCS = 3;

    public static string map(byte id) {
        if (!Civilizations.byteToString.ContainsKey(id)) {
            return string.Empty;
        }

        return Civilizations.byteToString[id];
    }

    public static byte map(string civ) {
        foreach (KeyValuePair<byte, string> civilization in Civilizations.byteToString) {
            if (civilization.Value.Equals(civ)) {
                return civilization.Key;
            }
        }

        return 0;
    }
}