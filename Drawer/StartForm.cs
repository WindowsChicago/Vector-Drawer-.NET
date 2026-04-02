using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Drawer
{
    public partial class StartForm : Form
    {
        public StartForm()
        {
            InitializeComponent();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            int i = 100;
            progressBar1.Value = progressBar1.Value + 1;
            i = 100 - progressBar1.Value;
            if (i == 0)
            {
                timer1.Enabled = false;
                this.Hide(); // 隐藏当前开始界面    
               LoginForm loginForm = new LoginForm(); // 创建登录界面实例 
               loginForm.ShowDialog(); // 显示登录界面 
                this.Close(); // 关闭当前窗体

            }

        }
    }
}
