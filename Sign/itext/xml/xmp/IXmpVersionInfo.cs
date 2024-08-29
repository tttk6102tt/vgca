namespace Sign.itext.xml.xmp
{
    public interface IXmpVersionInfo
    {
        int Major { get; }

        int Minor { get; }

        int Micro { get; }

        int Build { get; }

        bool Debug { get; }

        string Message { get; }
    }
}
