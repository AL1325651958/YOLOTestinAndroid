using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using SkiaSharp;

namespace PictureYOLO
{
    public class YoloOnnxUse
    {
        private InferenceSession _session;
        private string[] _classNames;
        private int _inputSize;
        private List<YoloPrediction> _predictions = new List<YoloPrediction>();

        // 存储预处理参数
        private float _scaleFactor;
        private float _xOffset;
        private float _yOffset;
        private SKSize _originalSize;

        public YoloOnnxUse(string modelPath, string[] classNames, int inputSize)
        {
            _classNames = classNames;
            _inputSize = inputSize;

            // 加载模型
            _session = new InferenceSession(modelPath);
        }

        public YoloOnnxUse(byte[] modelBytes, string[] classNames, int inputSize)
        {
            _classNames = classNames;
            _inputSize = inputSize;

            // 从字节数组加载模型
            _session = new InferenceSession(modelBytes);
        }

        public void Dispose()
        {
            _session?.Dispose();
        }

        /// <summary>
        /// 预处理图像并返回处理后的位图
        /// </summary>
        public SKBitmap PreprocessImage(SKBitmap originalBitmap)
        {
            // 保存原始尺寸
            _originalSize = new SKSize(originalBitmap.Width, originalBitmap.Height);

            // 创建目标位图
            var targetBitmap = new SKBitmap(_inputSize, _inputSize);
            _scaleFactor = Math.Min((float)_inputSize / originalBitmap.Width,
                                   (float)_inputSize / originalBitmap.Height);

            float scaledWidth = originalBitmap.Width * _scaleFactor;
            float scaledHeight = originalBitmap.Height * _scaleFactor;

            // 计算偏移量
            _xOffset = (_inputSize - scaledWidth) / 2;
            _yOffset = (_inputSize - scaledHeight) / 2;

            using (var canvas = new SKCanvas(targetBitmap))
            {
                canvas.Clear(SKColors.Black);
                var destRect = new SKRect(_xOffset, _yOffset,
                                         _xOffset + scaledWidth, _yOffset + scaledHeight);
                canvas.DrawBitmap(originalBitmap, destRect);
            }

            return targetBitmap;
        }

        /// <summary>
        /// 使用模型进行预测
        /// </summary>
        public List<YoloPrediction> Predict(SKBitmap preprocessedBitmap)
        {
            // 准备模型输入
            var inputTensor = PrepareInputTensor(preprocessedBitmap);

            // 创建输入容器
            var inputs = new List<NamedOnnxValue>
            {
                NamedOnnxValue.CreateFromTensor("images", inputTensor)
            };

            // 运行推理
            using var results = _session.Run(inputs);

            // 处理输出结果
            var output = results.FirstOrDefault()?.AsTensor<float>();
            if (output == null)
            {
                throw new Exception("模型未返回结果");
            }

            // 解析预测结果
            _predictions = ParsePredictions(output);
            return _predictions;
        }

        /// <summary>
        /// 在图像上绘制预测结果
        /// </summary>
        public SKBitmap DrawPredictions(SKBitmap originalBitmap)
        {
            if (_predictions == null || _predictions.Count == 0)
                return originalBitmap.Copy();

            using var canvas = new SKCanvas(originalBitmap);
            var paint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                StrokeWidth = 4,
                Color = SKColors.Red,
                IsAntialias = true
            };

            var textPaint = new SKPaint
            {
                Color = SKColors.White,
                TextSize = 24,
                IsAntialias = true,
                Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyleWeight.Bold,
                                                    SKFontStyleWidth.Normal, SKFontStyleSlant.Upright)
            };

            foreach (var pred in _predictions)
            {
                // 将坐标转换回原始图像空间
                var (x, y, width, height) = ConvertToOriginalCoordinates(pred);

                // 确保坐标在图像范围内
                float boxX = Math.Max(0, Math.Min(originalBitmap.Width - 1, x));
                float boxY = Math.Max(0, Math.Min(originalBitmap.Height - 1, y));
                float boxWidth = Math.Max(1, Math.Min(originalBitmap.Width - boxX, width));
                float boxHeight = Math.Max(1, Math.Min(originalBitmap.Height - boxY, height));

                // 绘制边界框
                canvas.DrawRect(new SKRect(boxX, boxY, boxX + boxWidth, boxY + boxHeight), paint);

                // 获取标签文本
                string label = pred.ClassId < _classNames.Length ? _classNames[pred.ClassId] : $"Class {pred.ClassId}";
                string text = $"{label} {pred.Confidence:P0}";

                // 计算文本尺寸
                SKRect textBounds = new SKRect();
                textPaint.MeasureText(text, ref textBounds);

                // 确保文本位置在图像范围内
                float textX = Math.Max(0, Math.Min(originalBitmap.Width - textBounds.Width, boxX));
                float textY = Math.Max(textBounds.Height, Math.Min(originalBitmap.Height, boxY - 5));

                // 绘制文本背景
                canvas.DrawRect(new SKRect(textX, textY - textBounds.Height,
                                          textX + textBounds.Width, textY),
                    new SKPaint { Color = SKColors.Black.WithAlpha(200) });

                // 绘制文本
                canvas.DrawText(text, textX, textY, textPaint);
            }

