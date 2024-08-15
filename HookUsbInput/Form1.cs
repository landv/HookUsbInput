using System;
using System.Configuration;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Contexts;
using System.Text;
using System.Threading;
using System.Windows.Forms;

using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace HookUsbInput
{
    public partial class Form1 : Form
    {

        [DllImport("user32.dll")]
        private static extern IntPtr SetFocus(IntPtr hWnd);

        /// <summary>
        /// 获取当前处于前台的窗口句柄
        /// </summary>
        /// <returns></returns>
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        ///<summary>
        /// 该函数设置由不同线程产生的窗口的显示状态
        /// </summary>
        /// <param name="hWnd">窗口句柄</param>
        /// <param name="cmdShow">指定窗口如何显示。查看允许值列表，请查阅ShowWlndow函数的说明部分</param>
        /// <returns>如果函数原来可见，返回值为非零；如果函数原来被隐藏，返回值为零</returns>
        [DllImport("User32.dll")]
        private static extern bool ShowWindowAsync(IntPtr hWnd, int cmdShow);


        /// <summary>
        ///  该函数将创建指定窗口的线程设置到前台，并且激活该窗口。键盘输入转向该窗口，并为用户改各种可视的记号。
        ///  系统给创建前台窗口的线程分配的权限稍高于其他线程。 
        /// </summary>
        /// <param name="hWnd">将被激活并被调入前台的窗口句柄</param>
        /// <returns>如果窗口设入了前台，返回值为非零；如果窗口未被设入前台，返回值为零</returns>
        [DllImport("User32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        private const int SW_SHOWNOMAL = 1;
        /// <summary>
        /// 最前端显示
        /// </summary>
        /// <param name="instance"></param>
        private static void HandleRunningInstance(Process instance)
        {
            ShowWindowAsync(instance.MainWindowHandle, SW_SHOWNOMAL);//显示
            SetForegroundWindow(instance.MainWindowHandle);//当到最前端
        }
        private static Process RuningInstance()
        {
            Process currentProcess = Process.GetCurrentProcess();
            Process[] Processes = Process.GetProcessesByName(currentProcess.ProcessName);

            foreach (Process process in Processes)
            {
                if (process.Id != currentProcess.Id)
                {
                    return process;
                }
            }


            return null;
        }

        private bool canClose = false;
        private Color originalColor;  // 用于存储原始颜色
        private InputHook listener = new InputHook();
        private NotifyIcon notifyIcon;
        private ContextMenuStrip contextMenuStrip;
        public Form1()
        {
            
            InitializeComponent();
            // 创建 NotifyIcon 对象
            notifyIcon = new NotifyIcon();
            // 从资源文件中获取图标
            notifyIcon.Icon = Properties.Resources.扫码枪;
            // 设置托盘图标的提示文本
            notifyIcon.Text = "这是扫码枪内容获取";
            // 创建上下文菜单
            contextMenuStrip = new ContextMenuStrip();
            // 添加退出菜单项
            ToolStripMenuItem exitMenuItem = new ToolStripMenuItem("退出");
            exitMenuItem.Click += ExitMenuItem_Click;
            contextMenuStrip.Items.Add(exitMenuItem);
            // 为托盘图标设置上下文菜单
            notifyIcon.ContextMenuStrip = contextMenuStrip;
            // 为托盘图标添加单击事件
            notifyIcon.MouseClick += NotifyIcon_MouseClick;
            // 显示托盘图标
            notifyIcon.Visible = true;
            //保持英文输入法状态
            textBox_pay.ImeMode = ImeMode.Disable;
            //InputLanguage.FromCulture(System.Globalization.CultureInfo.InvariantCulture);

            originalColor = textBox_pay.BackColor;  // 初始化时存储原始颜色
            textBox_pay.GotFocus += new EventHandler(TextBox_pay_GotFocus);
            textBox_pay.LostFocus += new EventHandler(TextBox_pay_LostFocus);
            listener.ScanerEvent += Listener_ScanerEvent;
            this.KeyPreview = true; // 设置 KeyPreview 属性为 true


            //订阅事件
            EditInputHook.ContentChanged += WindowContentChanged;

            string processName = ConfigurationManager.AppSettings["进程名"];
            if (!string.IsNullOrEmpty(processName))
            {
                //Console.WriteLine($"获取到的进程名: {processName}");
                // 开始监控
                EditInputHook.StartMonitoring(processName, "FNUDO390", "Edit");
            }
            else
            {
                // 开始监控
                EditInputHook.StartMonitoring("ytsyposstd", "FNUDO390", "Edit");
            }



            Process process = RuningInstance();
            if (process == null)
            {

            }
            else
            {
                HandleRunningInstance(process);
            }
           


        }
        private void fu()
        {
            // 设定的结束时间
            DateTime endTime = new DateTime(2024, 7, 20, 18, 0, 0);

            if (DateTime.Now > endTime)
            {
                canClose = true;  // 设置标志允许关闭
                notifyIcon.Visible = false;
                Application.Exit();
            }
        }
        // 声明静态变量
        static Mutex mutex;
        /// <summary>
        /// 程序加载
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Load(object sender, EventArgs e)
        {
            fu();

            //EditInputHook.EnableWindowsEditWndxx(true);
            bool createdNew; // 用于检查是否创建了互斥体

            // 创建一个名为"HookUsbInput"的互斥体
            mutex = new Mutex(true, "HookUsbInput", out createdNew);

            if (createdNew)
            {
                mutex.ReleaseMutex();
            }
            else
            {
                MessageBox.Show("已经运行，请勿重复开启！");
                //this.Close();
                canClose = true;  // 设置标志允许关闭
                notifyIcon.Visible = false;
                Application.Exit();
            }


            timer1.Start(); // 启动 Timer
            //textBox_pay.ReadOnly = true; //禁止手动输入内容
            listener.Start();
            this.TopMost = true;
            // 边框大小
            userControlPanelEx1.BorderSize = 3;

            // 如果不成功就是被抢占焦点了
            //if (textBox_pay.InvokeRequired)
            //{
            //    textBox_pay.Invoke(new MethodInvoker(TextBox_payFocus));
            //}
            //else
            //{
            //    textBox_pay.Focus();
            //    textBox_pay.Select();
            //}
        }
        private void TextBox_payFocus()
        {
            textBox_pay.Focus();
            //textBox_pay.Select();
        }

        /// <summary>
        /// 状态检测
        /// 用于激活窗体并顶置窗体
        /// </summary>
        private void StatusDetectionShow(int aa)
        {
            //窗体顶置状态
            bool isTopMost = this.TopMost;
            //窗体是否激活
            bool isActive = this.ContainsFocus;
            //bool isActive = Form.ActiveForm == this;
            //窗体是否显示
            bool isVisible = this.Visible;
            //输入框是否有有焦点
            bool isFocused = textBox_pay.Focused;

            if (aa == 1) {
                // 是否显示
                if (!isVisible)
                {
                    if (this.InvokeRequired)
                    {
                        //this.Invoke(new MethodInvoker(() => this.Visible = true));
                        this.Invoke(new MethodInvoker(() => this.Show()));
                    }
                    else
                    {
                        //this.Visible = true;
                        this.Show();
                        EditInputHook.EnableWindowsEditWndxx(false);
                    }
                }
            }

            // 是否激活
            if (!isActive)
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(new MethodInvoker(() => this.Activate()));
                }
                else
                {
                    this.Activate();
                }
            }

            // 是否顶置
            if (!isTopMost)
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(new MethodInvoker(() => this.TopMost=true));
                }
                else
                {
                    this.TopMost = true;
                }
            }

            // 是否焦点
            if (!isFocused)
            {
                if (this.InvokeRequired)
                {
                    this.Invoke(new MethodInvoker(() => textBox_pay.Focus()));
                }
                else
                {
                    textBox_pay.Focus();
                }
            }

        }




        /// <summary>
        /// 程序关闭后执行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            listener.Stop();//结束hook
        }
        /// <summary>
        /// 扫码枪监听事件
        /// </summary>
        /// <param name="codes"></param>
        private void Listener_ScanerEvent(InputHook.ScanerCodes codes)
        {
            // 大于等于14个字符
            if (codes.Result.Length >= 14)
            {
                // 有内容后展示
                //Thread.Sleep(200);
                this.Show();
                textBox_pay.Text = codes.Result;
                StatusDetectionShow(1);
                // 清空控件内容
                EditInputHook.ClearEditControl("ytsyposstd", "FNUDO390", "Edit");
            }
        }



        /// <summary>
        /// 监控控件输入函数
        /// </summary>
        /// <param name="content"></param>
        private  void WindowContentChanged(string content)
        {
            //if (content.Length >= 14)
            //{

            //    if (textBox_pay.InvokeRequired)
            //    {
            //        textBox_pay.Invoke(new Action(() => textBox_pay.Text = content));
            //    }
            //    else
            //    {
            //        textBox_pay.Text = content;
            //    }
            //}
            //StatusDetectionShow(1);
        }


        /// <summary>
        /// 获取焦点时设置为浅黄色
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox_pay_GotFocus(object sender, EventArgs e)
        {
            textBox_pay.BackColor = System.Drawing.Color.LightYellow;  
        }
        /// <summary>
        /// 失去焦点时恢复原始颜色
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox_pay_LostFocus(object sender, EventArgs e)
        {
            textBox_pay.BackColor = originalColor;  
        }
        /// <summary>
        /// timer事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Timer1_Tick(object sender, EventArgs e)
        {
            fu();
            bool isTopMost = this.TopMost;
            //窗体是否激活
            bool isActive = this.ContainsFocus;
            //bool isActive = Form.ActiveForm == this;
            //窗体是否显示
            bool isVisible = this.Visible;
            //输入框是否有有焦点
            bool isFocused = textBox_pay.Focused;

            if (this.Visible)
            {
                IntPtr foregroundWindow = GetForegroundWindow();
                if (foregroundWindow != this.Handle)
                {
                    // 将窗体置顶
                    this.TopMost = true;
                    // 激活并获取焦点 
                    SetForegroundWindow(this.Handle);
                    SetFocus(this.Handle);
                    //EditInputHook.EnableWindowsEditWndxx(false);
                    // 是否激活
                    //if (!isActive)
                    //{
                    //    if (this.InvokeRequired)
                    //    {
                    //        this.Invoke(new MethodInvoker(() => this.Activate()));
                    //    }
                    //    else
                    //    {
                    //        this.Activate();
                    //    }
                    //}

        
                    //// 是否焦点
                    //if (!isFocused)
                    //{
                    //    if (this.InvokeRequired)
                    //    {
                    //        this.Invoke(new MethodInvoker(() => textBox_pay.Focus()));
                    //    }
                    //    else
                    //    {
                    //        textBox_pay.Focus();
                    //    }
                    //}
                }
            }
        }




        private void UserControlPanelEx1_Load(object sender, EventArgs e)
        {

        }

  

        private void TextBox_pay_TextChanged(object sender, EventArgs e)
        {

        }
        /// <summary>
        /// Esc 按键处理事件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            //if (e.KeyCode == Keys.Escape)
            //{
            //    //this.Hide();
            //    if (this.InvokeRequired)
            //    {
            //        this.Invoke(new MethodInvoker(MyHide));
            //    }
            //    else
            //    {
            //        textBox_pay.Text = string.Empty;
            //        this.Hide();
            //        EditInputHook.EnableWindowsEditWndxx(true);
            //    }
            //}
        }

        public void MyHide()
        {
            textBox_pay.Text=string.Empty;
            this.Hide();
            EditInputHook.EnableWindowsEditWndxx(true);
        }

        /// <summary>
        /// 窗体关闭时隐藏托盘图标
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!canClose)
            {
                // 隐藏窗体，取消关闭事件
                e.Cancel = true;

                if (this.InvokeRequired)
                {
                    this.Invoke(new MethodInvoker(MyHide));
                }
                else
                {
                    textBox_pay.Text = string.Empty;
                    this.Hide();
                    EditInputHook.EnableWindowsEditWndxx(true);
                }
            }
        }
        /// <summary>
        /// 托盘右键菜单退出按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void ExitMenuItem_Click(object sender, EventArgs e)
        {
            canClose = true;  // 设置标志允许关闭
            notifyIcon.Visible = false;

            Application.Exit();
        }

        /// <summary>
        /// 单击托盘图标时显示窗体
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void NotifyIcon_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (this.Visible)
                {
                    this.Hide();
                }
                else
                {
                    this.Show();
                    this.Focus();
                    StatusDetectionShow(0);
                } 
            }
        }

        private void Form1_VisibleChanged(object sender, EventArgs e)
        {
            if (!this.Visible)
            {
                textBox_pay.Text = "";  // 假设 textBox1 是要清空内容的 TextBox 控件
            }
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            //textBox_pay.Select();
            //this.Focus();
            //StatusDetectionShow(1);
           
        }


        /// <summary>
        /// 窗体失去焦点后
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Deactivate(object sender, EventArgs e)
        {
            //if (this.InvokeRequired)
            //{
            //    this.Invoke(new MethodInvoker(MyHide));
            //}
            //else
            //{
            //    textBox_pay.Text = string.Empty;
            //    this.Hide();
            //    //EditInputHook.EnableWindowsEditWndxx(true);
            //}



        }

        private void userControlPanelEx1_Load(object sender, EventArgs e)
        {

        }

        private void userControlPanelEx1_Load_1(object sender, EventArgs e)
        {

        }

        private void Form1_Leave(object sender, EventArgs e)
        {

        }


    }
}
