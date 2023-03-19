using System.Net;
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

        // 서버로 전송할 문자열 입력
        Console.Write(">> ");
        string str = Console.ReadLine();

        // 문자열 없이 [Enter] 입력 시 프로그램 종료
        if (str == "")
            return;

        // 직렬화: string 객체를 전송 가능한 byte 배열로 변환
        byte[] buffer = Encoding.UTF8.GetBytes(str);

        // 서버로 데이터 전송
        clientSocket.Send(buffer);
    }
}