using System;
using System.IO;

namespace Opeity.Components {
    public class Profiler {
        public void CheckCreateDirectory(String _Path) {
            if (Directory.Exists(_Path))
                return;

            Directory.CreateDirectory(_Path);
        }

        public String GetUserName() {
            String[] Identity = System.Security.Principal.WindowsIdentity.GetCurrent().Name.Split('\\');

            if (Identity.Length >= 1) {
                return Identity[1];
            }

            return "_Default";
        }

        public String BaseDir {
            get {
                String _Path = Path.Combine(Path.GetDirectoryName(System.AppDomain.CurrentDomain.BaseDirectory), "Profiles");
                CheckCreateDirectory(_Path);
                return _Path;
            }
        }

        public String UserProfileBase {
            get {
                String _Path = Path.Combine(BaseDir, GetUserName());
                CheckCreateDirectory(_Path);
                return _Path;
            }
        }
    }
}
