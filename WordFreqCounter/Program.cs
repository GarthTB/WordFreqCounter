namespace WordFreqCounter
{
    internal static class Program
    {
        private static bool IsInvalidWordLength(int wordLength) => wordLength is < 1 or > (int.MaxValue / 2 + 1);

        private static string GetReadPath()
        {
            Console.Write("请输入要读取的文件路径，或将\"语料.txt\"放在当前目录下并按回车键：\n");
            for (; ; )
            {
                string? readPath = Console.ReadLine();
                if (string.IsNullOrEmpty(readPath)) readPath = Path.Combine(".", "语料.txt");
                if (File.Exists(readPath)) return readPath;
                Console.Write("找不到文件，请重试！");
            }
        }

        private static int GetWordLength()
        {
            Console.Write("请指定要统计的词长：");
            int wordLength;
            while (!int.TryParse(Console.ReadLine(), out wordLength) || IsInvalidWordLength(wordLength))
                Console.Write($"词长错误，请输入一个大于0且小于{int.MaxValue / 2 + 1}的整数！");
            return wordLength;
        }

        private static int GetFilter()
        {
            Console.Write("请指定过滤频率，低于该频率的词将被忽略，留空则默认为2：");
            return (int.TryParse(Console.ReadLine(), out int filter) && filter > 0)
                ? filter
                : 2;
        }

        private static HashSet<int> GetExtraChars()
        {
            Console.Write("请输入一行要纳入的非中文字符，留空则只统计\\u4e00-\\u9fff：\n");
            HashSet<int> extraChars = new(0);
            foreach (char c in Console.ReadLine() ?? string.Empty)
                extraChars.Add(c);
            extraChars.RemoveWhere(c => c is > '䷿' and < 'ꀀ');
            return extraChars;
        }

        private static int GetParallelNum()
        {
            Console.Write("请输入并行处理的线程数，留空则默认为CPU核数：");
            return (int.TryParse(Console.ReadLine(), out int parallelNum) && parallelNum > 0)
                ? parallelNum
                : Environment.ProcessorCount;
        }

        private static void Main(string[] args)
        {
            if (args.Length == 5) RunWithArgs(args);
            else RunWithoutArgs();
        }

        /// <summary>
        /// 参数1：词长
        /// 参数2：读取的文件路径
        /// 参数3：过滤频率
        /// 参数4：要纳入的非中文字符
        /// 参数5：并行处理的线程数
        /// </summary>
        /// <param name="args"></param>
        private static void RunWithArgs(string[] args)
        {
            try
            {
                if (int.TryParse(args[0], out int wordLength) && !IsInvalidWordLength(wordLength))
                {
                    if (File.Exists(args[1]))
                    {
                        string writePath = Path.Combine(Path.GetDirectoryName(args[1]) ?? ".", $"{wordLength}字统计结果.txt");
                        int filter = (int.TryParse(args[2], out filter) && filter > 0)
                            ? filter
                            : 2;
                        int parallelNum = (int.TryParse(args[4], out parallelNum) && parallelNum > 0)
                            ? parallelNum
                            : Environment.ProcessorCount;
                        HashSet<int> extraChars = new(0);
                        foreach (char c in args[3])
                            extraChars.Add(c);
                        Counter.SetCounter(args[1], writePath, wordLength, filter, extraChars, parallelNum);
                        Counter.Run();
                    }
                    else throw new ArgumentException("文件不存在！");
                }
                else throw new ArgumentException("词长错误！");
            }
            catch (ArgumentException e) { Console.WriteLine(e.Message); }
            Console.Write("按任意键退出。");
            Console.ReadKey();
        }

        private static void RunWithoutArgs()
        {
            for (; ; )
            {
                string readPath = GetReadPath();
                int wordLength = GetWordLength();
                string writePath = Path.Combine(Path.GetDirectoryName(readPath) ?? ".", $"{wordLength}字统计结果.txt");
                int filter = GetFilter();
                HashSet<int> extraChars = GetExtraChars();
                int parallelNum = GetParallelNum();
                Counter.SetCounter(readPath, writePath, wordLength, filter, extraChars, parallelNum);
                Counter.Run();
                Console.Write("按任意键重新指定参数并进行新一轮统计。");
                Console.ReadKey();
            }
        }
    }
}
