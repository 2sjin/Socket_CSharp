using System.Net;
using System.Net.Sockets;
using System.Text;    

namespace Server;

internal class Server {
    static readonly IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("192.168.0.14"), 20000);
    const int HEADER_SIZE = 2;      // 헤더의 크기

    static void Main(string[] args) {
        Console.WriteLine("Server Program\n\n");

        // 서버 소켓 생성
        // (주소 체계: IPv4), (소켓 타입: 연결 지향), (프로토콜 타입: TCP)
        using (Socket serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)) {
            // 서버 소켓의 SO_REUSEADDR 활성화(Time-Wait 상태인 소켓의 포트를 다른 소켓이 사용할 수 있도록 설정)
            serverSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            // 서버 소켓에 IP, Port 할당
            serverSocket.Bind(endPoint);

            // 클라이언트 연결 요청을 대기하는 상태로 만듦
            // 백로그 큐 생성(최대 20개의 클라이언트가 백로그 큐에서 대기 가능)
            serverSocket.Listen(20);

            // 연결 요청 수락, 데이터 통신에 사용할 소켓 생성
            using (Socket clientSocket = serverSocket.Accept()) {
                // SO_LINGER 활성화
                // (소켓 Close가 호출되면, 출력 버퍼의 데이터와 FIN 패킷을 전송함. 5초 안에 ACK 패킷을 받지 못하면 비정상 종료됨)
                clientSocket.LingerState = new LingerOption(true, 0);

                Console.WriteLine("Client 연결됨(" + clientSocket.RemoteEndPoint + ")");

                while (true) {
                    // 수신해야 할 데이터의 크기를 얻는 과정
                    byte[] headerBuffer = new byte[HEADER_SIZE];                // 헤더 버퍼
                    int ReceiveSizeHeader = clientSocket.Receive(headerBuffer); // 클라이언트로부터 버퍼의 헤더 부분 받아오기

                    // 헤더를 받지 않았으면 연결 종료
                    if (ReceiveSizeHeader <= 0) {
                        Console.WriteLine("클라이언트의 연결 종료");
                        clientSocket.Shutdown(SocketShutdown.Both);     // 스트림 연결 종료(Send 및 Receive 불가)
                        clientSocket.Close();                           // 소켓 자원 해제
                        return;
                    }
                    // 헤더를 일부만 받았을 경우, 나머지를 추가로 받아옴
                    else if (ReceiveSizeHeader < HEADER_SIZE) {
                        clientSocket.Receive(headerBuffer, HEADER_SIZE, HEADER_SIZE-ReceiveSizeHeader, SocketFlags.None);
                    }

                    // 헤더 역직렬화(바이트 배열 -> 정수) 후, 받아야 할 데이터의 크기 저장
                    short dataSize = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(headerBuffer));

                    //////////////////////////////////////////////////////////////////////////////////////////////

                    // 실제 데이터를 얻는 과정
                    byte[] dataBuffer = new byte[dataSize];     // 데이터 버퍼
                    int totalReceivedSize = 0;  // 지금까지 받은 데이터의 크기(Bytes)

                    // 모든 데이터를 받을 때까지, 반복해서 데이터 수신
                    while (totalReceivedSize < dataSize) {
                        int ReceiveSizeNow = clientSocket.Receive(dataBuffer, totalReceivedSize, dataSize - totalReceivedSize, SocketFlags.None);
                        totalReceivedSize += ReceiveSizeNow;
                    }

                    // 역직렬화: byte 배열을 객체 형태(문자열)로 변환
                    string str = Encoding.UTF8.GetString(dataBuffer);
                    Console.WriteLine("Client: " + str);

                    // 클라이언트로 데이터 전송
                    // clientSocket.Send(buffer);
                }
            }
        }
    }
}