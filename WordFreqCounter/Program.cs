namespace WordFreqCounter
{
    internal static class Program
    {
        private static int GetWordLength()
        {
            Console.Write("请指定要统计的词长：");
            int wordLength;
            while (!int.TryParse(Console.ReadLine(), out wordLength) || IsInvalidWordLength(wordLength))
                Console.Write($"词长错误，请输入一个大于1且小于{int.MaxValue / 2 + 1}的整数！");
            return wordLength;
        }

        private static bool IsInvalidWordLength(int wordLength) => wordLength is < 1 or > (int.MaxValue / 2 + 1);

        private static string GetReadPath()
        {
            Console.Write("请输入要读取的文件路径，或将\"语料.txt\"放在当前目录下并按回车键：");
            for (; ; )
            {
                string? readPath = Console.ReadLine();
                if (string.IsNullOrEmpty(readPath)) readPath = Path.Combine(".", "语料.txt");
                if (File.Exists(readPath)) return readPath;
                Console.Write("找不到文件，请重试！");
            }
        }

        private static int GetFilter()
        {
            Console.Write("请指定过滤频率，低于该频率的词将被忽略，留空则默认为2，可以为0：");
            return int.TryParse(Console.ReadLine(), out int filter) ? filter : 2;
        }

        private static HashSet<int> GetExtraChars()
        {
            Console.Write("请输入要纳入的非中文字符，留空则只统计\\u4e00-\\u9fff：");
            HashSet<int> extraChars = new(0);
            foreach (char c in Console.ReadLine() ?? string.Empty)
                extraChars.Add(c);
            return extraChars;
        }

        private static void Main(string[] args)
        {
            if (args.Length == 4) RunWithArgs(args);
            else RunWithoutArgs();
        }

        /// <summary>
        /// 第一个参数为词长
        /// 第二个参数为读取的文件路径
        /// 第三个参数为过滤频率
        /// 第四个参数为要纳入的非中文字符
        /// </summary>
        /// <param name="args"></param>
        private static void RunWithArgs(string[] args)
        {
            if (int.TryParse(args[0], out int wordLength) && !IsInvalidWordLength(wordLength))
            {
                if (File.Exists(args[1]))
                {
                    string writePath = Path.Combine(Path.GetDirectoryName(args[1]) ?? ".", $"{wordLength}字统计结果.txt");
                    if (int.TryParse(args[2], out int filter))
                    {
                        HashSet<int> extraChars = new(0);
                        foreach (char c in args[3])
                            extraChars.Add(c);
                        Counter.SetCounter(args[1], writePath, extraChars, wordLength, filter);
                        Counter.Run();
                    }
                }
            }
            Console.Write("按任意键退出。");
            Console.ReadKey();
        }

        private static void RunWithoutArgs()
        {
            for (; ; )
            {
                int wordLength = GetWordLength();
                string readPath = GetReadPath();
                string writePath = Path.Combine(Path.GetDirectoryName(readPath) ?? ".", $"{wordLength}字统计结果.txt");
                int filter = GetFilter();
                HashSet<int> extraChars = GetExtraChars();
                Counter.SetCounter(readPath, writePath, extraChars, wordLength, filter);
                Counter.Run();
                Console.Write("按任意键重新指定参数并进行新一轮统计。");
                Console.ReadKey();
            }
        }
    }
}
