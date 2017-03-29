/*
auth: Xiang ChunSong
purpose:
*/

namespace Base
{
    public static class ResourceDataExtension
    {
        public static bool IsInstall(this ResourceData rd)
        {
            return (rd.Type & ResourceType.Install) != 0;
        }

        public static bool IsOptional(this ResourceData rd)
        {
            return (rd.Type & ResourceType.Optional) != 0;
        }

        public static bool IsUnpackage(this ResourceData rd)
        {
            return (rd.Type & ResourceType.Unpackage) != 0;
        }
    }
}