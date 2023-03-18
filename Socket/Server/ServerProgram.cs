using System.Net;
using System.Net.Sockets;
    
namespace Server;

internal class ServerProgram {
    static void Main(string[] args) {
        Console.WriteLine("Server Program\n\n");

        // 서버 소켓 생성
        // (주소 체계: IPv4), (소켓 타입: 연결 지향), (프로토콜 타입: TCP)
        Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        
        // 서버 소켓에 IP, Port 할당
        IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("192.168.0.14"), 20000);
        serverSocket.Bind(endPoint);

        // 클라이언트 연결 요청을 대기하는 상태로 만듦
        // 백로그 큐 생성(최대 20개의 클라이언트가 백로그 큐에서 대기 가능)
        serverSocket.Listen(20);

        // 연결 요청 수락, 데이터 통신에 사용할 소켓 생성
        Socket clientSocket = serverSocket.Accept();
        Console.WriteLine("연결됨 " + clientSocket.RemoteEndPoint);
    }
}