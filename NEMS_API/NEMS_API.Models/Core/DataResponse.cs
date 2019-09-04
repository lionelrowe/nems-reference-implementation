namespace NEMS_API.Models.Core
{
    public class DataResponse<T> : Response
    {
        public DataResponse() : base() { }

        public DataResponse(bool success) : base(success) { }

        public DataResponse(string message) : base(message) { }

        public DataResponse(bool success, string message) : base(success, message) { }

        public T Data { get; set; }

    }
}
