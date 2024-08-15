using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

using static HookUsbInput.NativeMethods;
using static System.Net.Mime.MediaTypeNames;
// edit控件内容获取，也就是手动输入内容获取。
namespace HookUsbInput
{
    public class NativeMethods
    {
        // 定义常量
        public const int WM_GETTEXT = 0x000D;
        public const int WM_GETTEXTLENGTH = 0x000E;
        public const int WH_KEYBOARD_LL = 13;
        public const int WM_KEYDOWN = 0x0100;
        public const int VK_RETURN = 0x0D;
        public const int WM_SETTEXT = 0x000C;

        [DllImport("user32.dll")]
        public static extern bool EnableWindow(IntPtr hWnd, bool bEnable);

        // 导入 SendMessage 函数
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, string lParam);

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);


        // 定义委托
        public delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
        // 导入 Windows API 函数
        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool EnumChildWindows(IntPtr hWnd, EnumChildProc lpEnumFunc, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern int GetClassName(IntPtr hWnd, StringBuilder lpClassName, int nMaxCount);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, StringBuilder lParam);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
        // 定义枚举窗口的委托
        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
        public delegate bool EnumChildProc(IntPtr hWnd, IntPtr lParam);


        public enum WindowStyles : uint
        {
            WS_OVERLAPPED = 0x00000000,
            WS_POPUP = 0x80000000,
            WS_CHILD = 0x40000000,
            WS_MINIMIZE = 0x20000000,
            WS_VISIBLE = 0x10000000,
            WS_DISABLED = 0x08000000,
            WS_CLIPSIBLINGS = 0x04000000,
            WS_CLIPCHILDREN = 0x02000000,
            WS_MAXIMIZE = 0x01000000,
            WS_CAPTION = 0x00C00000,     // WS_BORDER | WS_DLGFRAME
            WS_BORDER = 0x00800000,
            WS_DLGFRAME = 0x00400000,
            WS_VSCROLL = 0x00200000,
            WS_HSCROLL = 0x00100000,
            WS_SYSMENU = 0x00080000,
            WS_THICKFRAME = 0x00040000,
            WS_GROUP = 0x00020000,
            WS_TABSTOP = 0x00010000,
            WS_MINIMIZEBOX = 0x00020000,
            WS_MAXIMIZEBOX = 0x00010000,
            WS_POPUPWINDOW = WS_OVERLAPPED | WS_POPUP | WS_BORDER | WS_SYSMENU,
            WS_OVERLAPPEDWINDOW = WS_OVERLAPPED | WS_CAPTION | WS_SYSMENU | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX
        }

        public static readonly int GWL_STYLE = -16; // 获取或设置窗口样式用的参数
        public static readonly int GWL_EXSTYLE = -20; // 获取或设置扩展窗口样式用的参数

        [DllImport("user32.dll", SetLastError = true)]
        public static extern int SetWindowLong(IntPtr hWnd, int nIndex, uint dwNewLong);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint GetWindowLong(IntPtr hWnd, int nIndex);

    }
    public class EditInputHook
    {
        // 用于禁止和恢复编辑框的句柄
        public static IntPtr editWndxx = IntPtr.Zero;


        private static System.Threading.Timer timer;
        private static string processName;
        private static string topLevelClassName;
        private static string childClassName;
        private static string lastText = "";

        private static IntPtr hookId = IntPtr.Zero;
        private static NativeMethods.LowLevelKeyboardProc hookCallback;
        // 自定义委托和事件
        public delegate void ContentChangedEventHandler(string content);
        public static event ContentChangedEventHandler ContentChanged;

        // 开始监控
        public static void StartMonitoring(string processName, string topLevelClassName, string childClassName)
        {
            EditInputHook.processName = processName;
            EditInputHook.topLevelClassName = topLevelClassName;
            EditInputHook.childClassName = childClassName;

            timer = new System.Threading.Timer(TimerCallback, null, 0, 100);

            hookCallback = KeyboardHookCallback;
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                hookId = NativeMethods.SetWindowsHookEx(NativeMethods.WH_KEYBOARD_LL, hookCallback, NativeMethods.GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        // 定时器回调函数
        private static void TimerCallback(object state)
        {
            Process[] processes = Process.GetProcessesByName(processName);
            if (processes.Length > 0)
            {
                uint processId = (uint)processes[0].Id;

                NativeMethods.EnumWindows(new NativeMethods.EnumWindowsProc((hWnd, lParam) =>
                {
                    NativeMethods.GetWindowThreadProcessId(hWnd, out uint windowProcessId);
                    if (windowProcessId == processId)
                    {

                        NativeMethods.EnumChildWindows(hWnd, new NativeMethods.EnumChildProc((childWnd, childLParam) =>
                        {
                            StringBuilder className = new StringBuilder(256);
                            NativeMethods.GetClassName(childWnd, className, className.Capacity);

                            if (className.ToString() == topLevelClassName)
                            {
                                NativeMethods.EnumChildWindows(childWnd, new NativeMethods.EnumChildProc((editWnd, editLParam) =>
                                {
                                    StringBuilder editClassName = new StringBuilder(256);
                                    NativeMethods.GetClassName(editWnd, editClassName, editClassName.Capacity);

                                    if (editClassName.ToString() == childClassName)
                                    {
                                        int textLength = NativeMethods.SendMessage(editWnd, NativeMethods.WM_GETTEXTLENGTH, 0, null).ToInt32();
                                        StringBuilder text = new StringBuilder(textLength + 1);
                                        NativeMethods.SendMessage(editWnd, NativeMethods.WM_GETTEXT, text.Capacity, text);
                                        //lastText = text.ToString();
                                        if (textLength >= 14)
                                        {
                                            lastText = text.ToString();
                                            editWndxx = editWnd;
                                        }
                                    }

                                    return true;
                                }), IntPtr.Zero);
                            }

                            return true;
                        }), IntPtr.Zero);
                    }

                    return true;
                }), IntPtr.Zero);
            }
            else
            {
                //Console.WriteLine($"未找到名为{processName}.exe的进程。");
            }
        }


        // 内容变更事件处理函数
        private static void OnContentChanged(string content)
        {
            ContentChanged?.Invoke(content);
        }

        // 键盘钩子回调函数
        private static IntPtr KeyboardHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)NativeMethods.WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                if (vkCode == NativeMethods.VK_RETURN)
                {
                    if (lastText.Length >= 14)
                    {
                        OnContentChanged(lastText);
                        lastText = string.Empty;
                        lastText = "";
                        ClearEditControl(processName, topLevelClassName, childClassName);
                        return (IntPtr)1;
                    }
                    else
                    {
                        return NativeMethods.CallNextHookEx(hookId, nCode, wParam, lParam);
                    }

                }
            }
            // 在这里返回下一个钩子的处理结果
            return NativeMethods.CallNextHookEx(hookId, nCode, wParam, lParam);
        }


        public static void EnableWindowsEditWndxx(bool a)
        {
            if (editWndxx!=IntPtr.Zero) {
                if (a)
                {
                    //MessageBox.Show(editWndxx.ToString());
                    NativeMethods.EnableWindow(editWndxx, true);
                }
                else
                {
                    // 禁用窗口
                    NativeMethods.EnableWindow(editWndxx, false);
                    //MessageBox.Show("禁用输入");
                }
            }

        }
        // 清空编辑控件内容方法
        public static void ClearEditControl(string processName, string topLevelClassName, string childClassName)
        {
            IntPtr editWndx = IntPtr.Zero;
            Process[] processes = Process.GetProcessesByName(processName);
            if (processes.Length > 0)
            {
                uint processId = (uint)processes[0].Id;

                NativeMethods.EnumWindows(new NativeMethods.EnumWindowsProc((hWnd, lParam) =>
                {
                    NativeMethods.GetWindowThreadProcessId(hWnd, out uint windowProcessId);
                    if (windowProcessId == processId)
                    {
                        NativeMethods.EnumChildWindows(hWnd, new NativeMethods.EnumChildProc((childWnd, childLParam) =>
                        {
                            StringBuilder className = new StringBuilder(256);
                            NativeMethods.GetClassName(childWnd, className, className.Capacity);

                            if (className.ToString() == topLevelClassName)
                            {
                                NativeMethods.EnumChildWindows(childWnd, new NativeMethods.EnumChildProc((editWnd, editLParam) =>
                                {
                                    StringBuilder editClassName = new StringBuilder(256);
                                    NativeMethods.GetClassName(editWnd, editClassName, editClassName.Capacity);

                                    if (editClassName.ToString() == childClassName)
                                    {
                                        int textLength = NativeMethods.SendMessage(editWnd, NativeMethods.WM_GETTEXTLENGTH, 0, null).ToInt32();
                                        StringBuilder text = new StringBuilder(textLength + 1);
                                        NativeMethods.SendMessage(editWnd, NativeMethods.WM_GETTEXT, text.Capacity, text);
                                        //lastText = text.ToString();
                                        if (textLength >= 14)
                                        {
                                            editWndx = editWnd;

                                            //NativeMethods.ShowWindow(editWnd, 0);
                                            //Thread.Sleep(200);
                                            //NativeMethods.ShowWindow(editWnd, 5);
                                            return true;
                                        }
                                    }

                                    return true;
                                }), IntPtr.Zero);
                            }

                            return true;
                        }), IntPtr.Zero);
                    }

                    return true;
                }), IntPtr.Zero);
            }
            // 清空编辑控件内容
            if (editWndx != IntPtr.Zero)
            {
                NativeMethods.SendMessage(editWndx, NativeMethods.WM_SETTEXT, 0, new StringBuilder(""));
                //Thread.Sleep(500);
                //NativeMethods.ShowWindow(editWndx, 5);

            }
        }
    }
}

