﻿using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Client;

internal class Client {
    static void Main(string[] args) {
        Console.WriteLine("Client Program\n\n");

        // 클라이언트 소켓 생성
        // (주소 체계: IPv4), (소켓 타입: 연결 지향), (프로토콜 타입: TCP)
        Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        // 클라이언트가 서버에 연결 요청
        IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("192.168.0.14"), 20000);
        clientSocket.Connect(endPoint);

        Console.WriteLine("서버로 전송할 문자열을 입력하세요.");
        Console.WriteLine("(문자열 없이 [Enter] 입력하면 프로그램 종료)\n");

        while (true) {
            // 서버로 전송할 문자열 입력
            Console.Write(">> ");
            string str = Console.ReadLine();

            // 문자열 없이 [Enter] 입력 시 프로그램 종료
            if (str == "") {
                return;
            }

            // 직렬화: string 객체를 전송 가능한 byte 배열로 변환
            byte[] buffer = Encoding.UTF8.GetBytes(str);

            // 서버로 데이터 전송
            clientSocket.Send(buffer);

            // 서버로부터 데이터 수신(Echo)
            byte[] bufferEcho = new byte[256];                  // 데이터 내용이 저장됨
            int bytesRead = clientSocket.Receive(bufferEcho);   // 데이터 크기(bytes)가 저장됨

            // 받은 데이터가 없으면 연결 종료
            if (bytesRead < 1) {
                Console.WriteLine("서버의 연결 종료");
                return;
            }

            // 역직렬화: byte 배열을 string 객체 형태로 변환
            string strEcho = Encoding.UTF8.GetString(bufferEcho);
            Console.WriteLine("Server: " + strEcho + "\n");
        }
    }
}