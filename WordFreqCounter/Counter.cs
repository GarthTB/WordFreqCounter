using System.Collections.Concurrent;
using System.Text;

namespace WordFreqCounter
{
    internal static class Counter
    {
        public static string fp = string.Empty, wp = string.Empty;
        public static int wl, th;
        public static HashSet<char> ex = new();
        public static Func<char, bool> IsShit = static c => false;

        public static void Run()
        {
            try
            {
                t_wl = wl - 1;
                t_wz = t_wl * 2;
                Console.WriteLine("开始第一轮统计...");
                FirstRound();
                Console.WriteLine("开始第二轮统计...");
                SecondRound();
                Console.WriteLine("统计完成！排序并保存中...");
                SortAndSave();
                Console.WriteLine("完毕。词频文件已生成。");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"出错：{ex.Message}");
            }
        }

        private static readonly ConcurrentDictionary<string, int> tempDict = new(), realDict = new();
        private static int t_wl, t_wz;

        private static void FirstRound()
        {
            _ = Parallel.ForEach(File.ReadLines(fp),
                static line =>
                {
                    var sb = new StringBuilder(line);
                    int head = 0, tail = 0;
                    for (; tail < line.Length; tail++)
                    {
                        if (IsShit(line[tail]))
                            head = tail + 1;
                        else if (tail == head + t_wl)
                        {
                            _ = tempDict.AddOrUpdate(sb.ToString(head, wl), 1, static (key, count) => count + 1);
                            head++;
                        }
                    }
                });
        }

        private static void SecondRound()
        {
            _ = Parallel.ForEach(File.ReadLines(fp),
                static line =>
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
                            if (m_freq > 0)
                                _ = realDict.AddOrUpdate(m_word, 1, static (key, count) => count + 1);
                        }
                    }
                });
        }

        private static void SortAndSave()
        {
            var sorted = th > 0
                ? realDict.AsParallel()
                          .Where(x => x.Value > th)
                          .OrderByDescending(x => x.Value)
                : realDict.AsParallel()
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
