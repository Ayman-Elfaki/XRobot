using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XRobot.Services;
public interface IAlertMessage
{
    void LongAlert(string message);
    void ShortAlert(string message);
}
