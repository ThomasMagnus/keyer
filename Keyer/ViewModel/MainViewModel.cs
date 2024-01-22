using System.Windows;
using System.Windows.Input;
using Keyer.Commands;
using Microsoft.Win32;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.IO;
using System.Diagnostics;

namespace Keyer.Model;

internal class MainViewModel : ViewModelBase
{
    public ICommand OpenFile { get; private set; }
    public ICommand ConvertFile { get; private set; }

    private readonly MainModel _mainModel;

    public MainViewModel(MainModel mainModel)
    {
        _mainModel = mainModel;
        OpenFile = new Command(OpenFileCommand, CanExecuteCommand);
        ConvertFile = new Command(ConvertImage, CanExecuteConvertCommand);
    }
    
    public string? FilePath
    {
        get => _mainModel.FilePath;
        set
        {
            _mainModel.FilePath = value;
            OnPropertyChanged(nameof(FilePath));
        }
    }

    public string? ConvertFilePath
    {
        get => _mainModel.ConvertFilePath;
        set
        {
            _mainModel.ConvertFilePath = value;
            OnPropertyChanged(nameof(ConvertFilePath));
        }
    }

    public string? KeyColor
    {
        get => _mainModel.KeyColor;
        set
        {
            _mainModel.KeyColor = value;
            OnPropertyChanged(nameof(KeyColor));
        }
    }

    private void OpenFileCommand(object? parameter)
    {
        OpenFileDialog openFileDialog = new OpenFileDialog();

        if (openFileDialog.ShowDialog() == true)
        {
            string fileName = openFileDialog.FileName;

            FilePath = fileName;
        }
        
    }

    private void ConvertImage(object? parameter)
    {
        Color targetColor = Color.FromRgb(
            Convert.ToByte(KeyColor.Substring(1, 2), 16),
            Convert.ToByte(KeyColor.Substring(3, 2), 16),
            Convert.ToByte(KeyColor.Substring(5, 2), 16));
        

        BitmapSource bitmapSource = new BitmapImage(new Uri(FilePath, UriKind.RelativeOrAbsolute));
        int width = bitmapSource.PixelWidth;
        int height = bitmapSource.PixelHeight;
        int stride = width * (bitmapSource.Format.BitsPerPixel / 8);

        byte[] pixels = new byte[height * stride];

        bitmapSource.CopyPixels(pixels, stride, 0);

        for (int i = 0; i < pixels.Length; i += 4)
        {
            Color pixelColor = Color.FromArgb(pixels[i + 3], pixels[i + 2], pixels[i + 1], pixels[i]);

            if (pixelColor == targetColor)
            {
                pixels[i] = 255;
                pixels[i + 1] = 255;
                pixels[i + 2] = 255;
                pixels[i + 3] = 0;
            }
        }

        WriteableBitmap transparentBitmap = new WriteableBitmap(width, height, bitmapSource.DpiX, bitmapSource.DpiY, bitmapSource.Format, null);
        transparentBitmap.WritePixels(new Int32Rect(0, 0, width, height), pixels, stride, 0);

        SaveImageToFile(transparentBitmap, Path.Combine(Directory.GetCurrentDirectory(), "output.png"));

        ConvertFilePath = Path.Combine(Directory.GetCurrentDirectory(), "output.png");
    }

    private void SaveImageToFile(BitmapSource image, string filePath)
    {
        PngBitmapEncoder pngBitmapDecoder = new PngBitmapEncoder();
        pngBitmapDecoder.Frames.Add(BitmapFrame.Create(image));

        using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
        {
            pngBitmapDecoder.Save(fileStream);
        }
    }

    private bool CanExecuteCommand(object? parameter)
    {
        return true;
    }

    private bool CanExecuteConvertCommand(object? parameter)
    {
        if (string.IsNullOrEmpty(FilePath) || string.IsNullOrEmpty(KeyColor)) 
            return false;
        return true;
    }
}