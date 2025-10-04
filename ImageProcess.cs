using SkiaSharp;
namespace PictureYOLO
{
    public class ImageProcess
    {
        public SKBitmap? _saveBitmap;
        public SKBitmap? _originalBitmap;
        public SKBitmap? _processedBitmap;
        public byte[]? _DataValues; 
        public int _startX = 0; 
        public int _endX = 0; 
        public SKBitmap? _croppedColorBitmap;
        public double TCrate;
        public ImageProcess()
        {
            _originalBitmap = null;
            _processedBitmap = null;
        }
        public ImageProcess(SKBitmap sKBitmap)
        {
            _originalBitmap = sKBitmap.Copy();
            _processedBitmap = _originalBitmap.Copy();
        }
        public void LoadImage(SKBitmap sKBitmap)
        {
            _originalBitmap = sKBitmap;
            _processedBitmap = null;
        }
        public void ApplyGrayscale(SKBitmap bitmap)
        {
            using var pixmap = bitmap.PeekPixels();
            var pixels = pixmap.GetPixelSpan<byte>();
            int width = bitmap.Width;
            int height = bitmap.Height;
            int pixelCount = width * height;
            int middleY = height / 2;
            _startX = (int)(width * 0.1);
            _endX = (int)(width * 0.9);
            int segmentWidth = _endX - _startX;
            _DataValues = new byte[segmentWidth];
#if WINDOWS
        for (int i = 0; i < pixelCount; i++)
            {
                int idx = i * 4;  // 每个像素4字节 (RGBA)
                int x = i % width;
                int y = i / width;
                byte b = pixels[idx];
                byte g = pixels[idx + 1];
                byte r = pixels[idx + 2];
                byte gray = (byte)(r * 0.299f + g * 0.587f + b * 0.114f);
                pixels[idx] = gray;     // R
                pixels[idx + 1] = gray; // G
                pixels[idx + 2] = gray; // B
                if (y == middleY && x >= _startX && x < _endX)
                {
                    _DataValues[x - _startX] = gray;
                }
            }
#endif
#if ANDROID
        for (int i = 0; i < pixelCount; i++)
            {
                int idx = i * 4;  // 每个像素4字节 (RGBA)
                int x = i % width;
                int y = i / width;
                byte r = pixels[idx];
                byte g = pixels[idx + 1];
                byte b = pixels[idx + 2];
                byte gray = (byte)(r * 0.299f + g * 0.587f + b * 0.114f);
                pixels[idx] = gray;     // R
                pixels[idx + 1] = gray; // G
                pixels[idx + 2] = gray; // B
                if (y == middleY && x >= _startX && x < _endX)
                {
                    _DataValues[x - _startX] = gray;
                }
            }
#endif
        }

        public Task ApplyBinary(SKBitmap bitmap, byte threshold)
        {
            return Task.Run(() =>
            {
                using var pixmap = bitmap.PeekPixels();
                var pixels = pixmap.GetPixelSpan<byte>();
                int width = bitmap.Width;
                int height = bitmap.Height;
                int pixelCount = width * height;
#if ANDROID
            for (int i = 0; i < pixelCount; i++)
                {
                    int idx = i * 4;
                    byte gray = pixels[idx];
                    byte binary = gray > threshold ? (byte)255 : (byte)0;

                    pixels[idx] = binary;     // R
                    pixels[idx + 1] = binary; // G
                    pixels[idx + 2] = binary; // B
                }
#endif
#if WINDOWS
                for (int i = 0; i < pixelCount; i++)
                {
                    int idx = i * 4;
                    byte gray = pixels[idx];
                    byte binary = gray > threshold ? (byte)255 : (byte)0;

                    pixels[idx + 2] = binary;     // R
                    pixels[idx + 1] = binary; // G
                    pixels[idx + 0] = binary; // B
                }
#endif

            });
        }

