using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AndroidApp = Android.App;

namespace XRobot.Services;
public class AlertMessage : IAlertMessage
{
    public void LongAlert(string message)
    {
        Toast.MakeText(AndroidApp.Application.Context, message, ToastLength.Long).Show();
    }

    public void ShortAlert(string message)
    {
        Toast.MakeText(AndroidApp.Application.Context, message, ToastLength.Short).Show();
    }
}
