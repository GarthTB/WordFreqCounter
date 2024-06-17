using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace WordFreqCounter
{
    internal struct Counter
    {
        private static string _readPath = string.Empty;
        private static string _writePath = string.Empty;
        private static HashSet<int> _extraChars = new(0);
        private static readonly Dictionary<string, int> _freqDict = new(131072);
        private static readonly Dictionary<string, int> _resultDict = new(65536);
        private static int _wordLength;
        private static int _windowSize;
        private static int _filter;

        public static void SetCounter(string readPath, string writePath, HashSet<int> extraChars, int wordLength, int filter)
        {
            _freqDict.Clear();
            _resultDict.Clear();
            _readPath = readPath;
            _writePath = writePath;
            _extraChars = extraChars;
            _wordLength = wordLength;
            _windowSize = wordLength * 2 - 1;
            _filter = filter;
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private static void LoadAllWords()
        {
            using StreamReader sr = new(_readPath, Encoding.UTF8);
            while (!sr.EndOfStream)
            {
                int tail = 0;
                var span = sr.ReadLine().AsSpan();
                for (int head = 0; head < span.Length; head++)
                {
                    var c = span[head];
                    if ((c < '一' || c > '鿿') && !_extraChars.Contains(c))
                    {
                        tail = head + 1;
                        continue;
                    }
                    if (head == tail + _wordLength - 1)
                    {
                        string word = new(span.Slice(tail, _wordLength));
                        ref var freq = ref CollectionsMarshal.GetValueRefOrNullRef(_freqDict, word);
                        if (Unsafe.IsNullRef(ref freq)) _freqDict.Add(word, 1);
                        else freq++;
                        tail++;
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private static void LoadGoodWords()
        {
            using StreamReader sr = new(_readPath, Encoding.UTF8);
            while (!sr.EndOfStream)
            {
                int tail = 0;
                var span = sr.ReadLine().AsSpan();
                for (int head = 0; head < span.Length; head++)
                {
                    var c = span[head];
                    if ((c < '一' || c > '鿿') && !_extraChars.Contains(c))
                    {
                        tail = head + 1;
                        continue;
                    }
                    if (head == tail + _windowSize - 1)
                    {
                        int maxFreq = 0;
                        string maxWord = string.Empty;
                        for (int j = 0; j < _wordLength; j++)
                        {
                            string _word = new(span.Slice(tail + j, _wordLength));
                            ref var _freq = ref CollectionsMarshal.GetValueRefOrNullRef(_freqDict, _word);
                            if (_freq < _filter) continue;
                            if (_freq > maxFreq)
                            {
                                maxFreq = _freq;
                                maxWord = _word;
                            }
                        }
                        tail += _wordLength;
                        if (maxFreq == 0) continue;
                        ref var freq = ref CollectionsMarshal.GetValueRefOrNullRef(_resultDict, maxWord);
                        if (Unsafe.IsNullRef(ref freq)) _resultDict.Add(maxWord, 1);
                        else freq++;
                    }
                }
            }
        }

        private static IEnumerable<KeyValuePair<string, int>> Sort()
        {
            return _filter > 1
                ? _resultDict.Where(x => x.Value >= _filter)
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
                LoadAllWords();
                Console.WriteLine("第一轮统计结束。");
                LoadGoodWords();
                Console.WriteLine("第二轮统计结束。");
                Output(Sort());
                Console.WriteLine("统计结束，词频文件已生成。");
            }
            catch (Exception ex)
                when (ex is InvalidOperationException
                         or IOException
                         or ArgumentException)
            {
                Console.WriteLine($"错误：{ex.Message}");
            }
        }
    }
}
