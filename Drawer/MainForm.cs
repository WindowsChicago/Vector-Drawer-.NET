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
using System.IO;
using System.Drawing.Drawing2D;


namespace Drawer
{
    public partial class MainForm : Form
    {
        // 图像和绘图相关
        private Image newImage;
        private Color backColor = Color.White;
        private Color foreColor = Color.Black;
        private string newFileName;
        private Graphics ig;

        // 绘图工具和状态
        private bool isDrawing;
        private Point startPoint;
        private Point oldPoint;
        private drawTools drawTool = drawTools.None;
        private int drawWidth = 1;
        private bool is_save = false;

        //文本框
        private TextBox floatingTextBox;
        private bool isTextEditing = false;
        private Point textBoxDragStart;
        private bool isDraggingTextBox = false;
        private Font currentTextFont = new Font("Arial", 12);
        private Color currentTextColor = Color.Black;

        //离屏缓冲
        private Bitmap offScreenBitmap; // 离屏位图
        private Graphics offScreenGraphics; // 离屏Graphics对象

        // 折线相关状态
        private List<Point> polylinePoints = new List<Point>();
        private bool isPolylineDrawing = false;
        private Point currentPolylinePosition;

        // 添加多边形专用点列表
        private List<Point> polygonPoints = new List<Point>();
        private bool isPolygonDrawing = false;
        private Point currentPolygonPosition;

        private float zoomFactor = 1.0f;          // 当前缩放比例
        private Size originalCanvasSize;          // 原始画布尺寸
        private PointF scrollOffset = PointF.Empty; // 滚动偏移量
        private bool enableScaling = true;        // 缩放功能开关

        // 工具枚举
        private enum drawTools
        {
            pen = 0,
            Line,
            Ellipse,
            Rectangle,
            String,
            Rubber,
            Polyline,
            Polygon,  
            None
        };

        // 在主窗体中重写OnFormClosing
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if(is_save == false)
            {
                if (MessageBox.Show("文件尚未保存，是否退出程序", "提示",MessageBoxButtons.YesNo) != DialogResult.Yes)
                {
                    e.Cancel = true;
                }
                base.OnFormClosing(e);

            }

        }
        // 构造函数
        public MainForm()
        {
            InitializeComponent();
            InitializeClock();

            // 初始化离屏位图
            offScreenBitmap = new Bitmap(this.ClientRectangle.Width, this.ClientRectangle.Height);
            offScreenGraphics = Graphics.FromImage(offScreenBitmap);

            // 初始化浮动文本框
            floatingTextBox = new TextBox();
            floatingTextBox.Visible = false;
            floatingTextBox.Multiline = true;
            floatingTextBox.BorderStyle = BorderStyle.None;
            floatingTextBox.BackColor = Color.LightYellow;
            floatingTextBox.KeyDown += FloatingTextBox_KeyDown;
            floatingTextBox.MouseDown += FloatingTextBox_MouseDown;
            floatingTextBox.MouseMove += FloatingTextBox_MouseMove;
            floatingTextBox.MouseUp += FloatingTextBox_MouseUp;
            this.Controls.Add(floatingTextBox);

            this.DoubleBuffered = true;
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer |
                         ControlStyles.UserPaint |
                         ControlStyles.AllPaintingInWmPaint, true);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            //lblTime.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"); // 更新时间标签
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
            SaveDialog.Filter = "BMP位图(*.bmp)|*.bmp|JPEG图像(*.jpg)|*.jpg|PNG图像(*.png)|*.png";
            SaveDialog.FileName = newFileName;

