namespace DSFinalProject
{
    // Box Size Struct (X, Y)
    public struct BoxSize
    {
        public double x;
        public double y;

        // Parse to allow saving the data of this struct in JSON (in the format of 'X'-'Y')
        public static BoxSize Parse(string s)
        {
            string[] parts = s.Split('-');

            return new BoxSize
            {
                x = int.Parse(parts[0]),
                y = int.Parse(parts[1]),
            };
        }
        public override string ToString()
        {
            return $"{x}-{y}";
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(x, y);
        }
    }
}
