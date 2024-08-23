using System.Collections.Concurrent;
using System.Text;

namespace WordFreqCounter
{
    internal struct Counter
    {
        private const char LO = '一';
        private const char HI = '鿿';
        private static string _readPath = string.Empty;
        private static string _writePath = string.Empty;
        private static HashSet<int> _extraChars = new(0);
        private static ConcurrentDictionary<string, int> _freqDict = new();
        private static ConcurrentDictionary<string, int> _resultDict = new();
        private static int _wordLength;
        private static int _wordLengthB;
        private static int _windowSizeB;
        private static int _filter;
        private static int _parallelNum;
        private static Func<char, bool> 字符无效 = (char c) => c is < LO or > HI;

        public static void SetCounter(string readPath, string writePath, int wordLength, int filter, HashSet<int> extraChars, int parallelNum)
        {
            _readPath = readPath;
            _writePath = writePath;
            _wordLength = wordLength;
            _wordLengthB = wordLength - 1;
            _windowSizeB = _wordLengthB * 2;
            _filter = filter - 1;
            _extraChars = extraChars;
            _parallelNum = parallelNum;
            _freqDict = new(parallelNum, 480000);
            _resultDict = new(parallelNum, 120000);
            if (_extraChars.Count > 0)
                字符无效 = (char c) => c is < LO or > HI && !_extraChars.Contains(c);
        }

        private static void LoadAllWords()
        {
            _ = Parallel.ForEach(File.ReadLines(_readPath),
                new ParallelOptions { MaxDegreeOfParallelism = _parallelNum },
                static line =>
            {
                int tail = 0;
                var span = line.AsSpan();
                for (int head = 0; head < span.Length; head++)
                {
                    if (字符无效(span[head]))
                        tail = head + 1;
                    else if (head == tail + _wordLengthB)
                    {
                        _ = _freqDict.AddOrUpdate(new string(span.Slice(tail, _wordLength)), 1, static (key, count) => count + 1);
                        tail++;
                    }
                }
            });
        }

        private static void LoadGoodWords()
        {
            _ = Parallel.ForEach(File.ReadLines(_readPath),
                new ParallelOptions { MaxDegreeOfParallelism = _parallelNum },
                static line =>
            {
                int tail = 0;
                var span = line.AsSpan();
                string maxWord = string.Empty, word = string.Empty;
                int maxFreq = 0, freq = 0;
                for (int head = 0; head < span.Length; head++)
                {
                    if (字符无效(span[head]))
                        tail = head + 1;
                    else if (head == tail + _windowSizeB)
                    {
                        maxFreq = 0;
                        for (int j = 0; j < _wordLength; j++)
                        {
                            word = new(span.Slice(tail + j, _wordLength));
                            freq = _freqDict[word];
                            if (freq > maxFreq && freq > _filter)
                            {
                                maxFreq = freq;
                                maxWord = word;
                            }
                        }
                        tail += _wordLength;
                        if (maxFreq > 0)
                            _ = _resultDict.AddOrUpdate(maxWord, 1, static (key, count) => count + 1);
                    }
                }
            });
        }

        private static IOrderedEnumerable<KeyValuePair<string, int>> Sort()
        {
            return _filter > 0
                ? _resultDict.Where(x => x.Value > _filter)
                             .OrderByDescending(x => x.Value)
                : _resultDict.OrderByDescending(x => x.Value);
        }

        private static void Output(IOrderedEnumerable<KeyValuePair<string, int>> sortedDict)
        {
            if (!sortedDict.Any()) throw new InvalidOperationException("没有符合条件的词。");
            using StreamWriter writer = new(_writePath, false, Encoding.UTF8);
            writer.WriteLine("词\t频率");
            foreach (var item in sortedDict)
                writer.WriteLine($"{item.Key}\t{item.Value}");
        }

        public static void Run()
        {
            try
            {
                Console.WriteLine("开始统计……");
                LoadAllWords();
                Console.WriteLine("第一轮统计结束。");
                LoadGoodWords();
                Console.WriteLine("第二轮统计结束。");
                Output(Sort());
                Console.WriteLine("统计结束，词频文件已生成。");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"错误：{ex.Message}");
            }
        }
    }
}
