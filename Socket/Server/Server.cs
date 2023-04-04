using System.Net;
using System.Net.Sockets;
using System.Text;    

namespace Server;

internal class Server {
    static readonly IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("192.168.0.14"), 20000);

    // Echo 전송 메소드
    static void SendEcho(Socket socket, string echoStr) {
        // 직렬화: 객체(문자열, 정수 등)를 전송 가능한 byte 배열로 변환
        byte[] echoStrBuffer = Encoding.UTF8.GetBytes(echoStr);         // 실제 데이터
        byte[] echoSizeBuffer = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)echoStrBuffer.Length));     // 데이터의 크기

        // 클라이언트로 데이터 전송(Echo)
        socket.Send(echoSizeBuffer, SocketFlags.None);
        socket.Send(echoStrBuffer, SocketFlags.None);
        return;
    }

    // 비동기 방식으로 데이터를 수신하는 메소드
    private static async void ReadAsync(object? sender) {
        Socket clientSocket = (Socket)sender;

        while (true) {
            // 수신할 데이터의 크기 가져오기
            byte[] sizeBuffer = new byte[2];
            int sizeToReceive = await clientSocket.ReceiveAsync(sizeBuffer, SocketFlags.None);

            // 수신할 데이터가 없으면 연결 종료
            if (sizeToReceive <= 0) {
                Console.WriteLine("[클라이언트 연결 해제됨] " + clientSocket.RemoteEndPoint);
                clientSocket.Shutdown(SocketShutdown.Both);     // 스트림 연결 종료(Send 및 Receive 불가)
                clientSocket.Close();                           // 소켓 자원 해제
                return;
            }

            // 역직렬화(바이트 배열 -> 정수) 후, 수신할 데이터의 크기 저장
            short dataSize = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(sizeBuffer));

            // 데이터 수신
            byte[] dataBuffer = new byte[dataSize];     // 데이터 버퍼
            int ReceiveSizeNow = await clientSocket.ReceiveAsync(dataBuffer, SocketFlags.None);

            // 클라이언트 이름 가져오기
            byte[] nameBuffer = new byte[20];
            await clientSocket.ReceiveAsync(nameBuffer, SocketFlags.None);

            // 역직렬화: byte 배열을 객체 형태(문자열)로 변환
            string str = Encoding.UTF8.GetString(dataBuffer);
            string name = Encoding.UTF8.GetString(nameBuffer);
            Console.WriteLine("{0}({1}): {2}", name, clientSocket.RemoteEndPoint, str);

            // Client로 Echo 전송
            SendEcho(clientSocket, str);
        }
    }

    // Main 메소드
    static async Task Main(string[] args) {
        Console.WriteLine("Server Program\n\n");

        // 서버 소켓 생성
        // (주소 체계: IPv4), (소켓 타입: 연결 지향), (프로토콜 타입: TCP)
        using (Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)) {
            // 서버 소켓의 SO_REUSEADDR 활성화
            // (Time-Wait 상태인 소켓의 포트를 다른 소켓이 사용할 수 있도록 설정)
            serverSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            // 서버 소켓에 IP, Port 할당
            serverSocket.Bind(endPoint);

            // 클라이언트 연결 요청을 대기하는 상태로 만듦
            // 백로그 큐 생성(최대 20개의 클라이언트가 백로그 큐에서 대기 가능)
            serverSocket.Listen(20);

            // 연결 요청 수락, 데이터 통신에 사용할 소켓 생성
            while (true) {
                Socket clientSocket = await serverSocket.AcceptAsync();
                Console.WriteLine("[클라이언트 연결됨] " + clientSocket.RemoteEndPoint);

                // SO_LINGER 활성화
                // (소켓 Close가 호출되면, 출력 버퍼의 데이터와 FIN 패킷을 전송함. 5초 안에 ACK 패킷을 받지 못하면 비정상 종료됨)
                clientSocket.LingerState = new LingerOption(true, 0);

                // Nagle 알고리즘 ON (기본값)
                clientSocket.NoDelay = false;

                // TAP(작업 기반 비동기 패턴): 스레드풀의 작업 큐에 메소드를 대기시킴
                ThreadPool.QueueUserWorkItem(ReadAsync, clientSocket);
            }
        }
    }
}