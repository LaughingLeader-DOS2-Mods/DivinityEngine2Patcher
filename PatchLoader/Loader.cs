using PatchLoader.Win32;

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PatchLoader
{
	public class Loader
	{
        public static void SetDLLSearchPath(IntPtr _handle, Process _process, string searchPath)
        {
            // (in?)sanity check, pretty sure this is never possible as the constructor will error - left over from how it previously was developed
            if (_process == null)
                throw new InvalidOperationException("This injector has no associated process and thus cannot inject a library");
            if (_handle == IntPtr.Zero)
                throw new InvalidOperationException("This injector does not have a valid handle to the associated process and thus cannot inject a library");

            if (!Directory.Exists(searchPath))
                throw new FileNotFoundException(string.Format("Unable to find DLL search path to inject into process {1}", searchPath, _process.ProcessName), searchPath);

            // convenience variables
            string fullPath = Path.GetFullPath(searchPath);

            // declare resources that need to be freed in finally
            IntPtr pSearchRemote = IntPtr.Zero; // pointer to allocated memory of search path string
            IntPtr hThread = IntPtr.Zero; // handle to thread from CreateRemoteThread
            IntPtr pSearchFullPathUnmanaged = Marshal.StringToHGlobalUni(fullPath); // unmanaged C-String pointer

            try
            {
                uint sizeUni = (uint)Encoding.Unicode.GetByteCount(fullPath);

                // Get Handle to Kernel32.dll and pointer to SetDllDirectory
                IntPtr hKernel32 = Imports.GetModuleHandle("Kernel32");
                if (hKernel32 == IntPtr.Zero)
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                IntPtr hSetDir = Imports.GetProcAddress(hKernel32, "SetDllDirectoryW");
                if (hSetDir == IntPtr.Zero)
                    throw new Win32Exception(Marshal.GetLastWin32Error());

                // allocate memory to the local process for searchFullPath
                pSearchRemote = Imports.VirtualAllocEx(_handle, IntPtr.Zero, sizeUni, AllocationType.Commit, MemoryProtection.ReadWrite);
                if (pSearchRemote == IntPtr.Zero)
                    throw new Win32Exception(Marshal.GetLastWin32Error());

                // write searchFullPath to pSearchRemote
                int bytesWritten;
                if (!Imports.WriteProcessMemory(_handle, pSearchRemote, pSearchFullPathUnmanaged, sizeUni, out bytesWritten) || bytesWritten != (int)sizeUni)
                    throw new Win32Exception(Marshal.GetLastWin32Error());

                // set dll search path via call to SetDllDirectory using CreateRemoteThread
                hThread = Imports.CreateRemoteThread(_handle, IntPtr.Zero, 0, hSetDir, pSearchRemote, 0, IntPtr.Zero);
                if (hThread == IntPtr.Zero)
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                if (Imports.WaitForSingleObject(hThread, (uint)ThreadWaitValue.Infinite) != (uint)ThreadWaitValue.Object0)
                    throw new Win32Exception(Marshal.GetLastWin32Error());

                uint success;
                if (!Imports.GetExitCodeThread(hThread, out success))
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                if (success == 0)
                    throw new Exception("Return value of SetDllDirectory was 0, possible Win32Exception", new Win32Exception(Marshal.GetLastWin32Error()));
            }
            finally
            {
                Marshal.FreeHGlobal(pSearchFullPathUnmanaged); // free unmanaged string
                Imports.CloseHandle(hThread); // close thread from CreateRemoteThread
                Imports.VirtualFreeEx(_process.Handle, pSearchRemote, 0, AllocationType.Release); // Free memory allocated
            }
        }

        public static List<string> GetPatches(string pluginsPath)
		{
            return Directory.EnumerateFiles(pluginsPath, "*.dll", SearchOption.AllDirectories).ToList();
		}

        private static bool TryLoadPatch(string exeDir, string path, Process process, IntPtr hProcess, IntPtr loadLibraryPtr)
		{
            IntPtr hThread = IntPtr.Zero;

            try
			{
                int bytesout;
                uint bufferSize = (uint)path.Length + 1;

                byte[] pdll = System.Text.Encoding.Default.GetBytes(path);
                IntPtr AllocMem = Imports.VirtualAllocEx(hProcess, IntPtr.Zero, bufferSize, AllocationType.Commit, MemoryProtection.ReadWrite);
                Imports.WriteProcessMemory(hProcess, AllocMem, pdll, bufferSize, out bytesout);
                hThread = Imports.CreateRemoteThread(hProcess, (IntPtr)null, 0, loadLibraryPtr, AllocMem, 0, IntPtr.Zero);

                // Call the remote entry point to verify that the DLL has been injected and we can start a thread on it's entrypoint.
                // Dynamically load the DLL into our own process.
                IntPtr patcher = Imports.LoadLibraryEx(path, IntPtr.Zero, LoadLibraryExFlags.DontResolveDllReferences);
                // Get the address of our entry point.
                IntPtr loadLibraryAnsiPtr = Imports.GetProcAddress(patcher, "LoadPlugin");
                if(loadLibraryAnsiPtr != IntPtr.Zero)
				{
                    // Invoke the entry point in the remote process
                    IntPtr modulePath = Imports.CreateRemoteThread(process.Handle, IntPtr.Zero, 0, loadLibraryAnsiPtr, IntPtr.Zero, 0, IntPtr.Zero);
                }
                Imports.CloseHandle(hThread);
                return true;
            }
            catch(Exception ex)
			{
                Console.WriteLine(ex);
			}
            finally
			{
                if(hThread != IntPtr.Zero)
				{
                    Imports.CloseHandle(hThread);
                }
			}
            return false;
        }

        public static bool LoadPatches(string exePath, string pluginsPath)
		{
            var patches = GetPatches(pluginsPath);

            Console.WriteLine("Starting program...");
            Process process = Process.Start(exePath);
            var exeDir = Directory.GetParent(exePath).FullName;

            if (patches.Count > 0)
			{
                // Wait for the process to run
                Thread.Sleep(2000);

                string procName = Path.GetFileNameWithoutExtension(exePath);
                Process engineProc = Process.GetProcessesByName(procName).FirstOrDefault();

                //IntPtr hProcess = OpenProcess(0x1F0FFF, 1, process.Id);
                IntPtr hProcess = Imports.OpenProcess(
                    ProcessAccessFlags.QueryInformation | ProcessAccessFlags.CreateThread |
                    ProcessAccessFlags.VMOperation | ProcessAccessFlags.VMWrite |
                    ProcessAccessFlags.VMRead, false, process.Id);
                //SetDLLSearchPath(hProcess, engineProc, exeDir);
                SetDLLSearchPath(hProcess, engineProc, pluginsPath);
                IntPtr loadLibraryPtr = Imports.GetProcAddress(Imports.GetModuleHandle("kernel32.dll"), "LoadLibraryA");

                //Directory.SetCurrentDirectory(pluginsPath);
                Console.WriteLine("Injecting patches...");
                
                patches.ForEach(p => TryLoadPatch(exeDir, p, process, hProcess, loadLibraryPtr));
                Console.WriteLine("All done.");
                return true;
            }
            Console.WriteLine("No patches found. Skipping.");
            return false;
        }
    }
}
