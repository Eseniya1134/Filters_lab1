using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using static System.Net.Mime.MediaTypeNames;

namespace lab1
{
    public partial class Form1 : Form
    {
        Bitmap img;
        Stack<Bitmap> undoStack = new Stack<Bitmap>();

        public Form1()
        {
            InitializeComponent();
           // pictureBox1.MouseClick += pictureBox1_MouseClick;
        }

        private void файлToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "Image files | *.png; *.jpg; *.bmp | All Files (*.*) | *.*";

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                img = new Bitmap(dialog.FileName);
                pictureBox1.Image = img;
                pictureBox1.Refresh();
            }
        }

        private void ApplyFilter(Filters fltr)
        {
            if (img != null)
            {
                undoStack.Push(new Bitmap(img));
                backgroundWorker1.RunWorkerAsync(fltr);
            }
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            Bitmap newImg = ((Filters)e.Argument).ProcessImg(img, backgroundWorker1);
            if (!backgroundWorker1.CancellationPending)
                img = newImg;
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (!e.Cancelled)
            {
                pictureBox1.Image = img;
                pictureBox1.Refresh();
            }
            progressBar1.Value = 0;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            backgroundWorker1.CancelAsync();
        }

        private void back_Click(object sender, EventArgs e)
        {
            if (undoStack.Count > 0)
            {
                img = undoStack.Pop();
                pictureBox1.Image = img;
                pictureBox1.Refresh();
            }
            else
            {
                MessageBox.Show("История пуста");
            }
        }

        private void save_Click(object sender, EventArgs e)
        {
            SaveFileDialog dialog1 = new SaveFileDialog();
            dialog1.Filter = "Image files | *.png; *.jpg; *.bmp | All Files (*.*) | *.*";

            if (dialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    if (pictureBox1.Image != null)
                    {
                        Bitmap bmp = new Bitmap(pictureBox1.Image);
                        bmp.Save(dialog1.FileName);
                    }
                    else
                    {
                        MessageBox.Show("Нет изображения для сохранения.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при сохранении: " + ex.Message);
                }
            }
        }

        // Все фильтры
        private void инверсияToolStripMenuItem_Click(object sender, EventArgs e) { ApplyFilter(new InvertFilter()); }
        private void размытиеToolStripMenuItem_Click(object sender, EventArgs e) { ApplyFilter(new BlurFilter()); }
        private void гауссToolStripMenuItem_Click(object sender, EventArgs e) { ApplyFilter(new GaussianFilter()); }
        private void чернобелоеToolStripMenuItem_Click(object sender, EventArgs e) { ApplyFilter(new GrayScaleFilter()); }
        private void сепияToolStripMenuItem_Click(object sender, EventArgs e) { ApplyFilter(new SepiaFilter()); }
        private void яркостьToolStripMenuItem_Click(object sender, EventArgs e) { ApplyFilter(new BrightnessFilter()); }
        private void собельToolStripMenuItem_Click(object sender, EventArgs e) { ApplyFilter(new SobelFilter()); }
        private void резкостьToolStripMenuItem_Click(object sender, EventArgs e) { ApplyFilter(new SharpnessFilter()); }
        private void тиснениеToolStripMenuItem_Click(object sender, EventArgs e) { ApplyFilter(new EmbossingFilter()); }
        private void светящиесяКраяToolStripMenuItem_Click(object sender, EventArgs e) { ApplyFilter(new BorderFilter()); }
        private void переносToolStripMenuItem_Click(object sender, EventArgs e) { ApplyFilter(new TransferFilter()); }
        private void поворотToolStripMenuItem_Click(object sender, EventArgs e) { ApplyFilter(new RotateFilter()); }
        private void волныToolStripMenuItem_Click(object sender, EventArgs e) { ApplyFilter(new WavesFilter()); }
        private void волны2ToolStripMenuItem_Click(object sender, EventArgs e) { ApplyFilter(new WavesFilter2()); }
        private void стеклоToolStripMenuItem_Click(object sender, EventArgs e) { ApplyFilter(new GlassFilter()); }
        private void резкость2ToolStripMenuItem_Click(object sender, EventArgs e) { ApplyFilter(new SharpnessFilterTwo()); }
        private void границыЩарраToolStripMenuItem_Click(object sender, EventArgs e) { ApplyFilter(new BorderSharaFilter()); }
        private void границыПрюиттаToolStripMenuItem_Click(object sender, EventArgs e) { ApplyFilter(new BorderPruittaFIlter()); }
        private void dilationToolStripMenuItem_Click(object sender, EventArgs e) { ApplyFilter(new Dilation()); }
        private void erosionToolStripMenuItem_Click(object sender, EventArgs e) { ApplyFilter(new Erosion()); }
        private void openingToolStripMenuItem_Click(object sender, EventArgs e) { ApplyFilter(new Opening()); }

        private void closingToolStripMenuItem_Click(object sender, EventArgs e) { ApplyFilter(new Closing()); }

        private void topHatToolStripMenuItem_Click(object sender, EventArgs e) { ApplyFilter(new TopHat()); }
        private void backHatToolStripMenuItem_Click(object sender, EventArgs e) { ApplyFilter(new BlackHat()); }
        private void gradToolStripMenuItem_Click(object sender, EventArgs e) { ApplyFilter(new Gradient()); }

        private void размытиеВДвиженииToolStripMenuItem_Click(object sender, EventArgs e) { ApplyFilter(new MotionBlurFilter()); }

        private void серыйМирToolStripMenuItem_Click(object sender, EventArgs e) { ApplyFilter(new GrayWorld()); }

        private void идеальныйОтражательToolStripMenuItem_Click(object sender, EventArgs e) { ApplyFilter(new PerfectReflector()); }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void фильтраToolStripMenuItem_Click(object sender, EventArgs e) { ApplyFilter(new FourFilter()); }


        //Обработчик клика по изображению с локальным фильтром
        /* private void pictureBox1_MouseClick(object sender, MouseEventArgs e)
         {
             int areaSize = 30;
             int x = e.X;
             int y = e.Y;

             ApplySimpleFilterAroundPoint(x, y, areaSize);
             pictureBox1.Image = img;
             pictureBox1.Refresh();
             float scaleX = (float)img.Width / pictureBox1.Width;
             float scaleY = (float)img.Height / pictureBox1.Height;
             int imgX = (int)(e.X * scaleX);
             int imgY = (int)(e.Y * scaleY);
             ApplySimpleFilterAroundPoint(imgX, imgY, areaSize);
             /*
             if (img == null) return;

             Rectangle displayRect = GetImageDisplayRectangle(pictureBox1);
             if (!displayRect.Contains(e.Location)) return;

             float scaleX = (float)img.Width / displayRect.Width;
             float scaleY = (float)img.Height / displayRect.Height;

             int x = (int)((e.X - displayRect.X) * scaleX);
             int y = (int)((e.Y - displayRect.Y) * scaleY);

             int size = 80;
             Rectangle area = new Rectangle(
                 Math.Max(0, x - size / 2),
                 Math.Max(0, y - size / 2),
                 Math.Min(size, img.Width - x + size / 2),
                 Math.Min(size, img.Height - y + size / 2)
             );

             undoStack.Push(new Bitmap(img));

             Filters localFilter = new InvertFilter();
             localFilter.ProcessPartial(img, area);

             pictureBox1.Image = img;
             pictureBox1.Refresh();*/
        /* }

         private void ApplySimpleFilterAroundPoint(int centerX, int centerY, int size)
         {

             int startX = Math.Max(0, centerX - size / 2);
             int startY = Math.Max(0, centerY - size / 2);
             int endX = Math.Min(img.Width - 1, centerX + size / 2);
             int endY = Math.Min(img.Height - 1, centerY + size / 2);


             for (int x = startX; x <= endX; x++)
             {
                 for (int y = startY; y <= endY; y++)
                 {
                     Color pixel = img.GetPixel(x, y);
                     int gray = (int)(pixel.R * 0.3 + pixel.G * 0.59 + pixel.B * 0.11);
                     img.SetPixel(x/(1/2), y/(1/2), Color.FromArgb(gray, gray, gray));
                 }
             }
         }

         /*
         private Rectangle GetImageDisplayRectangle(PictureBox pb)
         {
             if (pb.Image == null) return Rectangle.Empty;

             float imageAspect = (float)pb.Image.Width / pb.Image.Height;
             float boxAspect = (float)pb.Width / pb.Height;

             int width, height;
             if (imageAspect > boxAspect)
             {
                 width = pb.Width;
                 height = (int)(pb.Width / imageAspect);
             }
             else
             {
                 height = pb.Height;
                 width = (int)(pb.Height * imageAspect);
             }

             int x = (pb.Width - width) / 2;
             int y = (pb.Height - height) / 2;

             return new Rectangle(x, y, width, height);
         }*/
    }
}
