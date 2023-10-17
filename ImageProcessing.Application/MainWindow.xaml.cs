﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;

namespace UAM.PTO
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ImageViewModel imgvm = new ImageViewModel();

        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.scrollViewer.Background = Brushes.Aquamarine;
#endif
            this.DataContext = imgvm;
            BindCommands();
        }

        private void BindCommands()
        {
            BindFileCommands();
            BindEditCommands();
            BindFiltersCommands();
            BindToolsCommands();
        }

        private void BindFileCommands()
        {
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Open, (s, e) => Commands.File.OpenExecuted(imgvm, e)));
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Save, (s, e) => Commands.File.SaveExecuted(imgvm, e), (s, e) => { e.CanExecute = imgvm.IsImageOpen; }));
            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.SaveAs, (s, e) => Commands.File.SaveAsExecuted(imgvm, e), (s, e) => { e.CanExecute = imgvm.IsImageOpen; }));
            this.CommandBindings.Add(new CommandBinding(Commands.File.Exit, Commands.File.ExitExecuted));
        }

        private void BindEditCommands()
        {

            this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Undo, (s, e) => { imgvm.Undo(); }, (s, e) => { e.CanExecute = imgvm.CanUndo; }));
        }

        private void BindFiltersCommands()
        {
            this.CommandBindings.Add(new CommandBinding(Commands.Filters.Convolution, (s, e) => Commands.Filters.ConvolutionExecuted(this, e), (s, e) => { e.CanExecute = imgvm.IsImageOpen; }));
            this.CommandBindings.Add(new CommandBinding(Commands.Filters.Blur.Gaussian, (s, e) => { imgvm.ApplyGaussianBlur(); }, (s, e) => { e.CanExecute = imgvm.IsImageOpen; }));
            this.CommandBindings.Add(new CommandBinding(Commands.Filters.Blur.Uniform, (s, e) => { imgvm.ApplyUniformBlur(); }, (s, e) => { e.CanExecute = imgvm.IsImageOpen; }));
            this.CommandBindings.Add(new CommandBinding(Commands.Filters.Histogram.Equalize, (s, e) => { imgvm.EqualizeHistogram(); }, (s, e) => { e.CanExecute = imgvm.IsImageOpen; }));
            this.CommandBindings.Add(new CommandBinding(Commands.Filters.Histogram.Stretch, (s, e) => { imgvm.StretchHistogram(); }, (s, e) => { e.CanExecute = imgvm.IsImageOpen; }));
            this.CommandBindings.Add(new CommandBinding(Commands.Filters.Edges.Gradient, (s, e) => { imgvm.DetectEdgesGradient(); }, (s, e) => { e.CanExecute = imgvm.IsImageOpen; }));
            this.CommandBindings.Add(new CommandBinding(Commands.Filters.Edges.Laplace, (s, e) => { imgvm.DetectEdgesLaplace(); }, (s, e) => { e.CanExecute = imgvm.IsImageOpen; }));
            this.CommandBindings.Add(new CommandBinding(Commands.Filters.Edges.Sobel, (s, e) => { imgvm.DetectEdgesSobel(); }, (s, e) => { e.CanExecute = imgvm.IsImageOpen; }));
            this.CommandBindings.Add(new CommandBinding(Commands.Filters.Edges.Prewitt, (s, e) => { imgvm.DetectEdgesPrewitt(); }, (s, e) => { e.CanExecute = imgvm.IsImageOpen; }));
            this.CommandBindings.Add(new CommandBinding(Commands.Filters.Edges.Roberts, (s, e) => { imgvm.DetectEdgesRoberts(); }, (s, e) => { e.CanExecute = imgvm.IsImageOpen; }));
            this.CommandBindings.Add(new CommandBinding(Commands.Filters.Edges.LaplacianOfGaussian, (s, e) => { imgvm.DetectEdgesLoG(); }, (s, e) => { e.CanExecute = imgvm.IsImageOpen; }));
            this.CommandBindings.Add(new CommandBinding(Commands.Filters.Edges.DifferenceOfGaussian, (s, e) => { imgvm.DetectEdgesDoG(); }, (s, e) => { e.CanExecute = imgvm.IsImageOpen; }));
            this.CommandBindings.Add(new CommandBinding(Commands.Filters.Edges.ZeroCrossing, (s, e) => { imgvm.DetectEdgesZero(); }, (s, e) => { e.CanExecute = imgvm.IsImageOpen; }));
            this.CommandBindings.Add(new CommandBinding(Commands.Filters.Edges.Canny, (s, e) => { imgvm.DetectEdgesCanny(); }, (s, e) => { e.CanExecute = imgvm.IsImageOpen; }));
            this.CommandBindings.Add(new CommandBinding(Commands.Filters.Denoise.Median, (s, e) => { imgvm.DenoiseMedian(); }, (s, e) => { e.CanExecute = imgvm.IsImageOpen; }));
            this.CommandBindings.Add(new CommandBinding(Commands.Filters.Morphology.Dilation, (s, e) => { imgvm.MorphDilation(); }, (s, e) => { e.CanExecute = imgvm.IsImageOpen; }));
            this.CommandBindings.Add(new CommandBinding(Commands.Filters.Morphology.Erosion, (s, e) => { imgvm.MorphErosion(); }, (s, e) => { e.CanExecute = imgvm.IsImageOpen; }));
            this.CommandBindings.Add(new CommandBinding(Commands.Filters.Morphology.Opening, (s, e) => { imgvm.MorphOpening(); }, (s, e) => { e.CanExecute = imgvm.IsImageOpen; }));
            this.CommandBindings.Add(new CommandBinding(Commands.Filters.Morphology.Closing, (s, e) => { imgvm.MorphClosing(); }, (s, e) => { e.CanExecute = imgvm.IsImageOpen; }));


            this.CommandBindings.Add(new CommandBinding(Commands.Filters.Thresholding.Plain, (s, e) => { Commands.Filters.Thresholding.PlainExecuted(this,e); }, (s, e) => { e.CanExecute = imgvm.IsImageOpen; }));
            this.CommandBindings.Add(new CommandBinding(Commands.Filters.Thresholding.Otsu, (s, e) => { imgvm.ThresholdOtsu(); }, (s, e) => { e.CanExecute = imgvm.IsImageOpen; }));
            this.CommandBindings.Add(new CommandBinding(Commands.Filters.Thresholding.Triangle, (s, e) => { imgvm.ThresholdTriangle(); }, (s, e) => { e.CanExecute = imgvm.IsImageOpen; }));
            this.CommandBindings.Add(new CommandBinding(Commands.Filters.Thresholding.Entropy, (s, e) => { imgvm.ThresholdEntropy(); }, (s, e) => { e.CanExecute = imgvm.IsImageOpen; }));
            this.CommandBindings.Add(new CommandBinding(Commands.Filters.Thresholding.Niblack, (s, e) => { imgvm.ThresholdNiblack(); }, (s, e) => { e.CanExecute = imgvm.IsImageOpen; }));


            this.CommandBindings.Add(new CommandBinding(Commands.Filters.Artistic.Oil, (s, e) => { imgvm.Oil(); }, (s, e) => { e.CanExecute = imgvm.IsImageOpen; }));
            this.CommandBindings.Add(new CommandBinding(Commands.Filters.Artistic.FishEye, (s, e) => { imgvm.FishEye(); }, (s, e) => { e.CanExecute = imgvm.IsImageOpen; }));
            this.CommandBindings.Add(new CommandBinding(Commands.Filters.Artistic.Mirror, (s, e) => { imgvm.Mirror(); }, (s, e) => { e.CanExecute = imgvm.IsImageOpen; }));
            this.CommandBindings.Add(new CommandBinding(Commands.Filters.Artistic.Negative, (s, e) => { imgvm.Negative(); }, (s, e) => { e.CanExecute = imgvm.IsImageOpen; }));
            this.CommandBindings.Add(new CommandBinding(Commands.Filters.Artistic.Emboss, (s, e) => { imgvm.Emboss(); }, (s, e) => { e.CanExecute = imgvm.IsImageOpen; }));

            this.CommandBindings.Add(new CommandBinding(Commands.Filters.Mapping.Normal, (s, e) => { imgvm.NormalMapping(); }, (s, e) => { e.CanExecute = imgvm.IsImageOpen; }));
            this.CommandBindings.Add(new CommandBinding(Commands.Filters.Mapping.Horizon, (s, e) => Commands.Filters.Mapping.HorizonExecuted(this,e), (s, e) => { e.CanExecute = imgvm.IsImageOpen; }));

            this.CommandBindings.Add(new CommandBinding(Commands.Filters.Lines.Hough, (s, e) => { imgvm.HoughTransform(); }, (s, e) => { e.CanExecute = imgvm.IsImageOpen; }));

            this.CommandBindings.Add(new CommandBinding(Commands.Filters.Corners.Harris, (s, e) => { imgvm.HarrisDetector(); }, (s, e) => { e.CanExecute = imgvm.IsImageOpen; }));
            
            this.CommandBindings.Add(new CommandBinding(Commands.Filters.Rectangles.Hough, (s, e) => { imgvm.HoughRectanglesDetector(); }, (s, e) => { e.CanExecute = imgvm.IsImageOpen; }));

            this.CommandBindings.Add(new CommandBinding(Commands.Filters.Distance, (s, e) => { imgvm.ToDistance(); }, (s, e) => { e.CanExecute = imgvm.IsImageOpen; }));
            this.CommandBindings.Add(new CommandBinding(Commands.Filters.Grayscale, (s, e) => { imgvm.ToGrayscale(); }, (s, e) => { e.CanExecute = imgvm.IsImageOpen; }));
        }

        private void BindToolsCommands()
        {
            this.CommandBindings.Add(new CommandBinding(Commands.Tools.Histogram, (s, e) => Commands.Tools.HistogramExecuted(this, e), (s, e) => { e.CanExecute = imgvm.IsImageOpen; }));
            this.CommandBindings.Add(new CommandBinding(Commands.Tools.BrightnessContrast, (s, e) => Commands.Tools.BrightnessContrastExecuted(this, e), (s, e) => { e.CanExecute = imgvm.IsImageOpen; }));
            this.CommandBindings.Add(new CommandBinding(Commands.Tools.Gamma, (s, e) => Commands.Tools.GammaExecuted(this, e), (s, e) => { e.CanExecute = imgvm.IsImageOpen; }));
            this.CommandBindings.Add(new CommandBinding(Commands.Tools.CMYK, (s, e) => Commands.Tools.CMYKExecuted(this, e), (s, e) => { e.CanExecute = imgvm.IsImageOpen; }));
        }

        private void OnDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                Commands.File.OpenExecutedInternal(imgvm, ((string[])e.Data.GetData(DataFormats.FileDrop))[0]);
            }
            e.Handled = true;
        }

    }
}
