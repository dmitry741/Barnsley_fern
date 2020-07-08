using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace FractalLeaf
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        #region members

        Bitmap _bitmap = null;
        List<PointF> _points = new List<PointF>();
        const int c_pointNumber = 96000;

        const float c_MinX = -5;
        const float c_MaxX = 5;
        const float c_MinY = 0.1f;
        const float c_MaxY = 10.5f;

        float _kx, _ky;

        // Массив коэффциентов вероятностей
        float[] _probability = new float[4]
        {
            0.01f,
            0.06f,
            0.08f,
            0.85f
        };

        // Матрица коэффициентов
        float[,] _funcCoef = new float[4, 6]
        {
            //a      b       c      d      e  f
            {0,      0,      0,     0.16f, 0, 0   },    // 1 функция
            {-0.15f, 0.28f,  0.26f, 0.24f, 0, 0.44f},   // 2 функция
            {0.2f,  -0.26f,  0.23f, 0.22f, 0, 1.6f},    // 3 функция
            {0.85f,  0.04f, -0.04f, 0.85f, 0, 1.6f}     // 4 функция
        };

        #endregion

        #region private

        void Render()
        {
            if (_bitmap == null)
                return;

            Graphics g = Graphics.FromImage(_bitmap);
            g.Clear(Color.Black);

            Rectangle rect = new Rectangle(0, 0, _bitmap.Width, _bitmap.Height);
            System.Drawing.Imaging.BitmapData bitmapData = _bitmap.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite, _bitmap.PixelFormat);
            IntPtr ptr = bitmapData.Scan0;

            int bytes = Math.Abs(bitmapData.Stride) * _bitmap.Height;
            byte[] rgbValues = new byte[bytes];

            System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);

            foreach (PointF point in _points)
            {
                int X = Convert.ToInt32(point.X);
                int Y = Convert.ToInt32(point.Y);
                int index = Y * Math.Abs(bitmapData.Stride) + X * 3;

                if (index < 0 || index >= bytes)
                    continue;

                rgbValues[index + 1] = 255; // зеленый цвет
            }

            System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, bytes);
            _bitmap.UnlockBits(bitmapData);

            pictureBox1.Image = _bitmap;
        }

        bool CreateBitmap()
        {
            if (pictureBox1.Width < 1 || pictureBox1.Height < 1)
            {
                _bitmap = null;
                return false;
            }

            _bitmap = new Bitmap(pictureBox1.Width, pictureBox1.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);

            return _bitmap.Width > 0 && _bitmap.Height > 0;
        }

        private void DrawFern()
        {
            // очищаем список точек
            _points.Clear();

            // будем начинать рисовать с точки (0, 0)
            float xtemp = 0, ytemp = 0;

            // класс для генерации случайных чисел
            Random random = new Random();

            for (int i = 0; i < c_pointNumber; i++)
            {
                // случайное число от 0 до 1
                double rd = random.NextDouble();

                // проверяем какой функцией воспользуемся для вычисления следующей точки
                float s = _probability.First(z => (rd -= z) <= 0);
                int numF = Array.IndexOf(_probability, s);

                // вычисляем координаты
                float X = _funcCoef[numF, 0] * xtemp + _funcCoef[numF, 1] * ytemp + _funcCoef[numF, 4];
                float Y = _funcCoef[numF, 2] * xtemp + _funcCoef[numF, 3] * ytemp + _funcCoef[numF, 5];

                // сохраняем значения для следующей итерации
                xtemp = X;
                ytemp = Y;

                // преобразуем в оконные координаты
                X = xtemp * _kx + _bitmap.Width / 2;
                Y = ytemp * _ky;

                _points.Add(new PointF(X, Y));
            }
        }

        #endregion

        private void Form1_Load(object sender, EventArgs e)
        {
            pictureBox1.BackColor = Color.White;

            if (CreateBitmap())
            {
                // вычисляем коэффициент
                _kx = pictureBox1.Width / (c_MaxX - c_MinX);
                _ky = pictureBox1.Height / (c_MaxY - c_MinY);

                DrawFern();
            }
        }

        private void pictureBox1_SizeChanged(object sender, EventArgs e)
        {
            if (CreateBitmap())
            {
                // вычисляем коэффициент
                _kx = pictureBox1.Width / (c_MaxX - c_MinX);
                _ky = pictureBox1.Height / (c_MaxY - c_MinY);

                DrawFern();
            }
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            Render();
        }
    }
}
