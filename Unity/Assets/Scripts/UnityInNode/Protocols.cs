public class Protocols
{
    public class Packets
    {
        public class common
        {
            public int cmd;                         // ��� ���� ǥ��
            public string message;                  // �޼���
        }

        public class req_data : common
        {
            public int id;                          // ID�� �޴´�.
            public string data;                     // ���� ������
        }

        public class res_data : common
        {
            public req_data[] result;               // list or Array ���� �޴´�.
        }
    }
}
