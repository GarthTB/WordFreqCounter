namespace WordFreqCounter
{
    internal static class Tools
    {
        public static string GetFilePath()
        {
            Console.WriteLine("请指定语料文件：");
            var filePath = Console.ReadLine();
            while (!File.Exists(filePath))
            {
                Console.WriteLine("文件不存在，请重新指定：");
                filePath = Console.ReadLine();
            }
            return filePath;
        }

        public static string GetWritePath(string fp, int wl)
        {
            var dir = Path.GetDirectoryName(fp)
                ?? throw new InvalidOperationException();
            var name = Path.GetFileNameWithoutExtension(fp);
            return Path.Combine(dir, $"{name}_{wl}字词频统计结果.txt");
        }

        public static int GetWordLen()
        {
            Console.WriteLine("请指定词长：");
            var input = Console.ReadLine();
            int wordLen;
            while (!int.TryParse(input, out wordLen) || wordLen <= 0)
            {
                Console.WriteLine("词长必须为正整数，请重新指定：");
                input = Console.ReadLine();
            }
            return wordLen;
        }

        public static int GetThreshold()
        {
            Console.WriteLine("请指定阈值，此次数及以下的词将被忽略，默认值为1：");
            var input = Console.ReadLine();
            return int.TryParse(input, out var threshold) && threshold >= 0
                ? threshold : 1;
        }

        public static HashSet<char> GetExtraChars()
        {
            Console.WriteLine("请指定需要纳入的额外字符，回车结束，若无请留空：");
            var extraChars = Console.ReadLine();
            return extraChars is null || extraChars.Length == 0
                ? [] : [.. extraChars.Where(c => c is < '一' or > '鿿')];
        }
    }
}
