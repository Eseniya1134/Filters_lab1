using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Runtime.Remoting.Messaging;
using System.ComponentModel;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.ToolTip;

namespace lab1
{
    public abstract class Filters
    {

        // создает пустое изображение такого же размера, как и попадающееся ей на вход
        public Bitmap ProcessImg(Bitmap sourseImg, BackgroundWorker worker)
        {
            Bitmap resImg = new Bitmap(sourseImg.Width, sourseImg.Height);
            for (int i = 0; i < sourseImg.Width; i++)
            {
                if (worker.IsBusy) // Проверяем, выполняется ли процесс
                {
                    worker.ReportProgress((int)((float)i / sourseImg.Width * 100));
                }
                //worker.ReportProgress((int)((float)i / sourseImg.Width * 100));
                if (worker.CancellationPending)
                    return null;

                for (int j = 0; j < sourseImg.Height; j++)
                {
                    resImg.SetPixel(i, j, calcNewPixelColor(sourseImg, i, j));
                }
            }

            return resImg;
        }

        protected abstract Color calcNewPixelColor(Bitmap sourseImg, int i, int j);

        // напишите функцию Clamp, чтобы привести значения(один из трех компонента цвета) к допустимому диапазону.
        public int Clamp(int val, int min, int max)
        {
            if (val < min)
            {
                val = min;
            }
            if (val > max)
            {
                val = max;
            }
            return val;
        }

        public virtual void ProcessPartial(Bitmap image, Rectangle area)
        {
            for (int y = area.Top; y < area.Bottom && y < image.Height; y++)
            {
                for (int x = area.Left; x < area.Right && x < image.Width; x++)
                {
                    Color c = image.GetPixel(x, y);
                    image.SetPixel(x, y, Color.FromArgb(255 - c.R, 255 - c.G, 255 - c.B)); // Пример: инверсия
                }
            }
        }
    }


    class MatrixFilter : Filters
    {
        protected float[,] kernel = null;
        protected MatrixFilter() { }
        public MatrixFilter(float[,] kernel)
        {
            this.kernel = kernel;
        }

        //будет вычислять цвет пикселя на основании своих соседей.
        //1)найдите радиусы фильтра по ширине и по высоте на основании матрицы
        protected override Color calcNewPixelColor(Bitmap sourceImg, int x, int y)
        {
            int radiusX = kernel.GetLength(0) / 2;
            int radiusY = kernel.GetLength(1) / 2;

            //перебирать окрестность пикселя.
            //В каждой из точек окрестности вычислите цвет,
            //умножьте на значение из ядра и прибавьте к результирующим компонентамцвета
            float resultR = 0;
            float resultG = 0;
            float resultB = 0;
            for (int l = -radiusY; l <= radiusY; l++)
            {
                for (int k = -radiusX; k <= radiusX; k++)
                {
                    int idX = Clamp(x + k, 0, sourceImg.Width - 1);
                    int idY = Clamp(y + k, 0, sourceImg.Height - 1);
                    Color neighborColor = sourceImg.GetPixel(idX, idY);
                    resultR += neighborColor.R * kernel[k + radiusX, l + radiusY];
                    resultG += neighborColor.G * kernel[k + radiusX, l + radiusY];
                    resultB += neighborColor.B * kernel[k + radiusX, l + radiusY];
                }
            }

            //создайте экземпляр класса Color,
            //состоящий из вычисленных вами компонент цвета.
            return Color.FromArgb(
                Clamp((int)resultR, 0, 255),
                Clamp((int)resultG, 0, 255),
                Clamp((int)resultB, 0, 255)
                );


        }
    }


    class InvertFilter : Filters
    {
        //вычисление инверсии
        protected override Color calcNewPixelColor(Bitmap sourseImg, int i, int j)
        {
            Color sourseColor = sourseImg.GetPixel(i, j);
            Color resColor = Color.FromArgb(255 - sourseColor.R, 255 - sourseColor.G, 255 - sourseColor.B);
            return resColor;
        }
    }

