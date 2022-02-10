import json
import socket
import os
HOST = '127.0.0.1'  
PORT = 6000        

GUEST_ID = 0
ADMIN_ID = -1
SUCCESS_STATUS = 0
FAIL_STATUS = 1

with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
    s.connect((HOST, PORT))
    man_content = ""
    with open("clientManual.txt", 'r') as man:
        man_content = man.read()
    print(man_content)
    while 1:
        cmd = input(">>> ")
        if cmd == "break":
            break
        s.sendall(bytes(cmd, encoding='utf8'))
        try:
            data = s.recv(1024)
        except Exception:
            break
        if not data:
            break
        else:
            print("==========================================================")
            response = json.loads(data)
            date_time = response["Datetime"].split("T")
            uid = response["UID"]
            if uid == GUEST_ID:
                uid = "Guest"
            elif uid == ADMIN_ID:
                uid = "Admin"
            status = response["Status"]
            if status == SUCCESS_STATUS:
                status = "SUCCESS"
            elif status == FAIL_STATUS:
                status = "FAIL"
            data = response["ExecuterResponse"]
            if data is None:
                data = response["Message"]
            os.system("clear")
            print(f'{man_content}\nUser: {uid}\nServer response: {data}\nStatus: {status}')
            print("==========================================================")
            