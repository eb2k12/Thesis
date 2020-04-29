# -*- coding: utf-8 -*-
"""
Created on Fri Feb  7 14:04:07 2020

@author: eoinb
"""
from __future__ import absolute_import
from __future__ import print_function
import os
import sys

if 'SUMO_HOME' in os.environ:
    tools = os.path.join(os.environ['SUMO_HOME'], 'tools')
    sys.path.append(tools)
else:
    sys.exit("please declare environment variable 'SUMO_HOME'")
from sumolib import checkBinary
import traci
import socket
import time 

abspath = os.path.abspath(__file__)
dname = os.path.dirname(abspath)
os.chdir(dname)

scenario = '3bhigh.sumocfg'    
fname = 'GCD2.sumocfg'
net = 'GCD.net.xml'
rou = '2.rou.xml'
end_message = "disconnect_1 "

sumoBinary = checkBinary('sumo')
sumoBinary1 = checkBinary('randomTrips.py')
comp = [-1073741824.0, -1073741824.0]
###############################################################################
class Vehicle:
    def __init__(self, vehicle_id, current_x, current_y, vehicle_type):
        self.vehicle_id = vehicle_id
        self.current_x = current_x
        self.current_y = current_y
        self.vehicle_type = vehicle_type
###############################################################################
def getCname(vtype):
    if vtype == "DEFAULT_VEHTYPE":
        return "Car"  
    if vtype == "DEFAULT_BIKETYPE":
        return "Motor"
    if vtype == "vType_2":
        return "Car"
    
###############################################################################
def create_vehicle_dict(l):
    lista = {}
    for x in l:
        temp = traci.vehicle.getPosition(x)
        vtype = traci.vehicle.getTypeID(x)
        v = Vehicle(x, temp[0], temp[1], getCname(vtype))
        #print(vtype)
        lista[x] = v
    return lista
 ###############################################################################  
def update_vehicle_dict_c(v, l, xy):
    l[v].current_x = xy[0]
    l[v].current_y = xy[1]
###############################################################################
def update_vehicle_dict(curr, dep, arr):
    for x in dep:
        del curr[x]
    for y in arr:
        temp = traci.vehicle.getPosition(y)
        vtype = traci.vehicle.getTypeID(y)
        #print(vtype)
        v = Vehicle(y, temp[0], temp[1], getCname(vtype))
        curr[y] = v
#############################################################################
def push(l,c):
    str1 = ''
    for x in l:
        if(l[x].current_x!=comp[0]):
            str1+= str(l[x].vehicle_id) + " " + str(l[x].current_x) + " " + str(l[x].current_y) + " " + str(l[x].vehicle_type) + " " 
    c.send(str1.encode())
###############################################################################
if __name__ == "__main__":
    
    sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM) 
    server_address = ('localhost', 10000)
    sock.bind(server_address)
    sock.listen(1)
    traci.start([sumoBinary,"-c" ,scenario])
    print("\nSumo simulation starting \n")
    Veh_ID_dict = create_vehicle_dict(traci.simulation.getLoadedIDList())
    
    try:
        while True:
            connection, client_address = sock.accept()
            print("Connected")
            status = connection.recv(16).decode()
            try:
                step = 0
                while step<130:
                    traci.simulationStep()
                    print(step)
                    update_vehicle_dict(Veh_ID_dict, traci.simulation.getArrivedIDList(), traci.simulation.getLoadedIDList())
                    for x in Veh_ID_dict:
                        temp = traci.vehicle.getPosition(x)
                        update_vehicle_dict_c(x, Veh_ID_dict, temp)
                    push(Veh_ID_dict, connection)
                    time.sleep(0.1)
                    step+=1
                traci.close()
                connection.send(end_message.encode())
                break
            
            finally:
                connection.close()
                print("End of Simulation")
    except:
        connection.close()
        print("Error in connection")