            return originalBitmap;
        }

        /// <summary>
        /// 获取预测结果
        /// </summary>
        public List<YoloPrediction> GetPredictions()
        {
            return _predictions;
        }

        /// <summary>
        /// 获取预处理参数
        /// </summary>
        public (float scaleFactor, float xOffset, float yOffset, SKSize originalSize) GetPreprocessParams()
        {
            return (_scaleFactor, _xOffset, _yOffset, _originalSize);
        }

        private DenseTensor<float> PrepareInputTensor(SKBitmap bitmap)
        {
            // 创建3x输入尺寸x输入尺寸的张量
            var tensor = new DenseTensor<float>(new[] { 1, 3, _inputSize, _inputSize });

            // 将图片数据转换为模型输入格式
            for (int y = 0; y < _inputSize; y++)
            {
                for (int x = 0; x < _inputSize; x++)
                {
                    var pixel = bitmap.GetPixel(x, y);

                    // 归一化到0-1范围并转换为BGR顺序
                    tensor[0, 0, y, x] = pixel.Blue / 255.0f;  // B
                    tensor[0, 1, y, x] = pixel.Green / 255.0f; // G
                    tensor[0, 2, y, x] = pixel.Red / 255.0f;   // R
                }
            }

            return tensor;
        }

        private List<YoloPrediction> ParsePredictions(Tensor<float> output)
        {
            var predictions = new List<YoloPrediction>();

            int numClasses = _classNames.Length;

            // 确定张量维度顺序
            bool isChannelsFirst = output.Dimensions[1] == (numClasses + 4);
            int numPredictions = isChannelsFirst ? output.Dimensions[2] : output.Dimensions[1];

            for (int i = 0; i < numPredictions; i++)
            {
                // 获取边界框坐标 (center_x, center_y, width, height)
                float cx, cy, w, h;

                if (isChannelsFirst)
                {
                    cx = output[0, 0, i];
                    cy = output[0, 1, i];
                    w = output[0, 2, i];
                    h = output[0, 3, i];
                }
                else
                {
                    cx = output[0, i, 0];
                    cy = output[0, i, 1];
                    w = output[0, i, 2];
                    h = output[0, i, 3];
                }

                // 获取置信度最高的类别
                float maxConfidence = 0;
                int classId = -1;

                for (int c = 0; c < numClasses; c++)
                {
                    float confidence;

                    if (isChannelsFirst)
                    {
                        confidence = output[0, 4 + c, i];
                    }
                    else
                    {
                        confidence = output[0, i, 4 + c];
                    }

                    if (confidence > maxConfidence)
                    {
                        maxConfidence = confidence;
                        classId = c;
                    }
                }

                // 过滤低置信度预测
                if (maxConfidence < 0.5f) continue;

                predictions.Add(new YoloPrediction
                {
                    X = cx,
                    Y = cy,
                    Width = w,
                    Height = h,
                    Confidence = maxConfidence,
                    ClassId = classId
                });
            }

            return ApplyNMS(predictions);
        }

        private List<YoloPrediction> ApplyNMS(List<YoloPrediction> predictions)
        {
            predictions = predictions.OrderByDescending(p => p.Confidence).ToList();
            var filteredPredictions = new List<YoloPrediction>();
            const float iouThreshold = 0.5f;

            while (predictions.Count > 0)
            {
                var current = predictions[0];
                filteredPredictions.Add(current);
                predictions.RemoveAt(0);

                for (int i = predictions.Count - 1; i >= 0; i--)
                {
                    if (CalculateIoU(current, predictions[i]) > iouThreshold)
                    {
                        predictions.RemoveAt(i);
                    }
                }
            }

            return filteredPredictions;
        }

        private float CalculateIoU(YoloPrediction a, YoloPrediction b)
        {
            float aX1 = a.X - a.Width / 2;
            float aY1 = a.Y - a.Height / 2;
            float aX2 = a.X + a.Width / 2;
            float aY2 = a.Y + a.Height / 2;

            float bX1 = b.X - b.Width / 2;
            float bY1 = b.Y - b.Height / 2;
            float bX2 = b.X + b.Width / 2;
            float bY2 = b.Y + b.Height / 2;

            float interX1 = Math.Max(aX1, bX1);
            float interY1 = Math.Max(aY1, bY1);
            float interX2 = Math.Min(aX2, bX2);
            float interY2 = Math.Min(aY2, bY2);

            float interWidth = Math.Max(0, interX2 - interX1);
            float interHeight = Math.Max(0, interY2 - interY1);
            float interArea = interWidth * interHeight;

            float aArea = a.Width * a.Height;
            float bArea = b.Width * b.Height;
            float unionArea = aArea + bArea - interArea;

            return interArea / unionArea;
        }

        public (float x, float y, float width, float height) ConvertToOriginalCoordinates(YoloPrediction pred)
        {
            // 首先将中心点坐标转换为左上角坐标
            float boxX1 = pred.X - pred.Width / 2;
            float boxY1 = pred.Y - pred.Height / 2;

            // 将坐标从预处理图像空间转换回原始图像空间
            float originalX1 = (boxX1 - _xOffset) / _scaleFactor;
            float originalY1 = (boxY1 - _yOffset) / _scaleFactor;
            float originalWidth = pred.Width / _scaleFactor;
            float originalHeight = pred.Height / _scaleFactor;

            // 确保坐标在图像范围内
            originalX1 = Math.Max(0, Math.Min(_originalSize.Width - 1, originalX1));
            originalY1 = Math.Max(0, Math.Min(_originalSize.Height - 1, originalY1));
            originalWidth = Math.Max(1, Math.Min(_originalSize.Width - originalX1, originalWidth));
            originalHeight = Math.Max(1, Math.Min(_originalSize.Height - originalY1, originalHeight));

            return (originalX1, originalY1, originalWidth, originalHeight);
        }
    }

    public class YoloPrediction
    {
        public float X { get; set; }        // 中心点X坐标
        public float Y { get; set; }        // 中心点Y坐标
        public float Width { get; set; }    // 宽度
        public float Height { get; set; }   // 高度
        public float Confidence { get; set; } // 置信度
        public int ClassIndex { get; set; } // 类别索引
        public int ClassId { get; set; }    // 类别ID
    }
}