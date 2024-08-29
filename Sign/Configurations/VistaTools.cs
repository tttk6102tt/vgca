using System.Collections;
using System.Management;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;

namespace Sign.Configurations
{
    public class VistaTools
    {
        public static bool Is64BitOperatingSystem()
        {
            bool wow64Process = false;
            return IntPtr.Size == 8 || ((!VistaTools.DoesWin32MethodExist("kernel32.dll", "IsWow64Process") ? 0 : (VistaTools.IsWow64Process(VistaTools.GetCurrentProcess(), out wow64Process) ? 1 : 0)) & (wow64Process ? 1 : 0)) != 0;
        }

        private static bool DoesWin32MethodExist(string moduleName, string methodName)
        {
            IntPtr moduleHandle = VistaTools.GetModuleHandle(moduleName);
            return !(moduleHandle == IntPtr.Zero) && VistaTools.GetProcAddress(moduleHandle, methodName) != IntPtr.Zero;
        }

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetCurrentProcess();

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr GetModuleHandle(string moduleName);

        [DllImport("kernel32", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetProcAddress(IntPtr hModule, [MarshalAs(UnmanagedType.LPStr)] string procName);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsWow64Process(IntPtr hProcess, out bool wow64Process);

        public static bool Is64BitOperatingSystem(
          string machineName,
          string domain,
          string userName,
          string password)
        {
            ConnectionOptions options = (ConnectionOptions)null;
            if (!string.IsNullOrEmpty(userName))
            {
                options = new ConnectionOptions();
                options.Username = userName;
                options.Password = password;
                options.Authority = "NTLMDOMAIN:" + domain;
            }
            ManagementScope scope = new ManagementScope("\\\\" + (string.IsNullOrEmpty(machineName) ? "." : machineName) + "\\root\\cimv2", options);
            scope.Connect();
            foreach (ManagementBaseObject managementBaseObject in new ManagementObjectSearcher(scope, new ObjectQuery("SELECT AddressWidth FROM Win32_Processor")).Get())
            {
                if (managementBaseObject["AddressWidth"].ToString() == "64")
                    return true;
            }
            return false;
        }

        public static bool AtLeastVista() => Environment.OSVersion.Platform == PlatformID.Win32NT && Environment.OSVersion.Version.Major > 5;

        [DllImport("shell32.dll", EntryPoint = "#680", CharSet = CharSet.Unicode)]
        public static extern bool IsUserAnAdmin();

        private static bool HaveFolderPermissions(string folder)
        {
            try
            {

                //AuthorizationRuleCollection accessRules = DirectoryInfo.GetAccessControl(folder).GetAccessRules(true, true, typeof(NTAccount));
                var accessRules = new DirectoryInfo(folder).GetAccessControl().GetAccessRules(true, true, typeof(NTAccount));
                WindowsPrincipal windowsPrincipal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
                FileSystemRights fileSystemRights1 = (FileSystemRights)0;
                FileSystemRights fileSystemRights2 = (FileSystemRights)0;
                foreach (FileSystemAccessRule systemAccessRule in (ReadOnlyCollectionBase)accessRules)
                {
                    if (systemAccessRule.IdentityReference.Value.StartsWith("S-1-"))
                    {
                        SecurityIdentifier sid = new SecurityIdentifier(systemAccessRule.IdentityReference.Value);
                        if (!windowsPrincipal.IsInRole(sid))
                            continue;
                    }
                    else if (!windowsPrincipal.IsInRole(systemAccessRule.IdentityReference.Value))
                        continue;
                    if (systemAccessRule.AccessControlType == AccessControlType.Deny)
                        fileSystemRights2 |= systemAccessRule.FileSystemRights;
                    else
                        fileSystemRights1 |= systemAccessRule.FileSystemRights;
                }
                return ((fileSystemRights1 & ~fileSystemRights2 ^ (FileSystemRights.Modify | FileSystemRights.DeleteSubdirectoriesAndFiles)) & (FileSystemRights.Modify | FileSystemRights.DeleteSubdirectoriesAndFiles)) == (FileSystemRights)0;
            }
            catch
            {
                return false;
            }
        }
    }
}
