using System;
using System.Runtime.InteropServices;
using System.Windows;

namespace WpfApp1.Services
{
    /// <summary>
    /// Vista/Win7+ Folder Browser Dialog
    /// Provides a modern folder selection dialog for WPF applications
    /// </summary>
    public class VistaFolderBrowserDialog : IDisposable
    {
        private IFileDialog _fileDialog;
        public string SelectedPath { get; set; }

        public VistaFolderBrowserDialog()
        {
            _fileDialog = (IFileDialog)new FileOpenDialogRCW();
        }

        public bool? ShowDialog(Window owner = null)
        {
            try
            {
                IFileDialogEvents pfde = null;
                uint dwCookie;
                _fileDialog.Advise(pfde, out dwCookie);

                uint options;
                _fileDialog.GetOptions(out options);
                options |= FOS_PICKFOLDERS;
                _fileDialog.SetOptions(options);

                var hr = _fileDialog.Show(owner != null ? new WindowInteropHelper(owner).Handle : IntPtr.Zero);
                if (hr == S_OK)
                {
                    IShellItem psiResult;
                    _fileDialog.GetResult(out psiResult);
                    IntPtr pszPath = IntPtr.Zero;
                    psiResult.GetDisplayName(SIGDN_FILESYSPATH, out pszPath);
                    SelectedPath = Marshal.PtrToStringUni(pszPath);
                    Marshal.FreeCoTaskMem(pszPath);
                    return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        public void Dispose()
        {
            if (_fileDialog != null)
            {
                Marshal.FinalReleaseComObject(_fileDialog);
            }
        }

        // Constants
        private const uint S_OK = 0;
        private const uint FOS_PICKFOLDERS = 0x20;
        private const uint SIGDN_FILESYSPATH = 0x80058000;

        [ComImport, Guid("42f85136-db7e-439c-85f1-e4075d135fc8"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        private interface IFileDialog
        {
            [PreserveSig]
            uint Show([In] IntPtr parent);

            void SetFileTypes([In] uint cFileTypes, [In] IntPtr rgFilterSpec);

            void SetFileTypeIndex([In] uint iFileType);

            void GetFileTypeIndex(out uint piFileType);

            void Advise([In, MarshalAs(UnmanagedType.Interface)] IFileDialogEvents pfde, out uint pdwCookie);

            void Unadvise([In] uint dwCookie);

            void SetOptions([In] uint fos);

            void GetOptions(out uint pfos);

            void SetDefaultFolder([In, MarshalAs(UnmanagedType.Interface)] IShellItem psi);

            void SetFolder([In, MarshalAs(UnmanagedType.Interface)] IShellItem psi);

            void GetFolder(out IShellItem ppsi);

            void GetCurrentSelection(out IShellItem ppsi);

            void SetFileName([In, MarshalAs(UnmanagedType.LPWStr)] string pszName);

            void GetFileName([MarshalAs(UnmanagedType.LPWStr)] out string pszName);

            void SetTitle([In, MarshalAs(UnmanagedType.LPWStr)] string pszTitle);

            void SetOkButtonLabel([In, MarshalAs(UnmanagedType.LPWStr)] string pszText);

            void SetFileNameLabel([In, MarshalAs(UnmanagedType.LPWStr)] string pszLabel);

            void GetResult(out IShellItem ppsi);

            void AddPlace([In, MarshalAs(UnmanagedType.Interface)] IShellItem psi, uint fdap);

            void SetDefaultExtension([In, MarshalAs(UnmanagedType.LPWStr)] string pszDefaultExtension);

            void Close([MarshalAs(UnmanagedType.Error)] uint hr);

            void SetClientGuid([In] ref Guid guid);

            void ClearClientData();

            void SetFilter([In, MarshalAs(UnmanagedType.Interface)] IntPtr pFilter);
        }

        [ComImport, Guid("973510DB-7D7F-452B-8975-74A85828D354")]
        private interface IFileDialogEvents
        {
        }

        [ComImport, Guid("43826D1E-E718-42EE-BC55-A1E261C37BFE")]
        private interface IShellItem
        {
            void BindToHandler(IntPtr pbc, [In] ref Guid rbuid, [In] ref Guid riid, out IntPtr ppvOut);

            void GetParent(out IShellItem ppsi);

            void GetDisplayName(uint sigdnName, out IntPtr ppszName);

            void GetAttributes(uint sfgaoMask, out uint psfgaoAttribs);

            void Compare(IShellItem psi, uint hint, out int piOrder);
        }

        [ComImport, Guid("DC1C5A9C-E88A-4DDE-A5A1-60F82A20AEB7"), ClassInterface(ClassInterfaceType.None)]
        private class FileOpenDialogRCW
        {
        }

        private class WindowInteropHelper
        {
            private Window _window;

            public WindowInteropHelper(Window window)
            {
                _window = window;
            }

            public IntPtr Handle
            {
                get
                {
                    var helper = new System.Windows.Interop.WindowInteropHelper(_window);
                    return helper.Handle;
                }
            }
        }
    }
}
