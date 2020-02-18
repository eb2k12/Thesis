using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Net;
using System.Text;
using System.Net.Sockets;
using System;


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
            this.current_x = - (curx - 680);
            this.current_y = 6;
            this.current_z = -(curz - 1131);
            this.g = Instantiate(Resources.Load(type_veh)) as GameObject;
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
            this.prev_x = current_x;
            this.prev_z = current_z;
            this.current_x = -(x - 680);
            this.current_z = -(z - 1131);
            Set_angle();
        }
    }
    class Vehicle_list
    {
        public Dictionary<string, Vehicle> list;

        public Vehicle_list()
        {
            //constructor
            this.list = new Dictionary<string, Vehicle>();
        }
        public void Update(string[] words)
        {
            Dictionary<string, string> live = new Dictionary<string, string>(); 
            int num_elements = (words.GetLength(0) - 1) / 3;
            int y = 0;
            for (int i = 0; i < num_elements; i++)
            {
                if (this.list.ContainsKey(words[y]))
                {
                    this.list[words[y]].SetXZ(float.Parse(words[y + 1]), float.Parse(words[y + 2]));
                    live[words[y]] = words[y];
                    y += 3;
                }

                else if (!this.list.ContainsKey(words[y]))
                {
                    Vehicle v = new Vehicle(words[y], float.Parse(words[y + 1]), float.Parse(words[y + 2]), "car");
                    this.list.Add(words[y], v);
                    live[words[y]] = words[y];
                    y += 3;
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
        tcpclnt.Connect("localhost", 10000);
        stm = tcpclnt.GetStream();
    }
    void Update()
    {
        if (Input.GetKeyDown("p") && !flag)
        {
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
            //Debug.Log(Veh_list.list["0"].id.ToString() + " " + Veh_list.list["0"].current_x.ToString() + " " + Veh_list.list["0"].current_y.ToString() + " " + Veh_list.list["0"].current_z.ToString());
        }
        if (Input.GetKeyDown("q"))
        {
            Quit();
        }

    }
}
