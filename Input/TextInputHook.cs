using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Hacknet.Input
{
    public class TextInputHook : IDisposable
    {
        public delegate int GetMsgProc(int nCode, int wParam, ref Message msg);

        public enum HookId
        {
            WH_MIN = -1,
            WH_MINHOOK = -1,
            WH_MSGFILTER = -1,
            WH_JOURNALRECORD = 0,
            WH_JOURNALPLAYBACK = 1,
            WH_KEYBOARD = 2,
            WH_GETMESSAGE = 3,
            WH_CALLWNDPROC = 4,
            WH_CBT = 5,
            WH_SYSMSGFILTER = 6,
            WH_HARDWARE = 8,
            WH_DEBUG = 9,
            WH_SHELL = 10,
            WH_FOREGROUNDIDLE = 11,
            WH_MAX = 11,
            WH_MAXHOOK = 11,
            WH_CALLWNDPROCRET = 12,
            WH_KEYBOARD_LL = 13,
            WH_MOUSE_LL = 14
        }

        public enum WindowMessage
        {
            WM_KEYDOWN = 256,
            WM_KEYUP = 257,
            WM_CHAR = 258
        }

        private readonly IntPtr HookHandle;
        private readonly GetMsgProc ProcessMessagesCallback;
        private bool backSpace;

        public TextInputHook(IntPtr whnd)
        {
            ProcessMessagesCallback = ProcessMessages;
            HookHandle = SetWindowsHookEx(HookId.WH_GETMESSAGE, ProcessMessagesCallback, IntPtr.Zero,
                GetCurrentThreadId());
        }

        public string Buffer { get; private set; } = "";

        public bool BackSpace
        {
            get
            {
                var flag = backSpace;
                backSpace = false;
                return flag;
            }
        }

        public void Dispose()
        {
            if (!(HookHandle != IntPtr.Zero))
                return;
            UnhookWindowsHookEx(HookHandle);
        }

        public event KeyEventHandler KeyUp;

        public event KeyEventHandler KeyDown;

        public event KeyPressEventHandler KeyPress;

        public void clearBuffer()
        {
            Buffer = "";
        }

        [DllImport("user32.dll", EntryPoint = "SetWindowsHookExA")]
        public static extern IntPtr SetWindowsHookEx(HookId idHook, GetMsgProc lpfn, IntPtr hmod, int dwThreadId);

        [DllImport("user32.dll")]
        public static extern int UnhookWindowsHookEx(IntPtr hHook);

        [DllImport("user32.dll")]
        public static extern int CallNextHookEx(int hHook, int ncode, int wParam, ref Message lParam);

        [DllImport("user32.dll")]
        public static extern bool TranslateMessage(ref Message lpMsg);

        [DllImport("kernel32.dll")]
        public static extern int GetCurrentThreadId();

        private int ProcessMessages(int nCode, int wParam, ref Message msg)
        {
            if (nCode == 0 && wParam == 1)
            {
                TranslateMessage(ref msg);
                switch (msg.Msg)
                {
                    case 256:
                        OnKeyDown(new KeyEventArgs((Keys) msg.WParam));
                        break;
                    case 257:
                        OnKeyUp(new KeyEventArgs((Keys) msg.WParam));
                        break;
                    case 258:
                        OnKeyPress(new KeyPressEventArgs((char) msg.WParam));
                        break;
                }
            }
            return CallNextHookEx(0, nCode, wParam, ref msg);
        }

        protected virtual void OnKeyUp(KeyEventArgs e)
        {
            if (KeyUp == null)
                return;
            KeyUp(this, e);
        }

        protected virtual void OnKeyDown(KeyEventArgs e)
        {
            if (KeyDown == null)
                return;
            KeyDown(this, e);
        }

        protected virtual void OnKeyPress(KeyPressEventArgs e)
        {
            if (KeyPress != null)
                KeyPress(this, e);
            if (e.KeyChar.GetHashCode().ToString() == "524296")
                backSpace = true;
            else
                Buffer += e.KeyChar;
        }
    }
}