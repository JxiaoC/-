using System;
using System.Windows.Forms;

namespace KeyboardDclickFix {
    public partial class Form1 : Form {
        public Form1() {
            InitializeComponent();
        }
        KeyboardHook hook = new KeyboardHook();

        private void Form1_Load(object sender, EventArgs e) {
            hook.OnKeyDownEvent += new KeyEventHandler(OnKeyDownEvent);
        }
        void OnKeyDownEvent(object sender, KeyEventArgs e) {
            //在这里就可以截获到所有的键盘按键了     
            if (textBox1.Text.Length > 10000) textBox1.Text = "";
            textBox1.Text = "已阻止意外的按键输入:" + e.KeyCode.ToString() + "\r\n" + textBox1.Text;
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e) {
            Show();
            Activate();
            WindowState = FormWindowState.Normal;
        }

        private void Form1_SizeChanged(object sender, EventArgs e) {
            if (WindowState == FormWindowState.Minimized) {
                Hide();
            }
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e) {
            hook.SetIntervalTime((int)numericUpDown1.Value);
        }
    }
}
