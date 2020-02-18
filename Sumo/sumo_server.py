# -*- coding: utf-8 -*-
"""
Created on Fri Feb  7 14:04:07 2020

@author: eoinb
"""
from __future__ import absolute_import
from __future__ import print_function
from sumolib import checkBinary
import os
import sys
import traci
import socket
import time 

abspath = os.path.abspath(__file__)
dname = os.path.dirname(abspath)
os.chdir(dname)

if 'SUMO_HOME' in os.environ:
    tools = os.path.join(os.environ['SUMO_HOME'], 'tools')
    sys.path.append(tools)
else:
    sys.exit("please declare environment variable 'SUMO_HOME'")
    
fname = 'GCD2.sumocfg'
sumoBinary = checkBinary('sumo')
comp = [-1073741824.0, -1073741824.0]
###############################################################################
class Vehicle:
    def __init__(self, vehicle_id, current_x, current_y):
        self.vehicle_id = vehicle_id
        self.current_x = current_x
        self.current_y = current_y
###############################################################################
def create_vehicle_dict(l):
    lista = {}
    for x in l:
        temp = traci.vehicle.getPosition(x)
        v = Vehicle(x, temp[0], temp[1])
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
        v = Vehicle(y, temp[0], temp[1])
        curr[y] = v
#############################################################################
def push(l,c):
    str1 = ''
    for x in l:
        if(l[x].current_x!=comp[0]):
            str1+= str(l[x].vehicle_id) + " " + str(l[x].current_x) + " " + str(l[x].current_y) + " "  
    #print(str1)
    c.send(str1.encode())
###############################################################################
if __name__ == "__main__":
    
    sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM) 
    server_address = ('localhost', 10000)
    sock.bind(server_address)
    sock.listen(1)
    traci.start([sumoBinary, "-c", fname])
    print("\nSumo simulation starting \n")
    Veh_ID_dict = create_vehicle_dict(traci.simulation.getLoadedIDList())
    
    try:
        while True:
            connection, client_address = sock.accept()
            print("Connected")
            status = connection.recv(16).decode()
            try:
                step = 0
                while step<100:
                    traci.simulationStep()
                    update_vehicle_dict(Veh_ID_dict, traci.simulation.getArrivedIDList(), traci.simulation.getLoadedIDList())
                    for x in Veh_ID_dict:
                        temp = traci.vehicle.getPosition(x)
                        update_vehicle_dict_c(x, Veh_ID_dict, temp)
                    push(Veh_ID_dict, connection)
                    time.sleep(0.3)
                    step+=1
                traci.close()
                end_message = "disconnect_1 "
                connection.send(end_message.encode())
                break
            
            finally:
                connection.close()
                print("Error in connection")
    except:
        connection.close()
        print("Error in connection")