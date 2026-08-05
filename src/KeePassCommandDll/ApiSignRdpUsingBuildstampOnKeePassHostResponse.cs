namespace KeePassCommandDll
{
    public class ApiSignRdpUsingBuildstampOnKeePassHostResponse
    {
        public int ExitCode { get; set; }
        public string StdOut { get; set; }
        public string StdErr { get; set; }
        public byte[] SignedBytes { get; set; }
    }
}