        public Task FanApplyBinary(SKBitmap bitmap, byte threshold)
        {
            return Task.Run(() =>
            {
                using var pixmap = bitmap.PeekPixels();
                var pixels = pixmap.GetPixelSpan<byte>();
                int width = bitmap.Width;
                int height = bitmap.Height;
                int pixelCount = width * height;
#if ANDROID
            for (int i = 0; i < pixelCount; i++)
                {
                    int idx = i * 4;
                    byte gray = pixels[idx];
                    byte binary = gray > threshold ? (byte)0 : (byte)255;

                    pixels[idx] = binary;     // R
                    pixels[idx + 1] = binary; // G
                    pixels[idx + 2] = binary; // B
                }
#endif
#if WINDOWS
                for (int i = 0; i < pixelCount; i++)
                {
                    int idx = i * 4;
                    byte gray = pixels[idx];
                    byte binary = gray > threshold ? (byte)0 : (byte)255;

                    pixels[idx + 2] = binary;     // R
                    pixels[idx + 1] = binary; // G
                    pixels[idx + 0] = binary; // B
                }
#endif

            });
        }
        public void ApplyGaussianBlur(SKBitmap bitmap, int kernelSize)
        {
            float[,] kernel = {
                {1f/16, 2f/16, 1f/16},
                {2f/16, 4f/16, 2f/16},
                {1f/16, 2f/16, 1f/16}
            };
            using var tempBitmap = new SKBitmap(bitmap.Width, bitmap.Height);
            using var pixmap = bitmap.PeekPixels();
            using var tempPixmap = tempBitmap.PeekPixels();

            var pixels = pixmap.GetPixelSpan<byte>();
            var tempPixels = tempPixmap.GetPixelSpan<byte>();

            int width = bitmap.Width;
            int height = bitmap.Height;
            int halfKernel = kernelSize / 2;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float r = 0, g = 0, b = 0;
                    for (int ky = -halfKernel; ky <= halfKernel; ky++)
                    {
                        for (int kx = -halfKernel; kx <= halfKernel; kx++)
                        {
                            int px = Math.Clamp(x + kx, 0, width - 1);
                            int py = Math.Clamp(y + ky, 0, height - 1);

                            int idx = (py * width + px) * 4;
                            float weight = kernel[ky + halfKernel, kx + halfKernel];

                            r += pixels[idx] * weight;
                            g += pixels[idx + 1] * weight;
                            b += pixels[idx + 2] * weight;
                        }
                    }
                    int outIdx = (y * width + x) * 4;
                    tempPixels[outIdx] = (byte)r;
                    tempPixels[outIdx + 1] = (byte)g;
                    tempPixels[outIdx + 2] = (byte)b;
                    tempPixels[outIdx + 3] = pixels[outIdx + 3]; // 保留Alpha
                }
            }
            tempPixels.CopyTo(pixels);
        }


        public void Rotate90(ref SKBitmap bitmap)
        {
            var rotatedBitmap = new SKBitmap(bitmap.Height, bitmap.Width);
            using (var canvas = new SKCanvas(rotatedBitmap))
            {
                canvas.Clear(SKColors.Transparent);
                canvas.Translate(rotatedBitmap.Width, 0);
                canvas.RotateDegrees(90);
                canvas.DrawBitmap(bitmap, 0, 0);
            }
            bitmap.Dispose();
            bitmap = rotatedBitmap;
        }

        public SKRectI FindLargestConnectedComponent(SKBitmap binaryBitmap)
        {
            int width = binaryBitmap.Width;
            int height = binaryBitmap.Height;
            using var pixmap = binaryBitmap.PeekPixels();
            var pixels = pixmap.GetPixelSpan<byte>();
            var parents = new Dictionary<int, int>();
            var sizes = new Dictionary<int, int>();
            int[,] labels = new int[height, width];
            int currentLabel = 1;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int idx = (y * width + x) * 4;

                    if (pixels[idx] == 255) 
                    {
                        int left = (x > 0) ? labels[y, x - 1] : 0;
                        int top = (y > 0) ? labels[y - 1, x] : 0;

                        if (left == 0 && top == 0)
                        {
                            labels[y, x] = currentLabel;
                            parents[currentLabel] = currentLabel;
                            sizes[currentLabel] = 1;
                            currentLabel++;
                        }
                        else
                        {
                            int minNeighbor = 0;
                            if (left != 0 && top != 0)
                                minNeighbor = Math.Min(left, top);
                            else
                                minNeighbor = (left != 0) ? left : top;

                            labels[y, x] = minNeighbor;
                            if (left != 0 && left != minNeighbor)
                                Union(minNeighbor, left, parents, sizes);
                            if (top != 0 && top != minNeighbor)
                                Union(minNeighbor, top, parents, sizes);
                            sizes[Find(minNeighbor, parents)]++;
                        }
                    }
                }
            }
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int label = labels[y, x];
                    if (label > 0)
                    {
                        labels[y, x] = Find(label, parents);
                    }
                }
            }

            var regionSizes = new Dictionary<int, int>();
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int label = labels[y, x];
                    if (label > 0)
                    {
                        if (!regionSizes.ContainsKey(label))
                            regionSizes[label] = 0;
                        regionSizes[label]++;
                    }
                }
            }

            int maxLabel = 0;
            int maxSize = 0;
            foreach (var kvp in regionSizes)
            {
                if (kvp.Value > maxSize)
                {
                    maxSize = kvp.Value;
                    maxLabel = kvp.Key;
                }
            }

            int minX = int.MaxValue, minY = int.MaxValue;
            int maxX = int.MinValue, maxY = int.MinValue;
            bool found = false;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (labels[y, x] == maxLabel)
                    {
                        found = true;
                        minX = Math.Min(minX, x);
                        minY = Math.Min(minY, y);
                        maxX = Math.Max(maxX, x);
                        maxY = Math.Max(maxY, y);
                    }
                }
            }

            if (!found || minX > maxX || minY > maxY)
            {
                return new SKRectI(0, 0, width, height);
            }

            int padding = 5;
            minX = Math.Max(0, minX - padding);
            minY = Math.Max(0, minY - padding);
            maxX = Math.Min(width - 1, maxX + padding);
            maxY = Math.Min(height - 1, maxY + padding);

            return new SKRectI(minX, minY, maxX, maxY);
        }

        private int Find(int label, Dictionary<int, int> parents)
        {
            if (!parents.ContainsKey(label)) return label;
            if (parents[label] != label)
            {
                parents[label] = Find(parents[label], parents);
            }
            return parents[label];
        }

        private void Union(int root1, int root2, Dictionary<int, int> parents, Dictionary<int, int> sizes)
        {
            int x = Find(root1, parents);
            int y = Find(root2, parents);
            if (x == y) return;
            if (!sizes.ContainsKey(x)) sizes[x] = 1;
            if (!sizes.ContainsKey(y)) sizes[y] = 1;

            if (sizes[x] < sizes[y])
            {
                parents[x] = y;
                sizes[y] += sizes[x];
            }
            else
            {
                parents[y] = x;
                sizes[x] += sizes[y];
            }
        }


        /// <summary>
        /// 裁剪指定边界框内的图片
        /// </summary>
        /// <param name="sourceBitmap">原始位图</param>
        /// <param name="boundingBox">裁剪的矩形区域（SKRectI）</param>
        /// <returns>裁剪后的新位图</returns>
        public SKBitmap? CropImage(SKBitmap sourceBitmap, SKRectI boundingBox)
        {
            // 确保边界框有效
            if (boundingBox.Width <= 0 || boundingBox.Height <= 0)
            {
                return null;
            }

            // 确保边界框在原始图片范围内
            int left = Math.Max(0, boundingBox.Left);
            int top = Math.Max(0, boundingBox.Top);
            int right = Math.Min(sourceBitmap.Width, boundingBox.Right);
            int bottom = Math.Min(sourceBitmap.Height, boundingBox.Bottom);

            // 重新计算裁剪区域
            int cropWidth = right - left;
            int cropHeight = bottom - top;

            if (cropWidth <= 0 || cropHeight <= 0)
            {
                return null;
            }

            // 使用 SKBitmap.ExtractSubset 方法进行裁剪，这是最简洁高效的方法
            // 注意：ExtractSubset 返回一个新的 SKBitmap，它是对源位图子集的引用。
            // 建议通过 Copy() 创建一个完全独立的副本，以确保后续操作（如导航）不会影响到其他部分。
            using var subset = new SKBitmap();
            if (sourceBitmap.ExtractSubset(subset, new SKRectI(left, top, right, bottom)))
            {
                return subset.Copy();
            }

            return null;
        }

        /// <summary>
        /// 将图像宽度拉伸至指定的宽度，保持宽高比不变
        /// </summary>
        /// <param name="sourceBitmap">原始位图</param>
        /// <param name="targetWidth">目标宽度</param>
        /// <returns>拉伸后的新位图</returns>
        public SKBitmap StretchImageWidth(SKBitmap sourceBitmap, int targetWidth)
        {
            if (sourceBitmap == null || targetWidth <= 0)
                return null;

            // 计算保持宽高比的目标高度
            float aspectRatio = (float)sourceBitmap.Height / sourceBitmap.Width;
            int targetHeight = (int)(targetWidth * aspectRatio);

            // 创建目标位图
            var stretchedBitmap = new SKBitmap(targetWidth, targetHeight);

            using (var canvas = new SKCanvas(stretchedBitmap))
            {
                // 设置高质量渲染
                canvas.Clear(SKColors.Transparent);

                // 创建缩放矩阵
                var scale = SKMatrix.CreateScale(
                    (float)targetWidth / sourceBitmap.Width,
                    (float)targetHeight / sourceBitmap.Height);

                // 设置抗锯齿
                canvas.SetMatrix(scale);

                // 绘制原始图像到缩放后的画布
                using (var paint = new SKPaint())
                {
                    paint.FilterQuality = SKFilterQuality.High; // 高质量过滤
                    canvas.DrawBitmap(sourceBitmap, 0, 0, paint);
                }
            }

            return stretchedBitmap;
        }

        /// <summary>
        /// 将图像宽度拉伸至指定的宽度（直接修改原始位图）
        /// </summary>
        /// <param name="bitmap">原始位图（将被修改）</param>
        /// <param name="targetWidth">目标宽度</param>
        public void StretchImageWidth(ref SKBitmap bitmap, int targetWidth)
        {
            if (bitmap == null || targetWidth <= 0)
                return;

            // 计算保持宽高比的目标高度
            float aspectRatio = (float)bitmap.Height / bitmap.Width;
            int targetHeight = (int)(targetWidth * aspectRatio);

            // 创建临时位图用于拉伸
            using (var tempBitmap = new SKBitmap(targetWidth, targetHeight))
            {
                using (var canvas = new SKCanvas(tempBitmap))
                {
                    // 设置高质量渲染
                    canvas.Clear(SKColors.Transparent);

                    // 创建缩放矩阵
                    var scale = SKMatrix.CreateScale(
                        (float)targetWidth / bitmap.Width,
                        (float)targetHeight / bitmap.Height);

                    // 设置抗锯齿
                    canvas.SetMatrix(scale);

                    // 绘制原始图像到缩放后的画布
                    using (var paint = new SKPaint())
                    {
                        paint.FilterQuality = SKFilterQuality.High; // 高质量过滤
                        canvas.DrawBitmap(bitmap, 0, 0, paint);
                    }
                }

                // 释放原始位图并替换为拉伸后的位图
                bitmap.Dispose();
                bitmap = tempBitmap.Copy();
            }
        }
        public void CropToRegion(SKBitmap sourceBitmap, SKRectI boundingBox, ref SKBitmap targetBitmap)
        {
            int minX = boundingBox.Left;
            int minY = boundingBox.Top;
            int maxX = boundingBox.Right;
            int maxY = boundingBox.Bottom;
            int cropWidth = maxX - minX + 1;
            int cropHeight = maxY - minY + 1;
            _croppedColorBitmap?.Dispose();
            _croppedColorBitmap = new SKBitmap(cropWidth, cropHeight, sourceBitmap.ColorType, sourceBitmap.AlphaType);

            using var sourcePixmap = sourceBitmap.PeekPixels();
            using var croppedPixmap = _croppedColorBitmap.PeekPixels();

            var sourcePixels = sourcePixmap.GetPixelSpan<byte>();
            var croppedPixels = croppedPixmap.GetPixelSpan<byte>();

            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    int srcIdx = (y * sourceBitmap.Width + x) * 4;
                    int dstY = y - minY;
                    int dstX = x - minX;
                    int dstIdx = (dstY * cropWidth + dstX) * 4;
                    croppedPixels[dstIdx] = sourcePixels[srcIdx];         // R
                    croppedPixels[dstIdx + 1] = sourcePixels[srcIdx + 1]; // G
                    croppedPixels[dstIdx + 2] = sourcePixels[srcIdx + 2]; // B
                    croppedPixels[dstIdx + 3] = sourcePixels[srcIdx + 3]; // A
                }
            }
            targetBitmap?.Dispose();
            targetBitmap = _croppedColorBitmap.Copy();
        }

        public class PeakInfo
        {
            public int Position { get; set; } 
            public int Start { get; set; }    
            public int End { get; set; }    
            public double Area { get; set; }    
            public bool IsT { get; set; }      
            public bool IsC { get; set; }       

            public double Average { get; set; } 
        }



        public void IdentifyTCPeaks(List<PeakInfo> peaks, int arrayLength)
        {
            if (peaks.Count == 0) return;
            foreach (var peak in peaks)
            {
                peak.IsT = false;
                peak.IsC = false;
            }
            PeakInfo? cPeak = null;
            double maxAreaRight = 0;

            for (int i = 0; i < peaks.Count; i++)
            {
                if (peaks[i].Position > arrayLength * 0.5)
                {
                    if (peaks[i].Area > maxAreaRight)
                    {
                        maxAreaRight = peaks[i].Area;
                        cPeak = peaks[i];
                    }
                }
            }

            if (cPeak != null)
            {
                cPeak.IsC = true;
            }
            PeakInfo? tPeak = null;
            double maxAreaLeft = 0;

            for (int i = 0; i < peaks.Count; i++)
            {
                if (peaks[i].Position < arrayLength * 0.5)
                {
                    if (peaks[i].Area > maxAreaLeft)
                    {
                        maxAreaLeft = peaks[i].Area;
                        tPeak = peaks[i];
                    }
                }
            }

            if (tPeak != null)
            {
                tPeak.IsT = true;
            }

            if (cPeak != null && tPeak != null)
            {
                if (cPeak.Start <= tPeak.End)
                {
                    if (cPeak.Area > tPeak.Area)
                    {
                        tPeak.IsT = false;
                    }
                    else
                    {
                        cPeak.IsC = false;
                    }
                }
            }
        }


        public List<PeakInfo> FindPeaksBasedOnAverage(int[] values)
        {
            var peaks = new List<PeakInfo>();
            if (values == null || values.Length < 3) return peaks;
            var allValues = new List<int>(values);
            allValues.Sort();
            double baseline = allValues[allValues.Count / 2];
            baseline = baseline * 1.1;
            int threshold = (int)(baseline * 1.05);
            bool inPeak = false;
            int peakStart = 0;
            int peakMaxIndex = 0;
            int peakMaxValue = 0;

            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] > threshold)
                {
                    if (!inPeak)
                    {
                        inPeak = true;
                        peakStart = i;
                        peakMaxIndex = i;
                        peakMaxValue = values[i];
                    }
                    else
                    {
                        if (values[i] > peakMaxValue)
                        {
                            peakMaxValue = values[i];
                            peakMaxIndex = i;
                        }
                    }
                }
                else if (inPeak)
                {
                    inPeak = false;
                    if (i - peakStart >= 3)
                    {
                        int start = peakStart;
                        while (start > 0 && values[start] > values[start - 1])
                            start--;

                        int end = i - 1;
                        while (end < values.Length - 1 && values[end] > values[end + 1])
                            end++;

                        double area = 0;
                        for (int j = start; j <= end; j++)
                        {
                            area += Math.Max(0, values[j] - baseline);

                        }

                        peaks.Add(new PeakInfo
                        {
                            Position = peakMaxIndex,
                            Start = start,
                            End = end,
                            Area = area,
                            Average = area / (end - start + 1)
                        });
                    }
                }
            }

            // 处理最后一个波峰
            if (inPeak && values.Length - peakStart >= 3)
            {
                int start = peakStart;
                while (start > 0 && values[start] > values[start - 1])
                    start--;

                int end = values.Length - 1;
                while (end < values.Length - 1 && values[end] > values[end + 1])
                    end++;

                double area = 0;
                for (int j = start; j <= end; j++)
                {
                    // 只计算高于基线的部分
                    area += Math.Max(0, values[j] - baseline);
                }

                peaks.Add(new PeakInfo
                {
                    Position = peakMaxIndex,
                    Start = start,
                    End = end,
                    Area = area
                });
            }

            return peaks;
        }


        //RGB分析专用
        public bool _R_enabled = true;
        public bool _G_enabled = true;
        public bool _B_enabled = true;
        public ChannelDiffMode CurrentAnalysisMode = ChannelDiffMode.MaxDifference;
        // RGB 分析处理函数
        public void ApplyRGBFilter(SKBitmap bitmap)
        {
            using var pixmap = bitmap.PeekPixels();
            var pixels = pixmap.GetPixelSpan<byte>();
            int pixelCount = bitmap.Width * bitmap.Height;
            for (int i = 0; i < pixelCount; i++)
            {
                int idx = i * 4;
                // 假设像素存储顺序是BGR
                if (!_B_enabled) pixels[idx + 2] = 0;   // B 通道置零
                if (!_G_enabled) pixels[idx + 1] = 0;   // G 通道置零
                if (!_R_enabled) pixels[idx + 0] = 0;   // R 通道置零
                                                        // 注意：这里交换了R和B，索引0是B，索引2是R
            }
        }

        public void ApplyChannelDiff(SKBitmap bitmap, ChannelDiffMode mode)
        {
            using var pixmap = bitmap.PeekPixels();
            var pixels = pixmap.GetPixelSpan<byte>();
            int pixelCount = bitmap.Width * bitmap.Height;
            //取中间行数据 10%~90%
            int width = bitmap.Width;
            int height = bitmap.Height;
            int middleY = height / 2;
            _startX = (int)(width * 0.1);
            _endX = (int)(width * 0.9);
            int segmentWidth = _endX - _startX;
            _DataValues = new byte[segmentWidth];
            byte gray = 0;
            byte r = 0, g = 0, b = 0;
            // 提取中间行的数据（灰度值）
            for (int x = _startX; x < _endX; x++)
            {
                int idx = (middleY * width + x) * 4;
#if ANDROID
                r = pixels[idx + 0]; // 注意：我们的像素顺序是RGB，所以R在索引0
                g = pixels[idx + 1]; // G在索引1
                b = pixels[idx + 2]; // B在索引2
#endif
#if WINDOWS
                r = pixels[idx + 2]; // 注意：我们的像素顺序是BGR，所以R在索引2
                g = pixels[idx + 1]; // G在索引1
                b = pixels[idx + 0]; // B在索引0
#endif
                gray = CalculateChannelDiff(r, g, b, CurrentAnalysisMode);
                _DataValues[x - _startX] = gray;

            }
        }


        public void ApplyInvertSmart(SKBitmap bitmap)
        {
            int w = bitmap.Width, h = bitmap.Height;
            if (w == 0 || h == 0) return;

            // 1) 估算边缘平均颜色（作为纸张背景参考）
            long sumR = 0, sumG = 0, sumB = 0, count = 0;
            int border = Math.Max(1, Math.Min(w, h) / 20); // 取 5% 的边框区域
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    if (x < border || x >= w - border || y < border || y >= h - border)
                    {
                        SKColor c = bitmap.GetPixel(x, y);
                        sumR += c.Red;
                        sumG += c.Green;
                        sumB += c.Blue;
                        count++;
                    }
                }
            }
            byte avgR = (byte)(sumR / Math.Max(1, count));
            byte avgG = (byte)(sumG / Math.Max(1, count));
            byte avgB = (byte)(sumB / Math.Max(1, count));

            // 2) 阈值（可以根据样本图片微调）
            double sThreshold = 0.25;   // 饱和度阈值，白色饱和度低
            double vThreshold = 0.85;   // 亮度/Value 阈值
            double distThreshold = 60.0; // RGB 欧式距离阈值（与背景颜色比较）

            // 3) 遍历像素并做判断与反色
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    SKColor c = bitmap.GetPixel(x, y);

                    // RGB -> 0..1
                    double rd = c.Red / 255.0;
                    double gd = c.Green / 255.0;
                    double bd = c.Blue / 255.0;

                    double max = Math.Max(rd, Math.Max(gd, bd));
                    double min = Math.Min(rd, Math.Min(gd, bd));
                    double v = max; // value/brightness
                    double s = (max == 0) ? 0.0 : (max - min) / max; // saturation

                    double dr = c.Red - avgR;
                    double dg = c.Green - avgG;
                    double db = c.Blue - avgB;
                    double dist = Math.Sqrt(dr * dr + dg * dg + db * db);

                    bool isWhiteLike = (v > vThreshold && s < sThreshold)    // HSV 判断白
                                       || (dist < distThreshold && v > 0.70); // 或者接近背景且亮

                    if (isWhiteLike)
                    {
                        var nc = new SKColor((byte)(255 - c.Red),
                                             (byte)(255 - c.Green),
                                             (byte)(255 - c.Blue),
                                             c.Alpha);
                        bitmap.SetPixel(x, y, nc);
                    }
                }
            }
        }

        //灰度融合法
        private static byte CalculateGray(byte r, byte g, byte b)
        {
            return (byte)((r * 0.299f + g * 0.587f + b * 0.114f));
        }
        public enum ChannelDiffMode
        {
            Gray,
            Standard,       // 标准通道差异 (R - (G+B)/2)
            EnhancedRed,    // 增强红色通道 (2*R - G - B)
            GreenBlueDiff,  // 绿蓝差异 (|G - B|)
            TargetColor,    // 针对特定颜色增强
            MaxDifference,  // 最大通道差异
            EnhancedGreenBlue,

        }

        public static byte CalculateChannelDiff(byte r, byte g, byte b, ChannelDiffMode mode = ChannelDiffMode.Standard)
        {
            return mode switch
            {
                // 灰度值：直接计算灰度值
                ChannelDiffMode.Gray => (CalculateGray(r, g, b)),
                // 标准差异：突出红色与背景的差异
                ChannelDiffMode.Standard => (byte)Math.Clamp((r - (g + b) / 2), 0, 255),
                // 增强红色：更强烈的红色对比
                ChannelDiffMode.EnhancedRed => (byte)Math.Clamp(( 2 * r - g - b)/2, 0, 255),
                // 增强绿蓝：突出绿色与蓝色的差异
                ChannelDiffMode.EnhancedGreenBlue => (byte)Math.Clamp(((g + b)/2 - 0.3f * r)/2, 0, 255),
                // 绿蓝差异：突出绿色与蓝色的差异
                ChannelDiffMode.GreenBlueDiff => (byte)Math.Abs((g - b)),
                // 目标颜色增强：针对特定目标颜色增强
                ChannelDiffMode.TargetColor => (CalculateTargetColorDiff(r, g, b)),
                // 最大通道差异：计算最大通道差异
                ChannelDiffMode.MaxDifference => (byte)((Math.Max(Math.Abs(r - g), Math.Max(Math.Abs(g - b), Math.Abs(b - r))))),
            };
        }

        // PCA融合方法
        private static byte CalculatePCADiff(byte r, byte g, byte b)
        {
            // 使用预训练的主成分向量（针对自然图像优化）
            const float pc1 = 0.577f; // 主成分1 - 亮度方向
            const float pc2 = 0.577f; // 主成分1 - 亮度方向
            const float pc3 = 0.577f; // 主成分1 - 亮度方向

            // 计算第一主成分投影值
            float projection = r * pc1 + g * pc2 + b * pc3;

            // 归一化到0-255范围
            // 自然图像中，投影值通常在0-255范围内
            return (byte)Math.Clamp(projection, 0, 255);
        }

        // 完整的PCA融合方法（可训练版本）
        public static byte[] CalculatePCADiffFull(byte[] rValues, byte[] gValues, byte[] bValues)
        {
            int length = rValues.Length;
            byte[] pcaValues = new byte[length];

            // 计算RGB通道的均值
            float meanR = CalculateMean(rValues);
            float meanG = CalculateMean(gValues);
            float meanB = CalculateMean(bValues);

            // 计算协方差矩阵
            float varR = 0, varG = 0, varB = 0;
            float covRG = 0, covRB = 0, covGB = 0;

            for (int i = 0; i < length; i++)
            {
                float dr = rValues[i] - meanR;
                float dg = gValues[i] - meanG;
                float db = bValues[i] - meanB;

                varR += dr * dr;
                varG += dg * dg;
                varB += db * db;
                covRG += dr * dg;
                covRB += dr * db;
                covGB += dg * db;
            }

            // 计算平均协方差
            varR /= length;
            varG /= length;
            varB /= length;
            covRG /= length;
            covRB /= length;
            covGB /= length;

            // 协方差矩阵
            float[,] covMatrix =
            {
                {varR, covRG, covRB},
                {covRG, varG, covGB},
                {covRB, covGB, varB}
            };

            // 使用幂迭代法计算主特征向量
            float[] eigenVector = PowerIteration(covMatrix);

            // 计算投影值
            float minValue = float.MaxValue;
            float maxValue = float.MinValue;
            float[] projections = new float[length];

            for (int i = 0; i < length; i++)
            {
                float projection =
                    eigenVector[0] * (rValues[i] - meanR) +
                    eigenVector[1] * (gValues[i] - meanG) +
                    eigenVector[2] * (bValues[i] - meanB);

                projections[i] = projection;

                if (projection < minValue) minValue = projection;
                if (projection > maxValue) maxValue = projection;
            }

            // 归一化到0-255范围
            float range = maxValue - minValue;
            if (range < 0.001f) range = 1f; // 避免除零

            for (int i = 0; i < length; i++)
            {
                float normalized = (projections[i] - minValue) / range * 255;
                pcaValues[i] = (byte)Math.Clamp(normalized * 0.5, 0, 255);
            }

            return pcaValues;
        }

        // 幂迭代法计算主特征向量
        private static float[] PowerIteration(float[,] matrix, int maxIterations = 10, float tolerance = 1e-6f)
        {
            int n = matrix.GetLength(0);
            float[] v = new float[n];

            // 初始化随机向量
            Random rand = new Random();
            for (int i = 0; i < n; i++)
            {
                v[i] = (float)rand.NextDouble();
            }

            // 归一化
            float norm = VectorNorm(v);
            for (int i = 0; i < n; i++)
            {
                v[i] /= norm;
            }

            for (int iter = 0; iter < maxIterations; iter++)
            {
                // 计算 Av
                float[] av = new float[n];
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        av[i] += matrix[i, j] * v[j];
                    }
                }

                // 计算特征值估计
                float eigenvalue = VectorDot(v, av);

                // 更新向量
                float[] newV = new float[n];
                float newNorm = VectorNorm(av);

                if (newNorm < tolerance) break;

                for (int i = 0; i < n; i++)
                {
                    newV[i] = av[i] / newNorm;
                }

                // 检查收敛
                float diff = 0;
                for (int i = 0; i < n; i++)
                {
                    diff += Math.Abs(newV[i] - v[i]);
                }

                if (diff < tolerance) break;

                v = newV;
            }

            return v;
        }

        // 计算向量范数
        private static float VectorNorm(float[] v)
        {
            float sum = 0;
            foreach (float value in v)
            {
                sum += value * value;
            }
            return (float)Math.Sqrt(sum);
        }

        // 计算向量点积
        private static float VectorDot(float[] a, float[] b)
        {
            float sum = 0;
            for (int i = 0; i < a.Length; i++)
            {
                sum += a[i] * b[i];
            }
            return sum;
        }

        // 计算均值
        private static float CalculateMean(byte[] values)
        {
            float sum = 0;
            foreach (byte value in values)
            {
                sum += value;
            }
            return sum / values.Length;
        }

        // 针对特定目标颜色的差异计算（可自定义目标色）
        private static byte CalculateTargetColorDiff(byte r, byte g, byte b)
        {
            const byte targetR = 220;
            const byte targetG = 50;
            const byte targetB = 50;

            // 计算颜色相似度差异
            float diffR = Math.Abs(r - targetR) / 255f;
            float diffG = Math.Abs(g - targetG) / 255f;
            float diffB = Math.Abs(b - targetB) / 255f;

            // 综合差异（值越小表示越接近目标色）
            float similarity = 1.0f - (diffR + diffG + diffB) / 3.0f;

            // 转换为差异值（越接近目标色值越大）
            return (byte)(255 * similarity);
        }
    }
}
