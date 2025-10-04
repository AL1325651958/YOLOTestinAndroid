using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using SkiaSharp;
using SkiaSharp.Views.Maui;

namespace PictureYOLO
{

    public class CroppedTarget : IDisposable
    {
        public SKBitmap Bitmap { get; set; }
        public int ClassIndex { get; set; }
        public ImageProcess Processor { get; set; }

        public SKRectI OriginalBoundingBox { get; set; } // 边界框

        public void Dispose()
        {
            Bitmap?.Dispose();
            Processor?._originalBitmap?.Dispose();
            Processor?._processedBitmap?.Dispose();
            Processor?._croppedColorBitmap?.Dispose();
            Processor = null;
        }
    }

    public partial class MainPage : ContentPage
    {
    
    private YoloOnnxUse _yoloModel;
        private SKBitmap _originalBitmap;
        private SKBitmap _preprocessedBitmap;
        private SKBitmap _annotatedBitmap;
        private List<CroppedTarget> _croppedTargets;
        private int _currentIndex = -1; 



        public MainPage()
        {
            InitializeComponent();
            _ = LoadYoloModelAsync();
            waveformCanvas.PaintSurface += OnWaveformPaintSurface;
            _croppedTargets = new List<CroppedTarget>();
        }

        private async Task LoadYoloModelAsync()
        {
            string[] classNames = { "blue", "fluorescence" };
            using var stream = await FileSystem.OpenAppPackageFileAsync("best.onnx");
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            _yoloModel = new YoloOnnxUse(memoryStream.ToArray(), classNames, 480);
        }

        private async void OnPickImageClicked(object sender, EventArgs e)
        {
            var result = await FilePicker.PickAsync(new PickOptions
            {
                PickerTitle = "选择图片",
                FileTypes = FilePickerFileType.Images
            });

            if (result == null)
                return;

            if (result != null)
            {
                // 异步加载并解码大图
                using var stream = await result.OpenReadAsync();
                _originalBitmap = await Task.Run(() => SKBitmap.Decode(stream));

                // 异步预处理（仅供模型输入）
                _preprocessedBitmap = await Task.Run(() => _yoloModel.PreprocessImage(_originalBitmap));

                // 显示原图（保持清晰，不显示缩放后的480x480）
                selectedImage.Source = GetImageSourceFromSKBitmap(_originalBitmap);
            }

            using (var stream = await result.OpenReadAsync())
            {
                _originalBitmap = SKBitmap.Decode(stream);
            }

            if (_originalBitmap != null)
            {
                // 假设预处理逻辑在 YoloOnnxUse 中
                _preprocessedBitmap = _yoloModel.PreprocessImage(_originalBitmap.Copy());

                // 默认显示原图
                selectedImage.Source = GetImageSourceFromSKBitmap(_originalBitmap);
                statusLabel.Text = $"图片加载成功: {_originalBitmap.Width}x{_originalBitmap.Height}";

                // 清空旧的裁剪结果

                UpdateNavigationButtonState();
                ClearCroppedTargets();
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    // 强制调用 DrawWaveform 并清空画布
                    waveformCanvas.InvalidateSurface();
                });
            }

            if (_originalBitmap == null || _preprocessedBitmap == null)
            {
                await DisplayAlert("提示", "请先选择图片", "确定");
                return;
            }

            // 异步执行推理和绘制
            var predictions = await Task.Run(() =>
            {
                var preds = _yoloModel.Predict(_preprocessedBitmap);
                // DrawPredictions 应该接受一个 SKBitmap 的副本，并在其上绘制
                _annotatedBitmap = _yoloModel.DrawPredictions(_originalBitmap.Copy());
                return preds;
            });

            // 清空旧的裁剪结果
            ClearCroppedTargets();


