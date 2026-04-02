using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;
using System.Drawing.Imaging;
using System.Diagnostics;


namespace Drawer
{
    public partial class MainForm : Form
    {

        private Image newImage;
        private Color backColor = Color.White;
        private Color foreColor = Color.Black;
        private string newFileName;
        private Graphics ig;
        //工具栏
        private bool isDrawing;
        private Point startPoint;
        private Point oldPoint;
        private drawTools drawTool = drawTools.None;
        private int drawWidth = 1;
        private enum drawTools
        {
            pen = 0,
            Line,
            Ellipse,
            Rectangle,
            String,
            Rubber,
            None
        };

        // 在主窗体中重写OnFormClosing
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (MessageBox.Show("是否退出程序", "提示",
                MessageBoxButtons.YesNo) != DialogResult.Yes)
            {
                e.Cancel = true;
            }
            base.OnFormClosing(e);
        }
        public MainForm()
        {
            InitializeComponent();
            InitializeClock();//时间初始化
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            //LoginForm loginForm = new LoginForm(); // 创建登录界面实例 
            //loginForm.ShowDialog(); // 显示登录界面 
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            //lblTime.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"); // 更新时间标签
        }

        private void pictureBoxCanvas_MouseMove(object sender, EventArgs e)
        {
            //if (isDrawing) // 判断是否正在绘制 
            {
                // Graphics g = panel1.CreateGraphics();
                // g.Clear(Color.White); // 清除背景
                // g.DrawRectangle(Pens.Black, startX, startY, e.X - startX, e.Y - startY); // 绘制矩形 }
            }
        }

        private void 新建ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //toolStrip1.Enabled = true;
            Graphics g = this.CreateGraphics();
            g.Clear(backColor);
            newImage = new Bitmap(this.ClientRectangle.Width, this.ClientRectangle.Height);
            newFileName = "新图像.bmp";
            this.Text = "Vector Drawer  " + newFileName;
            ig = Graphics.FromImage(newImage);
            ig.Clear(backColor);