    class SepiaFilter : Filters
    {
        protected override Color calcNewPixelColor(Bitmap sourseImg, int i, int j)
        {
            Color sourseColor = sourseImg.GetPixel(i, j);
            int resColor = (int)(0.36 * sourseColor.R + 0.53 * sourseColor.G + 0.11 * sourseColor.B);
            resColor = Clamp(resColor, 0, 255);
            float resultR = (float)(resColor + 2 * 21);
            float resultG = (float)(resColor + 0.5 * 21);
            float resultB = (float)(resColor - 1 * 21);
            return Color.FromArgb(
                Clamp((int)resultR, 0, 255),
                Clamp((int)resultG, 0, 255),
                Clamp((int)resultB, 0, 255)
                );

        }
    }

    class GrayScaleFilter : Filters
    {
        protected override Color calcNewPixelColor(Bitmap sourseImg, int i, int j)
        {
            Color sourseColor = sourseImg.GetPixel(i, j);
            int resColor = (int)(0.36 * sourseColor.R + 0.53 * sourseColor.G + 0.11 * sourseColor.B);
            resColor = Clamp(resColor, 0, 255);


            return Color.FromArgb(resColor, resColor, resColor);
        }
    }

    class BrightnessFilter : Filters
    {

        protected override Color calcNewPixelColor(Bitmap sourseImg, int i, int j)
        {
            Color OrColor = sourseImg.GetPixel(i, j);
            int R = OrColor.R + 70;
            int G = OrColor.G + 70;
            int B = OrColor.B + 70;
            return Color.FromArgb(
                Clamp(R, 0, 255),
                Clamp(G, 0, 255),
                Clamp(B, 0, 255));
        }
    }

    class SobelFilter : MatrixFilter
    {
        public SobelFilter()
        {
            int szX = 3;
            int szY = 3;
            kernel = new float[szX, szY];

            kernel[0, 0] = -1.0f;
            kernel[0, 1] = -2.0f;
            kernel[0, 2] = -1.0f;
            kernel[1, 0] = 0.0f;
            kernel[1, 1] = 0.0f;
            kernel[1, 2] = 0.0f;
            kernel[2, 0] = 1.0f;
            kernel[2, 1] = 2.0f;
            kernel[2, 2] = 1.0f;
        }

    }

    class SharpnessFilter : MatrixFilter
    {
        private int brightnessShift = 10; // Сдвиг по яркости

        public SharpnessFilter()
        {
            int szX = 3;
            int szY = 3;
            kernel = new float[szX, szY];

            kernel[0, 0] = 0.0f;
            kernel[0, 1] = -1.0f;
            kernel[0, 2] = 0.0f;
            kernel[1, 0] = -1.0f;
            kernel[1, 1] = 5.0f;
            kernel[1, 2] = -1.0f;
            kernel[2, 0] = 0.0f;
            kernel[2, 1] = -1.0f;
            kernel[2, 2] = 0.0f;
        }

        protected override Color calcNewPixelColor(Bitmap sourceImg, int x, int y)
        {
            int radiusX = kernel.GetLength(0) / 2;
            int radiusY = kernel.GetLength(1) / 2;

            float resultR = 0;
            float resultG = 0;
            float resultB = 0;

            float kernelSum = 0;

            for (int l = -radiusY; l <= radiusY; l++)
            {
                for (int k = -radiusX; k <= radiusX; k++)
                {
                    int idX = Clamp(x + k, 0, sourceImg.Width - 1);
                    int idY = Clamp(y + l, 0, sourceImg.Height - 1);
                    Color neighborColor = sourceImg.GetPixel(idX, idY);

                    float kernelVal = kernel[k + radiusX, l + radiusY];

                    resultR += neighborColor.R * kernelVal;
                    resultG += neighborColor.G * kernelVal;
                    resultB += neighborColor.B * kernelVal;

                    kernelSum += kernelVal;
                }
            }

            if (kernelSum <= 0) kernelSum = 1; // Защита от деления на ноль

            resultR = resultR / kernelSum + brightnessShift;
            resultG = resultG / kernelSum + brightnessShift;
            resultB = resultB / kernelSum + brightnessShift;

            return Color.FromArgb(
                Clamp((int)resultR, 0, 255),
                Clamp((int)resultG, 0, 255),
                Clamp((int)resultB, 0, 255)
            );
        }

