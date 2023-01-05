namespace audiamus.aux
{
    public abstract class Encoding : System.Text.Encoding
    {
        public static System.Text.Encoding Latin1 => System.Text.Encoding.GetEncoding("ISO-8859-1");
    }
}
