# 中文盲分词词频统计器

在我的个人电脑上，约10亿字的中文互联网语料，统计2字词，不加标点符号，大约1分半出结果。

语料文件须为UTF-8编码。默认中文范围为4e00-9fff（16进制）。

## 环境依赖

- [.NET 9.0运行时](https://dotnet.microsoft.com/zh-cn/download/dotnet/9.0)

## 统计原理：

每次进行两轮统计。假设要统计n字词。

- 第一轮：统计所有相邻的n个汉字组合出现的频率。
- 第二轮：每(2n-1)个相邻的字为一个滑动窗口，每个窗口中有n个词，滑动步长为n。根据第一轮统计的结果，统计窗口中词频最高的那一个词（最可能是词）。

## 更新日志

### [v0.5.0](https://github.com/GarthTB/WordFreqCounter/releases/tag/v0.5.0) - 2024-12-03

- 优化：升级为.NET 9框架

### [v0.4.0](https://github.com/GarthTB/WordFreqCounter/releases/tag/v0.4.0) - 2024-08-31

- 优化：经过跑分测试，改用性能最好的StringBuilder
- 优化：去除不必要的逻辑，去除命令行传参

### [v0.3.2](https://github.com/GarthTB/WordFreqCounter/releases/tag/v0.3.2) - 2024-08-24

- 修复：一处类型错误
- 优化：精简代码
- 优化：整理项目结构

### [v0.3.0](https://github.com/GarthTB/WordFreqCounter/releases/tag/v0.3) - 2024-06-19

- 并行计算，大幅提升性能

### [v0.2.2](https://github.com/GarthTB/WordFreqCounter/releases/tag/v0.2.2) - 2024-06-17

- 提升性能，漏洞修复

### [v0.1.0](https://github.com/GarthTB/WordFreqCounter/releases/tag/v0.1) - 2024-06-17

- 发布！