            if (SaveDialog.ShowDialog() == DialogResult.OK)
            {
                ImageFormat format = ImageFormat.Bmp;
                switch (Path.GetExtension(SaveDialog.FileName).ToLower())
                {
                    case ".jpg": format = ImageFormat.Jpeg; break;
                    case ".png": format = ImageFormat.Png; break;
                }

                newImage.Save(SaveDialog.FileName, format);
                this.Text = "Vector Drawer  " + SaveDialog.FileName;
                newFileName = SaveDialog.FileName;
            }
            is_save = true;
        }

        private void 退出ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void 关于ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Vector Drawer Build 18", "关于本程序", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
            SaveDialog.Title = "另存为";
            SaveDialog.Filter = "BMP位图(*.bmp)|*.bmp|JPEG图像(*.jpg)|*.jpg|PNG图像(*.png)|*.png";
            SaveDialog.FileName = newFileName;

            if (SaveDialog.ShowDialog() == DialogResult.OK)
            {
                ImageFormat format = ImageFormat.Bmp;
                switch (Path.GetExtension(SaveDialog.FileName).ToLower())
                {
                    case ".jpg": format = ImageFormat.Jpeg; break;
                    case ".png": format = ImageFormat.Png; break;
                }

                newImage.Save(SaveDialog.FileName, format);
                this.Text = "Vector Drawer  " + SaveDialog.FileName;
                newFileName = SaveDialog.FileName;
            }
            is_save = true;
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
        private void toolStripButton7_Click(object sender, EventArgs e)
        {
            drawTool = drawTools.Polyline;
        }
        private void toolStripButton8_Click(object sender, EventArgs e)
        {
            drawTool = drawTools.Polygon;
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = this.CreateGraphics();
            if (newImage != null)
            {
                //g.Clear(Color.White);
                //g.DrawImage(newImage, this.ClientRectangle);
                // 先绘制到离屏位图
                offScreenGraphics.Clear(backColor);
                offScreenGraphics.DrawImage(newImage, this.ClientRectangle);

                // 一次性绘制到屏幕
                e.Graphics.DrawImage(offScreenBitmap, 0, 0);

            }
        }
        private void MainForm_MouseDown(object sender, MouseEventArgs e)
        {
            if (drawTool == drawTools.Polyline)
            {
                if (e.Button == MouseButtons.Left)
                {
                    if (!isPolylineDrawing) // 开始新折线
                    {
                        polylinePoints.Clear();
                        polylinePoints.Add(e.Location);
                        isPolylineDrawing = true;
                        currentPolylinePosition = e.Location;
                    }
                }
                else if (e.Button == MouseButtons.Right && isPolylineDrawing) // 右键切换起点
                {
                    polylinePoints.Add(currentPolylinePosition);
                    startPoint = currentPolylinePosition; // 更新起始点
                }
            }
            if (drawTool == drawTools.Polygon)
            {
                if (e.Button == MouseButtons.Left)
                {
                    if (!isPolygonDrawing) // 开始新折线
                    {
                        polygonPoints.Clear();
                        polygonPoints.Add(e.Location);
                        isPolygonDrawing = true;
                        currentPolygonPosition = e.Location;
                    }
                }
                else if (e.Button == MouseButtons.Right && isPolygonDrawing) // 右键切换起点
                {
                    polygonPoints.Add(currentPolygonPosition);
                    startPoint = currentPolygonPosition; // 更新起始点
                }
            }
            if (e.Button == MouseButtons.Left && drawTool != drawTools.None)
            {

                isDrawing = true;
                if (e.Button == MouseButtons.Left)
                {
                    startPoint = new Point(e.X, e.Y);
                    oldPoint = new Point(e.X, e.Y);
                }
                if (drawTool == drawTools.String && !isTextEditing)
                {
                    // 创建新的文本框
                    floatingTextBox.Location = e.Location;
                    floatingTextBox.Size = new Size(200, 100);
                    floatingTextBox.Font = new Font("Arial", 12);
                    floatingTextBox.Text = "";
                    floatingTextBox.Visible = true;
                    floatingTextBox.BringToFront();
                    floatingTextBox.Focus();
                    isTextEditing = true;
                }

            }
        }

        private void MainForm_MouseUp(object sender, MouseEventArgs e)
        {
            if (drawTool == drawTools.Polyline && e.Button == MouseButtons.Left)
            {
                // 完成最终线段
                polylinePoints.Add(e.Location);

                // 绘制到正式图像
                if (polylinePoints.Count >= 2)
                {
                    for (int i = 0; i < polylinePoints.Count - 1; i++)
                    {
                        ig.DrawLine(new Pen(foreColor, drawWidth),
                                  polylinePoints[i], polylinePoints[i + 1]);
                    }
                }

                // 重置状态
                polylinePoints.Clear();
                isPolylineDrawing = false;
                //this.Invalidate();
            }
            if (drawTool == drawTools.Polygon && e.Button == MouseButtons.Left)
            {
                // 完成最终线段
                polygonPoints.Add(e.Location);

                // 绘制到正式图像
                if (polygonPoints.Count >= 2)
                {
                    for (int i = 0; i < polygonPoints.Count - 1; i++)
                    {
                        ig.DrawLine(new Pen(foreColor, drawWidth),
                                  polygonPoints[i], polygonPoints[i + 1]);
                    }
                }

                // 重置状态
                polygonPoints.Add(polygonPoints[0]); // 闭合路径
                ig.DrawPolygon(new Pen(foreColor, drawWidth), polygonPoints.ToArray());
                ForceRedraw();
                polygonPoints.Clear();
                isPolygonDrawing = false;
                //this.Invalidate();
            }

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

        // 修改绘图逻辑，使用临时位图进行绘制
        private void MainForm_MouseMove(object sender, MouseEventArgs e)
        {
            if (drawTool == drawTools.Polyline && isPolylineDrawing)
            {
                // 创建临时位图用于预览
                using (var tempBitmap = new Bitmap(ClientRectangle.Width, ClientRectangle.Height))
                using (var tempGraphics = Graphics.FromImage(tempBitmap))
                {
                    tempGraphics.DrawImage(newImage, ClientRectangle);

                    // 绘制已确定的线段
                    if (polylinePoints.Count >= 2)
                    {
                        for (int i = 0; i < polylinePoints.Count - 1; i++)
                        {
                            tempGraphics.DrawLine(new Pen(foreColor, drawWidth),
                                               polylinePoints[i], polylinePoints[i + 1]);
                        }
                    }

                    // 绘制当前预览线段
                    tempGraphics.DrawLine(new Pen(foreColor, drawWidth),
                                       startPoint, e.Location);

                    this.CreateGraphics().DrawImage(tempBitmap, ClientRectangle);
                    this.Update();
                }
                currentPolylinePosition = e.Location;

            }
            if (drawTool == drawTools.Polygon && isPolygonDrawing)
            {
                // 创建临时位图用于预览
                using (var tempBitmap = new Bitmap(ClientRectangle.Width, ClientRectangle.Height))
                using (var tempGraphics = Graphics.FromImage(tempBitmap))
                {
                    tempGraphics.DrawImage(newImage, ClientRectangle);

                    // 绘制已确定的线段
                    if (polygonPoints.Count >= 2)
                    {
                        for (int i = 0; i < polygonPoints.Count - 1; i++)
                        {
                            tempGraphics.DrawLine(new Pen(foreColor, drawWidth),
                                               polygonPoints[i], polygonPoints[i + 1]);
                        }
                    }

                    // 绘制当前预览线段
                    tempGraphics.DrawLine(new Pen(foreColor, drawWidth),
                                       startPoint, e.Location);

                    this.CreateGraphics().DrawImage(tempBitmap, ClientRectangle);
                    this.Update();
                }
                currentPolygonPosition = e.Location;

            }

            Graphics g = this.CreateGraphics();
            if (drawTool != drawTools.Polygon && drawTool != drawTools.Polyline && isDrawing && newImage != null)
            {
                // 先绘制当前图像到离屏位图
                offScreenGraphics.Clear(backColor);
                offScreenGraphics.DrawImage(newImage, this.ClientRectangle);
                // 创建临时位图用于绘制预览
                using (var tempBitmap = new Bitmap(this.ClientRectangle.Width, this.ClientRectangle.Height))
                using (var tempGraphics = Graphics.FromImage(tempBitmap))
                {
                    // 先绘制当前图像
                    tempGraphics.DrawImage(newImage, this.ClientRectangle);

                    // 根据当前工具绘制预览
                    switch (drawTool)
                    {
                        case drawTools.None:
                            break;
                        case drawTools.pen:
                            g.DrawLine(new Pen(foreColor, drawWidth), oldPoint, new Point(e.X, e.Y));
                            ig.DrawLine(new Pen(foreColor, drawWidth), oldPoint, new Point(e.X, e.Y));
                            //oldPoint.X = e.X;
                            //oldPoint.Y = e.Y;
                            //SetStyle(ControlStyles.OptimizedDoubleBuffer, true);
                            break;
                        case drawTools.Line:
                            tempGraphics.DrawLine(new Pen(foreColor, drawWidth), startPoint, new Point(e.X, e.Y));
                            break;
                        case drawTools.Rectangle:
                            tempGraphics.DrawRectangle(new Pen(foreColor, drawWidth),
                                Math.Min(startPoint.X, e.X), Math.Min(startPoint.Y, e.Y),
                                Math.Abs(e.X - startPoint.X), Math.Abs(e.Y - startPoint.Y));
                            break;
                        case drawTools.Ellipse:
                            tempGraphics.DrawEllipse(new Pen(foreColor, drawWidth),
                                Math.Min(startPoint.X, e.X), Math.Min(startPoint.Y, e.Y),
                                Math.Abs(e.X - startPoint.X), Math.Abs(e.Y - startPoint.Y));
                            break;
                        case drawTools.String:
                            break;
                        case drawTools.Polyline:
                            break;
                        case drawTools.Polygon:
                            break;
                        case drawTools.Rubber:
                            //画一个白色的线段，跟铅笔的写法相同
                            g.DrawLine(new Pen(backColor, drawWidth + 5), oldPoint, new Point(e.X, e.Y));
                            ig.DrawLine(new Pen(backColor, drawWidth + 5), oldPoint, new Point(e.X, e.Y));
                            //oldPoint.X = e.X;
                            //oldPoint.Y = e.Y;
                            break;
                    }

                    // 一次性绘制到屏幕上
                    this.CreateGraphics().DrawImage(tempBitmap, this.ClientRectangle);
                }
                // 强制重绘
                this.Update();

                oldPoint = new Point(e.X, e.Y);
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
                string url = "https://www.betaworld.cn/%E7%94%BB%E5%9B%BE";

                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true 
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


        private void FloatingTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter && !e.Shift)
            {
                // 隐藏文本框
                floatingTextBox.Visible = false;
                isTextEditing = false;

                // 将文本绘制到图像上
                using (SolidBrush brush = new SolidBrush(foreColor))
                {
                    ig.DrawString(floatingTextBox.Text, currentTextFont, brush,
                                 floatingTextBox.Left, floatingTextBox.Top);
                }

                Update(); // 强制立即重绘
                // 确保界面更新
                ForceRedraw();

                // 将焦点返回给主窗体
                this.Focus();

                e.Handled = true;
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                // ESC键取消文本输入
                floatingTextBox.Visible = false;
                isTextEditing = false;

                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void FloatingTextBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isDraggingTextBox = true;
                textBoxDragStart = e.Location;
            }
        }

        private void FloatingTextBox_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDraggingTextBox)
            {
                floatingTextBox.Left += e.X - textBoxDragStart.X;
                floatingTextBox.Top += e.Y - textBoxDragStart.Y;
            }
        }

        private void FloatingTextBox_MouseUp(object sender, MouseEventArgs e)
        {
            isDraggingTextBox = false;
        }
        private void ForceRedraw()
        {
            this.Update();

            // 如果使用双缓冲，可能需要额外处理
            if (this.DoubleBuffered)
            {
                using (Graphics g = this.CreateGraphics())
                {
                    Form1_Paint(this, new PaintEventArgs(g, this.ClientRectangle));
                }
            }
        }

        private void 字体ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FontDialog fontDialog = new FontDialog();
            fontDialog.Font = currentTextFont;
            if (fontDialog.ShowDialog() == DialogResult.OK)
            {
                currentTextFont = fontDialog.Font;
                if (floatingTextBox.Visible)
                {
                    floatingTextBox.Font = currentTextFont;
                }
            }
        }

    }
}
