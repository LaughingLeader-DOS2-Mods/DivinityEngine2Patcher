using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
     
namespace Injector
{
    class Program
    {
        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, int bInheritHandle, int dwProcessId);
     
        [DllImport("kernel32.dll")]
        public static extern int CloseHandle(IntPtr hObject);
     
        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, int dwSize, int flAllocationType, int flProtect);
     
        [DllImport("kernel32.dll")]
        static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, string lpBuffer, int nSize, out IntPtr lpNumberOfBytesWritten);
     
        [DllImport("kernel32.dll", CharSet = CharSet.Ansi, ExactSpelling = true)]
        public static extern IntPtr GetProcAddress(IntPtr hModule, string procName);
     
        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);
     
        [DllImport("kernel32")]
        public static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, int dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, int dwCreationFlags, out IntPtr lpThreadId);
     
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr LoadLibraryEx(string lpFileName, IntPtr hReservedNull, int dwFlags);
     
        static void Main(string[] args)
        {
            IntPtr bytesout;
            string path = $@"{Environment.CurrentDirectory}\LeaderTweaks.dll";
            int bufferSize = path.Length + 1;
     
            Process process = Process.Start("DivinityEngine2.exe");
     
            // Wait for the process to run
            Thread.Sleep(1000);
     
            IntPtr hProcess = OpenProcess(0x1F0FFF, 1, process.Id);
            IntPtr AllocMem = VirtualAllocEx(hProcess, IntPtr.Zero, bufferSize, 4096, 4);
            WriteProcessMemory(hProcess, AllocMem, path, bufferSize, out bytesout);
            IntPtr loadLibraryPtr = GetProcAddress(GetModuleHandle("kernel32.dll"), "LoadLibraryA");
            IntPtr hThread = CreateRemoteThread(hProcess, (IntPtr)null, 0, loadLibraryPtr, AllocMem, 0, out bytesout);
     
            // Call the remote entry point to verify that the DLL has been injected and we can start a thread on it's entrypoint.
     
            // Dynamically load the DLL into our own process.
            IntPtr patcher = LoadLibraryEx(path, IntPtr.Zero, 1);
            // Get the address of our entry point.
            IntPtr loadLibraryAnsiPtr = GetProcAddress(patcher, "Init");
            // Invoke the entry point in the remote process
            IntPtr modulePath = CreateRemoteThread(process.Handle, IntPtr.Zero, 0, loadLibraryAnsiPtr, IntPtr.Zero, 0, out bytesout);
            CloseHandle(hThread);
        }
    }
}