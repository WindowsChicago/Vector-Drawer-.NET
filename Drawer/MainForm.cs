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
        // 图像和绘图
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
        private Bitmap offScreenBitmap; 
        private Graphics offScreenGraphics;

        // 折线相关
        private List<Point> polylinePoints = new List<Point>();
        private bool isPolylineDrawing = false;
        private Point currentPolylinePosition;

        // 多边形点集
        private List<Point> polygonPoints = new List<Point>();
        private bool isPolygonDrawing = false;
        private Point currentPolygonPosition;

        // 添加画布尺寸跟踪
        private Size currentCanvasSize;

        //多线程计时器
        private System.Threading.Timer clockTimer1;

        //矢量命令
        private List<VectorCommand> vectorCommands = new List<VectorCommand>();

        // 绘制工具枚举
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

        // 矢量绘图命令基类
        public abstract class VectorCommand
        {
            public Color Color { get; set; }
            public float LineWidth { get; set; }
            public abstract void Execute(Graphics g);
        }

        // 直线
        public class LineCommand : VectorCommand
        {
            public Point Start { get; set; }
            public Point End { get; set; }

            public override void Execute(Graphics g)
            {
                using (Pen pen = new Pen(Color, LineWidth))
                {

                }
            }
        }

        // 矩形命令（其他形状类似，需补充EllipseCommand、PolylineCommand等）
        public class RectangleCommand : VectorCommand
        {
            public Rectangle Rect { get; set; }

            public override void Execute(Graphics g)
            {
                using (Pen pen = new Pen(Color, LineWidth))
                {
          
                }
            }
        }
        // 椭圆命令
        public class EllipseCommand : VectorCommand
        {
            public Rectangle Bounds { get; set; }

            public override void Execute(Graphics g)
            {
                using (Pen pen = new Pen(Color, LineWidth))
                {
        
                }
            }
        }

        // 折线命令
        public class PolylineCommand : VectorCommand
        {
            public List<Point> Points { get; set; } = new List<Point>();

            public override void Execute(Graphics g)
            {
                using (Pen pen = new Pen(Color, LineWidth))
                {
                    if (Points.Count >= 2)
                    {

                    }
                }
            }
        }
        // 多边形命令
        public class PolygonCommand : VectorCommand
        {
            public List<Point> Points { get; set; } = new List<Point>();

            public override void Execute(Graphics g)
            {
                using (Pen pen = new Pen(Color, LineWidth))
                {
                    if (Points.Count >= 2)
                    {
     
                    }
                }
            }
        }

        // 文本命令
        public class TextCommand : VectorCommand
        {
            public string Text { get; set; }
            public Point Location { get; set; }
            public Font Font { get; set; }

            public override void Execute(Graphics g)
            {
                using (SolidBrush brush = new SolidBrush(Color))
                {
              
                }
            }
        }
        public class EraserCommand : VectorCommand
        {
            public List<Point> Path { get; set; } = new List<Point>();
            public float EraserSize { get; set; }

            public override void Execute(Graphics g)
            {
                // 橡皮擦本质是绘制白色路径
                using (Pen eraserPen = new Pen(Color.White, EraserSize))
                {
                    if (Path.Count >= 2)
                    {
            
                    }
                }
            }
        }


        // 构造函数
        public MainForm()
        {
            InitializeComponent();
            InitializeClock1(); // 时钟初始化
            InitializeComponentExtensions();

            // 初始化离屏位图
            offScreenBitmap = new Bitmap(this.ClientRectangle.Width, this.ClientRectangle.Height);
            offScreenGraphics = Graphics.FromImage(offScreenBitmap);

            // 初始化浮动文本框
            floatingTextBox = new TextBox();
            floatingTextBox.Visible = false;
            floatingTextBox.Multiline = true;
            floatingTextBox.BorderStyle = BorderStyle.None;
            floatingTextBox.BackColor = Color.White;
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

        //初始化时钟
        private void InitializeClock1()
        {
            clockTimer1 = new System.Threading.Timer(ClockTimerCallback, null, 0, 1000);
            timeLabel = new Label
            {
                AutoSize = true,
                ForeColor = Color.Black,
                Font = new Font("Consolas", 12F, FontStyle.Regular),
                BackColor = Color.White
            };
            timeLabel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            // 初始位置
            UpdateClockPosition();
            this.Controls.Add(timeLabel);
        }
        private void UpdateClockPosition()
        {
            int margin = 10; // 边距
            timeLabel.Location = new Point(
                this.ClientSize.Width - timeLabel.Width - margin,
                this.ClientSize.Height - timeLabel.Height - margin
            );
        }
        private void ClockTimerCallback(object state)
        {
            UpdateTimeDisplay();
        }
        private void UpdateTimeDisplay()
        {
            if (this.InvokeRequired) // 检查是否跨线程
            {
                // 使用BeginInvoke异步更新
                this.BeginInvoke(new Action(() =>
                {

                    timeLabel.Text = DateTime.Now.ToString("HH:mm:ss");
                }));
            }
        }

        private void InitializeComponentExtensions()
        {
            this.Resize += (s, e) => ResetCanvas();
            this.DoubleBuffered = true;
            
        }
        private void ResetCanvas()
        {
            // 获取新的窗口客户区尺寸
            var newSize = this.ClientSize;

            // 创建新画布
            if (newSize.Width != 0)
            {
                var newBmp = new Bitmap(newSize.Width, newSize.Height);
                            using (var g = Graphics.FromImage(newBmp))
            {
                g.Clear(backColor);

                // 保留原有内容（左上角对齐）
                if (newImage != null)
                {
                    g.DrawImage(newImage, Point.Empty);
                }
            }

            // 更新画布引用
            if (newImage != null) newImage.Dispose();
            newImage = newBmp;
            ig = Graphics.FromImage(newImage);
            currentCanvasSize = newSize;
            }


            this.Invalidate();
            ForceRedraw();
            Update();

        }

        //菜单栏按钮
        private void 颜色ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                foreColor = colorDialog1.Color;
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
        private void 新建ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //toolStrip1.Enabled = true;
            if (newImage != null)
            {
                newImage.Dispose();
            }
            newImage = new Bitmap(this.ClientRectangle.Width, this.ClientRectangle.Height);
            newFileName = "新建图像.bmp";
            this.Text = "Vector Drawer  " + newFileName;
            ig = Graphics.FromImage(newImage);
            ig.Clear(backColor);

            toolStrip1.Visible = true;
            toolStrip1.Enabled = true;
            trackBar1.Visible = true;
            label1.Visible = true;
            ForceRedraw();
            vectorCommands.Clear(); 
        }
        private void 打开ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog OpenDialog = new OpenFileDialog();
            OpenDialog.Multiselect = false;
            OpenDialog.Title = "打开文件";
            OpenDialog.Filter = "图像文件(*.bmp;*.ico;*.jpg;*.wmf)|*.bmp;*.ico;*.jpg;*.wmf";
            if (OpenDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string ext = Path.GetExtension(OpenDialog.FileName).ToLower();
                if (ext == ".wmf")
                {
                    LoadWmfFile(OpenDialog.FileName);
                }
                else
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
        }
        private void LoadWmfFile(string filePath)
        {
            try
            {
                using (Metafile mf = new Metafile(filePath))
                {
                    newImage = new Bitmap(ClientSize.Width, ClientSize.Height);
                    ig = Graphics.FromImage(newImage);
                    ig.Clear(backColor);

                    // 将WMF绘制到位图上
                    ig.DrawImage(mf, ClientRectangle);

                    this.Text = "Vector Drawer  " + filePath;
                    newFileName = filePath;
                    toolStrip1.Visible = true;
                    this.Invalidate();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("无法打开WMF文件：" + ex.Message);
            }
        }
        private void 保存ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog SaveDialog = new SaveFileDialog();
            SaveDialog.Title = "保存文件";
            SaveDialog.Filter = "BMP位图(*.bmp)|*.bmp|JPEG图像(*.jpg)|*.jpg|PNG图像(*.png)|*.png|WMF矢量图(*.wmf)|*.wmf";
            SaveDialog.FileName = newFileName;

            if (SaveDialog.ShowDialog() == DialogResult.OK)
            {
                string ext = Path.GetExtension(SaveDialog.FileName).ToLower();
                if (ext == ".wmf")
                {
                    SaveAsWmf(SaveDialog.FileName);
                }
                else
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
                MessageBox.Show($"保存成功", "",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            is_save = true;
        }
        private void 另存为ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog SaveDialog = new SaveFileDialog();
            SaveDialog.Title = "另存为";
            SaveDialog.Filter = "BMP位图(*.bmp)|*.bmp|JPEG图像(*.jpg)|*.jpg|PNG图像(*.png)|*.png|WMF矢量图(*.wmf)|*.wmf";
            SaveDialog.FileName = newFileName;

            if (SaveDialog.ShowDialog() == DialogResult.OK)
            {
                string ext = Path.GetExtension(SaveDialog.FileName).ToLower();
                if (ext == ".wmf")
                {
                    SaveAsWmf(SaveDialog.FileName);
                }
                else
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
                MessageBox.Show($"保存成功", "",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            is_save = true;
        }
        private void SaveAsWmf(string filePath)
        {
            try
            {
                using (Graphics screenGraphics = this.CreateGraphics())
                {
                    IntPtr hdc = screenGraphics.GetHdc();
                    using (Metafile mf = new Metafile(filePath, hdc))
                    using (Graphics mfGraphics = Graphics.FromImage(mf))
                    {
                        // 设置高质量渲染
                        mfGraphics.SmoothingMode = SmoothingMode.AntiAlias;
                        mfGraphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

                        // 重绘所有矢量元素
                        RedrawVectorCommands(mfGraphics);
                    }
                    screenGraphics.ReleaseHdc(hdc);
                }
                is_save = true;
                this.Text = "Vector Drawer  " + filePath;
            }
            catch (Exception ex)
            {
                MessageBox.Show("保存WMF失败：" + ex.Message);
            }
        }
        private void RedrawVectorCommands(Graphics g)
        {
            // 执行所有矢量命令
            foreach (var cmd in vectorCommands)
            {
                cmd.Execute(g);
            }

            // 绘制当前图像（兼容位图内容）
            if (newImage != null)
            {
                g.DrawImage(newImage, ClientRectangle);
            }
        }
        private void 退出ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
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
        private void 关于ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Vector Drawer Build 24", "关于本程序", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }




        //工具按钮
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

        //线条粗细
        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            drawWidth = Convert.ToInt32(trackBar1.Value);
            label1.Text = "线条粗细：" + trackBar1.Value.ToString();
        }

        private void 清除画布ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Graphics g = this.CreateGraphics();
            g.Clear(backColor);
            newImage = new Bitmap(this.ClientRectangle.Width, this.ClientRectangle.Height);
            ig = Graphics.FromImage(newImage);
            ig.Clear(backColor);
        }

        //按下鼠标按键
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
                    floatingTextBox.Font = currentTextFont;
                    floatingTextBox.Text = "";
                    floatingTextBox.Visible = true;
                    floatingTextBox.BringToFront();
                    floatingTextBox.Focus();
                    isTextEditing = true;
                }

            }
        }

        //抬起鼠标按键
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
                        vectorCommands.Add(new PolylineCommand
                        {
                            Color = foreColor,
                            LineWidth = drawWidth,
                            Points = new List<Point>(polylinePoints)
                        });
                    }
                }

                // 重置状态
                polylinePoints.Clear();
                isPolylineDrawing = false;
                //this.Invalidate();
            }
            if (drawTool == drawTools.Polygon && e.Button == MouseButtons.Left)
            {

                // 绘制到正式图像
                if (polygonPoints.Count >= 2)
                {
                    for (int i = 0; i < polygonPoints.Count - 1; i++)
                    {
                        ig.DrawLine(new Pen(foreColor, drawWidth),
                                  polygonPoints[i], polygonPoints[i + 1]);
                        vectorCommands.Add(new PolygonCommand
                        {
                            Color = foreColor,
                            LineWidth = drawWidth,
                            Points = new List<Point>(polygonPoints)
                        });
                    }
                }
                // 完成最终线段
                polygonPoints.Add(e.Location);

                // 重置状态
                polygonPoints.Add(polygonPoints[0]); // 闭合路径
                ig.DrawPolygon(new Pen(foreColor, drawWidth), polygonPoints.ToArray());
                vectorCommands.Add(new PolygonCommand
                {
                    Color = foreColor,
                    LineWidth = drawWidth,
                    Points = new List<Point>(polygonPoints)
                });
                
                polygonPoints.Clear();
                isPolygonDrawing = false;
                ForceRedraw();
                //this.Invalidate();
            }
           
                switch (drawTool)
                {
                case drawTools.Line:
                    ig.DrawLine(new Pen(foreColor, drawWidth), startPoint, new Point(e.X, e.Y));
                    // 记录矢量命令
                    vectorCommands.Add(new LineCommand
                    {
                        Color = foreColor,
                        LineWidth = drawWidth,
                        Start = startPoint,
                        End = new Point(e.X, e.Y)
                    });
                    break;
                case drawTools.Rectangle:
                    var rect = new Rectangle(
                        Math.Min(startPoint.X, e.X),
                        Math.Min(startPoint.Y, e.Y),
                        Math.Abs(e.X - startPoint.X),
                        Math.Abs(e.Y - startPoint.Y));
                    ig.DrawRectangle(new Pen(foreColor, drawWidth), rect);
                    vectorCommands.Add(new RectangleCommand
                    {
                        Color = foreColor,
                        LineWidth = drawWidth,
                        Rect = rect
                    });
                    break;
                case drawTools.Ellipse:
                    var ellipseRect = new Rectangle(
                        Math.Min(startPoint.X, e.X),
                        Math.Min(startPoint.Y, e.Y),
                        Math.Abs(e.X - startPoint.X),
                        Math.Abs(e.Y - startPoint.Y));
                    ig.DrawEllipse(new Pen(foreColor, drawWidth), ellipseRect);
                    vectorCommands.Add(new EllipseCommand
                    {
                        Color = foreColor,
                        LineWidth = drawWidth,
                        Bounds = ellipseRect
                    });
                    break;
                // 折线工具
                case drawTools.Polyline:
                    if (polylinePoints.Count >= 2)
                    {
                        vectorCommands.Add(new PolylineCommand
                        {
                            Color = foreColor,
                            LineWidth = drawWidth,
                            Points = new List<Point>(polylinePoints)
                        });
                    }
                    break;

                // 多边形工具
                case drawTools.Polygon:
                    if (polygonPoints.Count >= 2)
                    {
                        vectorCommands.Add(new PolygonCommand
                        {
                            Color = foreColor,
                            LineWidth = drawWidth,
                            Points = new List<Point>(polygonPoints)
                        });
                    }
                    break;

                // 文本工具
                case drawTools.String:
                    if (!string.IsNullOrEmpty(floatingTextBox.Text))
                    {
                        vectorCommands.Add(new TextCommand
                        {
                            Color = currentTextColor, // 注意使用文本颜色
                            Text = floatingTextBox.Text,
                            Location = floatingTextBox.Location,
                            Font = currentTextFont
                        });
                    }
                    break;
            }

            isDrawing = false;
        }


        // 移动鼠标
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
                            g.DrawLine(new Pen(backColor, drawWidth + 5), oldPoint, new Point(e.X, e.Y));
                            ig.DrawLine(new Pen(backColor, drawWidth + 5), oldPoint, new Point(e.X, e.Y));

                            var lastCommand = vectorCommands.LastOrDefault() as EraserCommand;
                            if (lastCommand == null)
                            {
                                lastCommand = new EraserCommand
                                {
                                    Color = this.backColor, // 使用背景色
                                    LineWidth = drawWidth + 5,
                                    Path = new List<Point> { oldPoint }
                                };
                                vectorCommands.Add(lastCommand);
                            }
                            lastCommand.Path.Add(e.Location);
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

        //文本绘制相关工具
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

                Update(); 
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

        //强制重绘减少闪烁
        private void ForceRedraw()
        {
            this.Update();

            // 如果使用双缓冲，可能需要额外处理
            if (this.DoubleBuffered)
            {
                using (Graphics g = this.CreateGraphics())
                {
                    redraw(this, new PaintEventArgs(g, this.ClientRectangle));
                }
            }
        }
        private void redraw(object sender, PaintEventArgs e)
        {
            Graphics g = this.CreateGraphics();
            if (newImage != null)
            {
                // 先绘制到离屏位图
                offScreenGraphics.Clear(backColor);
                offScreenGraphics.DrawImage(newImage, this.ClientRectangle);
                // 一次性绘制到屏幕
                e.Graphics.DrawImage(offScreenBitmap, 0, 0);
                // 使用统一的重绘方法
                RedrawVectorCommands(e.Graphics);

            }
        }

        // 在主窗体中重写OnFormClosing
        protected override void OnFormClosing(FormClosingEventArgs e)
        {

            if (is_save == false)
            {
                if (MessageBox.Show("文件尚未保存，是否退出程序", "提示", MessageBoxButtons.YesNo) != DialogResult.Yes)
                {
                    e.Cancel = true;
                }
                base.OnFormClosing(e);

            }

        }

    }
}