            var imageCropper = new ImageProcess();
            foreach (var pred in predictions)
            {
                // 获取原始图像空间中的边界框
                var (x, y, width, height) = _yoloModel.ConvertToOriginalCoordinates(pred);

                // 将浮点数边界框转换为整数 SKRectI
                int left = (int)Math.Max(0, x);
                int top = (int)Math.Max(0, y);
                int right = (int)Math.Min(_originalBitmap.Width, x + width);
                int bottom = (int)Math.Min(_originalBitmap.Height, y + height);

                var boundingBox = new SKRectI(left, top, right, bottom);
                SKBitmap? croppedBitmap = imageCropper.CropImage(_originalBitmap, boundingBox);

                if (croppedBitmap != null)
                {
                    var targetProcessor = new ImageProcess(croppedBitmap);

                    if(targetProcessor._processedBitmap!=null)
                    {
                        if (targetProcessor._processedBitmap.Height > targetProcessor._processedBitmap.Width)
                        {
                            targetProcessor.Rotate90(ref targetProcessor._processedBitmap);
                        }
                        switch(pred.ClassId)
                        {
                            case 0:targetProcessor.CurrentAnalysisMode = ImageProcess.ChannelDiffMode.MaxDifference; break;

                            case 1:targetProcessor.CurrentAnalysisMode = ImageProcess.ChannelDiffMode.Gray;break;
                        }

                        targetProcessor._processedBitmap = targetProcessor.StretchImageWidth(targetProcessor._processedBitmap, 350).Copy();
                        targetProcessor.ApplyChannelDiff(targetProcessor._processedBitmap, targetProcessor.CurrentAnalysisMode);
                        _croppedTargets.Add(new CroppedTarget
                        {
                            Bitmap = targetProcessor._processedBitmap,
                            ClassIndex = pred.ClassId,
                            Processor = targetProcessor,
                            OriginalBoundingBox = boundingBox // <-- 在这里存储边界框
                        });
                    }
                }
            }


            imageCropper._originalBitmap?.Dispose();
            imageCropper._processedBitmap?.Dispose();
            if (_croppedTargets.Count > 0)
            {
                _currentIndex = 0;
                await UpdateImageDisplayAsync();
            }
            else
            {
                _currentIndex = -1;
                // 显示带框原图
                selectedImage.Source = GetImageSourceFromSKBitmap(_annotatedBitmap);
                statusLabel.Text = "未检测到目标。";
            }

