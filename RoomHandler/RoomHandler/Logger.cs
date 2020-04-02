using System;

public class Logger {
    public static void print(string message) {
        Console.WriteLine(String.Format("[{0:HH:mm:ss tt}]: {1}", DateTime.Now, message));
    }
}
