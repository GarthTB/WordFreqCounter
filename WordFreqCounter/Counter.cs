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
        private static Dictionary<string, int> _freqDict = new(131072);
        private static Dictionary<string, int> _resultDict = new(65536);
        private static int _wordLength;
        private static int _windowSize;
        private static int _filter;

        public static void SetCounter(string readPath, string writePath, HashSet<int> extraChars, int wordLength, int filter)
        {
            _freqDict = new Dictionary<string, int>(131072);
            _resultDict = new Dictionary<string, int>(65536);
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
            StringBuilder sb = new(_wordLength);
            while (!sr.EndOfStream)
            {
                var span = sr.ReadLine().AsSpan();
                for (int i = 0; i < span.Length; i++)
                {
                    var c = span[i];
                    if ((c < '一' || c > '鿿') && !_extraChars.Contains(c))
                    {
                        sb.Clear();
                        continue;
                    }
                    sb.Append(c);
                    if (sb.Length == _wordLength)
                    {
                        var word = sb.ToString();
                        ref var freq = ref CollectionsMarshal.GetValueRefOrNullRef(_freqDict, word);
                        if (Unsafe.IsNullRef(ref freq)) _freqDict.Add(word, 1);
                        else freq++;
                        sb.Remove(0, 1);
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private static void LoadGoodWords()
        {
            using StreamReader sr = new(_readPath, Encoding.UTF8);
            StringBuilder sb = new(_windowSize);
            while (!sr.EndOfStream)
            {
                var span = sr.ReadLine().AsSpan();
                for (int i = 0; i < span.Length; i++)
                {
                    var c = span[i];
                    if ((c < '一' || c > '鿿') && !_extraChars.Contains(c))
                    {
                        sb.Clear();
                        continue;
                    }
                    sb.Append(c);
                    if (sb.Length == _windowSize)
                    {
                        string window = sb.ToString();
                        sb.Remove(0, _wordLength);
                        int maxFreq = 0;
                        string maxWord = string.Empty;
                        for (int j = 0; j < _wordLength; j++)
                        {
                            string _word = window.Substring(j, _wordLength);
                            ref var _freq = ref CollectionsMarshal.GetValueRefOrNullRef(_freqDict, _word);
                            if (_freq < _filter) continue;
                            if (_freq > maxFreq)
                            {
                                maxFreq = _freq;
                                maxWord = _word;
                            }
                        }
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
