namespace InfotainmentSystem.Data
{
    public enum DataStatus { Success, Empty, Error } // This enum represents the status of the data fetch operation, Success: valid data, Empty: no data, Error: failed to fetch data

    public class DataResult<T>
    {
        public DataStatus Status { get; }
        public T Value { get; } // This property holds the actual data fetched from the source.
        public string Error { get; } // This property holds the error message if the data fetch operation failed.

        /// <summary>
        /// Creates a new instance of DataResult with the specified status, value, and error message.
        /// </summary>
        /// <param name="status"></param>
        /// <param name="value"></param>
        /// <param name="error"></param>
        private DataResult(DataStatus status, T value, string error)
        {
            Status = status;
            Value = value;
            Error = error;
        }

        /// <summary>
        /// Creates a new instance of DataResult representing a successful data fetch operation with the specified value.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static DataResult<T> Success(T value) =>
            new DataResult<T>(DataStatus.Success, value, null);

        /// <summary>
        /// Creates a new instance of DataResult representing an empty data fetch operation.
        /// </summary>
        /// <returns></returns>
        public static DataResult<T> Empty() =>
            new DataResult<T>(DataStatus.Empty, default, null);

        /// <summary>
        /// Creates a new instance of DataResult representing a failed data fetch operation with the specified error message.
        /// </summary>
        /// <param name="error"></param>
        /// <returns></returns>
        public static DataResult<T> Failure(string error) =>
            new DataResult<T>(DataStatus.Error, default, error);
    }
}
