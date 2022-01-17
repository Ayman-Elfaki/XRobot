using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XRobot.Platforms;
public class BluetoothPermission : Permissions.BasePlatformPermission
{
    public override (string androidPermission, bool isRuntime)[] RequiredPermissions =>
        new List<(string androidPermission, bool isRuntime)>
        {
                (Android.Manifest.Permission.AccessCoarseLocation,true),
                (Android.Manifest.Permission.AccessFineLocation,true) ,
        }.ToArray();

}

