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

        public static void SetCounter(string readPath, string writePath, int wordLength, int filter, HashSet<int> extraChars, int parallelNum)
        {
            _readPath = readPath;
            _writePath = writePath;
            _wordLength = wordLength;
            _wordLengthB = wordLength - 1;
            _windowSizeB = wordLength * 2 - 2;
            _filter = filter - 1;
            _extraChars = extraChars;
            _parallelNum = parallelNum;
            _freqDict = new(parallelNum, 131072);
            _resultDict = new(parallelNum, 65536);
        }

        private static void LoadAllWordsA()
        {
            Parallel.ForEach(File.ReadLines(_readPath),
                new ParallelOptions { MaxDegreeOfParallelism = _parallelNum },
                static line =>
            {
                int tail = 0;
                var span = line.AsSpan();
                for (int head = 0; head < span.Length; head++)
                {
                    var c = span[head];
                    if ((c < LO || c > HI) && !_extraChars.Contains(c))
                        tail = head + 1;
                    else if (head == tail + _wordLengthB)
                    {
                        _freqDict.AddOrUpdate(new string(span.Slice(tail, _wordLength)), 1, static (key, count) => count + 1);
                        tail++;
                    }
                }
            });
        }

        private static void LoadAllWordsB()
        {
            Parallel.ForEach(File.ReadLines(_readPath),
                new ParallelOptions { MaxDegreeOfParallelism = _parallelNum },
                static line =>
            {
                int tail = 0;
                var span = line.AsSpan();
                for (int head = 0; head < span.Length; head++)
                {
                    var c = span[head];
                    if (c < LO || c > HI)
                        tail = head + 1;
                    else if (head == tail + _wordLengthB)
                    {
                        _freqDict.AddOrUpdate(new string(span.Slice(tail, _wordLength)), 1, static (key, count) => count + 1);
                        tail++;
                    }
                }
            });
        }

        private static void LoadGoodWordsA()
        {
            Parallel.ForEach(File.ReadLines(_readPath),
                new ParallelOptions { MaxDegreeOfParallelism = _parallelNum },
                static line =>
            {
                int tail = 0;
                var span = line.AsSpan();
                int maxFreq = 0;
                var maxWord = string.Empty;
                for (int head = 0; head < span.Length; head++)
                {
                    var c = span[head];
                    if ((c < LO || c > HI) && !_extraChars.Contains(c))
                        tail = head + 1;
                    else if (head == tail + _windowSizeB)
                    {
                        maxFreq = 0;
                        for (int j = 0; j < _wordLength; j++)
                        {
                            string _word = new(span.Slice(tail + j, _wordLength));
                            int _freq = _freqDict[_word];
                            if (_freq > maxFreq && _freq > _filter)
                            {
                                maxFreq = _freq;
                                maxWord = _word;
                            }
                        }
                        tail += _wordLength;
                        if (maxFreq > 0)
                            _resultDict.AddOrUpdate(maxWord, 1, static (key, count) => count + 1);
                    }
                }
            });
        }

        private static void LoadGoodWordsB()
        {
            Parallel.ForEach(File.ReadLines(_readPath),
                new ParallelOptions { MaxDegreeOfParallelism = _parallelNum },
                static line =>
            {
                int tail = 0;
                var span = line.AsSpan();
                int maxFreq = 0;
                var maxWord = string.Empty;
                for (int head = 0; head < span.Length; head++)
                {
                    var c = span[head];
                    if (c < LO || c > HI)
                        tail = head + 1;
                    else if (head == tail + _windowSizeB)
                    {
                        maxFreq = 0;
                        for (int j = 0; j < _wordLength; j++)
                        {
                            string _word = new(span.Slice(tail + j, _wordLength));
                            int _freq = _freqDict[_word];
                            if (_freq > maxFreq && _freq > _filter)
                            {
                                maxFreq = _freq;
                                maxWord = _word;
                            }
                        }
                        tail += _wordLength;
                        if (maxFreq > 0)
                            _resultDict.AddOrUpdate(maxWord, 1, static (key, count) => count + 1);
                    }
                }
            });
        }

        private static IEnumerable<KeyValuePair<string, int>> Sort()
        {
            return _filter > 0
                ? _resultDict.Where(x => x.Value > _filter)
                             .OrderByDescending(x => x.Value)
                : _resultDict.OrderByDescending(x => x.Value);
        }

        private static void Output(IEnumerable<KeyValuePair<string, int>> sortedDict)
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
                if (_extraChars.Any())
                {
                    LoadAllWordsA();
                    Console.WriteLine("第一轮统计结束。");
                    LoadGoodWordsA();
                }
                else
                {
                    LoadAllWordsB();
                    Console.WriteLine("第一轮统计结束。");
                    LoadGoodWordsB();
                }
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
