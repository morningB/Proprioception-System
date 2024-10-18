import matplotlib.pyplot as plt
import numpy as np
from sklearn.metrics import confusion_matrix, classification_report
import io
from PIL import Image
import socket

# 이미지 생성
def generate_visualization():
    x = np.linspace(0, 10, 100)
    y = np.sin(x)

    plt.figure(figsize=(6, 4))
    plt.plot(x, y, label='Model Prediction')
    plt.legend()
    plt.title('Model Output Visualization')

    # 이미지를 메모리에 저장
    buf = io.BytesIO()
    plt.savefig(buf, format='png')
    buf.seek(0)

    image_bytes = buf.getvalue()
    buf.close()

    return image_bytes

# 성능 지표 생성
def generate_performance_metrics():
    y_true = [0, 1, 0, 1, 0, 1]
    y_pred = [0, 1, 0, 0, 1, 1]

    confusion = confusion_matrix(y_true, y_pred)
    report = classification_report(y_true, y_pred)

    return confusion, report

# 데이터 타입 전송: 이미지인지 텍스트인지 구분
def send_data_with_type(conn, data_type, data):
    # 1. 데이터 타입을 전송
    conn.sendall(data_type.encode())

    # 2. 데이터 길이 전송 (4바이트)
    conn.sendall(len(data).to_bytes(4, 'big'))

    # 3. 실제 데이터 전송
    conn.sendall(data)

def start_server():
    server = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
    server.bind(('localhost', 5000))
    server.listen(5)

    print("Server is running and waiting for connection...")

    while True:
        conn, addr = server.accept()
        print(f"Connected to {addr}")

        try:
            # 이미지 생성 및 전송
            image_bytes = generate_visualization()
            send_data_with_type(conn, 'image', image_bytes)

            # 성능 지표 생성 및 전송
            confusion, report = generate_performance_metrics()

            confusion_str = str(confusion)
            report_str = report

            send_data_with_type(conn, 'text', confusion_str.encode())
            send_data_with_type(conn, 'text', report_str.encode())

        except Exception as e:
            print(f"Error during communication: {e}")
        finally:
            conn.close()

start_server()
