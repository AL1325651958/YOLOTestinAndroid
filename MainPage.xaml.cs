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
    public partial class MainPage : ContentPage
    {
        private YoloOnnxUse _yoloModel;
        private SKBitmap _originalBitmap;
        private SKBitmap _preprocessedBitmap;
        private SKBitmap _annotatedBitmap;

        public MainPage()
        {
            InitializeComponent();
            _ = LoadYoloModelAsync(); // 异步初始化模型
        }

        private async Task LoadYoloModelAsync()
        {
            string[] classNames = { "blue" };

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

            if (result != null)
            {
                resultLabel.Text = "正在加载图片...";

                // 异步加载并解码大图
                using var stream = await result.OpenReadAsync();
                _originalBitmap = await Task.Run(() => SKBitmap.Decode(stream));

                // 异步预处理（仅供模型输入）
                _preprocessedBitmap = await Task.Run(() => _yoloModel.PreprocessImage(_originalBitmap));

                // 显示原图（保持清晰，不显示缩放后的480x480）
                selectedImage.Source = GetImageSourceFromSKBitmap(_originalBitmap);
                resultLabel.Text = "图片加载完成";
            }
        }

        private async void OnUseClicked(object sender, EventArgs e)
        {
            if (_originalBitmap == null || _preprocessedBitmap == null)
            {
                await DisplayAlert("提示", "请先选择图片", "确定");
                return;
            }

            resultLabel.Text = "正在检测，请稍候...";

            // 异步执行推理和绘制
            var predictions = await Task.Run(() =>
            {
                var preds = _yoloModel.Predict(_preprocessedBitmap);
                _annotatedBitmap = _yoloModel.DrawPredictions(_originalBitmap.Copy());
                return preds;
            });

            // 更新UI
            selectedImage.Source = GetImageSourceFromSKBitmap(_annotatedBitmap);
            resultLabel.Text = $"检测到 {predictions.Count} 个对象";
        }

        /// <summary>
        /// 将 SKBitmap 转换为 ImageSource（使用字节数组避免流被释放）
        /// </summary>
        private ImageSource GetImageSourceFromSKBitmap(SKBitmap bitmap)
        {
            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            var bytes = data.ToArray(); // 转成字节数组
            return ImageSource.FromStream(() => new MemoryStream(bytes));
        }
    }
}
