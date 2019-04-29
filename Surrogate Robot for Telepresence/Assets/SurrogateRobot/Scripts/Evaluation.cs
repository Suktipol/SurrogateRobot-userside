using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class Evaluation : MonoBehaviour
{
    public static bool sendUdpOnce;
    public Transform viveHeadset;
    public Transform viveTracker;
    private int head_x, head_y, head_z, tk_x, tk_y, tk_z;

    string path1, path2, path3, path4, path5, path6;

	void Start ()
    {
        InitialLogFile();
        Invoke("StopEditor", 31);
	}
	

	void Update ()
    {
        if (sendUdpOnce == true)
        {
            head_x = (int)WrapAngle(viveHeadset.eulerAngles.x);
            head_y = (int)WrapAngle(viveHeadset.eulerAngles.y);
            head_z = (int)WrapAngle(viveHeadset.eulerAngles.z);

            tk_x = (int)WrapAngle(viveTracker.eulerAngles.x);
            tk_y = (int)WrapAngle(viveTracker.eulerAngles.y);
            tk_z = (int)WrapAngle(viveTracker.eulerAngles.z);

            File.AppendAllText(path1, head_x + "\n");
            File.AppendAllText(path2, head_y + "\n");
            File.AppendAllText(path3, head_z + "\n");
            File.AppendAllText(path4, tk_x + "\n");
            File.AppendAllText(path5, tk_y + "\n");
            File.AppendAllText(path6, tk_z + "\n");

            sendUdpOnce = false;
        }
    }

    void StopEditor()
    {
        Debug.Break();
    }

    private float WrapAngle(float angle)
    {
        angle %= 360;
        if (angle > 180)
            return angle - 360;

        return angle;
    }

    private void InitialLogFile()
    {
        path1 = Application.dataPath + "/Headset_x.txt";
        path2 = Application.dataPath + "/Headset_y.txt";
        path3 = Application.dataPath + "/Headset_z.txt";
        path4 = Application.dataPath + "/Tracker_x.txt";
        path5 = Application.dataPath + "/Tracker_y.txt";
        path6 = Application.dataPath + "/Tracker_z.txt";

        File.WriteAllText(path1, "Initial log file.\n");
        File.WriteAllText(path2, "Initial log file.\n");
        File.WriteAllText(path3, "Initial log file.\n");
        File.WriteAllText(path4, "Initial log file.\n");
        File.WriteAllText(path5, "Initial log file.\n");
        File.WriteAllText(path6, "Initial log file.\n");
    }
}
