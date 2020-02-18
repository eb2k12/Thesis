# -*- coding: utf-8 -*-
"""
Created on Sun Feb  9 13:38:32 2020

@author: eoinb
"""

import socket

###############################################################################
class Vehicle:
    def __init__(self, vehicle_id, current_x, current_y):
        self.vehicle_id = vehicle_id
        self.current_x = current_x
        self.current_y = current_y
###############################################################################
if __name__ == "__main__":  
    sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    server_address = ('localhost', 10000)
    print ('connecting\n')
    sock.connect(server_address)
    
    vehicle_list = {}

    try:
        str1 = ''
        while True:
            str1 = sock.recv(1024).decode()
            list1 = str1.split()
            v = Vehicle(list1[0], list1[1], list1[2])
            vehicle_list[list1[0]] = v
            #
            
            for x in list1:
                print(x, " ")
            
            if(str1 == ''):
                break
            
    finally:
        print('closing socket\n')
        sock.close()