            toolStrip1.Visible = true;
            toolStrip1.Enabled = true;
            trackBar1.Visible = true;
            label1.Visible = true;
        }
        private void 保存ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog SaveDialog = new SaveFileDialog();
            SaveDialog.Title = "保存文件";
            SaveDialog.Filter = "BMP位图(*.bmp*)|*.bmp*";
            SaveDialog.FileName = newFileName;
            if (SaveDialog.ShowDialog() == DialogResult.OK)
            {
                newImage.Save(SaveDialog.FileName, ImageFormat.Bmp);
                this.Text = "Vector Drawer  " + SaveDialog.FileName;
                newFileName = SaveDialog.FileName;
            }

        }

        private void 退出ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void 关于ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Vector Drawer Build 10", "关于本程序", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void 打开ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog OpenDialog = new OpenFileDialog();
            OpenDialog.Multiselect = false;//该值确定是否可以选择多个文件
            OpenDialog.Title = "打开文件";
            OpenDialog.Filter = "图像文件(*.bmp;*.ico;*.jpg)|*.bmp;*.ico;*.jpg";
            if (OpenDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                //string file = OpenDialog.FileName;
                this.Text = "Vector Drawer  " + OpenDialog.FileName;
                newFileName = OpenDialog.FileName;
                newImage = Image.FromFile(newFileName);
                Graphics g = this.CreateGraphics();
                g.DrawImage(newImage, this.ClientRectangle);
                ig = Graphics.FromImage(newImage);
                ig.DrawImage(newImage, this.ClientRectangle);
                toolStrip1.Visible = true;
                toolStrip1.Enabled = true;
            }
        }

        private void 另存为ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog SaveDialog = new SaveFileDialog();
            SaveDialog.Title = "另存文件";
            SaveDialog.Filter = "BMP位图(*.bmp*)|*.bmp*";
            if (SaveDialog.ShowDialog() == DialogResult.OK)
            {
                // panel1.DrawToBitmap(new Bitm ap(panel1.Width, panel1.Height), new Rectangle(0, 0, panel1.Width, panel1.Height)); 
            }
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            drawTool = drawTools.pen;
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            drawTool = drawTools.Line;
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            drawTool = drawTools.Rectangle;
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            drawTool = drawTools.Ellipse;
        }

        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            drawTool = drawTools.String;
        }

        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            drawTool = drawTools.Rubber;
        }

        private void MainForm_MouseDown(object sender, MouseEventArgs e)
        {
            isDrawing = true;
            if (e.Button == MouseButtons.Left)
            {
                startPoint = new Point(e.X, e.Y);
                oldPoint = new Point(e.X, e.Y);
            }
        }

        private void MainForm_MouseUp(object sender, MouseEventArgs e)
        {
            switch (drawTool)
            {
                case drawTools.Line:
                    ig.DrawLine(new Pen(foreColor, drawWidth), startPoint, new Point(e.X, e.Y));
                    this.Form1_Paint(this, new PaintEventArgs(this.CreateGraphics(), this.ClientRectangle));
                    break;
                case drawTools.Rectangle:
                    ig.DrawRectangle(new Pen(foreColor, drawWidth), startPoint.X, startPoint.Y, e.X - startPoint.X, e.Y - startPoint.Y);
                    this.Form1_Paint(this, new PaintEventArgs(this.CreateGraphics(), this.ClientRectangle));
                    break;
                case drawTools.Ellipse:
                    ig.DrawEllipse(new Pen(foreColor, drawWidth), startPoint.X, startPoint.Y, e.X - startPoint.X, e.Y - startPoint.Y);
                    this.Form1_Paint(this, new PaintEventArgs(this.CreateGraphics(), this.ClientRectangle));
                    break;
            }

            isDrawing = false;
        }
        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = this.CreateGraphics();
            if (newImage != null)
            {
                g.Clear(Color.White);
                g.DrawImage(newImage, this.ClientRectangle);
            }
        }

        private void MainForm_MouseMove(object sender, MouseEventArgs e)
        {
            Graphics g = this.CreateGraphics();
            if (isDrawing)
            {
                switch (drawTool)
                {
                    case drawTools.None:
                        break;
                    case drawTools.Line:
                        //这部分借鉴了铅笔的代码，但是仍然有bug，需要解决的是：
                        //在画线过程中，新的线轨迹经过已经存在的图，会把黑色的图像给涂抹掉（未解决）
                        //使用了在mouseup中加入刷背景板，可以解决被抹去的图像出现，但是轨迹过程中还是会抹掉图像


                        //this.Form1_Paint(this, new PaintEventArgs(this.CreateGraphics(), this.ClientRectangle));
                        g.DrawLine(new Pen(backColor, drawWidth), startPoint, oldPoint);
                        g.DrawLine(new Pen(foreColor, drawWidth), startPoint, new Point(e.X, e.Y));
                        oldPoint.X = e.X;
                        oldPoint.Y = e.Y;
                        break;
                    case drawTools.pen:
                        g.DrawLine(new Pen(foreColor, drawWidth), oldPoint, new Point(e.X, e.Y));
                        ig.DrawLine(new Pen(foreColor, drawWidth), oldPoint, new Point(e.X, e.Y));
                        oldPoint.X = e.X;
                        oldPoint.Y = e.Y;
                        //SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
                        break;
                    case drawTools.Rectangle:
                        ////this.Form1_Paint(this, new PaintEventArgs(this.CreateGraphics(), this.ClientRectangle));
                        g.DrawRectangle(new Pen(backColor, drawWidth), Math.Min(startPoint.X, oldPoint.X), Math.Min(startPoint.Y, oldPoint.Y), Math.Abs(oldPoint.X - startPoint.X), Math.Abs(oldPoint.Y - startPoint.Y));
                        g.DrawRectangle(new Pen(foreColor, drawWidth), Math.Min(startPoint.X, e.X), Math.Min(startPoint.Y, e.Y), Math.Abs(e.X - startPoint.X), Math.Abs(e.Y - startPoint.Y));
                        oldPoint.X = e.X;
                        oldPoint.Y = e.Y;
                        break;
                    case drawTools.Ellipse:
                        //this.Form1_Paint(this, new PaintEventArgs(this.CreateGraphics(), this.ClientRectangle));
                        g.DrawEllipse(new Pen(backColor, drawWidth), Math.Min(startPoint.X, oldPoint.X), Math.Min(startPoint.Y, oldPoint.Y), Math.Abs(oldPoint.X - startPoint.X), Math.Abs(oldPoint.Y - startPoint.Y));
                        g.DrawEllipse(new Pen(foreColor, drawWidth), Math.Min(startPoint.X, e.X), Math.Min(startPoint.Y, e.Y), Math.Abs(e.X - startPoint.X), Math.Abs(e.Y - startPoint.Y));
                        oldPoint.X = e.X;
                        oldPoint.Y = e.Y;
                        break;
                    case drawTools.String:
                        break;
                    case drawTools.Rubber:
                        //画一个白色的线段，跟铅笔的写法相同
                        g.DrawLine(new Pen(backColor, drawWidth + 5), oldPoint, new Point(e.X, e.Y));
                        ig.DrawLine(new Pen(backColor, drawWidth + 5), oldPoint, new Point(e.X, e.Y));
                        oldPoint.X = e.X;
                        oldPoint.Y = e.Y;
                        break;
                }
            }
        }

        private void 颜色ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                foreColor = colorDialog1.Color;
            }
        }

        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            drawWidth = Convert.ToInt32(trackBar1.Value);
            label1.Text = "线条粗细：" + trackBar1.Value.ToString();
        }

        private void 使用教程ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                // 替换成你的目标URL
                string url = "https://www.betaworld.cn/%E7%94%BB%E5%9B%BE";

                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true  // 必须设置此项以使用系统默认浏览器
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"无法打开链接: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void 清除画布ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Graphics g = this.CreateGraphics();
            g.Clear(backColor);
            newImage = new Bitmap(this.ClientRectangle.Width, this.ClientRectangle.Height);
            ig = Graphics.FromImage(newImage);
            ig.Clear(backColor);
        }
    }
}
