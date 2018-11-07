using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        Random _random = new Random();
        const int c_pointNumber = 72000;

        const float c_MinX = -6;
        const float c_MaxX = 6;
        const float c_MinY = 0.1f;
        const float c_MaxY = 11;

        int _width;
        int _height;

        #endregion

        #region private

        // Массив коэффциентов вероятностей
        private float[] _probability = new float[4]
        {
            0.01f,
            0.06f,
            0.08f,
            0.85f
        };

        // Матрица коэффициентов
        private float[,] _funcCoef = new float[4, 6]
        {
            //a      b       c      d      e  f
            {0,      0,      0,     0.16f, 0, 0   },    // 1 функция
            {-0.15f, 0.28f,  0.26f, 0.24f, 0, 0.44f},   // 2 функция
            {0.2f,  -0.26f,  0.23f, 0.22f, 0, 1.6f},    // 3 функция
            {0.85f,  0.04f, -0.04f, 0.85f, 0, 1.6f}     // 4 функция
        };

        void Render()
        {
            if (_bitmap == null)
                return;

            Graphics g = Graphics.FromImage(_bitmap);
            g.Clear(Color.Black);

            Rectangle rect = new Rectangle(0, 0, _bitmap.Width, _bitmap.Height);
            System.Drawing.Imaging.BitmapData bitmapData = _bitmap.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite, _bitmap.PixelFormat);

            // Get the address of the first line.
            IntPtr ptr = bitmapData.Scan0;

            // Declare an array to hold the bytes of the bitmap.
            int bytes = Math.Abs(bitmapData.Stride) * _bitmap.Height;
            byte[] rgbValues = new byte[bytes];

            // Copy the RGB values into the array.
            System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);

            // Fill array.  
            foreach (PointF point in _points)
            {
                int X = Convert.ToInt32(point.X);
                int Y = Convert.ToInt32(point.Y);
                int index = Y * Math.Abs(bitmapData.Stride) + X * 3;

                if (index < 0 || index >= bytes)
                    continue;

                rgbValues[index + 0] = 0;
                rgbValues[index + 1] = 255;
                rgbValues[index + 2] = 0;
            }

            // Copy the RGB values back to the bitmap
            System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, bytes);

            // Unlock the bits.
            _bitmap.UnlockBits(bitmapData);

            pictureBox1.Image = _bitmap;
        }

        void CreateBitmap()
        {
            if (pictureBox1.Width < 1 || pictureBox1.Height < 1)
            {
                _bitmap = null;
                return;
            }

            _bitmap = new Bitmap(pictureBox1.Width, pictureBox1.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
        }

        private void DrawFern()
        {
            _points.Clear();

            // будем начинать рисовать с точки (0, 0)
            float xtemp = 0, ytemp = 0;

            // переменная хранения номера функции для вычисления следующей точки
            int numF = 0;

            for (int i = 0; i < c_pointNumber; i++)
            {
                // рандомное число от 0 до 1
                double num = _random.NextDouble();

                // проверяем какой функцией воспользуемся для вычисления следующей точки
                for (int j = 0; j <= 3; j++)
                {
                    // если рандомное число оказалось меньше или равно
                    // заданного коэффициента вероятности,
                    // задаем номер функции
                    num -= _probability[j];

                    if (num <= 0)
                    {
                        numF = j;
                        break;
                    }
                }

                // вычисляем координаты
                float X = _funcCoef[numF, 0] * xtemp + _funcCoef[numF, 1] * ytemp + _funcCoef[numF, 4];
                float Y = _funcCoef[numF, 2] * xtemp + _funcCoef[numF, 3] * ytemp + _funcCoef[numF, 5];

                // сохраняем значения для следующей итерации
                xtemp = X;
                ytemp = Y;

                // вычисляем значение пикселя
                X = Convert.ToInt32(xtemp * _width + _bitmap.Width / 2);
                Y = Convert.ToInt32(ytemp * _height);

                _points.Add(new PointF(X, Y));
            }
        }

        #endregion

        private void Form1_Load(object sender, EventArgs e)
        {
            pictureBox1.BackColor = Color.White;
            CreateBitmap();

            // вычисляем коэффициент
            _width = Convert.ToInt32(pictureBox1.Width / (c_MaxX - c_MinX));
            _height = Convert.ToInt32(pictureBox1.Height / (c_MaxY - c_MinY));

            DrawFern();
        }

        private void pictureBox1_SizeChanged(object sender, EventArgs e)
        {
            CreateBitmap();

            // вычисляем коэффициент
            _width = Convert.ToInt32(pictureBox1.Width / (c_MaxX - c_MinX));
            _height = Convert.ToInt32(pictureBox1.Height / (c_MaxY - c_MinY));

            DrawFern();
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            Render();
        }
    }
}
