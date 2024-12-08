using System.Collections.Concurrent;
using System.Text;

namespace WordFreqCounter
{
    internal static class Counter
    {
        internal static string fp = string.Empty, wp = string.Empty;
        internal static int wl, th;
        internal static HashSet<char> ex = [];
        internal static Func<char, bool> IsShit = static c => false;

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
            var t_wl = wl - 1;
            ConcurrentDictionary<string, int> tempDict = new();
            _ = Parallel.ForEach(File.ReadLines(fp), line =>
            {
                var sb = new StringBuilder(line);
                int head = 0, tail = 0;
                for (; tail < line.Length; tail++)
                {
                    if (IsShit(line[tail]))
                        head = tail + 1;
                    else if (tail == head + t_wl)
                    {
                        _ = tempDict.AddOrUpdate(
                            sb.ToString(head, wl), 1, static (key, count) => count + 1);
                        head++;
                    }
                }
            });
            return tempDict;
        }

        private static ConcurrentDictionary<string, int> SecondRound(ConcurrentDictionary<string, int> tempDict)
        {
            var t_wz = 2 * wl - 1 - 1;
            ConcurrentDictionary<string, int> finalDict = new();
            _ = Parallel.ForEach(File.ReadLines(fp), line =>
            {
                var sb = new StringBuilder(line);
                int head = 0, tail = 0, m_freq, freq;
                string m_word = string.Empty, word;
                for (; tail < line.Length; tail++)
                {
                    if (IsShit(line[tail]))
                        head = tail + 1;
                    else if (tail == head + t_wz)
                    {
                        m_freq = 0;
                        for (int i = 0; i < wl; i++)
                        {
                            word = sb.ToString(head + i, wl);
                            freq = tempDict[word];
                            if (freq > m_freq)
                            {
                                m_freq = freq;
                                m_word = word;
                            }
                        }
                        head += wl;
                        if (m_freq > th)
                            _ = finalDict.AddOrUpdate(
                                m_word, 1, static (key, count) => count + 1);
                    }
                }
            });
            return finalDict;
        }

        private static void SortAndSave(ConcurrentDictionary<string, int> finalDict)
        {
            var sorted = th > 0
                ? finalDict.AsParallel()
                           .Where(x => x.Value > th)
                           .OrderByDescending(x => x.Value)
                : finalDict.AsParallel()
                           .OrderByDescending(x => x.Value);

            var sb = new StringBuilder();
            foreach (var item in sorted)
                _ = sb.AppendLine($"{item.Key}\t{item.Value}");

            if (sb.Length == 0)
                throw new InvalidOperationException("没有任何大于阈值的词。");

            using StreamWriter writer = new(wp, false, Encoding.UTF8);
            writer.Write(sb.ToString());
        }
    }
}
