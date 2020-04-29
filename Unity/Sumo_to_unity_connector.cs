using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Net;
using System.Text;
using System.Net.Sockets;
using System;
using System.Diagnostics;


public class Sumo_to_unity_connector : MonoBehaviour
{
    //*************Class Declarations****************************************
    class Vehicle
    {
        public string id;
        public float current_x, current_y, current_z, prev_x, prev_z, angle_c;
        public GameObject g;


        public Vehicle(string id, float curx, float curz, string type_veh)
        {
            //constructor
            this.id = id;
            current_x = - (curx - 680);
            current_y = 4;
            current_z = -(curz - 1131);
            g = Instantiate(Resources.Load(type_veh)) as GameObject;
            g.name = "VehicleID: " + id + type_veh;
            g.AddComponent(Type.GetType("collision_detection_m"));
        }

        public void Set_angle()
        {
            //Used for vehicle orientation
            double angle = Math.Atan2(this.current_x - this.prev_x, this.current_z - this.prev_z) * 180 / Math.PI;
            this.angle_c = (float)angle;
        }
        public void SetXZ(float x, float z)
        {
            //update posn values and calculate orientation based on prev,curr values
            prev_x = current_x;
            prev_z = current_z;
            current_x = -(x - 680);
            current_z = -(z - 1131);
            Set_angle();
            UnityEngine.Debug.Log(id);
            UnityEngine.Debug.Log(current_x);
            UnityEngine.Debug.Log(current_y);
            UnityEngine.Debug.Log(current_z);
            UnityEngine.Debug.Log(angle_c);
            UnityEngine.Debug.Log(g);
        }
    }
    class Vehicle_list
    {
        public Dictionary<string, Vehicle> list;

        public Vehicle_list()
        {
            //constructor
            list = new Dictionary<string, Vehicle>();
        }
        public void remove_veh(string id)
        {
            list.Remove(id);
        }
        public void Update(string[] words)
        {
            Dictionary<string, string> live = new Dictionary<string, string>(); 
            int num_elements = (words.GetLength(0) - 1) / 4;
            int y = 0;
            for (int i = 0; i < num_elements; i++)
            {
                if (list.ContainsKey(words[y]))
                {
                    list[words[y]].SetXZ(float.Parse(words[y + 1]), float.Parse(words[y + 2]));
                    live[words[y]] = words[y];
                    y += 4;
                }

                else if (!list.ContainsKey(words[y]))
                {
                    Vehicle v = new Vehicle(words[y], float.Parse(words[y + 1]), float.Parse(words[y + 2]),words[y + 3]);
                    list.Add(words[y], v);
                    live[words[y]] = words[y];
                    y += 4;
                }
            }
            //remove expired vehicles
            String[] to_remove = new String[100];
            int i1 = 0;
            foreach(var item in list.Keys)
            {
                if (!live.ContainsKey(item))
                {
                    to_remove[i1] = item;
                    i1++;
                }
            }
            foreach(var item in to_remove)
            {
                if (item != null)
                {
                    remove_veh(item);
                }
            }

        }
    }
    //*************Globals****************************************
    bool flag = false;
    Vehicle_list Veh_list = new Vehicle_list();
    TcpClient tcpclnt = new TcpClient();
    byte[] ba = new byte[2048];
    Stream stm;
    //*************Display Functions****************************************
    void Display_init()
    {
        foreach (var item in Veh_list.list.Values)
        {
            Vector3 vec2 = new Vector3(item.current_x, item.current_y, item.current_z);
            item.g.transform.position = vec2;
            item.g.transform.rotation = Quaternion.Euler(0, (float)item.angle_c, 0);
        }
    }
    void Display_update()
    {
        foreach (var item in Veh_list.list.Values)
        {
            Vector3 vec2 = new Vector3(item.current_x, item.current_y, item.current_z);
            item.g.transform.position = vec2;

            item.g.transform.rotation = Quaternion.Euler(0, (float)item.angle_c, 0);


        }
    }
    //*************Network Functions****************************************
    void Send_start()
    {
        byte[] myWriteBuffer = Encoding.ASCII.GetBytes("go");
        stm.Write(myWriteBuffer, 0, myWriteBuffer.Length);
    }

    void Quit()
    {
        tcpclnt.Close();
        UnityEditor.EditorApplication.isPlaying = false;
    }
    string[] Get_info(Stream s)
    {
        s.Read(ba, 0, ba.Length);
        string init = Encoding.UTF8.GetString(ba);
        string[] info = init.Split(' ');
        for(int i = 0; i< info.Length; i++)
        {
            if(info[i] == "disconnect_1")
            {
                flag = false;
                Quit();
            }
        }
        return info;
    }
    //*************Unity runnners****************************************
    void Start()
    {
    }
    void Update()
    {
        if (Input.GetKeyDown("o"))
        {
            Process process = new Process();
            process.StartInfo.FileName = "sumo_server.py";
            process.StartInfo.WindowStyle = ProcessWindowStyle.Normal;
            process.StartInfo.WorkingDirectory = "C:\\Users\\eoinb\\Desktop\\Thesis\\Sumo\\";
            process.StartInfo.Arguments = "50";
            process.Start();

            tcpclnt.Connect("localhost", 10000);
            stm = tcpclnt.GetStream();
        }
        if (Input.GetKeyDown("p") && !flag)
        {
            tcpclnt.Connect("localhost", 10000);
            stm = tcpclnt.GetStream();
            Send_start();
            ba = new byte[2048];
            string[] pos1_string = Get_info(stm);
            Veh_list.Update(pos1_string);
            Display_init();
            flag = true;
        }
        if (flag)
        {
            ba = new byte[2048];
            string[] pos2_string = Get_info(stm);
            Veh_list.Update(pos2_string);
            Display_update();

        }
        if (Input.GetKeyDown("q"))
        {
            Quit();
        }
    }
}
