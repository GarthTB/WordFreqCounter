using System.Collections.Concurrent;
using System.Text;

namespace WordFreqCounter;

internal static class Counter
{
    internal static string Fp = string.Empty, Wp = string.Empty;
    internal static int Wl, Th;
    internal static Func<char, bool> IsShit = static _ => false;

    internal static void Run()
    {
        try
        {
            Console.WriteLine("开始第一轮统计...");
            var tempDict = FirstRound();
            Console.WriteLine("开始第二轮统计...");
            var finalDict = SecondRound(tempDict);
            Console.WriteLine("统计完成！排序并保存中...");
            SortAndSave(finalDict);
            Console.WriteLine("完毕。词频文件已生成。");
        }
        catch (Exception ex) { Console.WriteLine($"出错：{ex.Message}"); }
    }

    private static ConcurrentDictionary<string, int> FirstRound()
    {
        var tWl = Wl - 1;
        ConcurrentDictionary<string, int> tempDict = new();
        File.ReadLines(Fp)
            .AsParallel()
            .ForAll(
                line =>
                {
                    int head = 0, tail = 0;
                    for (; tail < line.Length; tail++)
                        if (IsShit(line[tail]))
                            head = tail + 1;
                        else if (tail == head + tWl)
                        {
                            _ = tempDict.AddOrUpdate(line.Substring(head, Wl), 1, static (_, count) => count + 1);
                            head++;
                        }
                });
        return tempDict;
    }

    private static ConcurrentDictionary<string, int> SecondRound(ConcurrentDictionary<string, int> tempDict)
    {
        var tWz = 2 * Wl - 1 - 1;
        ConcurrentDictionary<string, int> finalDict = new();
        File.ReadLines(Fp)
            .AsParallel()
            .ForAll(
                line =>
                {
                    int head = 0, tail = 0, mFreq = 0;
                    var mWord = string.Empty;
                    for (; tail < line.Length; tail++)
                        if (IsShit(line[tail]))
                            head = tail + 1;
                        else if (tail == head + tWz)
                        {
                            for (var i = 0; i < Wl; i++)
                            {
                                var word = line.Substring(head + i, Wl);
                                var freq = tempDict[word];
                                if (freq <= mFreq) continue;
                                mFreq = freq;
                                mWord = word;
                            }
                            head += Wl;
                            if (mFreq > Th) _ = finalDict.AddOrUpdate(mWord, 1, static (_, count) => count + 1);
                        }
                });
        return finalDict;
    }

    private static void SortAndSave(ConcurrentDictionary<string, int> finalDict)
    {
        var output = Th > 0
            ? finalDict.AsParallel().Where(x => x.Value > Th)
            : finalDict.AsParallel();

        var sb = new StringBuilder();
        foreach (var item in output.OrderByDescending(x => x.Value)) _ = sb.AppendLine($"{item.Key}\t{item.Value}");

        if (sb.Length == 0) throw new InvalidOperationException("没有任何大于阈值的词。");

        using StreamWriter writer = new(Wp, false, Encoding.UTF8);
        writer.Write(sb.ToString());
    }
}