using System.Net;
using System.Net.Sockets;

namespace Client;

internal class ClientProgram {
    static void Main(string[] args) {
        Console.WriteLine("Client Program\n\n");

        // 클라이언트 소켓 생성
        // (주소 체계: IPv4), (소켓 타입: 연결 지향), (프로토콜 타입: TCP)
        Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        // 클라이언트가 서버에 연결 요청
        IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("192.168.0.14"), 20000);
        clientSocket.Connect(endPoint);
    }
}