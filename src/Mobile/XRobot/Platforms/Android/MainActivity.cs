using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;

namespace XRobot;

[Activity(Theme = "@style/Maui.SplashTheme",
		  MainLauncher = true, 
		  ScreenOrientation = ScreenOrientation.Portrait,
		  ConfigurationChanges = ConfigChanges.ScreenSize | 
							     ConfigChanges.Orientation | 
								 ConfigChanges.UiMode | 
								 ConfigChanges.ScreenLayout | 
								 ConfigChanges.SmallestScreenSize)]
public class MainActivity : MauiAppCompatActivity
{
	protected override void OnCreate(Bundle savedInstanceState)
	{
		base.OnCreate(savedInstanceState);
		Platform.Init(this, savedInstanceState);
	}


	public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
	{
		Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
		base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
	}
}
