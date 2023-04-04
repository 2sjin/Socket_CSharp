using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Client;

internal class Client {
    static readonly IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("192.168.0.14"), 20000);

    // Echo 수신 메소드
    static string ReceiveEcho(Socket socket) {
        // 수신할 데이터의 크기 가져오기
        byte[] sizeBuffer = new byte[2];
        int sizeToReceive = socket.Receive(sizeBuffer);

        // 수신할 데이터가 없으면 연결 종료
        if (sizeToReceive <= 0) {
            Console.WriteLine("클라이언트와 연결 해제됨");
            socket.Shutdown(SocketShutdown.Both);     // 스트림 연결 종료(Send 및 Receive 불가)
            socket.Close();                           // 소켓 자원 해제
            return "";
        }

        // 역직렬화(바이트 배열 -> 정수) 후, 수신할 데이터의 크기 저장
        short echoDataSize = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(sizeBuffer));

        // Echo 데이터 수신
        byte[] echoDataBuffer = new byte[echoDataSize];     // 데이터 버퍼
        socket.Receive(echoDataBuffer, SocketFlags.None);

        // 역직렬화: byte 배열을 객체 형태(문자열)로 변환
        string echoStr = Encoding.UTF8.GetString(echoDataBuffer);

        // 수신한 Echo 문자열 반환
        return echoStr;
    }

    // Main 메소드
    static async Task Main(string[] args) {
        Console.WriteLine("Client Program\n\n");

        // 클라이언트 소켓 생성
        // (주소 체계: IPv4), (소켓 타입: 연결 지향), (프로토콜 타입: TCP)
        using (Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)) {
            // SO_LINGER 활성화
            // (소켓 Close가 호출되면, 출력 버퍼의 데이터를 비우고 상대 측에 비정상 종료를 알림)
            clientSocket.LingerState = new LingerOption(true, 0);

            // Nagle 알고리즘 ON (기본값)
            clientSocket.NoDelay = false;   

            // 클라이언트가 서버에 연결 요청
            clientSocket.Connect(endPoint);

            Console.WriteLine("서버로 전송할 문자열을 입력하세요.");
            Console.WriteLine("(문자열 없이 [Enter] 입력하면 클라이언트 종료)\n");

            while (true) {
                // 서버로 전송할 문자열 입력
                Console.Write(">> ");
                string str = Console.ReadLine();

                // 문자열 없이 [Enter] 입력 시 프로그램 종료
                if (str == "") {
                    Console.WriteLine("클라이언트 프로그램 종료");
                    clientSocket.Shutdown(SocketShutdown.Both);     // 스트림 연결 종료(Send 및 Receive 불가)
                    clientSocket.Close();                           // 소켓 자원 해제
                    return;
                }

                // 직렬화: 객체(문자열, 정수 등)를 전송 가능한 byte 배열로 변환
                byte[] strBuffer = Encoding.UTF8.GetBytes(str);         // 실제 데이터
                byte[] dataSize = BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)strBuffer.Length));     // 데이터의 크기

                // 서버로 데이터 전송
                await clientSocket.SendAsync(dataSize, SocketFlags.None);
                await clientSocket.SendAsync(strBuffer, SocketFlags.None);

                // 서버로부터 Echo 수신
                Console.WriteLine("Echo: " + ReceiveEcho(clientSocket));
            }
        }
    }
}