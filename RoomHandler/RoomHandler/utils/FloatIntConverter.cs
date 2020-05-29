using System;

public class FloatIntConverter {
    public static Tuple<short, short> convertFloat(float value) {
        short whole = (short)(value);
        short fractional = (short)((value - whole) * 1000);

        return new Tuple<short, short>(whole, fractional);
    }

    public static float convertInt(short whole, short fractional) {
        float result = whole;
        result += (float)(fractional / 1000f);

        return result;
    }
}
