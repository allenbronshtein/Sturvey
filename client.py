import socket

HOST = '127.0.0.1'  
PORT = 6000        

with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
    s.connect((HOST, PORT))
    while 1:
        cmd = input(">>> ")
        if cmd == "break":
            break
        s.sendall(bytes(cmd, encoding='utf8'))