using System;

public class Logger {
    public static void print(string message) {
        Console.WriteLine(String.Format("[{0:HH:mm:ss:ms}]: {1}", DateTime.Now, message));
    }
}
