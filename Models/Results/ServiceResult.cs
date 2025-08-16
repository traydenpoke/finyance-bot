namespace FinyanceApp.Results
{
  // Generic result wrapper for success/failure + optional data
  public class ServiceResult<T>
  {
    public bool Success { get; set; }
    public string Message { get; set; } = "";
    public T? Data { get; set; }

    public static ServiceResult<T> Ok(T data, string msg = "") =>
        new() { Success = true, Message = msg, Data = data };

    public static ServiceResult<T> Fail(string msg) =>
        new() { Success = false, Message = msg, Data = default };
  }
}