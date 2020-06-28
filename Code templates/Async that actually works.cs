using System.Threading.Tasks;
using System;
using System.Collections.Generic;

class Program {
    static void Main() {
        stuff();
        Console.WriteLine("DDDDDDDDDD");
    }

    static async void stuff() {
        List<Task> tasks = new List<Task>();
        Task t1 = Task.Run(() => f(50));
        Task t2 = Task.Run(() => g(8000));

        tasks.Add(t1);
        tasks.Add(t2);

        Task t = Task.WhenAll(tasks);
        try {
            t.Wait();
        } catch { }
    }

    static async Task<int> f(int x) {
        for (int i = 0; i < x; i++) {
            Console.WriteLine("X = " + i);
        }
        return x;
    }

    static async Task<int> g(int x) {
        for (int i = 0; i < x; i++) {
            Console.WriteLine("Y = " + i);
        }
        return x;
    }
}