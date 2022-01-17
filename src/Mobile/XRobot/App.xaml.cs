using XRobot.Views;

namespace XRobot;

public partial class App : Application
{
	public App(MainView mainView)
	{
		InitializeComponent();

		MainPage = mainView;
	}
}
