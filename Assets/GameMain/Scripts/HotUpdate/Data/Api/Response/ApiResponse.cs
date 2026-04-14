namespace Quester
{
    public class ApiResponse<T>
    {
        public int code;
        public string msg;
        public T data;
    }
}