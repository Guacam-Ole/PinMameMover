using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PinMameMover
{
    public class ProcessDetection
    {
        private bool _moveWindow = false;
        private string _mainProcessName;
        private string _mameClass;
        private int _delay;
        private Rect _newPosition;

        private readonly ILog _log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        delegate bool EnumThreadDelegate(IntPtr hWnd, IntPtr lParam);
        private const uint WM_GETTEXT = 0x000D;
        [DllImport("user32.dll", SetLastError = true)]
        internal static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, int wParam, StringBuilder lParam);
        [DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern long GetClassName(IntPtr hwnd, StringBuilder lpClassName, long nMaxCount);
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern int GetWindowTextLength(IntPtr hWnd);
        [DllImport("User32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern long GetWindowText(IntPtr hwnd, StringBuilder lpString, long cch);
        [DllImport("user32.dll")]
        static extern bool EnumThreadWindows(int dwThreadId, EnumThreadDelegate lpfn, IntPtr lParam);
        [DllImport("user32.dll")]
        public static extern bool GetWindowRect(IntPtr hwnd, ref Rect rectangle);

        public struct Rect
        {
            public int Left { get; set; }
            public int Top { get; set; }
            public int Right { get; set; }
            public int Bottom { get; set; }
        }

        public ProcessDetection(string mainProcessName, string mameClass, int delay)
        {
            _mainProcessName = mainProcessName.ToLower();
            _mameClass = mameClass.ToLower();
            _delay = delay;
        }

        public ProcessDetection(string mainProcessName, string mameClass, int delay, int x, int y, int width, int height) : this(mainProcessName, mameClass, delay)
        {
            _newPosition = new Rect
            {
                Left = x,
                Top = y,
                Right = x + width,
                Bottom = y + height
            };
            _moveWindow = true;
        }

        public void DetectChanges()
        {
            ManagementEventWatcher startWatch = new ManagementEventWatcher(new WqlEventQuery("SELECT * FROM Win32_ProcessStartTrace"));
            startWatch.EventArrived += new EventArrivedEventHandler(startWatch_EventArrived);
            startWatch.Start();
        }

        public void Manual()
        {
            _log.Debug("Manually searching for process");
            var allProcesses = Process.GetProcesses();
            foreach (var process in allProcesses)
            {
                if (process.ProcessName.ToLower().StartsWith(_mainProcessName))
                {
                    _log.Info("This should be VPinball. Searching for MAME-Window");
                    EnumerateProcessWindowHandles(process.Id); ;
                    return;
                }
            }
            var allNames = string.Join(",", allProcesses.Select(ap => ap.ProcessName));
            _log.Debug($"Process NOT found. All Processes:{allNames}");
        }

        private void startWatch_EventArrived(object sender, EventArrivedEventArgs e)
        {
            string processName = e.NewEvent.Properties["ProcessName"].Value as string;
            int processId = Convert.ToInt32(e.NewEvent.Properties["ProcessId"].Value);
            _log.Debug($"New Process Started: {processName}|{processId}");
            if (processName.ToLower().StartsWith(_mainProcessName))
            {
                _log.Info("This should be VPinball. Searching for MAME-Window");
                if (_delay>0)
                {
                    System.Threading.Thread.Sleep(_delay * 1000);
                }
                EnumerateProcessWindowHandles(processId);
            }
        }

        private string GetCaptionOfWindow(IntPtr hwnd)
        {
            string caption = "";
            StringBuilder windowText = null;
            try
            {
                int max_length = GetWindowTextLength(hwnd);
                windowText = new StringBuilder("", max_length + 5);
                GetWindowText(hwnd, windowText, max_length + 2);

                if (!String.IsNullOrEmpty(windowText.ToString()) && !String.IsNullOrWhiteSpace(windowText.ToString()))
                    caption = windowText.ToString();
            }
            catch (Exception ex)
            {
                caption = ex.Message;
            }
            finally
            {
                windowText = null;
            }
            return caption;
        }

        private string GetClassNameOfWindow(IntPtr hwnd)
        {
            string className = "";
            StringBuilder classText = null;
            try
            {
                int cls_max_length = 1000;
                classText = new StringBuilder("", cls_max_length + 5);
                GetClassName(hwnd, classText, cls_max_length + 2);

                if (!String.IsNullOrEmpty(classText.ToString()) && !String.IsNullOrWhiteSpace(classText.ToString()))
                    className = classText.ToString();
            }
            catch (Exception ex)
            {
                className = ex.Message;
            }
            finally
            {
                classText = null;
            }
            return className;
        }

        private IEnumerable<int> EnumerateProcessWindowHandles(int processId)
        {
            var handles = new List<int>();

            foreach (ProcessThread thread in Process.GetProcessById(processId).Threads)
            {
                EnumThreadWindows(thread.Id, (hWnd, lParam) =>
                {
                    string caption = GetCaptionOfWindow(hWnd);
                    string className = GetClassNameOfWindow(hWnd);

                    if (className.ToLower().StartsWith(_mameClass))
                    {
                        _log.Info("Mame-Class found.");
                        Rect currentPosition = new Rect();
                        GetWindowRect(hWnd, ref currentPosition);
                        _log.Debug($"Current Position: {currentPosition.Left}, {currentPosition.Top}, { currentPosition.Right - currentPosition.Left}, { currentPosition.Bottom - currentPosition.Top} ");

                        if (_moveWindow)
                        {
                            MoveWindow(hWnd, _newPosition.Left, _newPosition.Top, _newPosition.Right - _newPosition.Left, _newPosition.Bottom - _newPosition.Top, true);
                            _log.Debug($"New Position: {_newPosition.Left}, {_newPosition.Top}, { _newPosition.Right - _newPosition.Left}, { _newPosition.Bottom - _newPosition.Top} ");
                        }
                    }

                    return true;

                }, IntPtr.Zero);
            }
            return handles;
        }
    }
}
