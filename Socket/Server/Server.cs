using System.Net;
using System.Net.Sockets;
using System.Text;    

namespace Server;

internal class Server {
    static void Main(string[] args) {
        Console.WriteLine("Server Program\n\n");

        // 서버 소켓 생성
        // (주소 체계: IPv4), (소켓 타입: 연결 지향), (프로토콜 타입: TCP)
        using (Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)) {

            // 서버 소켓에 IP, Port 할당
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("192.168.0.14"), 20000);
            serverSocket.Bind(endPoint);

            // 클라이언트 연결 요청을 대기하는 상태로 만듦
            // 백로그 큐 생성(최대 20개의 클라이언트가 백로그 큐에서 대기 가능)
            serverSocket.Listen(20);

            // 연결 요청 수락, 데이터 통신에 사용할 소켓 생성
            using (Socket clientSocket = serverSocket.Accept()) {
                Console.WriteLine("Client 연결됨(" + clientSocket.RemoteEndPoint + ")");

                while (true) {
                    // 클라이언트로부터 데이터 수신
                    byte[] buffer = new byte[256];                  // 데이터 내용이 저장됨
                    int totalBytes = clientSocket.Receive(buffer);  // 데이터 크기(bytes)가 저장됨

                    // 받은 데이터가 없으면 연결 종료
                    if (totalBytes < 1) {
                        Console.WriteLine("클라이언트의 연결 종료");
                        return;
                    }

                    // 역직렬화: byte 배열을 string 객체 형태로 변환
                    string str = Encoding.UTF8.GetString(buffer);
                    Console.WriteLine("Client: " + str);
                }
            }
        }
    }
}