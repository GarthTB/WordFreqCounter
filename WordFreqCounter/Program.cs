namespace WordFreqCounter;

internal static class Program
{
    private static void Main()
    {
        for (;;)
        {
            var fp = Init.GetFilePath();
            var wl = Init.GetWordLen();
            var wp = Init.GetWritePath(fp, wl);
            var th = Init.GetThreshold();
            var ex = Init.GetExtraChars();

            Console.WriteLine("使用参数：");
            Console.WriteLine($"语料文件：{fp}");
            Console.WriteLine($"结果文件：{wp}");
            Console.WriteLine($"词长：{wl}");
            Console.WriteLine($"阈值：{th}");
            Console.WriteLine($"额外字符：\n{string.Concat(ex)}");

            (Counter.Fp, Counter.Wp, Counter.Wl, Counter.Th) = (fp, wp, wl, th);
            Counter.IsShit = ex.Count == 0
                ? c => c is < '一' or > '鿿'
                : c => c is < '一' or > '鿿' && !ex.Contains(c);
            Counter.Run();
            Console.WriteLine("按y键进行新一轮统计...");
            if (Console.ReadKey().KeyChar != 'y') break;
        }
    }
}