       protected int Clamp(int value, int min, int max)
        {
            return Math.Min(Math.Max(value, min), max);
        }
    }

    class EmbossingFilter : MatrixFilter
    {
        public EmbossingFilter()
        {
            int size = 3;
            int[,] yadro = { { 0, 1, 0 }, { 1, 0, -1 }, { 0, -1, 0 } };
            kernel = new float[size, size];
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    kernel[i, j] = yadro[i, j];
                }
            }
        }

        protected override Color calcNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int radiusX = kernel.GetLength(0) / 2;
            int radiusY = kernel.GetLength(1) / 2;
            float resultR = 0;
            float resultG = 0;
            float resultB = 0;
            for (int l = -radiusY; l <= radiusY; l++)
            {
                for (int k = -radiusX; k <= radiusY; k++)
                {
                    int idX = Clamp(x + k, 0, sourceImage.Width - 1);
                    int idY = Clamp(y + l, 0, sourceImage.Height - 1);
                    Color neighborColor = sourceImage.GetPixel(idX, idY);
                    resultR += neighborColor.R * kernel[k + radiusX, l + radiusY];
                    resultG += neighborColor.G * kernel[k + radiusX, l + radiusY];
                    resultB += neighborColor.B * kernel[k + radiusX, l + radiusY];

                }
            }
            return Color.FromArgb(
                Clamp((int)resultR + 70, 0, 255),
                Clamp((int)resultG + 70, 0, 255),
                Clamp((int)resultB + 70, 0, 255)
                );
        }
    }

    internal class BorderFilter : MatrixFilter
    {
        public BorderFilter()
        {
            int szX = 3;
            int szY = 3;
            kernel = new float[szX, szY];

            kernel[0, 0] = 3.0f;
            kernel[0, 1] = 10.0f;
            kernel[0, 2] = 3.0f;
            kernel[1, 0] = 0.0f;
            kernel[1, 1] = 0.0f;
            kernel[1, 2] = 0.0f;
            kernel[2, 0] = -3.0f;
            kernel[2, 1] = -10.0f;
            kernel[2, 2] = -3.0f;
        }
    }

    class TransferFilter : Filters
    {
        protected override Color calcNewPixelColor(Bitmap sourceIm, int x, int y)
        {
            x += 50;
            if (x >= sourceIm.Width || y >= sourceIm.Height)
            {
                return Color.FromArgb(200, 200, 200);
            }
            Color OrColor = sourceIm.GetPixel(x, y);
            int R = OrColor.R;
            int G = OrColor.G;
            int B = OrColor.B;
            return Color.FromArgb(
                Clamp(R, 0, 255),
                Clamp(G, 0, 255),
                Clamp(B, 0, 255));
        }
    }

    class RotateFilter : Filters
    {
        protected override Color calcNewPixelColor(Bitmap sourceIm, int x, int y)
        {
            int newx = (int)((x - 100) * Math.Cos(Math.PI / 6) - (y - 100) * Math.Sin(Math.PI / 6) + 100);
            int newy = (int)((x - 100) * Math.Sin(Math.PI / 6) + (y - 100) * Math.Cos(Math.PI / 6) + 100);
            if (newx >= sourceIm.Width || newy >= sourceIm.Height)
            {
                return Color.FromArgb(200, 200, 200);
            }

            if (newx <= 0 || newy <= 0)
            {
                return Color.FromArgb(200, 200, 200);
            }

            Color OrColor = sourceIm.GetPixel(newx, newy);
            int R = OrColor.R;
            int G = OrColor.G;
            int B = OrColor.B;
            return Color.FromArgb(
                Clamp(R, 0, 255),
                Clamp(G, 0, 255),
                Clamp(B, 0, 255));
        }
    }

    class WavesFilter : Filters
    {
        protected override Color calcNewPixelColor(Bitmap sourceIm, int x, int y)
        {
            int newx = (int)(x + 20 * Math.Sin((2 * Math.PI * y) / 60));
            int newy = y;

            if (newx < 0 || newx >= sourceIm.Width || newy < 0 || newy >= sourceIm.Height)
            {
                return Color.FromArgb(200, 200, 200);
            }
            Color OrColor = sourceIm.GetPixel(newx, newy);
            int R = OrColor.R;
            int G = OrColor.G;
            int B = OrColor.B;
            return Color.FromArgb(
                Clamp(R, 0, 255),
                Clamp(G, 0, 255),
                Clamp(B, 0, 255));
        }
    }

    class WavesFilter2 : Filters
    {
        protected override Color calcNewPixelColor(Bitmap sourceIm, int x, int y)
        {
            int newx = (int)(x + 20 * Math.Sin((2 * Math.PI * x) / 30));
            int newy = y;

            if (newx < 0 || newx >= sourceIm.Width || newy < 0 || newy >= sourceIm.Height)
            {
                return Color.FromArgb(200, 200, 200);
            }
            Color OrColor = sourceIm.GetPixel(newx, newy);
            int R = OrColor.R;
            int G = OrColor.G;
            int B = OrColor.B;
            return Color.FromArgb(
                Clamp(R, 0, 255),
                Clamp(G, 0, 255),
                Clamp(B, 0, 255));
        }
    }

    class GlassFilter : Filters
    {
        Random rand = new Random();
        protected override Color calcNewPixelColor(Bitmap sourceIm, int x, int y)
        {

            int newx = x + (int)((rand.NextDouble() - 0.5) * 15);
            int newy = y + (int)((rand.NextDouble() - 0.5) * 15);

            if (newx < 0 || newx >= sourceIm.Width || newy < 0 || newy >= sourceIm.Height)
            {
                return Color.FromArgb(200, 200, 200);
            }
            Color OrColor = sourceIm.GetPixel(newx, newy);
            int R = OrColor.R;
            int G = OrColor.G;
            int B = OrColor.B;
            return Color.FromArgb(
                Clamp(R, 0, 255),
                Clamp(G, 0, 255),
                Clamp(B, 0, 255));
        }
    }

    class SharpnessFilterTwo : MatrixFilter
    {
        public SharpnessFilterTwo()
        {
            int szX = 3;
            int szY = 3;
            kernel = new float[szX, szY];
            for (int i = 0; i < szX; i++)
            {
                for (int j = 0; j < szY; j++)
                {
                    kernel[i, j] = -1.0f;
                }
            }
            kernel[1, 1] = 9.0f;
        }
    }

    class BorderSharaFilter : MatrixFilter
    {
        public BorderSharaFilter()
        {
            int szX = 3, szY = 3;
            kernel = new float[szX, szY];

            kernel[0, 0] = 3.0f;
            kernel[0, 1] = 10.0f;
            kernel[0, 2] = 3.0f;
            kernel[1, 0] = 0.0f;
            kernel[1, 1] = 0.0f;
            kernel[1, 2] = 0.0f;
            kernel[2, 0] = -3.0f;
            kernel[2, 1] = -10.0f;
            kernel[2, 2] = -3.0f;
        }
    }

    class BorderPruittaFIlter : MatrixFilter
    {
        public BorderPruittaFIlter()
        {
            int szX = 3;
            int szY = 3;
            kernel = new float[szX, szY];

            kernel[0, 0] = -1.0f;
            kernel[0, 1] = -1.0f;
            kernel[0, 2] = -1.0f;
            kernel[1, 0] = 0.0f;
            kernel[1, 1] = 0.0f;
            kernel[1, 2] = 0.0f;
            kernel[2, 0] = 1.0f;
            kernel[2, 1] = 1.0f;
            kernel[2, 2] = 1.0f;
        }
    }

    class Dilation : MatrixFilter
    {
        public Dilation()
        {
            kernel = new float[,]
        {
            { 1.0f, 1.0f, 1.0f},
            { 1.0f, 1.0f, 1.0f},
            { 1.0f, 1.0f, 1.0f},
            {1.0f, 1.0f, 1.0f},
            {1.0f, 1.0f, 1.0f}
        };
        }
        protected override Color calcNewPixelColor(Bitmap sourceIm, int x, int y)
        {
            int radiusX = kernel.GetLength(0) / 2;
            int radiusY = kernel.GetLength(1) / 2;
            Color maxColor = Color.Black;
            Color OrCol = sourceIm.GetPixel(x, y);
            for (int l = -radiusY; l <= radiusY; l++)
            {
                for (int k = -radiusX; k <= radiusX; k++)
                {
                    int newX = x + k;
                    int newY = y + l;
                    if (newX >= 0 && newX < sourceIm.Width && newY >= 0 && newY < sourceIm.Height)
                    {
                        Color neighborCol = sourceIm.GetPixel(newX, newY);
                        if (neighborCol.GetBrightness() > maxColor.GetBrightness())
                        {
                            maxColor = neighborCol;
                        }
                    }
                }
            }
            return maxColor;
        }
    }

    class Erosion : MatrixFilter
    {
        public Erosion()
        {
            kernel = new float[,]
        {
            { 1.0f, 1.0f, 1.0f},
            { 1.0f, 1.0f, 1.0f},
            { 1.0f, 1.0f, 1.0f },
            {1.0f, 1.0f, 1.0f},
            {1.0f, 1.0f, 1.0f}
        };
        }
        protected override Color calcNewPixelColor(Bitmap sourceIm, int x, int y)
        {
            int radiusX = kernel.GetLength(0) / 2;
            int radiusY = kernel.GetLength(1) / 2;
            Color minColor = Color.White;
            Color OrCol = sourceIm.GetPixel(x, y);
            for (int l = -radiusY; l <= radiusY; l++)
            {
                for (int k = -radiusX; k <= radiusX; k++)
                {
                    int newX = x + k;
                    int newY = y + l;
                    if (newX >= 0 && newX < sourceIm.Width && newY >= 0 && newY < sourceIm.Height)
                    {
                        Color neighborCol = sourceIm.GetPixel(newX, newY);
                        if (neighborCol.GetBrightness() < minColor.GetBrightness())
                        {
                            minColor = neighborCol;
                        }
                    }
                }
            }
            return minColor;
        }
    }

    class Opening : Filters
    //Применяет эрозию, а затем дилатацию. Используется для удаления мелких объектов.
    //Opening=Dilation(Erosion(Image))
    {
        private Bitmap processedImage = null;

        protected override Color calcNewPixelColor(Bitmap sourceImg, int x, int y)
        {
            if (processedImage == null)
            {
                Erosion erosion = new Erosion();
                Bitmap eroded = erosion.ProcessImg(sourceImg, new BackgroundWorker());

                Dilation dilation = new Dilation();
                processedImage = dilation.ProcessImg(eroded, new BackgroundWorker());

                eroded.Dispose();
            }
            return processedImage.GetPixel(x, y);
        }
    }

    class MotionBlurFilter : MatrixFilter
    {
        public MotionBlurFilter()
        {
            int szX = 9;
            int szY = 9;
            kernel = new float[szX, szY];

            for (int i = 0; i < szX; i++)
            {
                for (int j = 0; j < szY; j++)
                {
                    kernel[i, j] = 0.0f;
                }
            }
            kernel[0, 0] = 1.0f;
            kernel[1, 1] = 1.0f;
            kernel[2, 2] = 1.0f;
            kernel[3, 3] = 1.0f;
            kernel[4, 4] = 1.0f;
            kernel[5, 5] = 1.0f;
            kernel[6, 6] = 1.0f;
            kernel[7, 7] = 1.0f;
            kernel[8, 8] = 1.0f;

        }
    }

    class Closing : Filters
    //Применяет дилатацию, а затем эрозию. Используется для заполнения мелких дыр в объектах.
    //Closing=Erosion(Dilation(Image))
    {
        private Bitmap processedImage = null; // для хранения промежуточного результата

        protected override Color calcNewPixelColor(Bitmap sourceImg, int x, int y)
        {
            // Если еще не обработали, обработать всё изображение один раз
            if (processedImage == null)
            {
                Dilation dilation = new Dilation();
                Bitmap dilated = dilation.ProcessImg(sourceImg, new BackgroundWorker());

                Erosion erosion = new Erosion();
                processedImage = erosion.ProcessImg(dilated, new BackgroundWorker());

                dilated.Dispose(); // Освободить память
            }

            // Вернуть уже обработанный пиксель
            return processedImage.GetPixel(x, y);
        }
    }

    class TopHat : Filters
    {
        private Bitmap processedImage = null;

        protected override Color calcNewPixelColor(Bitmap sourceImg, int x, int y)
        {
            if (processedImage == null)
            {
                // Выполняем Opening
                Erosion erosion = new Erosion();
                Bitmap eroded = erosion.ProcessImg(sourceImg, new BackgroundWorker());

                Dilation dilation = new Dilation();
                Bitmap opened = dilation.ProcessImg(eroded, new BackgroundWorker());
                eroded.Dispose();

                // Создаем изображение разности
                processedImage = new Bitmap(sourceImg.Width, sourceImg.Height);
                for (int i = 0; i < sourceImg.Width; i++)
                {
                    for (int j = 0; j < sourceImg.Height; j++)
                    {
                        Color original = sourceImg.GetPixel(i, j);
                        Color openedColor = opened.GetPixel(i, j);

                        int r = Clamp(original.R - openedColor.R, 0, 255);
                        int g = Clamp(original.G - openedColor.G, 0, 255);
                        int b = Clamp(original.B - openedColor.B, 0, 255);

                        processedImage.SetPixel(i, j, Color.FromArgb(r, g, b));
                    }
                }
                opened.Dispose();
            }
            return processedImage.GetPixel(x, y);
        }
    }

    class GrayWorld : Filters
    {
        private double[] avg;

        public void CalculateAverageBrightness(Bitmap sourceIm)
        {
            long totalR = 0;
            long totalG = 0;
            long totalB = 0;
            int pixelCount = sourceIm.Width * sourceIm.Height;

            for (int y = 0; y < sourceIm.Height; y++)
            {
                for (int x = 0; x < sourceIm.Width; x++)
                {
                    Color pixelColor = sourceIm.GetPixel(x, y);
                    totalR += pixelColor.R;
                    totalG += pixelColor.G;
                    totalB += pixelColor.B;
                }
            }

            double averageR = totalR / (double)pixelCount;
            double averageG = totalG / (double)pixelCount;
            double averageB = totalB / (double)pixelCount;
            avg = new double[] { averageR, averageG, averageB };
        }

        protected override Color calcNewPixelColor(Bitmap sourceIm, int x, int y)
        {
            if (avg == null)
            {
                CalculateAverageBrightness(sourceIm);
            }
            Color OrColor = sourceIm.GetPixel(x, y);
            double avgc = (avg[0] + avg[1] + avg[2]) / 3;
            double R = OrColor.R * (avgc / avg[0]);
            double G = OrColor.G * (avgc / avg[1]);
            double B = OrColor.B * (avgc / avg[2]);

            return Color.FromArgb(
                Clamp((int)R, 0, 255),
                Clamp((int)G, 0, 255),
                Clamp((int)B, 0, 255));
        }
    }

    class GaussianFilter : MatrixFilter
    {
        public GaussianFilter()
        {
            createGaussianKernel(3, 2);
        }

        public void createGaussianKernel(int radius, float sigma)
        {
            //определение размера ядра
            int size = 2 * radius + 1;
            //создаем ядро фильтра
            kernel = new float[size, size];
            //коэффициент нормировки ядра
            float norm = 0;
            //рассчет ядра линейн фильтра
            for (int i = -radius; i <= radius; i++)
            {
                for (int j = -radius; j <= radius; j++)
                {
                    kernel[i + radius, j + radius] = (float)(Math.Exp(-(i * i + j * j) / (sigma * sigma)));
                    norm += kernel[i + radius, j + radius];
                }
            }
            //нормируем ядро
            for (int i = 0; i < size; i++)
                for (int j = 0; j < size; j++)
                    kernel[i, j] /= norm;
        }
    }


    class PerfectReflector : Filters
    {
        private int maxR = 0, maxG = 0, maxB = 0;

        // Сначала находим максимальные значения каналов
        private void CalculateMaxColor(Bitmap sourceImg)
        {
            for (int y = 0; y < sourceImg.Height; y++)
            {
                for (int x = 0; x < sourceImg.Width; x++)
                {
                    Color c = sourceImg.GetPixel(x, y);
                    if (c.R > maxR) maxR = c.R;
                    if (c.G > maxG) maxG = c.G;
                    if (c.B > maxB) maxB = c.B;
                }
            }
        }

        // Основной метод обработки
        public Bitmap ProcessImg(Bitmap sourceImg, BackgroundWorker worker)
        {
            CalculateMaxColor(sourceImg);

            Bitmap result = new Bitmap(sourceImg.Width, sourceImg.Height);

            for (int x = 0; x < sourceImg.Width; x++)
            {
                if (worker != null)
                {
                    if (worker.CancellationPending)
                        return null;
                    worker.ReportProgress((int)((float)x / sourceImg.Width * 100));
                }

                for (int y = 0; y < sourceImg.Height; y++)
                {
                    Color c = sourceImg.GetPixel(x, y);

                    // Нормализация компонентов
                    int newR = Clamp((int)(c.R * 255.0 / maxR), 0, 255);
                    int newG = Clamp((int)(c.G * 255.0 / maxG), 0, 255);
                    int newB = Clamp((int)(c.B * 255.0 / maxB), 0, 255);

                    result.SetPixel(x, y, Color.FromArgb(newR, newG, newB));
                }
            }

            return result;
        }

        // Обязательная реализация (не используется напрямую)
        protected override Color calcNewPixelColor(Bitmap sourceImg, int x, int y)
        {
            return Color.Black;
        }
    }


    class Gradient : Filters
    {
        private Bitmap processedImage = null;

        protected override Color calcNewPixelColor(Bitmap sourceImg, int x, int y)
        {
            if (processedImage == null)
            {
                Dilation dilation = new Dilation();
                Bitmap dilated = dilation.ProcessImg(sourceImg, new BackgroundWorker());

                Erosion erosion = new Erosion();
                Bitmap eroded = erosion.ProcessImg(sourceImg, new BackgroundWorker());

                // Создаем изображение разности
                processedImage = new Bitmap(sourceImg.Width, sourceImg.Height);
                for (int i = 0; i < sourceImg.Width; i++)
                {
                    for (int j = 0; j < sourceImg.Height; j++)
                    {
                        Color dilatedColor = dilated.GetPixel(i, j);
                        Color erodedColor = eroded.GetPixel(i, j);

                        int r = Clamp(dilatedColor.R - erodedColor.R, 0, 255);
                        int g = Clamp(dilatedColor.G - erodedColor.G, 0, 255);
                        int b = Clamp(dilatedColor.B - erodedColor.B, 0, 255);

                        processedImage.SetPixel(i, j, Color.FromArgb(r, g, b));
                    }
                }
                dilated.Dispose();
                eroded.Dispose();
            }
            return processedImage.GetPixel(x, y);
        }
    }

    class BlackHat : Filters
    {
        private Bitmap processedImage = null;

        protected override Color calcNewPixelColor(Bitmap sourceImg, int x, int y)
        {
            if (processedImage == null)
            {
                // Выполняем Closing
                Dilation dilation = new Dilation();
                Bitmap dilated = dilation.ProcessImg(sourceImg, new BackgroundWorker());

                Erosion erosion = new Erosion();
                Bitmap closed = erosion.ProcessImg(dilated, new BackgroundWorker());
                dilated.Dispose();

                // Создаем изображение разности
                processedImage = new Bitmap(sourceImg.Width, sourceImg.Height);
                for (int i = 0; i < sourceImg.Width; i++)
                {
                    for (int j = 0; j < sourceImg.Height; j++)
                    {
                        Color closedColor = closed.GetPixel(i, j);
                        Color original = sourceImg.GetPixel(i, j);

                        int r = Clamp(closedColor.R - original.R, 0, 255);
                        int g = Clamp(closedColor.G - original.G, 0, 255);
                        int b = Clamp(closedColor.B - original.B, 0, 255);

                        processedImage.SetPixel(i, j, Color.FromArgb(r, g, b));
                    }
                }
                closed.Dispose();
            }
            return processedImage.GetPixel(x, y);
        }
    }


    class BlurFilter : MatrixFilter
    {
        public BlurFilter()
        {
            int sizeX = 3;
            int sizeY = 3;
            kernel = new float[sizeX, sizeY];
            for (int i = 0; i < sizeX; i++)
                for (int j = 0; j < sizeY; j++)
                    kernel[i, j] = 1.0f / (float)(sizeX * sizeY);

        }
    }

    class FourFilter : Filters {
        private Bitmap processedImage = null;

        protected override Color calcNewPixelColor(Bitmap sourceImg, int x, int y)
        {
            if (processedImage == null)
            {
                processedImage = ApplySegmentedFilter(sourceImg);
            }
            return processedImage.GetPixel(x, y);
        }

        private Bitmap ApplySegmentedFilter(Bitmap sourceImg)
        {
            int width = sourceImg.Width;
            int height = sourceImg.Height;

            Bitmap result = new Bitmap(width, height);

            Rectangle topLeft = new Rectangle(0, 0, width / 2, height / 2);
            Rectangle topRight = new Rectangle(width / 2, 0, width / 2, height / 2);
            Rectangle bottomLeft = new Rectangle(0, height / 2, width / 2, height / 2);
            Rectangle bottomRight = new Rectangle(width / 2, height / 2, width / 2, height / 2);

            // Applying existing filters to each segment
            ApplyFilter(new Dilation(), sourceImg, result, topLeft);
            ApplyFilter(new Erosion(), sourceImg, result, topRight);
            ApplyFilter(new Gradient(), sourceImg, result, bottomLeft);
            ApplyFilter(new Opening(), sourceImg, result, bottomRight);

            return result;
        }

        private void ApplyFilter(Filters filter, Bitmap source, Bitmap result, Rectangle area)
        {
            Bitmap filtered = filter.ProcessImg(source.Clone(area, source.PixelFormat), new BackgroundWorker());

            using (Graphics g = Graphics.FromImage(result))
            {
                g.DrawImage(filtered, area);
            }

            filtered.Dispose();
        }
    }


}
