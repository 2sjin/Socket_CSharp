using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Client;

internal class Client {
    static readonly IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("192.168.0.14"), 20000);
    const int HEADER_SIZE = 2;      // 헤더의 크기

    static void Main(string[] args) {
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
                byte[] newBuffer = new byte[HEADER_SIZE + strBuffer.Length];      // 2바이트 헤더를 추가한 새로운 버퍼

                // 새로운 버퍼에 데이터 입력
                // Array.Copy(복사할 배열, 시작 인덱스, 붙여넣을 배열, 시작 인덱스, 복사할 길이)
                Array.Copy(dataSize, 0, newBuffer, 0, dataSize.Length);     // 헤더에 데이터의 크기 입력
                Array.Copy(strBuffer, 0, newBuffer, HEADER_SIZE, strBuffer.Length);   // 데이터 입력

                // 서버로 데이터 전송
                clientSocket.Send(newBuffer, SocketFlags.None);

                ///////////////////////////////////////////////////////////////////////////////////////////////////

                // 수신해야 할 데이터의 크기를 얻는 과정
                byte[] headerBuffer = new byte[HEADER_SIZE];                // 헤더 버퍼
                int ReceiveSizeHeader = clientSocket.Receive(headerBuffer); // 클라이언트로부터 버퍼의 헤더 부분 받아오기

                // 헤더를 받지 않았으면 연결 종료
                if (ReceiveSizeHeader <= 0) {
                    Console.WriteLine("클라이언트와 연결 해제됨");
                    clientSocket.Shutdown(SocketShutdown.Both);     // 스트림 연결 종료(Send 및 Receive 불가)
                    clientSocket.Close();                           // 소켓 자원 해제
                    return;
                }
                // 헤더를 일부만 받았을 경우, 나머지를 추가로 받아옴
                else if (ReceiveSizeHeader < HEADER_SIZE) {
                    clientSocket.Receive(headerBuffer, HEADER_SIZE, HEADER_SIZE - ReceiveSizeHeader, SocketFlags.None);
                }

                // 헤더 역직렬화(바이트 배열 -> 정수) 후, 받아야 할 데이터의 크기 저장
                short echoDataSize = IPAddress.NetworkToHostOrder(BitConverter.ToInt16(headerBuffer));

                //////////////////////////////////////////////////////////////////////////////////////////////

                // 실제 데이터를 얻는 과정
                byte[] echoDataBuffer = new byte[echoDataSize];     // 데이터 버퍼
                int echoTotalReceivedSize = 0;  // 지금까지 받은 데이터의 크기(Bytes)

                // 모든 데이터를 받을 때까지, 반복해서 데이터 수신
                while (echoTotalReceivedSize < echoDataSize) {
                    int ReceiveSizeNow = clientSocket.Receive(echoDataBuffer, echoTotalReceivedSize, echoDataSize - echoTotalReceivedSize, SocketFlags.None);
                    echoTotalReceivedSize += ReceiveSizeNow;
                }

                // 역직렬화: byte 배열을 객체 형태(문자열)로 변환
                string echoStr = Encoding.UTF8.GetString(echoDataBuffer);
                Console.WriteLine("Echo: " + str);
            }
        }
    }
}