            DisplayCurrentCroppedImage(); // 调用方法显示当前图片和标签
            UpdateNavigationButtonState(); // 更新按钮状态

            
        }
        private ImageSource GetImageSourceFromSKBitmap(SKBitmap bitmap)
        {
            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            var bytes = data.ToArray(); 
            return ImageSource.FromStream(() => new MemoryStream(bytes));
        }

        private async void PreButton_Clicked(object sender, EventArgs e)
        {
            if (_croppedTargets == null || _croppedTargets.Count == 0) return;

            // 如果当前显示的是第一个裁剪目标，则回到检测结果图
            if (_currentIndex == 0)
            {
                _currentIndex = -1;
            }
            // 如果当前显示的是其他裁剪目标，则显示前一个裁剪目标
            else if (_currentIndex > 0)
            {
                _currentIndex--;
            }

            DisplayCurrentCroppedImage();
            UpdateNavigationButtonState();

            if (_currentIndex >= 0)
            {
                await UpdateImageDisplayAsync();
            }
            else
            {
                // 当显示检测结果图时，不需要更新波形显示
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    waveformCanvas.InvalidateSurface();
                });
            }
        }

        private async void NextButton_Clicked(object sender, EventArgs e)
        {
            if (_croppedTargets == null || _croppedTargets.Count == 0) return;

            // 如果当前显示的是检测结果图，则显示第一个裁剪目标
            if (_currentIndex == -1)
            {
                _currentIndex = 0;
            }
            // 如果当前显示的是裁剪目标，则显示下一个裁剪目标
            else if (_currentIndex < _croppedTargets.Count - 1)
            {
                _currentIndex++;
            }

            DisplayCurrentCroppedImage();
            UpdateNavigationButtonState();

            if (_currentIndex >= 0)
            {
                await UpdateImageDisplayAsync();
            }
        }

        private void ClearCroppedTargets()
        {
            if (_croppedTargets != null)
            {
                foreach (var target in _croppedTargets)
                {
                    target.Dispose();
                }
            }
            _croppedTargets = new List<CroppedTarget>();
        }

        /// <summary>
        /// 显示当前索引对应的裁剪图片，并更新状态标签
        /// </summary>
        private void DisplayCurrentCroppedImage()
        {
            if (_currentIndex == -1 && _annotatedBitmap != null)
            {
                // 显示原始图片的检测结果图
                selectedImage.Source = GetImageSourceFromSKBitmap(_annotatedBitmap);
                statusLabel.Text = $"检测结果图 | 共 {_croppedTargets.Count} 个目标";
            }
            else if (_croppedTargets != null && _currentIndex >= 0 && _currentIndex < _croppedTargets.Count)
            {
                var currentTarget = _croppedTargets[_currentIndex];
                selectedImage.Source = GetImageSourceFromSKBitmap(currentTarget.Bitmap);
                string objectName;
                switch(currentTarget.ClassIndex)
                {
                    case 0: objectName = "多色微球试纸条";
                        _croppedTargets[_currentIndex].Processor.CurrentAnalysisMode = ImageProcess.ChannelDiffMode.MaxDifference;
                        break;
                    case 1: objectName = "荧光试纸条";
                        _croppedTargets[_currentIndex].Processor.CurrentAnalysisMode = ImageProcess.ChannelDiffMode.Gray;
                        break;
                    default: objectName = "索引类别未知";
                        break;
                }
                statusLabel.Text = $"{objectName} | 当前: {_currentIndex + 1} / {_croppedTargets.Count}";
            }
            else if (_originalBitmap != null)
            {
                selectedImage.Source = GetImageSourceFromSKBitmap(_originalBitmap);
                statusLabel.Text = "图片已加载，等待检测。";
            }
        }

        /// <summary>
        /// 更新 '上一张' 和 '下一张' 按钮的启用状态
        /// </summary>
        private void UpdateNavigationButtonState()
        {
            if (PreButton == null || NextButton == null) return;

            if (_croppedTargets == null || _croppedTargets.Count == 0)
            {
                PreButton.IsEnabled = false;
                NextButton.IsEnabled = false;
            }
            else
            {
                // 当当前索引为-1时（显示检测结果图），"上一张"按钮禁用，"下一张"按钮启用
                if (_currentIndex == -1)
                {
                    PreButton.IsEnabled = false;
                    NextButton.IsEnabled = true;
                    resultFrame.IsVisible = false;
                }
                // 当当前索引为0时（第一个裁剪目标），"上一张"按钮启用（可以回到检测结果图）
                else if (_currentIndex == 0)
                {
                    resultFrame.IsVisible = true;
                    PreButton.IsEnabled = true;
                    NextButton.IsEnabled = _croppedTargets.Count > 1;
                }
                // 当当前索引为最后一个裁剪目标时，"下一张"按钮禁用
                else if (_currentIndex == _croppedTargets.Count - 1)
                {
                    PreButton.IsEnabled = true;
                    NextButton.IsEnabled = false;
                }
                // 其他情况，两个按钮都启用
                else
                {
                    PreButton.IsEnabled = true;
                    NextButton.IsEnabled = true;
                }
            }
        }


        private async Task UpdateImageDisplayAsync()
        {
            if(_currentIndex < 0 || _currentIndex >= _croppedTargets.Count)
                return;
            selectedImage.Source = GetImageSourceFromSKBitmap(_croppedTargets[_currentIndex].Bitmap);
            if (_croppedTargets[_currentIndex].Processor._processedBitmap != null)
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    waveformCanvas.InvalidateSurface();
                });
            }
        }


        private void OnWaveformPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            var surface = e.Surface;
            var canvas = surface.Canvas;
            var info = e.Info;
            DrawWaveform(canvas, info.Width, info.Height);
        }

        private void DrawWaveform(SKCanvas canvas, int width, int height)
        {
            if (_currentIndex < 0 || _currentIndex >= _croppedTargets.Count)
            {
                canvas.Clear();
                return;
            }
            var currentTarget = _croppedTargets[_currentIndex];

            var processor = currentTarget?.Processor;
            var dataValues = processor?._DataValues;

            if (currentTarget == null || processor == null || dataValues == null)
                return;

            canvas.Clear(SKColors.White);
            int segmentWidth = dataValues.Length;
            float scaleX = (float)width / segmentWidth;

            // 1. 计算数据的实际范围（最小值和最大值）
            byte minValue = 255;
            byte maxValue = 0;
            foreach (byte value in dataValues)
            {
                if (value < minValue) minValue = value;
                if (value > maxValue) maxValue = value;
            }

            // 2. 动态调整Y轴比例，使用实际数据范围
            float dataRange = maxValue - minValue;
            if (dataRange < 10) dataRange = 10; // 防止除零错误

            // 为T/C文本预留顶部空间
            const int TopMargin = 60; // 顶部留出60像素空间给T/C文本
            float availableHeight = height - TopMargin;
            float scaleY = availableHeight / dataRange;

            // 3. 计算平均值和基线
            double average = 0;
            foreach (byte value in dataValues)
            {
                average += value;
            }
            average /= dataValues.Length;
            float averageY = TopMargin + (float)(availableHeight - (average - minValue) * scaleY);

            var allValues = new List<byte>(dataValues);
            allValues.Sort();
            double baseline = allValues[allValues.Count / 2];
            baseline = baseline * 1.1;
            baseline = Math.Min(baseline, 255);
            float baselineY = TopMargin + (float)(availableHeight - (baseline - minValue) * scaleY);

            // 4. 应用滤波
            int windowSize = 3;
            int halfWindow = windowSize / 2;
            byte[] filteredValues = new byte[segmentWidth];
            for (int i = 0; i < segmentWidth; i++)
            {
                int sum = 0;
                int count = 0;
                for (int j = -halfWindow; j <= halfWindow; j++)
                {
                    if (i + j < 0 || i + j >= segmentWidth)
                        continue;
                    int index = i + j;
                    if (index >= 0 && index < segmentWidth)
                    {
                        sum += dataValues[index];
                        count++;
                    }
                }

                filteredValues[i] = (byte)(sum / count);
            }

            // 5. 调整基线区域
            int centerIndex = segmentWidth / 2;
            int startIndex = Math.Max(0, centerIndex - 2);
            int endIndex = Math.Min(segmentWidth - 1, centerIndex + 2);
            byte baselineValue = (byte)Math.Max(0, Math.Min(255, baseline - 2));

            //for (int i = startIndex; i <= endIndex; i++)
            //{
            //    filteredValues[i] = baselineValue;
            //}
            int oneFifth = (int)(segmentWidth * 0.12f);
            for (int i = 0; i < oneFifth; i++)
            {
                filteredValues[i] = baselineValue;
            }
            for (int i = segmentWidth - oneFifth; i < segmentWidth; i++)
            {
                filteredValues[i] = baselineValue;
            }

            // 6. 绘制波形路径（使用动态Y轴比例）
            using var path = new SKPath();
            path.MoveTo(0, TopMargin + availableHeight - (filteredValues[0] - minValue) * scaleY);
            for (int i = 1; i < segmentWidth; i++)
            {
                float y = TopMargin + availableHeight - (filteredValues[i] - minValue) * scaleY;
                path.LineTo(i * scaleX, y);
            }

            using var paint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SKColors.Blue,
                StrokeWidth = 2,
                IsAntialias = true
            };
            canvas.DrawPath(path, paint);

            // 7. 绘制平均线和基线（使用动态Y轴比例）
            using var avgPaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SKColors.Gray,
                StrokeWidth = 1,
                IsAntialias = true,
                PathEffect = SKPathEffect.CreateDash(new float[] { 3, 3 }, 0)
            };
            canvas.DrawLine(0, averageY, width, averageY, avgPaint);

            using var baselinePaint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SKColors.Green,
                StrokeWidth = 1,
                IsAntialias = true,
                PathEffect = SKPathEffect.CreateDash(new float[] { 5, 5 }, 0)
            };
            canvas.DrawLine(0, baselineY, width, baselineY, baselinePaint);

            using var baselineTextPaint = new SKPaint
            {
                Color = SKColors.Green,
                TextSize = 20,
                IsAntialias = true
            };
            canvas.DrawText($"Base: {baseline:F1}", 10, baselineY - 5, baselineTextPaint);

            // 8. 检测峰值并绘制
            int[] grayIntValues = new int[filteredValues.Length];
            for (int i = 0; i < filteredValues.Length; i++)
            {
                grayIntValues[i] = filteredValues[i];
            }
            var peaks = processor.FindPeaksBasedOnAverage(grayIntValues);
            processor.IdentifyTCPeaks(peaks, grayIntValues.Length);
            var tPeak = peaks.FirstOrDefault(p => p.IsT);
            var cPeak = peaks.FirstOrDefault(p => p.IsC);
            double ratio = 0;
            if (tPeak != null && cPeak != null && cPeak.Area > 0)
            {
                ratio = tPeak.Area / cPeak.Area;
            }
            else if (tPeak == null && cPeak != null && cPeak.Area > 0)
            {
                ratio = 0;
            }

            if (peaks.Count > 0)
            {
                using var highlightPaint = new SKPaint
                {
                    Color = SKColors.Red,
                    StrokeWidth = 3,
                    IsAntialias = true
                };

                using var fillPaint = new SKPaint
                {
                    Style = SKPaintStyle.Fill,
                    Color = new SKColor(0, 200, 0, 100),
                    IsAntialias = true
                };
                using var textPaint = new SKPaint
                {
                    Color = SKColors.Black,
                    TextSize = 20,
                    IsAntialias = true
                };
                using var ratioPaint = new SKPaint
                {
                    Color = SKColors.Black,
                    TextSize = 40,
                    IsAntialias = true,
                    Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold)
                };
                foreach (var peak in peaks)
                {
                    int safePosition = Math.Clamp(peak.Position, 0, grayIntValues.Length - 1);
                    float x = safePosition * scaleX;
                    float y = TopMargin + availableHeight - (grayIntValues[safePosition] - minValue) * scaleY;
                    canvas.DrawCircle(x, y, 5, highlightPaint);
                    int safeStart = Math.Clamp(peak.Start, 0, grayIntValues.Length - 1);
                    int safeEnd = Math.Clamp(peak.End, 0, grayIntValues.Length - 1);
                    if (safeStart > safeEnd)
                    {
                        (safeStart, safeEnd) = (safeEnd, safeStart);
                    }

                    float startX = safeStart * scaleX;
                    float endX = safeEnd * scaleX;
                    using (var peakPath = new SKPath())
                    {
                        peakPath.MoveTo(startX, baselineY);
                        for (int i = safeStart; i <= safeEnd; i++)
                        {
                            if (i < 0 || i >= grayIntValues.Length) continue;

                            float xi = i * scaleX;
                            float dataValue = Math.Clamp(grayIntValues[i], 0, 255);
                            float yi = TopMargin + availableHeight - (dataValue - minValue) * scaleY;
                            peakPath.LineTo(xi, yi);
                        }
                        peakPath.LineTo(endX, baselineY);
                        peakPath.Close();
                        canvas.DrawPath(peakPath, fillPaint);
                    }
                    string label = peak.IsT ? "T" : peak.IsC ? "C" : "P";
                    canvas.DrawText($"{label}:{peak.Area:F0}", x, y - 10, textPaint);
                }

                if (tPeak == null)
                {
                    ratio = 0;
                }

                // 9. 为T/C比例文本预留空间并绘制
                string ratioText = $"T/C: {ratio:F4}";
                processor.TCrate = ratio;
                float textWidth = ratioPaint.MeasureText(ratioText);
                float centerX = (width - textWidth) / 2;

                // 在顶部预留的空间内绘制T/C比例文本
                canvas.DrawText(ratioText, centerX, 40, ratioPaint);

                // 添加背景框使文本更突出
                using var bgPaint = new SKPaint
                {
                    Color = new SKColor(255, 255, 255, 180), // 半透明白色背景
                    Style = SKPaintStyle.Fill
                };
                const int padding = 10;
                canvas.DrawRect(
                    centerX - padding,
                    40 - ratioPaint.TextSize + padding,
                    textWidth + padding * 2,
                    ratioPaint.TextSize + padding,
                    bgPaint);

                // 重新绘制文本使其在背景上更清晰
                canvas.DrawText(ratioText, centerX, 40, ratioPaint);
            }
        }

        private void waveformCanvas_Touch(object sender, SKTouchEventArgs e)
        {

        }

        private async void SaveButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                // 权限请求 (适用于 .NET MAUI / Xamarin)
                var status = await Permissions.RequestAsync<Permissions.StorageWrite>();
                if (status != PermissionStatus.Granted)
                {
                    await DisplayAlert("权限被拒绝", "需要存储权限才能保存图片", "确定");
                    return;
                }

                // 模式 1: 只保存检测结果图
                if (_currentIndex == -1)
                {
                    if (_annotatedBitmap == null)
                    {
                        await DisplayAlert("提示", "没有可保存的图片，请先进行检测。", "确定");
                        return;
                    }
                    await ShareBitmap(_annotatedBitmap, "CT试纸检测结果");
                    return;
                }

                // 模式 2: 保存综合分析图
                if (_currentIndex < 0 || _currentIndex >= _croppedTargets.Count)
                    return;

                var currentTarget = _croppedTargets[_currentIndex];
                var processor = currentTarget?.Processor;
                var croppedImage = currentTarget?.Processor._processedBitmap;
                var dataValues = processor?._DataValues;

                if (_originalBitmap == null)
                {
                    await DisplayAlert("错误", "原始图片未加载，无法保存。", "确定");
                    return;
                }

                if (currentTarget == null || processor == null || dataValues == null || croppedImage == null)
                    return;

                // ***** 关键修改部分：在原图上绘制当前目标的边界框 *****
                using var annotatedOriginalBitmap = _originalBitmap.Copy();
                using (var canvas = new SKCanvas(annotatedOriginalBitmap))
                {
                    using var rectPaint = new SKPaint
                    {
                        Style = SKPaintStyle.Stroke,
                        Color = SKColors.Yellow,
                        StrokeWidth = 8,
                        IsAntialias = true
                    };
                    canvas.DrawRect(currentTarget.OriginalBoundingBox, rectPaint);
                }
                // *************************************************

                // --- 1. 获取图片和波形图的尺寸并确定最终组合宽度 ---
                int originalWidth = annotatedOriginalBitmap.Width;
                int croppedWidth = croppedImage.Width;
                int waveformWidth = (int)waveformCanvas.CanvasSize.Width;
                int waveformHeight = (int)waveformCanvas.CanvasSize.Height;

                // 原始图片缩放尺寸（例如限制最大 400 像素）
                const int MAX_ORIGINAL_WIDTH = 400;
                int scaledOriginalWidth = Math.Min(originalWidth, MAX_ORIGINAL_WIDTH);
                int scaledOriginalHeight = (int)((float)scaledOriginalWidth / originalWidth * annotatedOriginalBitmap.Height);

                // 最终组合图的宽度：取所有元素中最大的，并确保波形图能完全容纳
                // 裁剪子图的宽度（croppedWidth）和波形图的理想宽度（waveformWidth）是确定统一宽度的关键
                int combinedWidth = Math.Max(scaledOriginalWidth, Math.Max(croppedWidth, waveformWidth));

                // --- 2. 绘制波形图到单独的 Surface（宽度为 combinedWidth） ---
                using (var waveformSurface = SKSurface.Create(new SKImageInfo(combinedWidth, waveformHeight)))
                {
                    var waveformCanvasSurface = waveformSurface.Canvas;
                    // 在整个 combinedWidth 上绘制波形
                    DrawWaveform(waveformCanvasSurface, combinedWidth, waveformHeight);
                    using var waveformImage = waveformSurface.Snapshot();

                    // --- 3. 计算组合图的高度和布局 ---
                    const int Padding = 20;
                    const int TitleHeight = 40;

                    int currentY = Padding;

                    // 1. 原始图部分 (带框)
                    int originalDisplayY = currentY;
                    currentY += scaledOriginalHeight + TitleHeight + Padding;

                    // 2. 裁剪子图部分
                    int croppedDisplayY = currentY;
                    currentY += croppedImage.Height + TitleHeight + Padding;

                    // 3. 波形图部分
                    int waveformDisplayY = currentY;
                    currentY += waveformHeight + TitleHeight + Padding;

                    // 4. 文字信息部分
                    int infoDisplayY = currentY;
                    int infoHeight = 120;
                    currentY += infoHeight + Padding;

                    int combinedHeight = currentY;

                    // --- 4. 创建最终组合画布并绘制 ---
                    using (var combinedSurface = SKSurface.Create(new SKImageInfo(combinedWidth, combinedHeight)))
                    {
                        var combinedCanvas = combinedSurface.Canvas;
                        combinedCanvas.Clear(SKColors.White);

                        using var titlePaint = new SKPaint
                        {
                            Color = SKColors.Black,
                            TextSize = 48,
                            IsAntialias = true,
                            TextAlign = SKTextAlign.Center
                        };

                        // 获取 T/C Ratio 和对象类别
                        string ratioText = $"T/C Proportion: {processor.TCrate:F4}";
                        string objectName = currentTarget.ClassIndex switch
                        {
                            0 => "类别: 多色微球试纸条",
                            1 => "类别: 荧光试纸条",
                            _ => "类别: 未知"
                        };

                        // 绘制 1. 带框的原始图 (居中对齐)
                        combinedCanvas.DrawText("Original Image", combinedWidth / 2, originalDisplayY + TitleHeight - 5, titlePaint);
                        var originalDestRect = new SKRectI(
                            (combinedWidth - scaledOriginalWidth) / 2,
                            originalDisplayY + TitleHeight,
                            (combinedWidth + scaledOriginalWidth) / 2,
                            originalDisplayY + TitleHeight + scaledOriginalHeight);
                        combinedCanvas.DrawBitmap(annotatedOriginalBitmap, originalDestRect);

                        // 绘制 2. 裁剪子图 (居中对齐)
                        combinedCanvas.DrawText("Target Crop Image", combinedWidth / 2, croppedDisplayY + TitleHeight - 5, titlePaint);
                        int croppedX = (combinedWidth - croppedImage.Width) / 2;
                        combinedCanvas.DrawBitmap(croppedImage, croppedX, croppedDisplayY + TitleHeight);

                        // ***** 关键新增部分：绘制连接线 *****
                        using var linePaint = new SKPaint
                        {
                            Style = SKPaintStyle.Stroke,
                            Color = SKColors.Red,
                            StrokeWidth = 3,
                            IsAntialias = true
                        };

                        // 计算缩放比例
                        float scaleX = (float)scaledOriginalWidth / originalWidth;
                        float scaleY = (float)scaledOriginalHeight / annotatedOriginalBitmap.Height;

                        // 原始图边界框在组合画布上的坐标（底部）
                        SKPoint originalBoxBottomLeftInCombined = new SKPoint(
                            originalDestRect.Left + currentTarget.OriginalBoundingBox.Left * scaleX,
                            originalDestRect.Top + currentTarget.OriginalBoundingBox.Bottom * scaleY
                        );
                        SKPoint originalBoxBottomRightInCombined = new SKPoint(
                            originalDestRect.Left + currentTarget.OriginalBoundingBox.Right * scaleX,
                            originalDestRect.Top + currentTarget.OriginalBoundingBox.Bottom * scaleY
                        );

                        // 裁剪子图在组合画布上的坐标（顶部）
                        SKPoint croppedImageTopLeftInCombined = new SKPoint(
                            croppedX,
                            croppedDisplayY + TitleHeight
                        );
                        SKPoint croppedImageTopRightInCombined = new SKPoint(
                            croppedX + croppedImage.Width,
                            croppedDisplayY + TitleHeight
                        );

                        // 绘制连接线
                        combinedCanvas.DrawLine(croppedImageTopLeftInCombined, originalBoxBottomLeftInCombined, linePaint);
                        combinedCanvas.DrawLine(croppedImageTopRightInCombined, originalBoxBottomRightInCombined, linePaint);
                        // *************************************************

                        // 绘制 3. 波形图 (居中对齐)
                        combinedCanvas.DrawText("Analysis Result", combinedWidth / 2, waveformDisplayY + TitleHeight - 5, titlePaint);
                        int waveformX = (combinedWidth - waveformImage.Width) / 2;
                        combinedCanvas.DrawImage(waveformImage, waveformX, waveformDisplayY + TitleHeight);

                        // 绘制 4. 文字信息 (居中对齐)
                        using var infoPaint = new SKPaint
                        {
                            Color = SKColors.DarkBlue,
                            TextSize = 30,
                            IsAntialias = true,
                            TextAlign = SKTextAlign.Center
                        };
                        combinedCanvas.DrawText(objectName, combinedWidth / 2, infoDisplayY + 40, infoPaint);
                        combinedCanvas.DrawText(ratioText, combinedWidth / 2, infoDisplayY + 90, infoPaint);

                        // --- 5. 保存和分享 ---
                        using var combinedImage = combinedSurface.Snapshot();
                        using var data = combinedImage.Encode(SKEncodedImageFormat.Png, 100);
                        string fileName = $"CT_Analysis_Combined_{DateTime.Now:yyyyMMdd_HHmmss}.png";
                        // 使用缓存目录
                        string filePath = Path.Combine(FileSystem.CacheDirectory, fileName);

                        using (var stream = File.OpenWrite(filePath))
                        {
                            data.SaveTo(stream);
                        }

                        await Share.Default.RequestAsync(new ShareFileRequest
                        {
                            Title = "CT试纸综合分析结果",
                            File = new ShareFile(filePath)
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("错误", $"保存综合分析结果失败: {ex.Message}", "确定");
            }
        }

        //private async void SaveButton_Clicked(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        var status = await Permissions.RequestAsync<Permissions.StorageWrite>();
        //        if (status != PermissionStatus.Granted)
        //        {
        //            await DisplayAlert("权限被拒绝", "需要存储权限才能保存图片", "确定");
        //            return;
        //        }

        //        if (_currentIndex == -1)
        //        {
        //            if (_annotatedBitmap == null)
        //            {
        //                await DisplayAlert("提示", "没有可保存的图片，请先进行检测。", "确定");
        //                return;
        //            }

        //            await ShareBitmap(_annotatedBitmap, "CT试纸检测结果");
        //            return;
        //        }

        //        if (_currentIndex < 0 || _currentIndex >= _croppedTargets.Count)
        //            return;
        //        var currentTarget = _croppedTargets[_currentIndex];

        //        var processor = currentTarget?.Processor;
        //        var _image = currentTarget?.Processor._processedBitmap;
        //        var dataValues = processor?._DataValues;

        //        if (currentTarget == null || processor == null || dataValues == null || _image == null)
        //            return;

        //        int waveformWidth = (int)waveformCanvas.CanvasSize.Width;
        //        int waveformHeight = (int)waveformCanvas.CanvasSize.Height;
        //        int combinedWidth = Math.Max(_image.Width, waveformWidth);
        //        using (var waveformSurface = SKSurface.Create(new SKImageInfo(combinedWidth, waveformHeight)))
        //        {
        //            var waveformCanvasSurface = waveformSurface.Canvas;
        //            DrawWaveform(waveformCanvasSurface, combinedWidth, waveformHeight);
        //            using var waveformImage = waveformSurface.Snapshot();
        //            int combinedHeight = _image.Height + waveformHeight + 100;
        //            using (var combinedSurface = SKSurface.Create(new SKImageInfo(combinedWidth, combinedHeight)))
        //            {
        //                var combinedCanvas = combinedSurface.Canvas;
        //                combinedCanvas.Clear(SKColors.White);
        //                using var titlePaint = new SKPaint
        //                {
        //                    Color = SKColors.Black,
        //                    TextSize = 24,
        //                    IsAntialias = true,
        //                    TextAlign = SKTextAlign.Center
        //                };
        //                int croppedX = (combinedWidth - _image.Width) / 2;
        //                combinedCanvas.DrawBitmap(_image, croppedX, 50);
        //                int waveformX = (combinedWidth - combinedWidth) / 2;
        //                int waveformY = _image.Height + 50;
        //                combinedCanvas.DrawImage(waveformImage, waveformX, waveformY);
        //                using var combinedImage = combinedSurface.Snapshot();
        //                using var data = combinedImage.Encode(SKEncodedImageFormat.Png, 100);
        //                string fileName = $"CT_Analysis_{DateTime.Now:yyyyMMdd_HHmmss}.png";
        //                string filePath = Path.Combine(FileSystem.CacheDirectory, fileName);
        //                using (var stream = File.OpenWrite(filePath))
        //                {
        //                    data.SaveTo(stream);
        //                }
        //                await Share.Default.RequestAsync(new ShareFileRequest
        //                {
        //                    Title = "CT试纸分析结果",
        //                    File = new ShareFile(filePath)
        //                });
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        await DisplayAlert("错误", $"保存分析结果失败: {ex.Message}", "确定");
        //    }
        //}

        private async Task ShareBitmap(SKBitmap bitmap, string fileNamePrefix)
        {
            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);

            string fileName = $"{fileNamePrefix}_{DateTime.Now:yyyyMMdd_HHmmss}.png";
            string filePath = Path.Combine(FileSystem.CacheDirectory, fileName);

            using (var stream = File.OpenWrite(filePath))
            {
                data.SaveTo(stream);
            }

            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = "分析结果",
                File = new ShareFile(filePath)
            });
        }
    }
}
