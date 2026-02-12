namespace KeePassCommandDll
{
    public class ApiSignUsingBuildstampOnKeePassHostResponse
    {
        public int ExitCode { get; set; }
        public string StdOut { get; set; }
        public string StdErr { get; set; }
        public byte[] SignedBytes { get; set; }
    }
}
