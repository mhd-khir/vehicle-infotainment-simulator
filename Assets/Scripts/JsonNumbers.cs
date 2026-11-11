using Newtonsoft.Json.Linq;

namespace InfotainmentSystem.Data
{
    public static class JsonNumbers
    {
        /// <summary>
        /// Useful for parsing JSON numbers that may be either integers or floats, and may be out of range for float.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool TryFloat(JToken token, out float value)
        {
            value = 0f;
            if (token == null ||
                (token.Type != JTokenType.Float && token.Type != JTokenType.Integer))
                return false;
            double number = token.Value<double>();
            if (double.IsNaN(number) || double.IsInfinity(number) ||
                number > float.MaxValue || number < -float.MaxValue)
                return false;
            value = (float)number;
            return true;
        }
    }
}
