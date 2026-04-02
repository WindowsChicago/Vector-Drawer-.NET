using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Drawing.Imaging;
using System.Net;
using System.Threading.Tasks;


namespace Drawer
{
    public partial class LoginForm : Form
    {
        public LoginForm()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string username = textUserName.Text; 
            string password = textPassWD.Text; // 验证用户名和密码 
            string[] lines = File.ReadAllLines("data.bin"); // 读取文件中的用户信息 
            if (lines.Contains(username + " " + password)) 
            { 
              this.Hide(); // 隐藏当前窗体 
              this.Close(); // 关闭当前窗体
            } 
            else 
            {
                MessageBox.Show("用户名或密码错误", "登录失败", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            System.Environment.Exit(0);
           
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                textPassWD.PasswordChar = char.MinValue;
            }
            else
            {
                textPassWD.PasswordChar = '*';
            }
        }
    }
}
